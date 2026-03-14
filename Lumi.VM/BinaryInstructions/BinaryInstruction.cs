namespace Lumi.VM.BinaryInstructions;

/// <summary>
/// Represents a binary instruction that operates on two values from the stack.
/// </summary>
/// <param name="stack"></param>
internal sealed class BinaryInstruction(Stack stack) : VirtualMachineInstruction(stack)
{
    public void Add()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
            Stack.Push(Value.FromNumber(b.Number + a.Number));
        else
            throw VirtualMachineError.InvalidValueTypes(a, b, "Add");
    }

    public void Sub()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
            Stack.Push(Value.FromNumber(b.Number - a.Number));
        else
            throw VirtualMachineError.InvalidValueTypes(a, b, "Sub");
    }

    public void Mul()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
            Stack.Push(Value.FromNumber(b.Number * a.Number));
        else
            throw VirtualMachineError.InvalidValueTypes(a, b, "Mul");
    }

    public void Div()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
            Stack.Push(Value.FromNumber(b.Number / a.Number));
        else
            throw VirtualMachineError.InvalidValueTypes(a, b, "Div");
    }
}
