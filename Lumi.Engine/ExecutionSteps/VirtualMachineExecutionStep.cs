namespace Lumi.Engine.ExecutionSteps;

public sealed class VirtualMachineExecutionStep : IPipelineExecutionStep
{
    public int Order => ExecutionStepOrder.VirtualMachineExecutionStep;

    public bool TryExecute(PipelineExecutionContext context)
    {
        context.VirtualMachine.Execute(context.BytecodeResult!);
        return true;
    }
}
