using Lumi.AST;
using Lumi.Bytecode;
using ParserType = Lumi.Parser.Parser;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;
using Lumi.VM;

namespace Lumi.Engine;

public static class ExecutionPipeline
{
    public static bool TryExecute(string source, VirtualMachine vm, BytecodeGenerator bytecodeGenerator, SemanticAnalyzerType semanticAnalyzer, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(vm);
        ArgumentNullException.ThrowIfNull(bytecodeGenerator);
        ArgumentNullException.ThrowIfNull(semanticAnalyzer);
        ArgumentNullException.ThrowIfNull(output);

        var originalOut = Console.Out;

        try
        {
            Console.SetOut(output);

            var parser = new ParserType(source);
            var ast = parser.Parse();

            if (!TryPrintParseErrors(parser))
            {
                return false;
            }

            if (!TryRunSemanticAnalysis(semanticAnalyzer, ast))
            {
                return false;
            }

            return TryExecuteBytecode(vm, bytecodeGenerator, ast);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static bool TryPrintParseErrors(ParserType parser)
    {
        if (!parser.HasErrors)
        {
            return true;
        }

        Console.WriteLine("Errors encountered during parsing: ");

        foreach (var error in parser.Errors)
        {
            Console.WriteLine(error);
        }

        return false;
    }

    private static bool TryRunSemanticAnalysis(SemanticAnalyzerType semanticAnalyzer, Node? ast)
    {
        if (ast is not AST.Program program)
        {
            Console.WriteLine("Error: AST is null");
            return false;
        }

        var analysisResult = semanticAnalyzer.Analyze(program);

        if (analysisResult.IsValid)
        {
            return true;
        }

        foreach (var error in analysisResult.Errors)
        {
            Console.WriteLine($"Semantic error: {error.Message}");
        }

        return false;
    }

    private static bool TryExecuteBytecode(VirtualMachine vm, BytecodeGenerator bytecodeGenerator, Node? ast)
    {
        if (ast == null)
        {
            Console.WriteLine("Error: AST is null");
            return false;
        }

        vm.Execute(bytecodeGenerator.Generate(ast));
        return true;
    }
}