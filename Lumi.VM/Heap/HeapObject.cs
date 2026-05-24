namespace Lumi.VM.Heap;

abstract class HeapObject()
{
    public bool IsMarked { get; set; }
    public bool IsAllocated { get; set; } = false;
    public ValueKind Kind;
    public int SizeEstimate { get; set; }
    public abstract string PrintValue();
    protected abstract void VisitReferences(Action<int> visitHandle, Action<Value> visitValue);
}