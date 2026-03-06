namespace Lumi.Bytecode.Instructions;

/// <summary>
/// Represents a single instruction with an associated kind and optional operands.
/// </summary>
/// <remarks>An instruction may include either an integer operand, which can represent a constant index or a jump
/// target, or a string operand, which can represent a function name. The kind of instruction determines its behavior
/// and usage within the bytecode sequence. Use the appropriate constructor to specify the instruction kind and any
/// required operand.</remarks>
public class Instruction
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

    public override string ToString()
    {
        return IntOperand.HasValue ? $"{Kind} {IntOperand.Value}" : (StringOperand != null ? $"{Kind} \"{StringOperand}\"" : Kind.ToString());
    }
}