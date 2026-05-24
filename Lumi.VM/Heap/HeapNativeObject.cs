namespace Lumi.VM.Heap;

internal sealed class HeapNativeObject : HeapObject
{
    public HeapNativeObject(string nativeObjectName, Dictionary<string, Value> fields)
    {
        NativeObjectName = nativeObjectName;
        Fields = fields;
        Kind = ValueKind.NativeObject;
    }

    public string NativeObjectName { get; }
    public Dictionary<string, Value> Fields { get; }

    public override string PrintValue() => $"<{NativeObjectName ?? "native"}>";

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var field in Fields.Values)
        {
            visitValue(field);
        }
    }
}