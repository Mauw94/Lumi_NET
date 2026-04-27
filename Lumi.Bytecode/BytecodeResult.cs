using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Locals;

namespace Lumi.Bytecode;

public sealed record BytecodeResult(
    IReadOnlyList<Instruction> Instructions,
    IReadOnlyList<Constant> Constants,
    IReadOnlyList<Local> Locals,
    IReadOnlyDictionary<string, int> FunctionAddresses,
    IReadOnlyDictionary<string, IReadOnlyList<string>> StructDefinitions)
{
    public static BytecodeResult FromGenerator(BytecodeGenerator generator) =>
        new(generator.Instructions, generator.Constants, generator.Locals, generator.FunctionAddresses, generator.StructDefinitions);
}