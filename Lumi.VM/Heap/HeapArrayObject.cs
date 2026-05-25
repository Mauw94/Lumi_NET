namespace Lumi.VM.Heap;

internal sealed class HeapArrayObject : HeapObject
{
    public HeapArrayObject(List<Value> elements)
    {
        Elements = elements;
    }

    public List<Value> Elements { get; }

    public override ValueKind Kind => ValueKind.Array;

    public override string PrintValue() => $"[{string.Join(", ", (Elements ?? []).Select(static v => v.PrintValue()))}]";

    internal override void VisitReferences(Action<Value> visitValue)
    {
        foreach (var element in Elements)
        {
            visitValue(element);
        }
    }
}