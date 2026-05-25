namespace Lumi.VM.Heap;

abstract class HeapObject()
{
    public bool IsMarked { get; set; }
    public abstract ValueKind Kind { get; }

    internal abstract void VisitReferences(Action<Value> visitValue);
    public abstract string PrintValue();
}