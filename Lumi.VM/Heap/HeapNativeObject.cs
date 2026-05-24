namespace Lumi.VM.Heap;

internal sealed class HeapNativeObject : HeapObject
{
    public HeapNativeObject(string nativeObjectName, Dictionary<string, Value> fields)
    {
        NativeObjectName = nativeObjectName;
        Fields = fields;
    }

    public string NativeObjectName { get; }
    public Dictionary<string, Value> Fields { get; }
    public override ValueKind Kind => ValueKind.NativeObject;

    public override string PrintValue() => $"<{NativeObjectName ?? "native"}>";

    internal override void VisitReferences(Action<Value> visitValue)
    {
        foreach (var field in Fields.Values)
        {
            visitValue(field);
        }
    }
}