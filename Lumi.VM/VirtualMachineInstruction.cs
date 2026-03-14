namespace Lumi.VM;

/// <summary>
/// Represents an abstract instruction that operates on a virtual machine stack.
/// </summary>
/// <param name="stack">The stack instance on which the instruction operates. Cannot be null.</param>
internal abstract class VirtualMachineInstruction(Stack stack)
{
    public Stack Stack { get; } = stack;
}