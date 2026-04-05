using Lumi.Bytecode.Constants;

namespace Lumi.VM;

/// <summary>
/// Represents a VM value as a tagged union. Storing Value as a readonly struct means every
/// stack push is an inline copy into the List backing array — no heap allocation, no GC pressure.
/// Only the payload field matching Kind carries a meaningful value; all others are at their default.
/// </summary>
internal readonly struct Value
{
    public ValueKind Kind { get; }

    // Payload fields — only the one matching Kind is populated.
    public double Number { get; }
    public string? String { get; }
    public bool Bool { get; }

    private Value(ValueKind kind, double number = 0, string? str = null, bool b = false)
    {
        Kind = kind;
        Number = number;
        String = str;
        Bool = b;
    }

    public static Value FromNumber(double n) => new(ValueKind.Number, number: n);
    public static Value FromString(string s) => new(ValueKind.String, str: s);
    public static Value FromBoolean(bool b) => new(ValueKind.Boolean, b: b);

    public static Value ConstantToValue(Constant constant) => constant.Kind switch
    {
        ConstantKind.Number => FromNumber(constant.Number),
        ConstantKind.String => FromString(constant.String!),
        ConstantKind.Boolean => FromBoolean(constant.Boolean),
        _ => throw VirtualMachineError.UnkownConstantKind(constant.Kind),
    };

    public string PrintValue() => Kind switch
    {
        ValueKind.Number => Number.ToString(),
        ValueKind.String => String ?? string.Empty,
        ValueKind.Boolean => Bool.ToString(),
        _ => throw VirtualMachineError.UnkownValueKind(Kind),
    };
}