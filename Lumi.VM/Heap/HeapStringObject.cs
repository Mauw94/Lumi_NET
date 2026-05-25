namespace Lumi.VM.Heap;

internal sealed class HeapStringObject(string value) : HeapObject
{
    public string Value { get; } = value;

    public override string PrintValue() => $"\"{Value}\"";

    public override ValueKind Kind => ValueKind.String;

    internal override void VisitReferences(Action<Value> visitValue)
    {
        // No references to visit for a string object.
    }
}