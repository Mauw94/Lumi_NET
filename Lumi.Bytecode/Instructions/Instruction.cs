namespace Lumi.Bytecode.Instructions;

/// <summary>
/// Represents a single instruction with an associated kind and optional operands.
/// </summary>
public sealed class Instruction
{
    public InstructionKind Kind { get; }

    // Optional integer operand (e.g., constant index or jump target)
    public int? IntOperand { get; }

    // Optional string operand (e.g., function name)
    public string? StringOperand { get; }

    public Instruction(InstructionKind kind)
    {
        Kind = kind;
    }

    public Instruction(InstructionKind kind, int intOperand)
    {
        Kind = kind;
        IntOperand = intOperand;
    }

    public Instruction(InstructionKind kind, string stringOperand)
    {
        Kind = kind;
        StringOperand = stringOperand;
    }

    /// <summary>
    /// Safely retrieves the integer operand of the instruction, throwing an exception if it is not present. This method
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public int SafeGetIntOperand()
    {
        if (!IntOperand.HasValue)
            throw BytecodeError.InstructionKindNoIntegerOperand(Kind);

        return IntOperand.Value;
    }

    public override string ToString()
    {
        return IntOperand.HasValue ? $"{Kind} {IntOperand.Value}" : (StringOperand != null ? $"{Kind} \"{StringOperand}\"" : Kind.ToString());
    }
}