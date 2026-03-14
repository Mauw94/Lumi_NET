namespace Lumi.VM.Tests;

[TestClass]
public sealed class StackTests
{
    [TestMethod]
    public void Stack_Push_And_Peek_Returns_Top_Value()
    {
        var stack = new Stack();
        stack.Push(new NumberValue(42.0));

        var result = stack.Peek();

        Assert.AreEqual(ValueKind.Number, result.Kind);
        Assert.AreEqual(42.0, ((NumberValue)result).Value);
    }

    [TestMethod]
    public void Stack_Pop_Returns_And_Removes_Value()
    {
        var stack = new Stack();
        stack.Push(new NumberValue(10.0));

        var result = stack.Pop();

        Assert.AreEqual(10.0, ((NumberValue)result).Value);
        Assert.AreEqual(0, stack.Count);
    }

    [TestMethod]
    public void Stack_Pop_LIFO_Order()
    {
        var stack = new Stack();
        stack.Push(new NumberValue(1.0));
        stack.Push(new NumberValue(2.0));
        stack.Push(new NumberValue(3.0));

        Assert.AreEqual(3.0, ((NumberValue)stack.Pop()).Value);
        Assert.AreEqual(2.0, ((NumberValue)stack.Pop()).Value);
        Assert.AreEqual(1.0, ((NumberValue)stack.Pop()).Value);
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
        stack.Push(new NumberValue(1.0)); // bottom
        stack.Push(new NumberValue(2.0)); // top

        Assert.AreEqual(2.0, ((NumberValue)stack.Peek(0)).Value); // top
        Assert.AreEqual(1.0, ((NumberValue)stack.Peek(1)).Value); // one below top
    }

    [TestMethod]
    public void Stack_PeekAtOffset_InvalidOffset_Throws()
    {
        var stack = new Stack();
        stack.Push(new NumberValue(1.0));

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Peek(1));  // only one element, offset 1 is out of range
    }

    [TestMethod]
    public void Stack_Count_Tracks_Correctly()
    {
        var stack = new Stack();
        Assert.AreEqual(0, stack.Count);

        stack.Push(new NumberValue(1.0));
        Assert.AreEqual(1, stack.Count);

        stack.Push(new NumberValue(2.0));
        Assert.AreEqual(2, stack.Count);

        stack.Pop();
        Assert.AreEqual(1, stack.Count);
    }

    [TestMethod]
    public void Stack_Overflow_Throws()
    {
        var stack = new Stack();
        for (int i = 0; i < 1024; i++)
            stack.Push(new NumberValue(i));

        Assert.ThrowsExactly<VirtualMachineError>(() => stack.Push(new NumberValue(0)));
    }
}