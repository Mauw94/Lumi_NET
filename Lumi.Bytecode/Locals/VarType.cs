namespace Lumi.Bytecode.Locals;

/// <summary>
/// Represents the declared type of a local variable.
/// </summary>
public enum VarType
{
    Unknown,
    Int,
    Float,
    Double,
    Str,
    Bool,
    Byte,
    Char,
    Long,
    Short,
}

internal static class VarTypeExtensions
{
    /// <summary>
    /// Parses a type keyword string from source into a <see cref="VarType"/>.
    /// Returns <see cref="VarType.Unknown"/> for any unrecognised name.
    /// </summary>
    public static VarType Parse(string name) => name switch
    {
        "int" => VarType.Int,
        "float" => VarType.Float,
        "double" => VarType.Double,
        "str" => VarType.Str,
        "bool" => VarType.Bool,
        "byte" => VarType.Byte,
        "char" => VarType.Char,
        "long" => VarType.Long,
        "short" => VarType.Short,
        _ => VarType.Unknown,
    };
}