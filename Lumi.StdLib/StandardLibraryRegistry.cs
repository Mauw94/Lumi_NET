namespace Lumi.StdLib;

/// <summary>
/// Provides a registry of standard library globals and methods, allowing lookup by name. 
/// This includes prelude globals (like "File") and their associated methods, as well as array methods. 
/// The registry is used by the VM to resolve method calls on native objects and arrays at runtime.
/// </summary>
public static class StandardLibraryRegistry
{
    public const string FilePreludeName = "File";

    private static readonly IReadOnlyDictionary<string, PreludeGlobalDescriptor> PreludeGlobalsByName
        = new Dictionary<string, PreludeGlobalDescriptor>(StringComparer.Ordinal)
        {
            [FilePreludeName] = new(FilePreludeName, StdLibTypeDescriptor.NativeObject(FilePreludeName))
        };

    private static readonly Dictionary<string, StdLibMethodDescriptor> ArrayMethods
        = new(StringComparer.Ordinal)
        {
            ["add"] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Array()),
            ["remove"] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Boolean()),
            ["length"] = new([], StdLibTypeDescriptor.Int()),
            ["contains"] = new([StdLibTypeDescriptor.Unknown()], StdLibTypeDescriptor.Boolean())
        };

    private static readonly Dictionary<string, IReadOnlyDictionary<string, StdLibMethodDescriptor>> PreludeMethods
        = new(StringComparer.Ordinal)
        {
            [FilePreludeName] = new Dictionary<string, StdLibMethodDescriptor>(StringComparer.Ordinal)
            {
                ["readText"] = new([StdLibTypeDescriptor.String()], StdLibTypeDescriptor.String()),
                ["writeText"] = new([StdLibTypeDescriptor.String(), StdLibTypeDescriptor.String()], StdLibTypeDescriptor.Undefined())
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
