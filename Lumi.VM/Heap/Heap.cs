namespace Lumi.VM.Heap;

abstract class HeapObject()
{
    public bool IsMarked { get; set; }
    public bool IsAllocated { get; set; } = false;
    public ValueKind Kind;
    public int SizeEstimate { get; set; }
    protected abstract void VisitReferences(Action<int> visitHandle, Action<Value> visitValue);
}

internal sealed class HeapArrayObject : HeapObject
{
    public HeapArrayObject(List<Value> elements)
    {
        Elements = elements;
        Kind = ValueKind.Array;
    }

    public List<Value> Elements { get; }

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var element in Elements)
        {
            visitValue(element);
        }
    }
}

internal sealed class HeapStructObject : HeapObject
{
    public HeapStructObject(string structName, Dictionary<string, Value> fields)
    {
        StructName = structName;
        Fields = fields;
        Kind = ValueKind.Struct;
    }

    public string StructName { get; }
    public Dictionary<string, Value> Fields { get; }

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var field in Fields.Values)
        {
            visitValue(field);
        }
    }
}

internal sealed class HeapNativeObject(string nativeObjectName, Dictionary<string, Value> fields) : HeapObject
{
    public string NativeObjectName { get; } = nativeObjectName;
    public Dictionary<string, Value> Fields { get; } = fields;

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var field in Fields.Values)
        {
            visitValue(field);
        }
    }
}

internal sealed class HeapStringObject(string value) : HeapObject
{
    public string Value { get; } = value;
    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        // No references to visit for a string object.
    }
}

/// <summary>
/// Represents the heap of the virtual machine, which manages all heap-allocated objects.
/// </summary>
internal sealed class Heap
{
    private readonly HeapObject[] _slots = [];
    private readonly Stack<int> _freeSlots = [];
    private int _nextSlot = 0;

    public Heap(int capacity = 1024)
    {
        _slots = new HeapObject[capacity];
        _freeSlots = new Stack<int>(capacity);

        for (int i = capacity - 1; i >= 0; i--)
        {
            _freeSlots.Push(i);
        }
    }

    /// <summary>
    /// Allocates a handle for the specified object in the heap.
    /// </summary>
    /// <remarks>If the heap has no available slots, an OutOfMemoryException is thrown. The returned handle
    /// can be used to reference the allocated object within the heap.</remarks>
    /// <param name="heapObject">The object to allocate in the heap. Cannot be null.</param>
    /// <returns>A handle representing the allocated object in the heap.</returns>
    public HeapHandle Allocate(HeapObject heapObject)
    {
        if (!_freeSlots.TryPop(out var slot))
        {
            throw HeapError.OutOfMemory();
        }

        _slots[slot] = heapObject;

        return new HeapHandle(slot);
    }

    /// <summary>
    /// Deallocates the object associated with the specified handle from the heap, making its slot available for future allocations.
    /// </summary>
    /// <param name="handle">The handle of the object to deallocate.</param>
    public void Deallocate(HeapHandle handle)
    {
        if (_slots[handle.HandleId].IsAllocated)
        {
            _slots[handle.HandleId] = null!;
            _freeSlots.Push(handle.HandleId);
        }
    }

    public HeapObject Get(HeapHandle handle)
        => _slots[handle.HandleId] ?? throw HeapError.DanglingReference();

    public HeapArrayObject GetArray(HeapHandle handle)
        => _slots[handle.HandleId] as HeapArrayObject ?? throw HeapError.DanglingReference();

    public HeapObject GetStruct(HeapHandle handle)
        => _slots[handle.HandleId] as HeapStructObject ?? throw HeapError.DanglingReference();

    HeapObject GetString(HeapHandle handle)
    {
        throw new NotImplementedException();
    }

    public void MaybeCollect(int requestedCapacity)
    {
        if (_slots.Length >= requestedCapacity)
        {
            CollectGarbage();
        }
    }

    int AllocateString(string text)
    {
        throw new NotImplementedException();
    }

    bool TryGetInternedString(string text, out HeapHandle handle)
    {
        throw new NotImplementedException();
    }

    void MarkRoots(IEnumerable<Value> roots)
    {
        throw new NotImplementedException();
    }

    void MarkValue(Value value)
    {
        if (value.IsHeapAllocated())
        {
            MarkReachable(value.GetRequiredHeapHandle());
        }
    }

    void MarkObject(HeapHandle handle)
    {
        throw new NotImplementedException();
    }

    void Sweep()
    {
        throw new NotImplementedException();

    }

    public void MarkReachable(HeapHandle handle)
    {

    }

    public bool IsReachable(HeapHandle handle)
    {
        return false;
    }

    public void CollectGarbage()
    {
        foreach (var obj in _slots)
        {
        }
    }
}