namespace Lumi.VM.Heap;

internal sealed class HeapEnvironmentObject : HeapObject
{
    public HeapEnvironmentObject(IEnumerable<Value> captures)
    {
        Captures = [.. captures];
    }

    public IReadOnlyList<Value> Captures { get; }

    public override ValueKind Kind => ValueKind.HeapObject;

    public override string PrintValue() => $"[{string.Join(", ", Captures.Select(FormatCapture))}]";

    internal override void VisitReferences(Action<Value> visitValue)
    {
        foreach (var capture in Captures)
        {
            visitValue(capture);
        }
    }

    private static string FormatCapture(Value value) => value.IsHeapAllocated()
        ? "<heap-object>"
        : value.PrintValue();
}
