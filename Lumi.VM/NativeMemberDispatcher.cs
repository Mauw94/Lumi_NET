using Lumi.StdLib;
using Lumi.VM.Heap;

namespace Lumi.VM;

/// <summary>
/// Provides functionality to dispatch method calls on native members, such as arrays and prelude objects.
/// </summary>
internal static class NativeMemberDispatcher
{
    public static Value Invoke(HeapManager heap, Value receiverValue, string methodName, IReadOnlyList<Value> args)
    {
        var target = heap.Get<HeapObject>(receiverValue.GetRequiredHeapHandle());
        return target.Kind switch
        {
            ValueKind.Array => InvokeArrayMethod((HeapArrayObject)target, receiverValue, methodName, args),
            ValueKind.NativeObject => InvokePreludeMethod(heap, (HeapNativeObject)target, methodName, args),
            _ => throw VirtualMachineError.MethodTargetNotSupported(methodName, target.Kind)
        };
    }

    private static Value InvokeArrayMethod(HeapArrayObject target, Value receiverValue, string methodName, IReadOnlyList<Value> args)
    {
        if (target.Elements is null)
            throw VirtualMachineError.ListMethodTargetNotArray(target.Kind);

        if (!StandardLibraryRegistry.TryGetArrayMethod(methodName, out var descriptor))
            throw VirtualMachineError.UnknownListMethod(methodName);

        ValidateArgumentCount(methodName, descriptor!.ParameterTypes.Count, args.Count);

        return methodName switch
        {
            StdLibConstants.ArrayMethods.Add => AddArrayItem(target, args[0], receiverValue),
            StdLibConstants.ArrayMethods.Remove => RemoveArrayItem(target, args[0]),
            StdLibConstants.ArrayMethods.Length => Value.FromNumber(target.Elements.Count),
            StdLibConstants.ArrayMethods.Contains => Value.FromBoolean(target.Elements.Contains(args[0])),
            _ => throw VirtualMachineError.UnknownListMethod(methodName)
        };
    }

    private static Value InvokePreludeMethod(HeapManager heap, HeapNativeObject target, string methodName, IReadOnlyList<Value> args)
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

    private static Value InvokeFilePreludeMethod(HeapManager heap, string methodName, IReadOnlyList<Value> args)
    {
        try
        {
            return methodName switch
            {
                StdLibConstants.FilePreludeMethods.ReadText => ReadText(heap, args),
                StdLibConstants.FilePreludeMethods.WriteText => WriteText(heap, args),
                StdLibConstants.FilePreludeMethods.AppendText => AppendText(heap, args),
                StdLibConstants.FilePreludeMethods.ReadLines => ReadLines(heap, args),
                StdLibConstants.FilePreludeMethods.WriteLines => WriteLines(heap, args),
                StdLibConstants.FilePreludeMethods.Delete => Delete(heap, args),
                StdLibConstants.FilePreludeMethods.Create => Create(heap, args),
                _ => throw VirtualMachineError.UnknownPreludeMethod(StandardLibraryRegistry.FilePreludeName, methodName)
            };
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException or ArgumentException)
        {
            throw VirtualMachineError.PreludeMethodIoFailure(StandardLibraryRegistry.FilePreludeName, methodName, ex);
        }
    }

    private static string ReadAllText(HeapManager heap, string methodName, IReadOnlyList<Value> args)
        => File.ReadAllText(GetRequiredStringArgument(methodName, 0, args[0], heap));

    private static Value ReadText(HeapManager heap, IReadOnlyList<Value> args)
        => Value.FromHeapObject(heap.InternString(ReadAllText(heap, StdLibConstants.FilePreludeMethods.ReadText, args)));

    private static Value Delete(HeapManager heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.Delete, 0, args[0], heap);
        File.Delete(path);

        return Value.Undefined();
    }

    private static Value Create(HeapManager heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.Create, 0, args[0], heap);
        using var fileStream = File.Create(path);

        return Value.Undefined();
    }

    private static Value AppendText(HeapManager heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.AppendText, 0, args[0], heap);
        var contents = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.AppendText, 1, args[1], heap);
        File.AppendAllText(path, contents);

        return Value.Undefined();
    }

    private static Value WriteLines(HeapManager heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteLines, 0, args[0], heap);
        var linesArray = GetRequiredArrayArgument(StdLibConstants.FilePreludeMethods.WriteLines, 1, args[1], heap);

        var lines = new string[linesArray.Elements.Count];
        for (var i = 0; i < linesArray.Elements.Count; i++)
        {
            var element = linesArray.Elements[i];
            lines[i] = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteLines, 1, element, heap);
        }

        File.WriteAllLines(path, lines);

        return Value.Undefined();
    }

    private static Value AddArrayItem(HeapArrayObject target, Value item, Value receiverValue)
    {
        target.Elements!.Add(item);

        return receiverValue;
    }

    private static Value RemoveArrayItem(HeapArrayObject target, Value item)
    {
        var removed = target.Elements!.Remove(item);

        return Value.FromBoolean(removed);
    }

    private static Value WriteText(HeapManager heap, IReadOnlyList<Value> args)
    {
        var path = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteText, 0, args[0], heap);
        var contents = GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.WriteText, 1, args[1], heap);
        File.WriteAllText(path, contents);

        return Value.Undefined();
    }

    private static string GetRequiredStringArgument(string methodName, int parameterIndex, Value value, HeapManager? heap)
    {
        if (heap is not null && value.IsHeapAllocated())
        {
            return heap.GetStringValue(value.GetRequiredHeapHandle());
        }

        if (value.Kind != ValueKind.String || value.String is null)
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.String, value.Kind);

        return value.String;
    }

    private static HeapArrayObject GetRequiredArrayArgument(string methodName, int parameterIndex, Value value, HeapManager heap)
    {
        if (!value.IsHeapAllocated())
            throw VirtualMachineError.MethodArgumentTypeMismatch(methodName, parameterIndex, ValueKind.Array, value.Kind);

        return heap.Get<HeapArrayObject>(value.GetRequiredHeapHandle());
    }

    private static void ValidateArgumentCount(string methodName, int expected, int actual)
    {
        if (expected != actual)
            throw VirtualMachineError.MethodArgumentCountMismatch(methodName, expected, actual);
    }

    private static Value ReadLines(HeapManager heap, IReadOnlyList<Value> args)
    {
        var lines = File.ReadAllLines(GetRequiredStringArgument(StdLibConstants.FilePreludeMethods.ReadLines, 0, args[0], heap));
        var values = new List<Value>(lines.Length);

        foreach (var line in lines)
        {
            var handle = heap.InternString(line);
            values.Add(Value.FromHeapObject(handle));
        }

        return Value.FromHeapObject(heap.Allocate(new HeapArrayObject(values)));
    }
}