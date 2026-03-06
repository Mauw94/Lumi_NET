using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode.Core;

/// <summary>
/// Represents a function definition that includes its name, parameter count, instructions, and constants used within
/// its body.
/// </summary>
/// <remarks>Use this type to encapsulate the essential components of a function for execution or analysis. The
/// properties allow inspection and modification of the function's structure and behavior.</remarks>
/// <param name="name">The name that identifies the function within its context.</param>
/// <param name="arity">The number of parameters that the function accepts.</param>
/// <param name="instructions">The collection of instructions that define the function's operational logic.</param>
/// <param name="constants">The list of constants available for use within the function's instructions.</param>
public sealed class FunctionObject(string name, int arity, List<Instruction> instructions, List<Constant> constants)
{
    public string Name { get; set; } = name;
    public int Arity { get; set; } = arity;
    public List<Instruction> Instructions { get; set; } = instructions;
    public List<Constant> Constants { get; set; } = constants;
}
