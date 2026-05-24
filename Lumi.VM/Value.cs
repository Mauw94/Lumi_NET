using Lumi.Bytecode.Constants;
using Lumi.VM.Heap;

namespace Lumi.VM;

/// <summary>
/// Represents a VM value as a tagged union. Storing Value as a readonly struct means every
/// stack push is an inline copy into the List backing array — no heap allocation, no GC pressure.
/// Only the payload field matching Kind carries a meaningful value; all others are at their default.
/// </summary>
internal readonly struct Value
{
    public ValueKind Kind { get; }

    // Payload fields — only the one matching Kind is populated.
    public double Number { get; }
    public string? String { get; }
    public bool Bool { get; }
    public List<Value>? Array { get; }
    public Dictionary<string, Value>? Struct { get; }
    public string? StructName { get; }
    public string? NativeObjectName { get; }
    public HeapHandle? HeapHandle { get; }

    private Value(
        ValueKind kind,
        double number = 0,
        string? str = null,
        bool b = false,
        List<Value>? array = null,
        Dictionary<string, Value>? structValue = null,
        string? structName = null,
        string? nativeObjectName = null,
        HeapHandle? heapHandle = null)
    {
        Kind = kind;
        Number = number;
        String = str;
        Bool = b;
        Array = array;
        Struct = structValue;
        StructName = structName;
        NativeObjectName = nativeObjectName;
        HeapHandle = heapHandle;
    }

    public static Value FromNumber(double n) => new(ValueKind.Number, number: n);
    public static Value FromString(string s) => new(ValueKind.String, str: s);
    public static Value FromBoolean(bool b) => new(ValueKind.Boolean, b: b);
    // TODO: remove fromarray, fromstruct, fromnativeobject and just heap allocate
    // fromstring later
    public static Value FromArray(List<Value> values) => new(ValueKind.Array, array: values);
    public static Value FromStruct(string structName, Dictionary<string, Value> fields) => new(ValueKind.Struct, structValue: fields, structName: structName);
    public static Value FromNativeObject(string name) => new(ValueKind.NativeObject, nativeObjectName: name);
    public static Value Undefined() => new(ValueKind.Undefined);
    public static Value FromHeapObject(HeapHandle heapHandle) => new(ValueKind.HeapObject, heapHandle: heapHandle);

    public bool IsHeapAllocated() => HeapHandle is not null;
    public HeapHandle GetRequiredHeapHandle() => HeapHandle ?? throw VirtualMachineError.ValueNotHeapAllocated(Kind);

    public static Value ConstantToValue(Constant constant) => constant.Kind switch
    {
        ConstantKind.Number => FromNumber(constant.Number),
        ConstantKind.String => FromString(constant.String!), // TODO: we leave strings on the stack for now. Move to heap later.
        ConstantKind.Boolean => FromBoolean(constant.Boolean),
        ConstantKind.Null => new(ValueKind.Null),
        ConstantKind.Undefined => new(ValueKind.Undefined),
        _ => throw VirtualMachineError.UnkownConstantKind(constant.Kind),
    };

    public string PrintValue() => Kind switch
    {
        ValueKind.Number => Number.ToString(),
        ValueKind.String => "\"" + String + "\"" ?? string.Empty,
        ValueKind.Boolean => Bool.ToString(),
        ValueKind.Array => $"[{string.Join(", ", (Array ?? []).Select(static v => v.PrintValue()))}]",
        ValueKind.Struct => "{" + string.Join(", ", (Struct ?? []).Select(static kv => $"{kv.Key}: {kv.Value.PrintValue()}")) + "}",
        ValueKind.NativeObject => $"<{NativeObjectName ?? "native"}>",
        ValueKind.Null => "null",
        ValueKind.Undefined => "undefined",
        _ => throw VirtualMachineError.UnkownValueKind(Kind),
    };
}
