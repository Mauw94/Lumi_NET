namespace Lumi.Bytecode.Instructions;

/// <summary>
/// Specifies the set of instructions that a virtual machine can execute, including operations for arithmetic, control
/// flow, variable manipulation, and program management.
/// </summary>
/// <remarks>This enumeration defines the fundamental operations supported by the virtual machine, such as pushing
/// and popping values, performing arithmetic and comparison, managing variables, controlling program flow, and handling
/// function calls. Each member represents a distinct instruction that can be interpreted and executed by the virtual
/// machine. Use this enumeration to identify or implement the behavior of individual instructions when building or
/// extending a virtual machine.</remarks>
public enum InstructionKind
{
    PushConst,
    Pop,
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Inc,
    Dec,
    Eq,
    Neq,
    Lt,
    Gt,
    Leq,
    Geq,
    Negate,
    Not,
    Jump,
    JumpIfTrue,
    JumpIfFalse,
    CallFn,
    CallListMethod,
    Return,
    LoadVar,
    StoreVar,
    MakeArray,
    IndexArray,
    Print,
    Nop,
    Halt,
}