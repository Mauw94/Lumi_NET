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

        Assert.HasCount(3, steps);
        Assert.IsInstanceOfType<ParsingStep>(steps[0]);
        Assert.IsInstanceOfType<SemanticAnalysisStep>(steps[1]);
        Assert.IsInstanceOfType<BytecodeExecutionStep>(steps[2]);
    }

    private static ExecutionPipeline CreatePipeline()
    {
        var services = new ServiceCollection();
        services.AddLumiEngine();
        return services.BuildServiceProvider().GetRequiredService<ExecutionPipeline>();
    }

    private static string[] SplitLines(string value) =>
        value.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}