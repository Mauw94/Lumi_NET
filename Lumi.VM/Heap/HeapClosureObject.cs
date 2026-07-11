namespace Lumi.VM.Heap;

internal sealed class HeapClosureObject : HeapObject
{
    public HeapClosureObject(int functionAddress, Value environment)
    {
        if (!environment.IsHeapAllocated())
            throw new ArgumentException("Closures must reference a heap-allocated environment.", nameof(environment));

        FunctionAddress = functionAddress;
        Environment = environment;
    }

    public int FunctionAddress { get; }
    public Value Environment { get; }

    public override ValueKind Kind => ValueKind.Function;

    public override string PrintValue() => $"<closure@{FunctionAddress}>";

    internal override void VisitReferences(Action<Value> visitValue)
    {
        visitValue(Environment);
    }
}
