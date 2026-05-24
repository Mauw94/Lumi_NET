namespace Lumi.VM.Heap;

internal sealed class HeapStructObject : HeapObject
{
    public HeapStructObject(string structName, Dictionary<string, Value> fields)
    {
        StructName = structName;
        Fields = fields;
        Kind = ValueKind.Struct;
    }

    public string StructName { get; }
    public Dictionary<string, Value> Fields { get; }

    public override string PrintValue() => "{" + string.Join(", ", (Fields ?? []).Select(static kv => $"{kv.Key}: {kv.Value.PrintValue()}")) + "}";

    protected override void VisitReferences(Action<int> visitHandle, Action<Value> visitValue)
    {
        foreach (var field in Fields.Values)
        {
            visitValue(field);
        }
    }
}