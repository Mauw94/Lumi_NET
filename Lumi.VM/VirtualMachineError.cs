using Lumi.Bytecode.Instructions;

namespace Lumi.VM;

/// <summary>
/// Represents errors that occur during the execution of a virtual machine.
/// </summary>
/// <param name="message">The error message that describes the reason for the exception.</param>
public sealed class VirtualMachineError(string message) : Exception(message)
{
    public static VirtualMachineError InvalidValueTypes(Value a, Value b, string operation)
        => new($"Invalid value types for operation '{operation}': {a.Kind} and {b.Kind}");

    public static VirtualMachineError InvalidUnaryOperation(Value value, string operation)
        => new($"Invalid value type for unary operation '{operation}': {value.Kind}");
}