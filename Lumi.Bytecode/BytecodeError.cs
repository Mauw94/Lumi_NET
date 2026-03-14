using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode;

/// <summary>
/// Represents errors that occur during bytecode processing or validation.
/// </summary>
/// <param name="message">The error message that describes the reason for the exception.</param>
public sealed class BytecodeError(string message) : Exception(message)
{
    public static BytecodeError InvalidInstruction(Instruction instruction) => new($"Invalid instruction: {instruction}");
    public static BytecodeError UndefinedLabel(Label label) => new($"Undefined label: {label}");
    public static BytecodeError UnpatchedJump(Label label) => new($"Unpatched jump to label: {label}");
    public static BytecodeError InvalidConstantIndex(int index) => new($"Invalid constant index: {index}");
    public static BytecodeError InstructionKindNoIntegerOperand(InstructionKind instructionKind)
        => new($"Instruction of kind {instructionKind} does not have an integer operand.");
}