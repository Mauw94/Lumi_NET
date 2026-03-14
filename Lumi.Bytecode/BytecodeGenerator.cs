using Lumi.Ast;
using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Jumps;

namespace Lumi.Bytecode;

/// <summary>
/// Generates bytecode instructions and manages constants for a given abstract syntax tree (AST).
/// </summary>
public sealed class BytecodeGenerator
{
    private readonly List<Instruction> _instructions = [];
    private readonly ConstantPool _constantPool = new();
    private readonly Dictionary<Label, int> _labelPositions = [];
    private readonly Dictionary<Label, int> _symbolTable = [];
    private readonly Dictionary<Label, List<PendingJump>> _unpatchedJumps = [];
    private readonly int _nextLabelId = 0;

    public IReadOnlyList<Instruction> Instructions => _instructions;
    public IReadOnlyList<Constant> Constants => _constantPool.Values;

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

        if (node is ExpressionStatement expressionStatement)
        {
            Visit(expressionStatement.Expression);
        }

        if (node is BinaryExpression binaryExpression)
        {
            Visit(binaryExpression.Left);
            Visit(binaryExpression.Right);
            switch (binaryExpression.Operator)
            {
                case "+":
                    Emit(new Instruction(InstructionKind.Add));
                    break;
                case "-":
                    Emit(new Instruction(InstructionKind.Sub));
                    break;
                case "*":
                    Emit(new Instruction(InstructionKind.Mul));
                    break;
                case "/":
                    Emit(new Instruction(InstructionKind.Div));
                    break;
                default:
                    throw new NotSupportedException($"Unsupported operator: {binaryExpression.Operator}");
            }
        }
    }

    private void Emit(Instruction instruction)
    {
        _instructions.Add(instruction);
    }

    private int AddConstant(Constant constant) => _constantPool.Add(constant);
}