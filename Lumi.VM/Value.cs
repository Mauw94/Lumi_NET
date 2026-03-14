using Lumi.Bytecode.Constants;

namespace Lumi.VM;

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
            _ => throw new InvalidOperationException($"Unknown constant kind: {constant.Kind}"),
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
            _ => throw new InvalidOperationException($"Unknown value kind: {Kind}"),
        };
    }
}

internal class NumberValue(double value) : Value(ValueKind.Number)
{
    public double Value { get; } = value;
}

public enum ValueKind
{
    Number,
    String,
    Boolean,
    Function,
    Null,
    Undefined
}