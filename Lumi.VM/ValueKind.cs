namespace Lumi.VM;

/// <summary>
/// All the different ValueKinds.
/// </summary>
internal enum ValueKind
{
    Number,
    String,
    Boolean,
    Array,
    Struct,
    NativeObject,
    Function,
    Null,
    Undefined
}