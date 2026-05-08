using Lumi.AST;
using Lumi.Bytecode;
using Lumi.VM;

namespace Lumi.Engine;

public sealed class PipelineExecutionContext
{
    public required string Source { get; init; }
    public required VirtualMachine VirtualMachine { get; init; }
    public required BytecodeGenerator BytecodeGenerator { get; init; }
    public required SemanticAnalyzer.SemanticAnalyzer SemanticAnalyzer { get; init; }
    public required TextWriter Output { get; init; }
    public Node? Ast { get; private set; }

    public Parser.Parser CreateParser()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Source);
        return new(Source);
    }

    public void SetAst(Node ast)
    {
        ArgumentNullException.ThrowIfNull(ast);
        Ast = ast;
    }
}
