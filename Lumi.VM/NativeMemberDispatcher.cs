using Lumi.StdLib;
using Lumi.VM.Heap;

namespace Lumi.VM;

/// <summary>
/// Provides functionality to dispatch method calls on native members, such as arrays and prelude objects.
/// </summary>
internal static class NativeMemberDispatcher
{
    public static Value Invoke(Heap.Heap heap, HeapObject target, string methodName, IReadOnlyList<Value> args)
    {
        return target.Kind switch
        {
            ValueKind.Array => InvokeArrayMethod((HeapArrayObject)target, methodName, args),
            ValueKind.NativeObject => InvokePreludeMethod(heap, (HeapNativeObject)target, methodName, args),
            _ => throw VirtualMachineError.MethodTargetNotSupported(methodName, target.Kind)
        };
    }

    private static Value InvokeArrayMethod(HeapArrayObject target, string methodName, IReadOnlyList<Value> args)
    {
        if (target.Elements is null)
            throw VirtualMachineError.ListMethodTargetNotArray(target.Kind);

        if (!StandardLibraryRegistry.TryGetArrayMethod(methodName, out var descriptor))
            throw VirtualMachineError.UnknownListMethod(methodName);

        ValidateArgumentCount(methodName, descriptor!.ParameterTypes.Count, args.Count);

        return methodName switch
        {
            StdLibConstants.ArrayMethods.Add => AddArrayItem(target, args[0]),
            StdLibConstants.ArrayMethods.Remove => RemoveArrayItem(target, args[0]),
            StdLibConstants.ArrayMethods.Length => Value.FromNumber(target.Elements.Count),
            StdLibConstants.ArrayMethods.Contains => Value.FromBoolean(target.Elements.Contains(args[0])),
            _ => throw VirtualMachineError.UnknownListMethod(methodName)
        };
    }

    private static Value InvokePreludeMethod(Heap.Heap heap, HeapNativeObject target, string methodName, IReadOnlyList<Value> args)
    {
        var preludeName = target.NativeObjectName ?? throw VirtualMachineError.UnknownPreludeGlobal("<unknown>");

        if (!StandardLibraryRegistry.TryGetPreludeMethod(preludeName, methodName, out var descriptor))
            throw VirtualMachineError.UnknownPreludeMethod(preludeName, methodName);

        ValidateArgumentCount(methodName, descriptor!.ParameterTypes.Count, args.Count);

        return preludeName switch
        {
            StandardLibraryRegistry.FilePreludeName => InvokeFilePreludeMethod(heap, methodName, args),
            _ => throw VirtualMachineError.UnknownPreludeGlobal(preludeName)
        };
    }

    private static Value InvokeFilePreludeMethod(Heap.Heap heap, string methodName, IReadOnlyList<Value> args)
    {
        try
        {
            return methodName switch
            {
                StdLibConstants.FilePreludeMethods.ReadText => Value.FromString(ReadAllText(methodName, args)),
                StdLibConstants.FilePreludeMethods.WriteText => WriteText(args),
                StdLibConstants.FilePreludeMethods.AppendText => AppendText(args),
                StdLibConstants.FilePreludeMethods.ReadLines => ReadLines(heap, args),
                StdLibConstants.FilePreludeMethods.WriteLines => WriteLines(heap, args),
                StdLibConstants.FilePreludeMethods.Delete => Delete(args),
                StdLibConstants.FilePreludeMethods.Create => Create(args),
                _ => throw VirtualMachineError.UnknownPreludeMethod(StandardLibraryRegistry.FilePreludeName, methodName)
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            throw VirtualMachineError.PreludeMethodIoFailure(StandardLibraryRegistry.FilePreludeName, methodName, ex);
        }
    }

    private static string ReadAllText(string methodName, IReadOnlyList<Value> args)
        => File.ReadAllText(GetRequiredStringArgument(methodName, 0, args[0]));

    private static Value Delete(IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.Delete, 0, args[0]);
        File.Delete(path);

        return Value.Undefined();
    }

    private static Value Create(IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.Create, 0, args[0]);
        using var fileStream = File.Create(path);

        return Value.Undefined();
    }

    private static Value AppendText(IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.AppendText, 0, args[0]);
        var contents = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.AppendText, 1, args[1]);
        File.AppendAllText(path, contents);

        return Value.Undefined();
    }

    private static Value WriteLines(Heap.Heap heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteLines, 0, args[0]);
        var linesArray = GetRequiredArrayArgument(StdLibConstants.FilePreludeMethods.WriteLines, 1, args[1], heap);

        var lines = new string[linesArray.Elements.Count];
        for (var i = 0; i < linesArray.Elements.Count; i++)
        {
            var element = linesArray.Elements[i];
            if (element.Kind != ValueKind.String || element.String is null)
                throw VirtualMachineError.MethodArgumentTypeMismatch(StdLibConstants.FilePreludeMethods.WriteLines, 1, ValueKind.String, element.Kind);

            lines[i] = element.String;
        }

        File.WriteAllLines(path, lines);

        return Value.Undefined();
    }

    private static Value AddArrayItem(HeapArrayObject target, Value item)
    {
        target.Elements!.Add(item);

        return Value.Undefined();
    }

    private static Value RemoveArrayItem(HeapArrayObject target, Value item)
    {
        var removed = target.Elements!.Remove(item);

        return Value.FromBoolean(removed);
    }

    private static Value WriteText(IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteText, 0, args[0]);
        var contents = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteText, 1, args[1]);
        File.WriteAllText(path, contents);

        return Value.Undefined();
    }

    private static string GetRequiredStringArgument(string methodName, int parameterIndex, Value value)
    {
        if (value.Kind != ValueKind.String || value.String is null)
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.String, value.Kind);

        return value.String;
    }

    private static HeapArrayObject GetRequiredArrayArgument(string methodName, int parameterIndex, Value value, Heap.Heap heap)
    {
        if (!value.IsHeapAllocated())
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.Array, value.Kind);

        var heapObject = heap.Get<HeapArrayObject>(value.GetRequiredHeapHandle());
        if (heapObject is not HeapArrayObject arrayObject)
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.Array, heapObject.Kind);

        return arrayObject;
    }

    private static void ValidateArgumentCount(string methodName, int expected, int actual)
    {
        if (expected != actual)
            throw VirtualMachineError.MethodArgumentCountMismatch(methodName, expected, actual);
    }

    private static Value ReadLines(Heap.Heap heap, IReadOnlyList<Value> args)
    {
        var lines = File.ReadAllLines(GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.ReadLines, 0, args[0]));
        var values = new List<Value>(lines.Length);

        foreach (var line in lines)
        {
            values.Add(Value.FromString(line));
        }

        var heapArray = new HeapArrayObject(values);

        return Value.FromHeapObject(heap.Allocate(heapArray));
    }
}
