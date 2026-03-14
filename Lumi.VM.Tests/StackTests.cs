using Lumi.VM;

namespace Lumi.VM.Tests;

[TestClass]
public sealed class StackTests
{
    [TestMethod]
    public void Stack_Push_And_Peek_Returns_Top_Value()
    {
        var stack = new Stack();
        stack.Push(Value.FromNumber(42.0));

        var result = stack.Peek();

        Assert.AreEqual(ValueKind.Number, result.Kind);
        Assert.AreEqual(42.0, result.Number);
    }

    [TestMethod]
    public void Stack_Pop_Returns_And_Removes_Value()
    {
        var stack = new Stack();
        stack.Push(Value.FromNumber(10.0));

        var result = stack.Pop();

        Assert.AreEqual(10.0, result.Number);
        Assert.AreEqual(0, stack.Count);
    }

    [TestMethod]
    public void Stack_Pop_LIFO_Order()
    {
        var stack = new Stack();
        stack.Push(Value.FromNumber(1.0));
        stack.Push(Value.FromNumber(2.0));
        stack.Push(Value.FromNumber(3.0));

        Assert.AreEqual(3.0, stack.Pop().Number);
        Assert.AreEqual(2.0, stack.Pop().Number);
        Assert.AreEqual(1.0, stack.Pop().Number);
    }

    [TestMethod]
    public void Stack_Pop_On_Empty_Throws_StackUnderflow()
    {
        var stack = new Stack();

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Pop());
    }

    [TestMethod]
    public void Stack_Peek_On_Empty_Throws_StackUnderflow()
    {
        var stack = new Stack();

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Peek());
    }

    [TestMethod]
    public void Stack_PeekAtOffset_Returns_Correct_Value()
    {
        var stack = new Stack();
        stack.Push(Value.FromNumber(1.0)); // bottom
        stack.Push(Value.FromNumber(2.0)); // top

        Assert.AreEqual(2.0, stack.Peek(0).Number); // top
        Assert.AreEqual(1.0, stack.Peek(1).Number); // one below top
    }

    [TestMethod]
    public void Stack_PeekAtOffset_InvalidOffset_Throws()
    {
        var stack = new Stack();
        stack.Push(Value.FromNumber(1.0));

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Peek(1));
    }

    [TestMethod]
    public void Stack_Count_Tracks_Correctly()
    {
        var stack = new Stack();
        Assert.AreEqual(0, stack.Count);

        stack.Push(Value.FromNumber(1.0));
        Assert.AreEqual(1, stack.Count);

        stack.Push(Value.FromNumber(2.0));
        Assert.AreEqual(2, stack.Count);

        stack.Pop();
        Assert.AreEqual(1, stack.Count);
    }

    [TestMethod]
    public void Stack_Overflow_Throws()
    {
        var stack = new Stack();
        for (int i = 0; i < 1024; i++)
            stack.Push(Value.FromNumber(i));

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Push(Value.FromNumber(0)));
    }
}