namespace Lumi.Engine.ExecutionSteps;

internal static class ExecutionStepOrder
{
    public const int ParsingStep = 10;
    public const int SemanticAnalysisStep = 20;
    public const int BytecodeExecutionStep = 30;
    public const int VirtualMachineExecutionStep = 40;
}