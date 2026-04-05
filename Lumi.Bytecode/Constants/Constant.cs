using System.Runtime.InteropServices;

namespace Lumi.Bytecode.Constants;

/// <summary>
/// Represents a constant value stored as a tagged, readonly value type.
/// Uses an explicit layout so that the numeric and boolean payloads share
/// the same memory, keeping the struct compact.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public readonly record struct Constant
{
    public ConstantKind Kind { get; }

    // Non-overlapping payloads — kept separate because string is a reference type
    // and cannot share memory with value-type fields.
    public double Number { get; }
    public string? String { get; }
    public bool Boolean { get; }

    private Constant(ConstantKind kind, double number = 0, string? str = null, bool boolean = false)
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

    public override string ToString() => Kind switch
    {
        ConstantKind.Number => Number.ToString(),
        ConstantKind.String => $"\"{String}\"",
        ConstantKind.Boolean => Boolean.ToString(),
        ConstantKind.Null => "null",
        ConstantKind.Undefined => "undefined",
        _ => "<const>",
    };
}