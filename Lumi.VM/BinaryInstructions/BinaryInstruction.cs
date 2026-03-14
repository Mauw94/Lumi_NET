namespace Lumi.VM.BinaryInstructions;

internal sealed class BinaryInstruction(Stack stack) : VirtualMachineInstruction(stack)
{
    public void Add()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
        {
            var result = ((NumberValue)a).Value + ((NumberValue)b).Value;
            Stack.Push(new NumberValue(result));
        }
        else
        {
            throw VirtualMachineError.InvalidValueTypes(a, b, "Add");
        }
    }

    public void Sub()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
        {
            var result = ((NumberValue)a).Value + ((NumberValue)b).Value;
            Stack.Push(new NumberValue(result));
        }
        else
        {
            throw VirtualMachineError.InvalidValueTypes(a, b, "Sub");
        }
    }
    public void Mul()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
        {
            var result = ((NumberValue)a).Value * ((NumberValue)b).Value;
            Stack.Push(new NumberValue(result));
        }
        else
        {
            throw VirtualMachineError.InvalidValueTypes(a, b, "Mul");
        }
    }

    public void Div()
    {
        var a = Stack.Pop();
        var b = Stack.Pop();

        if (a.Kind == ValueKind.Number && b.Kind == ValueKind.Number)
        {
            var result = ((NumberValue)a).Value / ((NumberValue)b).Value;
            Stack.Push(new NumberValue(result));
        }
        else
        {
            throw VirtualMachineError.InvalidValueTypes(a, b, "Div");
        }
    }
}
