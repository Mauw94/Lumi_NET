namespace Lumi.StdLib;

/// <summary>
/// Represents the type of a value in the standard library, including primitive types (int, string, boolean), arrays, and native objects.
/// </summary>
/// <param name="Kind">The kind of the value.</param>
/// <param name="Name">The name of the native object, if applicable.</param>
public readonly record struct StdLibTypeDescriptor(StdLibValueType Kind, string? Name = null)
{
    public static StdLibTypeDescriptor Unknown() => new(StdLibValueType.Unknown);
    public static StdLibTypeDescriptor Int() => new(StdLibValueType.Int);
    public static StdLibTypeDescriptor String() => new(StdLibValueType.String);
    public static StdLibTypeDescriptor Boolean() => new(StdLibValueType.Boolean);
    public static StdLibTypeDescriptor Array() => new(StdLibValueType.Array);
    public static StdLibTypeDescriptor Undefined() => new(StdLibValueType.Undefined);
    public static StdLibTypeDescriptor NativeObject(string name) => new(StdLibValueType.NativeObject, name);
}