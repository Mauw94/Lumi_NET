namespace Lumi.VM.Heap;

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

    /// <summary>
    /// Retrieves the heap-allocated object associated with the specified handle. If the handle is invalid or the slot is unallocated, an exception is thrown.
    /// </summary>
    /// <param name="handle">The handle of the object to retrieve.</param>
    /// <returns>The heap-allocated object associated with the specified handle.</returns>
    public HeapObject Get(HeapHandle handle) => _slots[handle.HandleId] ?? throw HeapError.DanglingReference();

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

    // TODO: move to garbage collector class
    public void CollectGarbage()
    {
        foreach (var obj in _slots)
        {
        }
    }
}