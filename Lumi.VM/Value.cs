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
    public HeapHandle? HeapHandle { get; }

    private Value(
        ValueKind kind,
        double number = 0,
        string? str = null,
        bool b = false,
        HeapHandle? heapHandle = null)
    {
        Kind = kind;
        Number = number;
        String = str;
        Bool = b;
        HeapHandle = heapHandle;
    }

    public static Value FromNumber(double n) => new(ValueKind.Number, number: n);
    public static Value FromString(string s) => new(ValueKind.String, str: s); // REMOVE after full implementation to heap-allocated strings
    public static Value FromBoolean(bool b) => new(ValueKind.Boolean, b: b);
    public static Value Undefined() => new(ValueKind.Undefined);
    public static Value FromHeapObject(HeapHandle heapHandle) => new(ValueKind.HeapObject, heapHandle: heapHandle);

    /// <summary>
    /// Determines whether this Value represents a heap-allocated object. If true, the HeapHandle field contains
    /// a valid reference to the heap-allocated object.
    /// </summary>
    public bool IsHeapAllocated() => HeapHandle is not null;

    /// <summary>
    /// Returns the HeapHandle for this Value if it is heap-allocated, or throws an exception if it is not. 
    /// </summary>
    /// <returns><see cref="HeapHandle"/></returns>
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
        ValueKind.String => "\"" + String + "\"" ?? string.Empty, // TODO: will be obsolute once we move string to the heap
        ValueKind.Boolean => Bool.ToString(),
        ValueKind.Null => "null",
        ValueKind.Undefined => "undefined",
        _ => throw VirtualMachineError.UnkownValueKind(Kind),
    };
}
