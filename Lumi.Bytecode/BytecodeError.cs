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
    public static BytecodeError InstructionKindNoStringOperand(InstructionKind instructionKind)
        => new($"Instruction of kind {instructionKind} does not have a string operand.");
    public static BytecodeError UnsupportedOperator(string ope) => new($"Unsupported operator: {ope}");
    public static BytecodeError ExpectedIdentifierInVariableDeclaration() => new($"Expected identifier in variable declaration.");
    public static BytecodeError ExpectedIdentifierInFunctionDeclaration() => new($"Expected identifier in function declaration.");
    public static BytecodeError ExpectedIdentifierInFunctionParameter() => new($"Expected identifier in function parameter.");
    public static BytecodeError ExpectedIdentifierInFunctionCall() => new($"Expected identifier in function call.");
    public static BytecodeError ExpectedIdentifierInMemberAccess() => new("Expected identifier property in member access.");
    public static BytecodeError UnsupportedListMethod(string methodName) => new($"Unsupported list method: {methodName}");
    public static BytecodeError StructAlreadyDefined(string structName) => new($"Struct '{structName}' is already defined.");
    public static BytecodeError UndefinedStruct(string structName) => new($"Undefined struct: {structName}");
    public static BytecodeError StructConstructorArgumentCountMismatch(string structName, int maxExpected, int actual)
        => new($"Struct '{structName}' constructor accepts up to {maxExpected} argument(s) but was called with {actual}.");
    public static BytecodeError InvalidStructConstructorArgumentsMixing(string structName)
        => new($"Struct '{structName}' constructor cannot mix named and positional arguments.");
    public static BytecodeError UndefinedVariable(string name) => new($"Undefined variable: {name}");
    public static BytecodeError NoActiveScope() => new("No active scope.");
    public static BytecodeError ForStatementMissingIterator() => new("For statement is missing an iterator expression.");
    public static BytecodeError NoValidIteratorFound() => new("No valid iterator found for for statement. Expected an identifier.");
    public static Exception InvalidAssignmentTarget() => new("Invalid assignment target. Expected a variable or a property.");
}