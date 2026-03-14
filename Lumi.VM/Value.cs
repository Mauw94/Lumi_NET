using Lumi.Bytecode.Constants;

namespace Lumi.VM;

/// <summary>
/// Represents a value of a specific kind within the system. Serves as the abstract base class for all value types, such
/// as numbers, strings, and functions.
/// </summary>
/// <param name="kind">The kind of value represented by this instance. Determines the specific type and behavior of the value.</param>
public abstract class Value(ValueKind kind)
{
    public ValueKind Kind { get; } = kind;

    public static Value ConstantToValue(Constant constant)
    {
        return constant.Kind switch
        {
            ConstantKind.Number => new NumberValue(constant.Number!.Value),
            //case ConstantKind.String:
            //    return new StringValue(constant.String!);
            //case ConstantKind.Boolean:
            //    return new BooleanValue(constant.Boolean!.Value);
            //case ConstantKind.Function:
            //    return new FunctionValue(constant.Function!);
            //case ConstantKind.Null:
            //    return NullValue.Instance;
            //case ConstantKind.Undefined:
            //    return UndefinedValue.Instance;
            _ => throw VirtualMachineError.UnkownConstantKind(constant.Kind),
        };
    }

    public string PrintValue()
    {
        return Kind switch
        {
            ValueKind.Number => ((NumberValue)this).Value.ToString(),
            //case ValueKind.String:
            //    return ((StringValue)this).Value;
            //case ValueKind.Boolean:
            //    return ((BooleanValue)this).Value.ToString();
            //case ValueKind.Function:
            //    return $"[Function: {((FunctionValue)this).Name}]";
            //case ValueKind.Null:
            //    return "null";
            //case ValueKind.Undefined:
            //    return "undefined";
            _ => throw VirtualMachineError.UnkownValueKind(ValueKind.Number),
        };
    }
}

internal sealed class NumberValue(double value) : Value(ValueKind.Number)
{
    public double Value { get; } = value;
}