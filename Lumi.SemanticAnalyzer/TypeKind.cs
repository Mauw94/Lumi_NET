namespace Lumi.SemanticAnalyzer;


/// <summary>
/// Represents the inferred or declared type of a symbol in the semantic analysis pass.
/// </summary>
public enum TypeKind
{
    Unknown,
    Int,
    String,
    Boolean,
    Null,
    Undefined,
    Function,
    Array,
    Struct,
    NativeObject,
}