namespace Lumi.VM.Heap;

internal sealed class HeapStringObject(string value) : HeapObject
{
    public string Value { get; } = value;

    public override string PrintValue() => $"\"{Value}\"";

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        // No references to visit for a string object.
    }
}