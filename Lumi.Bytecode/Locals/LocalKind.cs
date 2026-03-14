namespace Lumi.Bytecode.Locals;

/// <summary>
/// Specifies the kind of local variable declaration.
/// </summary>
/// <remarks>Use this enumeration to distinguish between different local variable declaration types, such as
/// 'let', 'const', and 'var'. This can be useful when analyzing or generating code that treats these declaration kinds
/// differently.</remarks>
internal enum LocalKind
{
    Let,
    Const,
    Var
}