namespace Lumi.VM.Heap;

internal sealed class HeapError(string message) : Exception(message)
{
    internal static HeapError OutOfMemory() => new("Heap out of memory: no free slots available for allocation.");

    internal static HeapError DanglingReference() => new("Dangling reference: attempted to access a heap slot that is not currently allocated.");
}