namespace Lumi.VM;

/// <summary>
/// Represents a single activation record on the call stack.
/// Stores the instruction pointer to resume after the callee returns and the caller's
/// base pointer so the variable frame can be restored.
/// </summary>
internal readonly record struct CallFrame(int ReturnAddress, int PreviousBasePointer);