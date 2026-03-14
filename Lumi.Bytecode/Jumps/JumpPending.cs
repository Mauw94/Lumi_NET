namespace Lumi.Bytecode.Jumps;

/// <summary>
/// Represents a pending jump operation to a specified target within the bytecode execution flow.
/// </summary>
internal sealed class JumpPending(int target) : PendingJump
{
    public int Target { get; } = target;
}