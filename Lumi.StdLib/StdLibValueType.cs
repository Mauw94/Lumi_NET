namespace Lumi.StdLib;

/// <summary>
/// Defines the various types of values that can exist in the standard library, 
/// including primitive types (int, string, boolean), arrays, native objects, and an unknown type for error handling.
/// </summary>
public enum StdLibValueType
{
    Unknown,
    Int,
    String,
    Boolean,
    Array,
    NativeObject,
    Undefined,
}