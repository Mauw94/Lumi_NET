namespace Lumi.Engine.ExecutionSteps;

public interface IPipelineExecutionStep
{
    int Order { get; }
    bool TryExecute(PipelineExecutionContext context);
}