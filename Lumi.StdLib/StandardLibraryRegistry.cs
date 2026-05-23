namespace Lumi.StdLib;

/// <summary>
/// Provides a registry of standard library globals and methods, allowing lookup by name. 
/// This includes prelude globals (like "File") and their associated methods, as well as array methods. 
/// The registry is used by the VM to resolve method calls on native objects and arrays at runtime.
/// </summary>
public static class StandardLibraryRegistry
{
    public const string FilePreludeName = "File";

    /// <summary>
    /// Set of globals automatically available in the standard library by default (no import needed), keyed by their name. 
    /// Each global has an associated descriptor that includes its name and type information.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, PreludeGlobalDescriptor> PreludeGlobalsByName
        = new Dictionary<string, PreludeGlobalDescriptor>(StringComparer.Ordinal)
        {
            [FilePreludeName] = new(FilePreludeName, StdLibTypeDescriptor.NativeObject(FilePreludeName))
        };

    // NOTE: will this be available in the prelude or do we want an import for this?
    // NOTE: type descriptors for array methods are a bit tricky since they depend on the type of the array elements, which is not known at compile time.
    // For simplicity, we can use a placeholder type descriptor (e.g. "Unknown") for the element type in the method signatures,
    // and the VM can handle the actual types at runtime.

    /// <summary>
    /// Provides a mapping of standard array method names to their corresponding type descriptors for use in the
    /// standard library.
    /// </summary>
    /// <remarks><see cref="Dictionary{TKey, TValue}"/>Dictionary&lt;string, StdLibMethodDescriptor&gt; 
    /// TKey: Method name (e.g. "add", "remove", "length", "contains")
    /// TValue: StdLibMethodDescriptor containing parameter types and return type for the method.
    /// </remarks>
    private static readonly Dictionary<string, StdLibMethodDescriptor> ArrayMethods
        = new(StringComparer.Ordinal)
        {
            [StdLibConstants.ArrayMethods.Add] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Array()),
            [StdLibConstants.ArrayMethods.Remove] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Boolean()),
            [StdLibConstants.ArrayMethods.Length] = new([], StdLibTypeDescriptor.Int()),
            [StdLibConstants.ArrayMethods.Contains] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Boolean())
        };

    /// <summary>
    /// Provides a mapping of prelude names to their corresponding standard library method descriptors.
    /// </summary>
    /// <remarks><see cref="Dictionary{TKey, TValue}"/>
    /// TKey: Prelude name 
    /// TValue: Dictionary of method names to descriptors.
    /// Descriptors: list of parameter types and return type for each method.</remarks>
    private static readonly Dictionary<string, IReadOnlyDictionary<string, StdLibMethodDescriptor>> PreludeMethods
        = new(StringComparer.Ordinal)
        {
            [FilePreludeName] = new Dictionary<string, StdLibMethodDescriptor>(StringComparer.Ordinal)
            {
                [StdLibConstants.FilePreludeMethods.ReadText] = new([StdLibTypeDescriptor.String()], StdLibTypeDescriptor.String()),
                [StdLibConstants.FilePreludeMethods.WriteText] = new([StdLibTypeDescriptor.String(), StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Undefined()),
                [StdLibConstants.FilePreludeMethods.AppendText] = new([StdLibTypeDescriptor.String(), StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Undefined()),
                [StdLibConstants.FilePreludeMethods.ReadLines] = new([StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Array()),
                [StdLibConstants.FilePreludeMethods.WriteLines] = new([StdLibTypeDescriptor.String(), StdLibTypeDescriptor.Array()], StdLibTypeDescriptor.Undefined()),
                [StdLibConstants.FilePreludeMethods.Create] = new([StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Undefined()),
                [StdLibConstants.FilePreludeMethods.Delete] = new([StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Undefined()),
            }
        };

    public static IReadOnlyCollection<PreludeGlobalDescriptor> PreludeGlobals => [.. PreludeGlobalsByName.Values];

    public static bool TryGetPreludeGlobal(string name, out PreludeGlobalDescriptor? descriptor)
        => PreludeGlobalsByName.TryGetValue(name, out descriptor);

    public static bool TryGetArrayMethod(string methodName, out StdLibMethodDescriptor? descriptor)
        => ArrayMethods.TryGetValue(methodName, out descriptor);

    public static bool TryGetPreludeMethod(string preludeName, string methodName, out StdLibMethodDescriptor? descriptor)
    {
        descriptor = null;

        return PreludeMethods.TryGetValue(preludeName, out var methods) && methods.TryGetValue(methodName, out descriptor);
    }
}
