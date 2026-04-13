using Lumi.Bytecode;
using Lumi.Engine;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;
using Lumi.VM;

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

        var succeeded = ExecutionPipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

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

        var succeeded = ExecutionPipeline.TryExecute(source, new VirtualMachine(), new BytecodeGenerator(), new SemanticAnalyzerType(), writer);

        var output = writer.ToString();
        var lines = SplitLines(output);

        Assert.IsFalse(succeeded);
        Assert.Contains("Semantic error:", output);
        Assert.Contains("read-only", output);
        Assert.IsFalse(lines.Contains("999999", StringComparer.Ordinal));
    }

    private static string[] SplitLines(string value) =>
        value.Split(["\r\n", "\n", "\r"], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}