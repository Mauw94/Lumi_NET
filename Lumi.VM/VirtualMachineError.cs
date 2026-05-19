using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.VM;

/// <summary>
/// Represents errors that occur during the execution of a virtual machine.
/// </summary>
/// <param name="message">The error message that describes the reason for the exception.</param>
internal sealed class VirtualMachineError(string message) : Exception(message)
{
    internal static VirtualMachineError InvalidValueTypes(Value a, Value b, string operation)
        => new($"Invalid value types for operation '{operation}': {a.Kind} and {b.Kind}");

    internal static VirtualMachineError InvalidUnaryOperation(Value value, string operation)
        => new($"Invalid value type for unary operation '{operation}': {value.Kind}");

    internal static VirtualMachineError StackUnderflow() => new("Stack contains no values to pop.");

    internal static VirtualMachineError StackOverflow() => new("Stack overflow: maximum stack size exceeded.");

    internal static VirtualMachineError InvalidPeekOffset() => new("Offset must be non-negative and less than the stack size.");

    internal static VirtualMachineError UnkownConstantKind(ConstantKind kind) => new($"Unknown constant kind: {kind}");

    internal static VirtualMachineError UnkownValueKind(ValueKind kind) => new($"Unknown value kind: {kind}");

    internal static VirtualMachineError UndefinedVariable(int slot) => new($"Undefined variable at slot {slot}.");

    internal static Exception InvalidJumpCondition(Value condition) => new($"Invalid jump condition: expected a boolean value but got {condition.Kind}.");

    internal static VirtualMachineError UndefinedFunction(string functionName) => new($"Undefined function: {functionName}");

    internal static VirtualMachineError InvalidFunctionCall(string message) => new($"Invalid function call: {message}");

    internal static VirtualMachineError ReturnWithoutCall() => new("Return instruction executed without a corresponding function call.");

    internal static VirtualMachineError InvalidArrayElementCount(int count) => new($"Array element count cannot be negative: {count}.");

    internal static VirtualMachineError IndexOutOfRange(int index, int length) => new($"Array index {index} is out of range for array of length {length}.");

    internal static VirtualMachineError IndexTargetNotArray(ValueKind kind) => new($"Cannot index into a value of kind {kind}. Expected an array.");

    internal static VirtualMachineError MissingStringOperand(InstructionKind kind) => new($"Instruction {kind} is missing a string operand.");

    internal static VirtualMachineError UnknownListMethod(string methodName) => new($"Unknown list method: {methodName}.");

    internal static VirtualMachineError ListMethodTargetNotArray(ValueKind kind) => new($"List method target must be an array, got {kind}.");

    internal static VirtualMachineError ListMethodArgumentCountMismatch(string methodName, int expected, int actual)
        => new($"List method '{methodName}' expects {expected} argument(s) but got {actual}.");
    internal static VirtualMachineError UndefinedStruct(string structName) => new($"Undefined struct: {structName}.");
    internal static VirtualMachineError FieldAccessTargetNotStruct(ValueKind kind) => new($"Field access target must be a struct, got {kind}.");
    internal static VirtualMachineError UnknownStructField(string fieldName) => new($"Unknown struct field: {fieldName}.");
    internal static VirtualMachineError UnknownStructMethod(string structName, string methodName) => new($"Struct '{structName}' does not contain method '{methodName}'.");
    internal static VirtualMachineError MethodTargetNotSupported(string methodName, ValueKind kind) => new($"Method '{methodName}' is not supported on values of kind {kind}.");
    internal static VirtualMachineError StructConstructorArgumentCountMismatch(string structName, int maxExpected, int actual)
        => new($"Struct '{structName}' constructor accepts up to {maxExpected} argument(s) but got {actual}.");
}
