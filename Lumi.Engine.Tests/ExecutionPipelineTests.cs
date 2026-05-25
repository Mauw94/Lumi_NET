using Lumi.Bytecode;
using Lumi.Engine.ExecutionSteps;
using Lumi.VM;
using Microsoft.Extensions.DependencyInjection;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;

namespace Lumi.Engine.Tests;

[TestClass]
[DoNotParallelize]
public sealed class ExecutionPipelineTests
{
    [TestMethod]
    public void TryExecute_ParseError_DoesNotExecuteScript()
    {
        var source = "print ; print 999999;";
        using var writer = new StringWriter();

        var pipeline = CreatePipeline();

        var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

        var output = writer.ToString();
        var lines = SplitLines(output);

        Assert.IsFalse(succeeded);
        Assert.Contains("Errors encountered during parsing", output);
        Assert.IsFalse(lines.Contains("999999", StringComparer.Ordinal));
    }

    [TestMethod]
    public void TryExecute_SemanticError_DoesNotExecuteScript()
    {
        var source = """
            const x -> 42;
            x = 100;
            print 999999;
            """;

        using var writer = new StringWriter();

        var pipeline = CreatePipeline();

        var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

        var output = writer.ToString();
        var lines = SplitLines(output);

        Assert.IsFalse(succeeded);
        Assert.Contains("Semantic error:", output);
        Assert.Contains("read-only", output);
        Assert.IsFalse(lines.Contains("999999", StringComparer.Ordinal));
    }

    [TestMethod]
    public void TryExecute_StructDefinition_And_FieldAccess_Succeeds()
    {
        var source = """
            struct Person {
                name: str;
                age: int;
            }
            let person: Person -> new Person;
            print person.name;
            """;

        using var writer = new StringWriter();

        var pipeline = CreatePipeline();

        var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

        var output = writer.ToString();
        var lines = SplitLines(output);

        Assert.IsTrue(succeeded);
        Assert.HasCount(1, lines);
        Assert.AreEqual("undefined", lines[0]);
    }

    [TestMethod]
    public void TryExecute_Struct_NewArgs_And_FieldAssignment_Succeeds()
    {
        var source = """
            struct Person {
                name: str;
                age: int;
            }
            let p: Person -> new Person("Alice", 30);
            p.age = 5;
            print p.name;
            print p.age;
            """;

        using var writer = new StringWriter();

        var pipeline = CreatePipeline();

        var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

        var output = writer.ToString();
        var lines = SplitLines(output);

        Assert.IsTrue(succeeded);
        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Alice\"", lines[0]);
        Assert.AreEqual("5", lines[1]);
    }

    [TestMethod]
    public void AddLumiEngine_RegistersExecutionPipelineSteps()
    {
        var services = new ServiceCollection();

        services.AddLumiEngine();

        var steps = services.BuildServiceProvider().GetServices<IPipelineExecutionStep>().ToArray();

        Assert.HasCount(4, steps);
        Assert.IsInstanceOfType<ParsingStep>(steps[0]);
        Assert.IsInstanceOfType<SemanticAnalysisStep>(steps[1]);
        Assert.IsInstanceOfType<BytecodeExecutionStep>(steps[2]);
        Assert.IsInstanceOfType<VirtualMachineExecutionStep>(steps[3]);
    }

    [TestMethod]
    public void TryExecute_FileWriteThenRead_Succeeds()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        var escapedPath = EscapeForLumi(tempPath);

        var source = $$"""
            File.writeText("{{escapedPath}}", "42");
            let contents: str -> File.readText("{{escapedPath}}");
            print contents;
            """;

        using var writer = new StringWriter();

        try
        {
            var pipeline = CreatePipeline();
            var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);
            var output = writer.ToString();
            var lines = SplitLines(output);

            Assert.IsTrue(succeeded);
            Assert.HasCount(1, lines);
            Assert.AreEqual("\"42\"", lines[0]);
            Assert.AreEqual("42", File.ReadAllText(tempPath));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [TestMethod]
    public void TryExecute_FileReadFailure_UsesVirtualMachineError()
    {
        var missingPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        var escapedPath = EscapeForLumi(missingPath);

        var source = $$"""
            let contents: str -> File.readText("{{escapedPath}}");
            print contents;
            """;

        using var writer = new StringWriter();

        var pipeline = CreatePipeline();
        var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);
        var output = writer.ToString();

        Assert.IsFalse(succeeded);
        Assert.Contains("Lumi.VM.VirtualMachineError", output);
        Assert.IsFalse(output.Contains("System.IO.FileNotFoundException", StringComparison.Ordinal));
    }

    [TestMethod]
    public void TryExecute_FileReadLines_Returns_HeapBacked_List()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        var escapedPath = EscapeForLumi(tempPath);

        File.WriteAllLines(tempPath, ["alpha", "beta"]);

        var source = $$"""
            let lines: list -> File.readLines("{{escapedPath}}");
            print lines.length();
            print lines[0];
            print lines[1];
            """;

        using var writer = new StringWriter();

        try
        {
            var pipeline = CreatePipeline();
            var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);
            var output = writer.ToString();
            var lines = SplitLines(output);

            Assert.IsTrue(succeeded);
            Assert.HasCount(3, lines);
            Assert.AreEqual("2", lines[0]);
            Assert.AreEqual("\"alpha\"", lines[1]);
            Assert.AreEqual("\"beta\"", lines[2]);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [TestMethod]
    public void TryExecute_FileWriteLines_Accepts_HeapBacked_List()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        var escapedPath = EscapeForLumi(tempPath);

        var source = $$"""
            let lines: list -> ["alpha", "beta"];
            File.writeLines("{{escapedPath}}", lines);
            """;

        using var writer = new StringWriter();

        try
        {
            var pipeline = CreatePipeline();
            var succeeded = pipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);
            var output = writer.ToString();

            Assert.IsTrue(succeeded);
            Assert.AreEqual(string.Empty, output);
            CollectionAssert.AreEqual(new[] { "alpha", "beta" }, File.ReadAllLines(tempPath));
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private static ExecutionPipeline CreatePipeline()
    {
        var services = new ServiceCollection();
        services.AddLumiEngine();
        return services.BuildServiceProvider().GetRequiredService<ExecutionPipeline>();
    }

    private static string EscapeForLumi(string value)
        => value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string[] SplitLines(string value) =>
        value.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
