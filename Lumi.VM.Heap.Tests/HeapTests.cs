using VmHeap = Lumi.VM.Heap.HeapManager;

namespace Lumi.VM.Heap.Tests;

[TestClass]
public sealed class HeapTests
{
    [TestMethod]
    public void InternString_SameText_ReturnsSameHandle()
    {
        var heap = new VmHeap();

        var first = heap.InternString("hello");
        var second = heap.InternString("hello");

        Assert.AreEqual(first, second);
    }

    [TestMethod]
    public void GetStringValue_InternedHandle_ReturnsStoredText()
    {
        var heap = new VmHeap();
        var handle = heap.InternString("hello");

        var value = heap.GetStringValue(handle);

        Assert.AreEqual("hello", value);
    }

    [TestMethod]
    public void Allocate_ArrayObject_RoundTrips()
    {
        var heap = new VmHeap();
        var handle = heap.Allocate(new HeapArrayObject([Value.FromNumber(1), Value.FromNumber(2)]));

        var array = heap.Get<HeapArrayObject>(handle);

        Assert.AreEqual(ValueKind.Array, array.Kind);
        Assert.HasCount(2, array.Elements);
        Assert.AreEqual(1d, array.Elements[0].Number);
        Assert.AreEqual(2d, array.Elements[1].Number);
    }

    [TestMethod]
    public void Allocate_StructObject_RoundTrips()
    {
        var heap = new VmHeap();
        var handle = heap.Allocate(new HeapStructObject("Point", new Dictionary<string, Value>(StringComparer.Ordinal)
        {
            ["x"] = Value.FromNumber(1),
            ["y"] = Value.FromNumber(2)
        }));

        var obj = heap.Get<HeapStructObject>(handle);

        Assert.AreEqual(ValueKind.Struct, obj.Kind);
        Assert.AreEqual("Point", obj.StructName);
        Assert.AreEqual(1d, obj.Fields["x"].Number);
        Assert.AreEqual(2d, obj.Fields["y"].Number);
    }

    [TestMethod]
    public void Allocate_NativeObject_RoundTrips()
    {
        var heap = new VmHeap();
        var handle = heap.Allocate(new HeapNativeObject("File", new Dictionary<string, Value>(StringComparer.Ordinal)));

        var obj = heap.Get<HeapNativeObject>(handle);

        Assert.AreEqual(ValueKind.NativeObject, obj.Kind);
        Assert.AreEqual("File", obj.NativeObjectName);
        Assert.HasCount(0, obj.Fields);
    }

    [TestMethod]
    public void FormatValue_HeapArrayValue_PrintsArrayContents()
    {
        var heap = new VmHeap();
        var handle = heap.Allocate(new HeapArrayObject([Value.FromNumber(1), Value.FromNumber(2)]));

        var text = heap.FormatValue(Value.FromHeapObject(handle));

        Assert.AreEqual("[1, 2]", text);
    }

    [TestMethod]
    public void CollectGarbage_UnrootedInternedString_RemovesInternEntry()
    {
        var heap = new VmHeap();
        heap.InternString("hello");

        heap.CollectGarbage([]);

        var found = heap.TryGetInternedString("hello", out _);
        Assert.IsFalse(found);
    }

    [TestMethod]
    public void CollectGarbage_ArrayRoot_PreservesReferencedString()
    {
        var heap = new VmHeap();
        var stringHandle = heap.InternString("hello");
        var arrayHandle = heap.Allocate(new HeapArrayObject([Value.FromHeapObject(stringHandle)]));

        heap.CollectGarbage([Value.FromHeapObject(arrayHandle)]);

        Assert.AreEqual("hello", heap.GetStringValue(stringHandle));
        Assert.IsTrue(heap.TryGetInternedString("hello", out var internedHandle));
        Assert.AreEqual(stringHandle, internedHandle);
    }

    [TestMethod]
    public void CollectGarbage_StructRoot_PreservesReferencedArray()
    {
        var heap = new VmHeap();
        var arrayHandle = heap.Allocate(new HeapArrayObject([Value.FromNumber(42)]));
        var structHandle = heap.Allocate(new HeapStructObject("Container", new Dictionary<string, Value>(StringComparer.Ordinal)
        {
            ["items"] = Value.FromHeapObject(arrayHandle)
        }));

        heap.CollectGarbage([Value.FromHeapObject(structHandle)]);

        var array = heap.Get<HeapArrayObject>(arrayHandle);
        Assert.AreEqual(42d, array.Elements[0].Number);
    }

    [TestMethod]
    public void CollectGarbage_NativeObjectRoot_PreservesReferencedString()
    {
        var heap = new VmHeap();
        var stringHandle = heap.InternString("payload");
        var nativeHandle = heap.Allocate(new HeapNativeObject("File", new Dictionary<string, Value>(StringComparer.Ordinal)
        {
            ["lastRead"] = Value.FromHeapObject(stringHandle)
        }));

        heap.CollectGarbage([Value.FromHeapObject(nativeHandle)]);

        var nativeObject = heap.Get<HeapNativeObject>(nativeHandle);
        Assert.AreEqual(stringHandle, nativeObject.Fields["lastRead"].GetRequiredHeapHandle());
        Assert.AreEqual("payload", heap.GetStringValue(stringHandle));
    }

    [TestMethod]
    public void MaybeCollect_UnrootedObject_ReclaimsSlotForNextAllocation()
    {
        var heap = new VmHeap(capacity: 1);
        heap.Allocate(new HeapArrayObject([Value.FromNumber(1)]));

        heap.MaybeCollect([], requestSlots: 1);
        var handle = heap.Allocate(new HeapNativeObject("File", new Dictionary<string, Value>(StringComparer.Ordinal)));

        var obj = heap.Get<HeapNativeObject>(handle);
        Assert.AreEqual("File", obj.NativeObjectName);
    }

    [TestMethod]
    public void MaybeCollect_RootedObject_ThrowsWhenNoSlotsCanBeFreed()
    {
        var heap = new VmHeap(capacity: 1);
        var handle = heap.InternString("hello");

        var exception = Assert.ThrowsExactly<HeapError>(() => heap.MaybeCollect([Value.FromHeapObject(handle)], requestSlots: 1));

        Assert.AreEqual("Heap out of memory: no free slots available for allocation.", exception.Message);
    }
}