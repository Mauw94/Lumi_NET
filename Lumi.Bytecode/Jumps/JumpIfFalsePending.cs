namespace Lumi.Bytecode.Jumps;

/// <summary>
/// Jump if false pending represents a jump instruction that will be resolved later. 
/// It indicates that if the condition is false, the execution should jump to the specified target address. 
/// The target address is represented as an integer, which will be resolved to an actual instruction index during the bytecode generation process.
/// </summary>
internal sealed class JumpIfFalsePending(int target) : PendingJump
{
    public int Target { get; } = target;
}