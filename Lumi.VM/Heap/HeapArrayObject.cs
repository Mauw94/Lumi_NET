namespace Lumi.VM.Heap;

internal sealed class HeapArrayObject : HeapObject
{
    public HeapArrayObject(List<Value> elements)
    {
        Elements = elements;
        Kind = ValueKind.Array;
    }

    public List<Value> Elements { get; }

    public override string PrintValue() => $"[{string.Join(", ", (Elements ?? []).Select(static v => v.PrintValue()))}]";

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var element in Elements)
        {
            visitValue(element);
        }
    }
}