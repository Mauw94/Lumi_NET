using Lumi.Ast;
using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode.Tests;

[TestClass]
public sealed class BytecodeTests
{
    [TestMethod]
    public void Test_Binary_Expression()
    {
        // Build AST: (1 + 2)
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 1.0 },
            Operator = "+",
            Right = new NumberNode { Value = 2.0 }
        };

        var program = new Program { Body = [new ExpressionStatement { Expression = expr }] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        // Expect: PushConst 1, PushConst 2, Add
        Assert.HasCount(3, result.Instructions, "Instruction count mismatch");
        Assert.HasCount(2, result.Constants, "Constants count mismatch");

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.Add, result.Instructions[2].Kind);

        Assert.AreEqual(ConstantKind.Number, result.Constants[0].Kind);
        Assert.AreEqual(1.0, result.Constants[0].Number);
        Assert.AreEqual(ConstantKind.Number, result.Constants[1].Kind);
        Assert.AreEqual(2.0, result.Constants[1].Number);
    }

    [TestMethod]
    public void Test_VariableDeclaration_WithInit()
    {
        // Build AST: let x -> 42
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var program = new Program { Body = [decl] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        // Expect: PushConst (42), StoreVar 0
        Assert.HasCount(2, result.Instructions);
        Assert.HasCount(1, result.Constants);

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(42.0, result.Constants[0].Number);

        // StoreVar should have an int operand pointing to the local slot (label id 0)
        Assert.IsTrue(result.Instructions[1].IntOperand.HasValue);
        Assert.AreEqual(0, result.Instructions[1].SafeGetIntOperand());
    }

    [TestMethod]
    public void Test_String_Constant()
    {
        // Build AST: "hello"
        var expr = new StringNode { Value = "hello" };
        var program = new Program { Body = [new ExpressionStatement { Expression = expr }] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        Assert.HasCount(1, result.Instructions);
        Assert.HasCount(1, result.Constants);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(ConstantKind.String, result.Constants[0].Kind);
        Assert.AreEqual("hello", result.Constants[0].String);
    }
}