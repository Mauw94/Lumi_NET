namespace Lumi.VM;

/// <summary>
/// Represents a last-in, first-out (LIFO) collection of Value objects.
/// </summary>
internal sealed class Stack
{
    public Stack<Value> Values { get; } = [];

    public void Push(Value value) => Values.Push(value);
    public Value Pop() => Values.Pop();
    public Value Peek() => Values.Peek();
    public Value Peek(int offset)
    {
        if (offset < 0 || offset >= Values.Count)
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset must be non-negative and less than the stack size.");

        return Values.ToArray()[Values.Count - 1 - offset];
    }
}