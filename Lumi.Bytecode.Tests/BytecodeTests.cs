using Lumi.AST;
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
        Assert.AreEqual(0, result.Instructions[1].GetSafeIntOperand());

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
        Assert.AreEqual(0, result.Instructions[1].GetSafeIntOperand());

        // Type annotation should be stored on the local
        var local = result.Locals.Single();
        Assert.AreEqual("x", local.Name);
        Assert.AreEqual(LocalKind.Let, local.Kind);
        Assert.AreEqual(VarType.Int, local.Type);
    }

    [TestMethod]
    public void Test_VariableDeclaration_WithListType()
    {
        // Build AST: let items: list
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "items" },
                    VarType = new IdentifierNode { Name = "list" }
                }
            ]
        };

        var program = new Program { Body = [decl] };

        var gen = new BytecodeGenerator();
        var result = gen.Generate(program);

        Assert.HasCount(0, result.Instructions);
        Assert.HasCount(0, result.Constants);

        var local = result.Locals.Single();
        Assert.AreEqual("items", local.Name);
        Assert.AreEqual(LocalKind.Let, local.Kind);
        Assert.AreEqual(VarType.List, local.Type);
    }

    [TestMethod]
    public void Test_ListMethodCall_Emits_CallMemberMethod()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "add" }
                        },
                        Arguments = [new NumberNode { Value = 2.0 }]
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("add", call.StringOperand);
        Assert.AreEqual(1, call.GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_ListRemoveMethodCall_Emits_CallMemberMethod()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "remove" }
                        },
                        Arguments = [new NumberNode { Value = 1.0 }]
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("remove", call.StringOperand);
        Assert.AreEqual(1, call.GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_ListLengthMethodCall_Emits_CallMemberMethod()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "length" }
                        },
                        Arguments = []
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("length", call.StringOperand);
        Assert.AreEqual(0, call.GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_ListContainsMethodCall_Emits_CallMemberMethod()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "contains" }
                        },
                        Arguments = [new NumberNode { Value = 1.0 }]
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("contains", call.StringOperand);
        Assert.AreEqual(1, call.GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_StructMethodCall_Emits_CallMemberMethod_And_Registers_Method_Address()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } }
                    ],
                    Methods =
                    [
                        new FunctionDeclaration
                        {
                            Id = new IdentifierNode { Name = "greet" },
                            Body = new BlockStatement
                            {
                                Body =
                                [
                                    new PrintStatement
                                    {
                                        Argument = new MemberExpression
                                        {
                                            Object = new IdentifierNode { Name = "this" },
                                            Property = new IdentifierNode { Name = "name" }
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "person" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "person" },
                            Property = new IdentifierNode { Name = "greet" }
                        }
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("greet", call.StringOperand);
        Assert.AreEqual(0, call.GetSafeIntOperand());
        Assert.IsTrue(result.StructMethodAddresses.ContainsKey("Person"));
        Assert.IsTrue(result.StructMethodAddresses["Person"].ContainsKey("greet"));
    }

    [TestMethod]
    public void Test_PreludeFileMethodCall_Loads_Prelude_Global_And_Emits_CallMemberMethod()
    {
        var program = new Program
        {
            Body =
            [
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "File" },
                            Property = new IdentifierNode { Name = "readText" }
                        },
                        Arguments = [new StringNode { Value = "input.txt" }]
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        Assert.AreEqual(InstructionKind.LoadPreludeGlobal, result.Instructions[0].Kind);
        Assert.AreEqual("File", result.Instructions[0].StringOperand);

        var call = result.Instructions.First(i => i.Kind == InstructionKind.CallMemberMethod);
        Assert.AreEqual("readText", call.StringOperand);
        Assert.AreEqual(1, call.GetSafeIntOperand());
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
        var loadOp = result.Instructions[4].GetSafeIntOperand();
        Assert.AreEqual(1, loadOp);
    }

    [TestMethod]
    public void Test_IfStatement_WithoutElse()
    {
        // if (true) { print 1 }
        //
        // Expected bytecode:
        //   0: PushConst  (true)
        //   1: JumpIfFalse -> 4
        //   2: PushConst  (1)
        //   3: Print
        //   4: <end>
        var ifStmt = new IfStatement
        {
            Expr = new BooleanNode { Value = true },
            Stmt = new PrintStatement { Argument = new NumberNode { Value = 1.0 } }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [ifStmt] });

        Assert.HasCount(4, result.Instructions);

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.JumpIfFalse, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.Print, result.Instructions[3].Kind);

        // JumpIfFalse must jump past the then-branch (index 4 = one past last instruction)
        Assert.AreEqual(4, result.Instructions[1].GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_IfStatement_WithElse()
    {
        // if (true) { print 1 } else { print 2 }
        //
        // Expected bytecode:
        //   0: PushConst  (true)        <- condition
        //   1: JumpIfFalse -> 5         <- jump to else-branch
        //   2: PushConst  (1)           <- then: argument
        //   3: Print                    <- then: print
        //   4: Jump -> 7                <- skip else-branch
        //   5: PushConst  (2)           <- else: argument
        //   6: Print                    <- else: print
        //   7: <end>
        var ifStmt = new IfStatement
        {
            Expr = new BooleanNode { Value = true },
            Stmt = new PrintStatement { Argument = new NumberNode { Value = 1.0 } },
            ElsePart = new PrintStatement { Argument = new NumberNode { Value = 2.0 } }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [ifStmt] });

        Assert.HasCount(7, result.Instructions);

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind); // condition
        Assert.AreEqual(InstructionKind.JumpIfFalse, result.Instructions[1].Kind); // jump to else
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind); // then: push 1
        Assert.AreEqual(InstructionKind.Print, result.Instructions[3].Kind); // then: print
        Assert.AreEqual(InstructionKind.Jump, result.Instructions[4].Kind); // jump past else
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[5].Kind); // else: push 2
        Assert.AreEqual(InstructionKind.Print, result.Instructions[6].Kind); // else: print

        // JumpIfFalse jumps to else-branch start (index 5)
        Assert.AreEqual(5, result.Instructions[1].GetSafeIntOperand());
        // Unconditional Jump jumps past else-branch (index 7 = one past last)
        Assert.AreEqual(7, result.Instructions[4].GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_ForStatement_DefaultStep()
    {
        // for i in 0 to 3 { print i }
        //
        // Locals (scoped, not visible after generation):
        //   slot 0 = i        (iterator)
        //   slot 1 = $end     (end bound)
        //   slot 2 = $step    (step value)
        //
        // Constants:
        //   0: Number(0)   — start
        //   1: Number(3)   — end
        //   2: Number(1)   — default step
        //
        // Expected bytecode:
        //    0: PushConst 0          — push start (0)
        //    1: StoreVar 0           — i = 0
        //    2: PushConst 1          — push end (3)
        //    3: StoreVar 1           — $end = 3
        //    4: PushConst 2          — push step (1)
        //    5: StoreVar 2           — $step = 1
        //    6: LoadVar 0            — push i          ← loopStart
        //    7: LoadVar 1            — push $end
        //    8: Leq                  — i <= $end
        //    9: JumpIfFalse -> 17    — exit loop
        //   10: LoadVar 0            — push i (print arg)
        //   11: Print                — print i
        //   12: LoadVar 0            — push i (increment)
        //   13: LoadVar 2            — push $step
        //   14: Add                  — i + step
        //   15: StoreVar 0           — i = result
        //   16: Jump 6               — back to loopStart
        //   17: <end>

        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 3.0 },
            Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [forStmt] });

        Assert.HasCount(17, result.Instructions);
        Assert.HasCount(3, result.Constants);

        // Initialization
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(0, result.Instructions[1].GetSafeIntOperand()); // slot 0 = i

        // End bound
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[3].Kind);
        Assert.AreEqual(1, result.Instructions[3].GetSafeIntOperand()); // slot 1 = $end

        // Default step
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[4].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[5].Kind);
        Assert.AreEqual(2, result.Instructions[5].GetSafeIntOperand()); // slot 2 = $step

        // Loop condition
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[6].Kind);  // load i
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[7].Kind);  // load $end
        Assert.AreEqual(InstructionKind.Leq, result.Instructions[8].Kind);
        Assert.AreEqual(InstructionKind.JumpIfFalse, result.Instructions[9].Kind);
        Assert.AreEqual(17, result.Instructions[9].GetSafeIntOperand()); // exit to end

        // Body: print i
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[10].Kind);
        Assert.AreEqual(InstructionKind.Print, result.Instructions[11].Kind);

        // Increment
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[12].Kind);  // load i
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[13].Kind);  // load $step
        Assert.AreEqual(InstructionKind.Add, result.Instructions[14].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[15].Kind); // store i

        // Backward jump
        Assert.AreEqual(InstructionKind.Jump, result.Instructions[16].Kind);
        Assert.AreEqual(6, result.Instructions[16].GetSafeIntOperand()); // back to loop condition

        // Constants
        Assert.AreEqual(0.0, result.Constants[0].Number); // start
        Assert.AreEqual(3.0, result.Constants[1].Number); // end
        Assert.AreEqual(1.0, result.Constants[2].Number); // default step
    }

    [TestMethod]
    public void Test_ForStatement_ExplicitStep()
    {
        // for i in 0 to 3 step 2 { print i }
        // 
        // Same structure as default step, but step constant is 2 instead of 1.
        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 10.0 },
            Step = new NumberNode { Value = 2.0 },
            Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [forStmt] });

        Assert.HasCount(17, result.Instructions);
        Assert.HasCount(3, result.Constants);

        // The step constant should be 2.0 instead of the default 1.0
        Assert.AreEqual(0.0, result.Constants[0].Number);  // start
        Assert.AreEqual(10.0, result.Constants[1].Number); // end
        Assert.AreEqual(2.0, result.Constants[2].Number);  // explicit step

        // Verify the step is stored in slot 2
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[5].Kind);
        Assert.AreEqual(2, result.Instructions[5].GetSafeIntOperand());

        // Backward jump and forward jump targets should be correct
        Assert.AreEqual(6, result.Instructions[16].GetSafeIntOperand());  // Jump back to loopStart
        Assert.AreEqual(17, result.Instructions[9].GetSafeIntOperand()); // JumpIfFalse to end
    }

    [TestMethod]
    public void Test_ForStatement_BlockBody()
    {
        // for i in 1 to 5 { let x -> i; print i }
        //
        // The body is a BlockStatement with two statements.
        // This verifies that the body's scope is managed correctly and that
        // the backward/forward jumps remain correct with a larger body.
        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 1.0 },
            End = new NumberNode { Value = 5.0 },
            Body = new BlockStatement
            {
                Body =
                [
                    new VariableDeclaration
                    {
                        Kind = "let",
                        Declarations =
                        [
                            new VariableDeclarator
                            {
                                VarName = new IdentifierNode { Name = "x" },
                                Init = new IdentifierNode { Name = "i" }
                            }
                        ]
                    },
                    new PrintStatement { Argument = new IdentifierNode { Name = "x" } }
                ]
            }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [forStmt] });

        // Verify the loop structure is intact:
        // The backward jump should target index 6 (loop condition start).
        var backwardJump = result.Instructions[^1];
        Assert.AreEqual(InstructionKind.Jump, backwardJump.Kind);
        Assert.AreEqual(6, backwardJump.GetSafeIntOperand());

        // The JumpIfFalse should exit past the last instruction.
        Assert.AreEqual(InstructionKind.JumpIfFalse, result.Instructions[9].Kind);
        Assert.AreEqual(result.Instructions.Count, result.Instructions[9].GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_ForStatement_NonIdentifierIterator_Throws()
    {
        // Using a NumberNode as the iterator should throw BytecodeError.
        var forStmt = new ForStatement
        {
            Iterator = new NumberNode { Value = 0.0 },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 5.0 },
            Body = new PrintStatement { Argument = new NumberNode { Value = 1.0 } }
        };

        var gen = new BytecodeGenerator();
        Assert.ThrowsExactly<BytecodeError>(() => gen.Generate(new Program { Body = [forStmt] }));
    }

    [TestMethod]
    public void Test_ForStatement_UniqueLocalSlots()
    {
        // Verify that the iterator, $end, and $step each get unique local slots
        // by checking the StoreVar operands in the initialization section.
        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 5.0 },
            Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
        };

        var result = new BytecodeGenerator().Generate(new Program { Body = [forStmt] });

        // Extract the three StoreVar slots from the initialization section (indices 1, 3, 5).
        var iterSlot = result.Instructions[1].GetSafeIntOperand();
        var endSlot = result.Instructions[3].GetSafeIntOperand();
        var stepSlot = result.Instructions[5].GetSafeIntOperand();

        // All three must be distinct.
        Assert.AreNotEqual(iterSlot, endSlot, "Iterator and $end share the same slot");
        Assert.AreNotEqual(iterSlot, stepSlot, "Iterator and $step share the same slot");
        Assert.AreNotEqual(endSlot, stepSlot, "$end and $step share the same slot");
    }

    [TestMethod]
    public void Test_Assignment_Expression()
    {
        // Declare x first, then assign to it: let x -> 10; x = 42;
        var varDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 10.0 }
                }
            ]
        };

        var assignmentExpression = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "=",
            Right = new NumberNode { Value = 42.0 }
        };

        var result = new BytecodeGenerator().Generate(new Program
        {
            Body =
            [
                varDecl,
                new ExpressionStatement { Expression = assignmentExpression }
            ]
        });

        // Expected bytecode:
        //   0: PushConst 10    (initialize x)
        //   1: StoreVar 0      (store in x)
        //   2: PushConst 42    (assignment value)
        //   3: StoreVar 0      (store in x)
        Assert.HasCount(4, result.Instructions);
        Assert.HasCount(2, result.Constants);

        // Initialization
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(0, result.Instructions[1].GetSafeIntOperand()); // slot 0 = x

        // Assignment
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[3].Kind);
        Assert.AreEqual(0, result.Instructions[3].GetSafeIntOperand()); // slot 0 = x

        // Constants: 10 and 42
        Assert.AreEqual(10.0, result.Constants[0].Number);
        Assert.AreEqual(42.0, result.Constants[1].Number);
    }

    [TestMethod]
    public void Test_Assignment_Expression_With_Binary_Operation()
    {
        // x = 5 + 3
        var varDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 0.0 }
                }
            ]
        };

        var assignmentExpression = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "=",
            Right = new BinaryExpression
            {
                Left = new NumberNode { Value = 5.0 },
                Operator = "+",
                Right = new NumberNode { Value = 3.0 }
            }
        };

        var result = new BytecodeGenerator().Generate(new Program
        {
            Body =
            [
                varDecl,
                new ExpressionStatement { Expression = assignmentExpression }
            ]
        });

        // Expected bytecode:
        //   0: PushConst 0     (initialize x)
        //   1: StoreVar 0      (store in x)
        //   2: PushConst 5     (left operand)
        //   3: PushConst 3     (right operand)
        //   4: Add             (compute 5 + 3)
        //   5: StoreVar 0      (store result in x)
        Assert.HasCount(6, result.Instructions);
        Assert.HasCount(3, result.Constants);

        // Initialization
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);

        // Assignment with binary operation
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[3].Kind);
        Assert.AreEqual(InstructionKind.Add, result.Instructions[4].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[5].Kind);
        Assert.AreEqual(0, result.Instructions[5].GetSafeIntOperand()); // slot 0 = x

        // Constants: 0, 5, 3
        Assert.AreEqual(0.0, result.Constants[0].Number);
        Assert.AreEqual(5.0, result.Constants[1].Number);
        Assert.AreEqual(3.0, result.Constants[2].Number);
    }

    [TestMethod]
    public void Test_Assignment_Expression_Undefined_Variable_Throws()
    {
        // x = 42 without declaring x first should throw
        var assignmentExpression = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "=",
            Right = new NumberNode { Value = 42.0 }
        };

        Assert.ThrowsExactly<BytecodeError>(() =>
            new BytecodeGenerator().Generate(new Program
            {
                Body = [new ExpressionStatement { Expression = assignmentExpression }]
            })
        );
    }

    [TestMethod]
    public void Test_ArrayIndex_EmitsCorrectInstructions()
    {
        // let x -> [1, 2]; x[0]
        // Expected bytecode:
        //   0: PushConst 1
        //   1: PushConst 2
        //   2: MakeArray 2
        //   3: StoreVar 0       (x)
        //   4: LoadVar 0        (x)
        //   5: PushConst 0      (index)
        //   6: IndexArray
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new ArrayLiteral
                    {
                        Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                    }
                }
            ]
        };

        var indexExpr = new IndexExpression
        {
            Object = new IdentifierNode { Name = "x" },
            Index = new NumberNode { Value = 0.0 }
        };

        var result = new BytecodeGenerator().Generate(new Program
        {
            Body = [decl, new ExpressionStatement { Expression = indexExpr }]
        });

        Assert.AreEqual(InstructionKind.MakeArray, result.Instructions[2].Kind);
        Assert.AreEqual(2, result.Instructions[2].GetSafeIntOperand());
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[3].Kind);
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[4].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[5].Kind);
        Assert.AreEqual(InstructionKind.IndexArray, result.Instructions[6].Kind);
    }

    [TestMethod]
    public void Test_ArrayIndex_InlineArrayLiteral_EmitsCorrectInstructions()
    {
        // [10, 20, 30][1]
        // Expected bytecode:
        //   0: PushConst 10
        //   1: PushConst 20
        //   2: PushConst 30
        //   3: MakeArray 3
        //   4: PushConst 1      (index)
        //   5: IndexArray
        var indexExpr = new IndexExpression
        {
            Object = new ArrayLiteral
            {
                Elements =
                [
                    new NumberNode { Value = 10.0 },
                    new NumberNode { Value = 20.0 },
                    new NumberNode { Value = 30.0 }
                ]
            },
            Index = new NumberNode { Value = 1.0 }
        };

        var result = new BytecodeGenerator().Generate(new Program
        {
            Body = [new ExpressionStatement { Expression = indexExpr }]
        });

        Assert.HasCount(6, result.Instructions);
        Assert.AreEqual(InstructionKind.MakeArray, result.Instructions[3].Kind);
        Assert.AreEqual(3, result.Instructions[3].GetSafeIntOperand());
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[4].Kind);
        Assert.AreEqual(InstructionKind.IndexArray, result.Instructions[5].Kind);
    }

    [TestMethod]
    public void Test_ArrayIndex_WithBinaryExpressionIndex_EmitsCorrectInstructions()
    {
        // let x -> [1, 2, 3]; x[1 + 1]
        // Index expression should emit: LoadVar(x), PushConst(1), PushConst(1), Add, IndexArray
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new ArrayLiteral
                    {
                        Elements =
                        [
                            new NumberNode { Value = 1.0 },
                            new NumberNode { Value = 2.0 },
                            new NumberNode { Value = 3.0 }
                        ]
                    }
                }
            ]
        };

        var indexExpr = new IndexExpression
        {
            Object = new IdentifierNode { Name = "x" },
            Index = new BinaryExpression
            {
                Left = new NumberNode { Value = 1.0 },
                Operator = "+",
                Right = new NumberNode { Value = 1.0 }
            }
        };

        var result = new BytecodeGenerator().Generate(new Program
        {
            Body = [decl, new ExpressionStatement { Expression = indexExpr }]
        });

        // After StoreVar(x): LoadVar(x), PushConst(1), PushConst(1), Add, IndexArray
        var loadVarIdx = result.Instructions
            .Select((ins, i) => (ins, i))
            .Last(t => t.ins.Kind == InstructionKind.StoreVar).i + 1;

        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[loadVarIdx].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[loadVarIdx + 1].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[loadVarIdx + 2].Kind);
        Assert.AreEqual(InstructionKind.Add, result.Instructions[loadVarIdx + 3].Kind);
        Assert.AreEqual(InstructionKind.IndexArray, result.Instructions[loadVarIdx + 4].Kind);
    }

    [TestMethod]
    public void Test_Struct_New_And_FieldAccess_Emit_Correct_Instructions()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "person" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new PrintStatement
                {
                    Argument = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "person" },
                        Property = new IdentifierNode { Name = "name" }
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        Assert.IsTrue(result.StructDefinitions.ContainsKey("Person"));
        Assert.HasCount(2, result.StructDefinitions["Person"]);
        Assert.AreEqual("name", result.StructDefinitions["Person"][0]);
        Assert.AreEqual("age", result.StructDefinitions["Person"][1]);

        Assert.AreEqual(InstructionKind.NewStruct, result.Instructions[0].Kind);
        Assert.AreEqual("Person", result.Instructions[0].GetSafeStringOperand());
        Assert.AreEqual(0, result.Instructions[0].GetSafeIntOperand());
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.LoadVar, result.Instructions[2].Kind);
        Assert.AreEqual(InstructionKind.LoadField, result.Instructions[3].Kind);
        Assert.AreEqual("name", result.Instructions[3].GetSafeStringOperand());
        Assert.AreEqual(InstructionKind.Print, result.Instructions[4].Kind);
    }

    [TestMethod]
    public void Test_Struct_Field_Assignment_Emits_StoreField()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new AssignmentExpression
                    {
                        Left = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "p" },
                            Property = new IdentifierNode { Name = "age" }
                        },
                        Operator = "=",
                        Right = new NumberNode { Value = 5.0 }
                    }
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);
        var storeField = result.Instructions.First(i => i.Kind == InstructionKind.StoreField);

        Assert.AreEqual("age", storeField.GetSafeStringOperand());
    }

    [TestMethod]
    public void Test_NewStruct_With_Arguments_Stores_ArgCount()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression
                            {
                                TypeName = new IdentifierNode { Name = "Person" },
                                Arguments = [new StringNode { Value = "Alice" }, new NumberNode { Value = 42.0 }]
                            }
                        }
                    ]
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);
        var ctor = result.Instructions.First(i => i.Kind == InstructionKind.NewStruct);

        Assert.AreEqual("Person", ctor.GetSafeStringOperand());
        Assert.AreEqual(2, ctor.GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_NewStruct_With_Field_Initializers_Materializes_Defaults_For_Missing_Arguments()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" }, Init = new StringNode { Value = "Unknown" } },
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.NewStruct, result.Instructions[2].Kind);
        Assert.AreEqual("Person", result.Instructions[2].GetSafeStringOperand());
        Assert.AreEqual(2, result.Instructions[2].GetSafeIntOperand());
    }

    [TestMethod]
    public void Test_NewStruct_With_Named_Arguments_Emits_Values_In_Field_Order()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Identifier = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Identifier = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression
                            {
                                TypeName = new IdentifierNode { Name = "Person" },
                                Arguments =
                                [
                                    new StructFieldInitializerArgument { Identifier = new IdentifierNode { Name = "age" }, Value = new NumberNode { Value = 5 } },
                                    new StructFieldInitializerArgument { Identifier = new IdentifierNode { Name = "name" }, Value = new StringNode { Value = "test" } }
                                ]
                            }
                        }
                    ]
                }
            ]
        };

        var result = new BytecodeGenerator().Generate(program);

        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[0].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.NewStruct, result.Instructions[2].Kind);
        Assert.AreEqual("Person", result.Instructions[2].GetSafeStringOperand());
        Assert.AreEqual(2, result.Instructions[2].GetSafeIntOperand());

        var firstConst = result.Constants[result.Instructions[0].GetSafeIntOperand()];
        var secondConst = result.Constants[result.Instructions[1].GetSafeIntOperand()];
        Assert.AreEqual(ConstantKind.String, firstConst.Kind);
        Assert.AreEqual("test", firstConst.String);
        Assert.AreEqual(ConstantKind.Number, secondConst.Kind);
        Assert.AreEqual(5d, secondConst.Number);
    }
}
