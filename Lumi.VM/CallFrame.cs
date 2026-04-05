namespace Lumi.VM;

/// <summary>
/// Represents a single activation record on the call stack.
/// Stores the instruction pointer to resume after the callee returns and a snapshot
/// of the caller's variable slots so recursive calls do not corrupt them.
/// </summary>
internal readonly record struct CallFrame(int ReturnAddress, Value?[] SavedVariables);