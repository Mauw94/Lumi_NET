using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Locals;

namespace Lumi.Bytecode;

public sealed record BytecodeResult(
    IReadOnlyList<Instruction> Instructions,
    IReadOnlyList<Constant> Constants,
    IEnumerable<Local> Locals)
{
    public static BytecodeResult FromGenerator(BytecodeGenerator generator) =>
        new(generator.Instructions, generator.Constants, generator.Locals);
}