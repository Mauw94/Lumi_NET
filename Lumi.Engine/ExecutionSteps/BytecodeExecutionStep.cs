namespace Lumi.Engine.ExecutionSteps;

public sealed class BytecodeExecutionStep : IPipelineExecutionStep
{
    public int Order => ExecutionStepOrder.BytecodeExecutionStep;

    public bool TryExecute(PipelineExecutionContext context)
    {
        if (context.Ast == null)
        {
            Console.WriteLine("Error: AST is null");
            return false;
        }

        context.SetBytecodeResult(context.BytecodeGenerator.Generate(context.Ast));
        return true;
    }
}
