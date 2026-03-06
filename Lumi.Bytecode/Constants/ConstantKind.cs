namespace Lumi.Bytecode.Constants;

/// <summary>
/// Kind of constants supported in the bytecode, including numbers, strings, booleans, functions, null, and undefined.
/// </summary>
public enum ConstantKind
{
    Number,
    String,
    Boolean,
    Function,
    Null,
    Undefined,
}