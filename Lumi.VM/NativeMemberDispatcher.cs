using Lumi.StdLib;

namespace Lumi.VM;

/// <summary>
/// Provides functionality to dispatch method calls on native members, such as arrays and prelude objects.
/// </summary>
internal static class NativeMemberDispatcher
{
    public static Value Invoke(Value target, string methodName, IReadOnlyList<Value> args)
    {
        return target.Kind switch
        {
            ValueKind.Array => InvokeArrayMethod(target, methodName, args),
            ValueKind.NativeObject => InvokePreludeMethod(target, methodName, args),
            _ => throw VirtualMachineError.MethodTargetNotSupported(methodName, target.Kind)
        };
    }

    private static Value InvokeArrayMethod(Value target, string methodName, IReadOnlyList<Value> args)
    {
        if (target.Array is null)
            throw VirtualMachineError.ListMethodTargetNotArray(target.Kind);

        if (!StandardLibraryRegistry.TryGetArrayMethod(methodName, out var descriptor))
            throw VirtualMachineError.UnknownListMethod(methodName);

        ValidateArgumentCount(methodName, descriptor!.ParameterTypes.Count, args.Count);

        return methodName switch
        {
            "add" => AddArrayItem(target, args[0]),
            "remove" => RemoveArrayItem(target, args[0]),
            "length" => Value.FromNumber(target.Array.Count),
            "contains" => Value.FromBoolean(target.Array.Contains(args[0])),
            _ => throw VirtualMachineError.UnknownListMethod(methodName)
        };
    }

    private static Value InvokePreludeMethod(Value target, string methodName, IReadOnlyList<Value> args)
    {
        var preludeName = target.NativeObjectName ?? throw VirtualMachineError.UnknownPreludeGlobal("<unknown>");

        if (!StandardLibraryRegistry.TryGetPreludeMethod(preludeName, methodName, out var descriptor))
            throw VirtualMachineError.UnknownPreludeMethod(preludeName, methodName);

        ValidateArgumentCount(methodName, descriptor!.ParameterTypes.Count, args.Count);

        return preludeName switch
        {
            StandardLibraryRegistry.FilePreludeName => InvokeFilePreludeMethod(methodName, args),
            _ => throw VirtualMachineError.UnknownPreludeGlobal(preludeName)
        };
    }

    private static Value InvokeFilePreludeMethod(string methodName, IReadOnlyList<Value> args)
    {
        return methodName switch
        {
            "readText" => Value.FromString(File.ReadAllText(GetRequiredStringArgument(methodName, 0, args[0]))),
            "writeText" => WriteText(args),
            _ => throw VirtualMachineError.UnknownPreludeMethod(StandardLibraryRegistry.FilePreludeName, methodName)
        };
    }

    private static Value AddArrayItem(Value target, Value item)
    {
        target.Array!.Add(item);
        return target;
    }

    private static Value RemoveArrayItem(Value target, Value item)
    {
        var removed = target.Array!.Remove(item);
        return Value.FromBoolean(removed);
    }

    private static Value WriteText(IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument("writeText", 0, args[0]);
        var contents = GetRequiredStringArgument("writeText", 1, args[1]);
        File.WriteAllText(path, contents);

        return Value.Undefined();
    }

    private static string GetRequiredStringArgument(string methodName, int parameterIndex, Value value)
    {
        if (value.Kind != ValueKind.String || value.String is null)
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.String, value.Kind);

        return value.String;
    }

    private static void ValidateArgumentCount(string methodName, int expected, int actual)
    {
        if (expected != actual)
            throw VirtualMachineError.MethodArgumentCountMismatch(methodName, expected, actual);
    }
}