namespace Lumi.VM.Heap;

internal sealed class HeapCellObject(Value value) : HeapObject
{
    public Value Value { get; set; } = value;

    public override ValueKind Kind => ValueKind.Reference;

    public override string PrintValue() => "<cell>";

    internal override void VisitReferences(Action<Value> visitValue)
    {
        visitValue(Value);
    }
}
