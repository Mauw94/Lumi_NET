namespace Lumi.VM;

/// <summary>
/// Represents a last-in, first-out (LIFO) collection of Value objects.
/// </summary>
internal sealed class Stack
{
    private const int MaxStackSize = 1024;
    // List used as the backing store so Peek(offset) is O(1) with no allocation
    private readonly List<Value> _values = new();

    public int Count => _values.Count;

    public void Push(Value value)
    {
        if (_values.Count >= MaxStackSize)
            throw VirtualMachineError.StackOverflow();

        _values.Add(value);
    }

    public Value Pop()
    {
        if (_values.Count == 0)
            throw VirtualMachineError.StackUnderflow();

        var value = _values[^1];
        _values.RemoveAt(_values.Count - 1);
        return value;
    }

    public Value Peek()
    {
        if (_values.Count == 0)
            throw VirtualMachineError.StackUnderflow();

        return _values[^1];
    }

    public Value Peek(int offset)
    {
        if (offset < 0 || offset >= _values.Count)
            throw VirtualMachineError.InvalidPeekOffset();

        return _values[_values.Count - 1 - offset];
    }
}