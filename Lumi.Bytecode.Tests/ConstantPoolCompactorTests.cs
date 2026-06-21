using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode.Tests;

[TestClass]
public sealed class ConstantPoolCompactorTests
{
    [TestMethod]
    public void Compact_RemovesUnusedConstants_AndRewritesPushConstOperands()
    {
        var constants = new[]
        {
            Constant.FromNumber(1),
            Constant.FromNumber(2),
            Constant.FromNumber(3),
        };

        var instructions = new[]
        {
            Instruction.PushConst(0),
            Instruction.PushConst(2),
            Instruction.Add(),
        };

        var (rewrittenInstructions, compactedConstants) = ConstantPoolCompactor.Compact(instructions, constants);

        Assert.HasCount(2, compactedConstants);
        Assert.AreEqual(1.0, compactedConstants[0].Number);
        Assert.AreEqual(3.0, compactedConstants[1].Number);

        Assert.HasCount(3, rewrittenInstructions);
        Assert.AreEqual(InstructionKind.PushConst, rewrittenInstructions[0].Kind);
        Assert.AreEqual(0, rewrittenInstructions[0].GetSafeIntOperand());
        Assert.AreEqual(InstructionKind.PushConst, rewrittenInstructions[1].Kind);
        Assert.AreEqual(1, rewrittenInstructions[1].GetSafeIntOperand());
        Assert.AreEqual(InstructionKind.Add, rewrittenInstructions[2].Kind);
    }

    [TestMethod]
    public void Compact_Throws_WhenPushConstIndexIsOutOfRange()
    {
        var constants = new[] { Constant.FromNumber(1) };
        var instructions = new[] { Instruction.PushConst(1) };

        Assert.ThrowsExactly<BytecodeError>(() => ConstantPoolCompactor.Compact(instructions, constants));
    }

    [TestMethod]
    public void Compact_RemovesAllConstants_WhenNoInstructionReferencesThem()
    {
        var constants = new[]
        {
            Constant.FromNumber(1),
            Constant.FromString("hello"),
        };

        var instructions = new[] { new Instruction(InstructionKind.Nop), new Instruction(InstructionKind.Halt) };

        var (rewrittenInstructions, compactedConstants) = ConstantPoolCompactor.Compact(instructions, constants);

        Assert.HasCount(0, compactedConstants);
        Assert.HasCount(2, rewrittenInstructions);
        Assert.AreEqual(InstructionKind.Nop, rewrittenInstructions[0].Kind);
        Assert.AreEqual(InstructionKind.Halt, rewrittenInstructions[1].Kind);
    }
}
