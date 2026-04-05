namespace Lumi.VM;

/// <summary>
/// Represents a single activation record on the call stack.
/// Stores the instruction pointer to resume after the callee returns.
/// </summary>
internal readonly record struct CallFrame(int ReturnAddress);