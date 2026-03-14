using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode;

public sealed record BytecodeResult(IReadOnlyList<Instruction> Instructions, IReadOnlyList<Constant> Constants)
{
    public static BytecodeResult FromGenerator(BytecodeGenerator generator) => new(generator.Instructions, generator.Constants);
}