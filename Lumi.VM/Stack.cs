namespace Lumi.VM;

/// <summary>
/// Represents a last-in, first-out (LIFO) collection of Value objects.
/// </summary>
internal sealed class Stack
{
    private const int MaxStackSize = 1024; // Define a maximum stack size to prevent overflow
    public Stack<Value> Values { get; } = [];

    public void Push(Value value)
    {
        if (Values.Count + 1 > MaxStackSize)
            throw VirtualMachineError.StackOverflow();

        Values.Push(value);
    }

    public Value Pop()
    {
        Values.TryPop(out var value);

        if (value is null)
            throw VirtualMachineError.StackUnderflow();

        return value;
    }

    public Value Peek() => Values.Peek();

    public Value Peek(int offset)
    {
        if (offset < 0 || offset >= Values.Count)
            throw VirtualMachineError.InvalidPeekOffset();

        return Values.ToArray()[Values.Count - 1 - offset];
    }
}