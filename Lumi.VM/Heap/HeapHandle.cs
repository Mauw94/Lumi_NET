namespace Lumi.VM.Heap;

/// <summary>
/// A handle to a heap-allocated object. 
/// This is a value type (struct) that can be stored on the stack or in registers, 
/// and it contains an integer ID that references the actual object in the heap. 
/// The Heap class manages the mapping from HandleId to HeapObject, 
/// allowing for efficient allocation and garbage collection without exposing raw pointers or references to the VM's internal memory management.
/// </summary>
/// <param name="HandleId"></param>
internal readonly record struct HeapHandle(int HandleId);