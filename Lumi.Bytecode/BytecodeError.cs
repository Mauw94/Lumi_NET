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
    public static BytecodeError UnsupportedOperator(string ope) => new($"Unsupported operator: {ope}");
    public static BytecodeError ExpectedIdentifierInVariableDeclaration() => new($"Expected identifier in variable declaration.");
    public static BytecodeError UndefinedVariable(string name) => new($"Undefined variable: {name}");
    public static BytecodeError NoActiveScope() => new("No active scope.");
    public static BytecodeError ForStatementMissingIterator() => new("For statement is missing an iterator expression.");
    public static BytecodeError NoValidIteratorFound() => new("No valid iterator found for for statement. Expected an identifier.");
}