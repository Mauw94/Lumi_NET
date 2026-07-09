using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Locals;

namespace Lumi.Bytecode;

public sealed record BytecodeResult(
    IReadOnlyList<Instruction> Instructions,
    IReadOnlyList<Constant> Constants,
    IReadOnlyList<Local> Locals,
    IReadOnlyDictionary<string, int> FunctionAddresses,
    IReadOnlyDictionary<int, FunctionDescriptor> FunctionDescriptors,
    IReadOnlyDictionary<string, int> FunctionDescriptorIds,
    IReadOnlyDictionary<string, IReadOnlyList<string>> StructDefinitions,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> StructMethodAddresses,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> StructMethodDescriptorIds)
{
    public static BytecodeResult FromGenerator(BytecodeGenerator generator)
    {
        var (instructions, constants) = ConstantPoolCompactor.Compact(generator.Instructions, generator.Constants);
        return new(
            instructions,
            constants,
            generator.Locals,
            generator.FunctionAddresses,
            generator.FunctionDescriptors,
            generator.FunctionDescriptorIds,
            generator.StructDefinitions,
            generator.StructMethodAddresses,
            generator.StructMethodDescriptorIds);
    }
}
