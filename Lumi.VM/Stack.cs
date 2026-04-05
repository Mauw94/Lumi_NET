namespace Lumi.VM;

/// <summary>
/// Represents a last-in, first-out (LIFO) collection of Value objects.
/// Uses a fixed-size array with a top pointer for maximum performance.
/// </summary>
internal sealed class Stack
{
    private const int MaxStackSize = 1024;
    private readonly Value[] _values = new Value[MaxStackSize];
    private int _top;

    public int Count => _top;

    public void Push(Value value)
    {
        if (_top >= MaxStackSize)
            throw VirtualMachineError.StackOverflow();

        _values[_top++] = value;
    }

    public Value Pop()
    {
        if (_top == 0)
            throw VirtualMachineError.StackUnderflow();

        return _values[--_top];
    }

    public Value Peek()
    {
        if (_top == 0)
            throw VirtualMachineError.StackUnderflow();

        return _values[_top - 1];
    }

    public Value Peek(int offset)
    {
        if (offset < 0 || offset >= _top)
            throw VirtualMachineError.InvalidPeekOffset();

        return _values[_top - 1 - offset];
    }
}