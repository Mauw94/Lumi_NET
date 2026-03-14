using Lumi.Ast;
using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;
using Lumi.Bytecode.Locals;

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

        // No type annotation — Type should be unknown
        var local = result.Locals.Single();
        Assert.AreEqual(VarType.Unknown, local.Type);
    }

    [TestMethod]
    public void Test_VariableDeclaration_WithVarType()
    {
        // Build AST: let x: int  (no initializer)
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    VarType = new IdentifierNode { Name = "int" }
                }
            ]
        };

        var program = new Program { Body = [decl] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        // No initializer — no instructions or constants emitted
        Assert.HasCount(0, result.Instructions);
        Assert.HasCount(0, result.Constants);

        // Variable should still be registered with its type
        var local = result.Locals.Single();
        Assert.AreEqual("x", local.Name);
        Assert.AreEqual(LocalKind.Let, local.Kind);
        Assert.AreEqual(VarType.Int, local.Type);
    }

    [TestMethod]
    public void Test_VariableDeclaration_WithVarTypeAndInit()
    {
        // Build AST: let x: int -> 42
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    VarType = new IdentifierNode { Name = "int" },
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

        Assert.AreEqual(ConstantKind.Number, result.Constants[0].Kind);
        Assert.AreEqual(42.0, result.Constants[0].Number);

        Assert.IsTrue(result.Instructions[1].IntOperand.HasValue);
        Assert.AreEqual(0, result.Instructions[1].SafeGetIntOperand());

        // Type annotation should be stored on the local
        var local = result.Locals.Single();
        Assert.AreEqual("x", local.Name);
        Assert.AreEqual(LocalKind.Let, local.Kind);
        Assert.AreEqual(VarType.Int, local.Type);
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

    [TestMethod]
    public void Test_Mixed_Local_Kinds_And_Shadowing()
    {
        // let x -> 1; { var x -> 2; x }
        var outerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator { VarName = new IdentifierNode { Name = "x" }, Init = new NumberNode { Value = 1.0 } }
            ]
        };

        var innerDecl = new VariableDeclaration
        {
            Kind = "var",
            Declarations =
            [
                new VariableDeclarator { VarName = new IdentifierNode { Name = "x" }, Init = new NumberNode { Value = 2.0 } }
            ]
        };

        // Block that contains inner declaration and then an identifier usage
        var block = new BlockStatement
        {
            Body =
            [
                innerDecl,
                new ExpressionStatement { Expression = new IdentifierNode { Name = "x" } }
            ]
        };

        var program = new Program { Body = [outerDecl, block] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        // Expect sequence: PushConst(1), StoreVar(outer x), PushConst(2), StoreVar(inner x), LoadVar(inner x)
        Assert.IsGreaterThanOrEqualTo(5, result.Instructions.Count);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[3].Kind);
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[4].Kind);

        // The LoadVar should point to the inner variable (label id 1)
        var loadOp = result.Instructions[4].SafeGetIntOperand();
        Assert.AreEqual(1, loadOp);
    }
}