namespace Lumi.Engine.ExecutionSteps;

public sealed class SemanticAnalysisStep : IPipelineExecutionStep
{
    public int Order => ExecutionStepOrder.SemanticAnalysisStep;

    public bool TryExecute(PipelineExecutionContext context)
    {
        if (context.Ast is not AST.Program program)
        {
            Console.WriteLine("Error: AST is null");
            return false;
        }

        var analysisResult = context.SemanticAnalyzer.Analyze(program);

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
}
