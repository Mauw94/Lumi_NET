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

    public Instruction(InstructionKind kind, int intOperand, string stringOperand)
    {
        Kind = kind;
        IntOperand = intOperand;
        StringOperand = stringOperand;
    }

    public int GetSafeIntOperand()
    {
        if (!IntOperand.HasValue)
            throw BytecodeError.InstructionKindNoIntegerOperand(Kind);

        return IntOperand.Value;
    }

    public string GetSafeStringOperand()
    {
        if (StringOperand is null)
            throw BytecodeError.InstructionKindNoStringOperand(Kind);

        return StringOperand;
    }

    public override string ToString()
    {
        if (IntOperand.HasValue && StringOperand is not null)
            return $"{Kind} {IntOperand.Value} \"{StringOperand}\"";

        return IntOperand.HasValue ? $"{Kind} {IntOperand.Value}" : (StringOperand != null ? $"{Kind} \"{StringOperand}\"" : Kind.ToString());
    }

    public static Instruction StoreVar(Label label) => new(InstructionKind.StoreVar, label.Id);
    public static Instruction LoadVar(Label label) => new(InstructionKind.LoadVar, label.Id);
    public static Instruction PushConst(int constIndex) => new(InstructionKind.PushConst, constIndex);
    public static Instruction Pop() => new(InstructionKind.Pop);
    public static Instruction Add() => new(InstructionKind.Add);
    public static Instruction JumpIfFalse(int operand) => new(InstructionKind.JumpIfFalse, operand);
    public static Instruction Jump(int operand) => new(InstructionKind.Jump, operand);
    public static Instruction CallFn(string functionName) => new(InstructionKind.CallFn, functionName);
    public static Instruction CallMemberMethod(string methodName, int argumentCount) => new(InstructionKind.CallMemberMethod, argumentCount, methodName);
    public static Instruction LoadPreludeGlobal(string name) => new(InstructionKind.LoadPreludeGlobal, name);
    public static Instruction NewStruct(string structName, int argumentCount) => new(InstructionKind.NewStruct, argumentCount, structName);
    public static Instruction LoadField(string fieldName) => new(InstructionKind.LoadField, fieldName);
    public static Instruction StoreField(string fieldName) => new(InstructionKind.StoreField, fieldName);
    public static Instruction MakeArray(int elementCount) => new(InstructionKind.MakeArray, elementCount);
    public static Instruction IndexArray() => new(InstructionKind.IndexArray);
    public static Instruction MakeClosure(int functionAddress) => new(InstructionKind.MakeClosure, functionAddress);
    public static Instruction LoadCapture(int captureIndex) => new(InstructionKind.LoadCapture, captureIndex);
    public static Instruction StoreCapture(int captureIndex) => new(InstructionKind.StoreCapture, captureIndex);
    public static Instruction CallValue(int argumentCount) => new(InstructionKind.CallValue, argumentCount);
}
