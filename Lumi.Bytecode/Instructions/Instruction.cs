namespace Lumi.Bytecode.Instructions;

/// <summary>
/// Represents a single instruction with an associated kind and optional operands.
/// Stored as a readonly struct so instructions are laid out inline in the list backing array,
/// eliminating per-instruction heap allocations.
/// </summary>
// NOTE: readonly struct are stored inline in arrays and avoid per-instance heap allocations, but they are copied by value when accessed.
public readonly struct Instruction
{
    public InstructionKind Kind { get; }
    public int? IntOperand { get; }
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

    public static Instruction StoreVar(Label label) => new(InstructionKind.StoreVar, label.Id);
    public static Instruction LoadVar(Label label) => new(InstructionKind.LoadVar, label.Id);
    public static Instruction PushConst(int constIndex) => new(InstructionKind.PushConst, constIndex);
    public static Instruction Add() => new(InstructionKind.Add);
    public static Instruction JumpIfFalse(int operand) => new(InstructionKind.JumpIfFalse, operand);
    public static Instruction Jump(int operand) => new(InstructionKind.Jump, operand);
    public static Instruction CallFn(string functionName) => new(InstructionKind.CallFn, functionName);
}
