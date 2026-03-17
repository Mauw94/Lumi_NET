namespace Lumi.Bytecode.Constants;

/// <summary>
/// Represents a constant value that can be of various types, including number, string, boolean, function, null, or
/// undefined.
/// </summary>
public sealed class Constant
{
    public ConstantKind Kind { get; }
    public double? Number { get; }
    public string? String { get; }
    public bool? Boolean { get; }

    private Constant(ConstantKind kind, double? number = null, string? str = null, bool? boolean = null)
    {
        Kind = kind;
        Number = number;
        String = str;
        Boolean = boolean;
    }

    public static Constant FromNumber(double n) => new(ConstantKind.Number, number: n);
    public static Constant FromString(string s) => new(ConstantKind.String, str: s);
    public static Constant FromBoolean(bool b) => new(ConstantKind.Boolean, boolean: b);
    public static Constant Null() => new(ConstantKind.Null);
    public static Constant Undefined() => new(ConstantKind.Undefined);

    public override string ToString()
    {
        return Kind switch
        {
            ConstantKind.Number => Number?.ToString() ?? "0",
            ConstantKind.String => $"\"{String}\"",
            ConstantKind.Boolean => Boolean?.ToString() ?? "false",
            ConstantKind.Null => "null",
            ConstantKind.Undefined => "undefined",
            _ => "<const>",
        };
    }
}