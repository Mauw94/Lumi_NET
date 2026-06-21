using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode.Constants;

/// <summary>
/// Compacts the constant pool after bytecode generation by removing unused constants
/// and remapping <see cref="InstructionKind.PushConst"/> operands.
/// </summary>
internal static class ConstantPoolCompactor
{
    public static (IReadOnlyList<Instruction> Instructions, IReadOnlyList<Constant> Constants) Compact(
        IReadOnlyList<Instruction> instructions,
        IReadOnlyList<Constant> constants)
    {
        var constantCount = constants.Count;

        if (constantCount == 0)
        {
            ValidateNoInvalidConstantReferences(instructions, constantCount);
            return (instructions, constants);
        }

        var live = new bool[constantCount];
        var liveCount = 0;

        foreach (var instruction in instructions)
        {
            if (!ReferencesConstantPool(instruction))
                continue;

            var oldIndex = instruction.GetSafeIntOperand();
            ValidateConstantIndex(oldIndex, constantCount);

            if (!live[oldIndex])
            {
                live[oldIndex] = true;
                liveCount++;
            }
        }

        if (liveCount == constantCount)
            return (instructions, constants);

        var remap = new int[constantCount];
        Array.Fill(remap, -1);

        var compactedConstants = new List<Constant>(liveCount);
        for (var oldIndex = 0; oldIndex < constantCount; oldIndex++)
        {
            if (!live[oldIndex])
                continue;

            remap[oldIndex] = compactedConstants.Count;
            compactedConstants.Add(constants[oldIndex]);
        }

        var rewrittenInstructions = new List<Instruction>(instructions.Count);
        foreach (var instruction in instructions)
        {
            if (!ReferencesConstantPool(instruction))
            {
                rewrittenInstructions.Add(instruction);
                continue;
            }

            var oldIndex = instruction.GetSafeIntOperand();
            var newIndex = remap[oldIndex];
            if (newIndex < 0)
                throw BytecodeError.InvalidConstantPoolIndex(oldIndex);

            rewrittenInstructions.Add(RewriteConstantReference(instruction, newIndex));
        }

        return (rewrittenInstructions, compactedConstants);
    }

    private static void ValidateNoInvalidConstantReferences(IReadOnlyList<Instruction> instructions, int constantCount)
    {
        foreach (var instruction in instructions)
        {
            if (!ReferencesConstantPool(instruction))
                continue;

            ValidateConstantIndex(instruction.GetSafeIntOperand(), constantCount);
        }
    }

    private static void ValidateConstantIndex(int constantIndex, int constantCount)
    {
        if (constantIndex < 0 || constantIndex >= constantCount)
            throw BytecodeError.InvalidConstantPoolIndex(constantIndex);
    }

    private static bool ReferencesConstantPool(Instruction instruction) =>
        instruction.Kind switch
        {
            InstructionKind.PushConst => true,
            _ => false,
        };

    private static Instruction RewriteConstantReference(Instruction instruction, int newIndex) =>
        instruction.Kind switch
        {
            InstructionKind.PushConst => Instruction.PushConst(newIndex),
            _ => instruction,
        };
}