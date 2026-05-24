namespace Lumi.VM.Heap;

/// <summary>
/// Represents the heap of the virtual machine, which manages all heap-allocated objects.
/// </summary>
internal sealed class HeapManager
{
    private readonly HeapObject[] _slots = [];
    private readonly Stack<int> _freeSlots = [];
    private readonly Dictionary<string, HeapHandle> _internedStrings = new(StringComparer.Ordinal);

    public HeapManager(int capacity = 1024)
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

        heapObject.IsMarked = false;
        _slots[slot] = heapObject;

        return new HeapHandle(slot);
    }

    /// <summary>
    /// Retrieves the heap-allocated object associated with the specified handle. If the handle is invalid or the slot is unallocated, an exception is thrown.
    /// </summary>
    /// <param name="handle">The handle of the object to retrieve.</param>
    /// <returns>The heap-allocated object associated with the specified handle.</returns>
    public T Get<T>(HeapHandle handle) where T : HeapObject
    {
        var obj = Get(handle);
        return obj as T ?? throw HeapError.DanglingReference();
    }

    private HeapObject Get(HeapHandle handle) => _slots[handle.HandleId] ?? throw HeapError.DanglingReference();

    /// <summary>
    /// Returns a handle for the specified string, using string interning to ensure that identical strings share the same heap allocation. 
    /// If the string has already been interned, the existing handle is returned; otherwise, a new heap object is allocated for the string and its handle is returned. 
    /// This method ensures that memory usage is optimized by avoiding duplicate allocations for identical strings.
    /// </summary>
    /// <param name="text">The string to intern.</param>
    /// <returns>A handle representing the interned string in the heap.</returns>
    public HeapHandle InternString(string text)
    {
        if (TryGetInternedString(text, out var handle))
        {
            return handle;
        }

        handle = Allocate(new HeapStringObject(text));
        _internedStrings[text] = handle;

        return handle;
    }

    public string GetStringValue(HeapHandle handle) => Get<HeapStringObject>(handle).Value;

    /// <summary>
    /// Attempts to retrieve the handle for the specified interned string. 
    /// If the string is found and its corresponding heap object is still valid, the method returns true and outputs the handle; 
    /// otherwise, it returns false and outputs a default handle. 
    /// </summary>
    /// <param name="text">The string to look up in the interned strings table.</param>
    /// <param name="handle">When this method returns, contains the handle associated with the interned string if found; otherwise, the default handle.</param>
    /// <returns>true if the interned string is found and valid; otherwise, false.</returns>
    public bool TryGetInternedString(string text, out HeapHandle handle)
    {
        if (_internedStrings.TryGetValue(text, out handle))
        {
            var slotObject = _slots[handle.HandleId];
            if (slotObject is HeapStringObject stringObject && stringObject.Value == text)
            {
                return true;
            }

            _internedStrings.Remove(text);
        }

        handle = default;
        return false;
    }

    /// <summary>
    /// Attempts to ensure that the specified number of free slots are available by performing garbage collection if
    /// necessary.  
    /// </summary>
    /// <remarks>If the required number of free slots cannot be made available after garbage collection, an
    /// out-of-memory error is thrown.</remarks>
    /// <param name="roots">A collection of root values that are used to determine which objects are reachable during garbage collection.</param>
    /// <param name="requestSlots">The minimum number of free slots required after garbage collection. Must be greater than zero. The default is 1.</param>
    public void MaybeCollect(IEnumerable<Value> roots, int requestSlots = 1)
    {
        if (_freeSlots.Count >= requestSlots)
        {
            return;
        }

        CollectGarbage(roots);

        if (_freeSlots.Count < requestSlots)
        {
            throw HeapError.OutOfMemory();
        }
    }

    /// <summary>
    /// Performs a garbage collection cycle using the specified root values as entry points.
    /// </summary>
    /// <remarks>Use this method to manually trigger garbage collection when managing memory for custom value
    /// objects. Only objects reachable from the provided roots will be preserved; all others are eligible for
    /// collection.</remarks>
    /// <param name="roots">A collection of root values from which object reachability is determined. Cannot be null.</param>
    public void CollectGarbage(IEnumerable<Value> roots)
    {
        MarkRoots(roots);
        Sweep();
    }

    public int FreeCount => _freeSlots.Count;

    public string FormatValue(Value value)
    {
        if (value.IsHeapAllocated()) return Get(value.GetRequiredHeapHandle()).PrintValue();

        return value.PrintValue();
    }

    void MarkRoots(IEnumerable<Value> roots)
    {
        foreach (var root in roots)
        {
            MarkValue(root);
        }
    }

    void MarkValue(Value value)
    {
        if (value.IsHeapAllocated())
        {
            MarkObject(value.GetRequiredHeapHandle());
        }
    }

    void MarkObject(HeapHandle handle)
    {
        var obj = _slots[handle.HandleId];
        if (obj is null || obj.IsMarked) return;

        obj.IsMarked = true;
        obj.VisitReferences(MarkValue);
    }

    void Sweep()
    {
        for (var i = 0; i < _slots.Length; i++)
        {
            var obj = _slots[i];
            if (obj is null) continue;

            if (obj.IsMarked)
            {
                obj.IsMarked = false;
                continue;
            }

            _slots[i] = null!;
            _freeSlots.Push(i);
        }

        RemoveDeadInternedStrings();
    }

    void RemoveDeadInternedStrings()
    {
        if (_internedStrings.Count == 0)
        {
            return;
        }

        List<string>? staleKeys = null;

        foreach (var entry in _internedStrings)
        {
            var slotObject = _slots[entry.Value.HandleId];
            if (slotObject is not HeapStringObject stringObject || stringObject.Value != entry.Key)
            {
                staleKeys ??= [];
                staleKeys.Add(entry.Key);
            }
        }

        if (staleKeys is null)
        {
            return;
        }

        foreach (var key in staleKeys)
        {
            _internedStrings.Remove(key);
        }
    }
}