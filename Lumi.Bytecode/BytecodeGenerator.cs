using Lumi.Ast;
using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Jumps;

namespace Lumi.Bytecode;

public sealed class BytecodeGenerator
{
    private readonly List<Instruction> _instructions = [];
    private readonly List<Constant> _constants = [];
    private readonly Dictionary<Label, int> _labelPositions = [];
    private readonly Dictionary<Label, int> _symbolTable = [];
    private readonly Dictionary<Label, List<PendingJump>> _unpatchedJumps = [];
    private readonly int _nextLabelId = 0;

    public IReadOnlyList<Instruction> Instructions => _instructions;
    public IReadOnlyList<Constant> Constants => _constants;

    public void Generate(Node node)
    {
        Visit(node);
    }

    private void Visit(Node node)
    {
        if (node is Program program)
        {
            for (int i = 0; i < program.Body.Count; i++)
            {
                Visit(program.Body[i]);
            }
        }

        if (node is VariableDeclaration)
        {
        }

        if (node is PrintStatement printStatement)
        {
            Visit(printStatement.Argument);
            Emit(new Instruction(InstructionKind.Print));
        }

        if (node is NumberNode number)
        {
            var idx = AddConstant(Constant.FromNumber(number.Value));
            Emit(new Instruction(InstructionKind.PushConst, intOperand: idx));
        }

        if (node is StringNode str)
        {
            var idx = AddConstant(Constant.FromString(str.Value));
            Emit(new Instruction(InstructionKind.PushConst, intOperand: idx));
        }
    }

    private void Emit(Instruction instruction)
    {
        _instructions.Add(instruction);
    }

    private int AddConstant(Constant constant)
    {
        int index = _constants.Count;
        _constants.Add(constant);

        return index;
    }
}