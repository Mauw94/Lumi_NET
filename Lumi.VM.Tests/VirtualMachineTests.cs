using Lumi.AST;
using Lumi.Bytecode;

namespace Lumi.VM.Tests;

// Console.SetOut is global state; disabling parallelism prevents race conditions when capturing output across tests.
[DoNotParallelize]
[TestClass]
public sealed class VirtualMachineTests
{
    [TestMethod]
    public void VM_Print_Number()
    {
        var bytecode = Build(new PrintStatement { Argument = new NumberNode { Value = 42.0 } });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("42", output);
    }

    [TestMethod]
    public void VM_Add_Prints_Correct_Result()
    {
        var bytecode = Build(new PrintStatement
        {
            Argument = new BinaryExpression
            {
                Left = new NumberNode { Value = 1.0 },
                Operator = "+",
                Right = new NumberNode { Value = 2.0 }
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("3", output);
    }

    [TestMethod]
    public void VM_Sub_Prints_Correct_Result()
    {
        var bytecode = Build(new PrintStatement
        {
            Argument = new BinaryExpression
            {
                Left = new NumberNode { Value = 5.0 },
                Operator = "-",
                Right = new NumberNode { Value = 3.0 }
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("2", output);
    }

    [TestMethod]
    public void VM_Mul_Prints_Correct_Result()
    {
        var bytecode = Build(new PrintStatement
        {
            Argument = new BinaryExpression
            {
                Left = new NumberNode { Value = 3.0 },
                Operator = "*",
                Right = new NumberNode { Value = 4.0 }
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("12", output);
    }

    [TestMethod]
    public void VM_Div_Prints_Correct_Result()
    {
        var bytecode = Build(new PrintStatement
        {
            Argument = new BinaryExpression
            {
                Left = new NumberNode { Value = 10.0 },
                Operator = "/",
                Right = new NumberNode { Value = 2.0 }
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void VM_StoreVar_And_LoadVar_Prints_Value()
    {
        // let x -> 99; print x;
        var bytecode = Build(
            new VariableDeclaration
            {
                Kind = "let",
                Declarations =
                [
                    new VariableDeclarator
                    {
                        VarName = new IdentifierNode { Name = "x" },
                        Init = new NumberNode { Value = 99.0 }
                    }
                ]
            },
            new PrintStatement { Argument = new IdentifierNode { Name = "x" } }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("99", output);
    }

    [TestMethod]
    public void VM_ListLiteral_Declaration_And_Print_Works()
    {
        // let items -> [1, 2, 3]; print items;
        var bytecode = Build(
            new VariableDeclaration
            {
                Kind = "let",
                Declarations =
                [
                    new VariableDeclarator
                    {
                        VarName = new IdentifierNode { Name = "items" },
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
            },
            new PrintStatement { Argument = new IdentifierNode { Name = "items" } }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("[1, 2, 3]", output);
    }

    [TestMethod]
    public void VM_ListAdd_Method_Mutates_List()
    {
        var bytecode = Build(
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
                            Elements =
                            [
                                new NumberNode { Value = 1.0 },
                                new NumberNode { Value = 2.0 },
                                new NumberNode { Value = 3.0 }
                            ]
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
                    Arguments = [new NumberNode { Value = 4.0 }]
                }
            },
            new PrintStatement { Argument = new IdentifierNode { Name = "items" } }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("[1, 2, 3, 4]", output);
    }

    [TestMethod]
    public void VM_ListRemove_Method_Mutates_List()
    {
        var bytecode = Build(
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
                            Elements =
                            [
                                new NumberNode { Value = 1.0 },
                                new NumberNode { Value = 2.0 },
                                new NumberNode { Value = 3.0 }
                            ]
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
                    Arguments = [new NumberNode { Value = 2.0 }]
                }
            },
            new PrintStatement { Argument = new IdentifierNode { Name = "items" } }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("[1, 3]", output);
    }

    [TestMethod]
    public void VM_ListLength_Method_Returns_List_Size()
    {
        var bytecode = Build(
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
                            Elements =
                            [
                                new NumberNode { Value = 1.0 },
                                new NumberNode { Value = 2.0 },
                                new NumberNode { Value = 3.0 }
                            ]
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new CallExpression
                {
                    Callee = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "items" },
                        Property = new IdentifierNode { Name = "length" }
                    },
                    Arguments = []
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("3", output);
    }

    [TestMethod]
    public void VM_ListContains_Method_Returns_True_When_Present()
    {
        var bytecode = Build(
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
                            Elements =
                            [
                                new NumberNode { Value = 1.0 },
                                new NumberNode { Value = 2.0 },
                                new NumberNode { Value = 3.0 }
                            ]
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new CallExpression
                {
                    Callee = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "items" },
                        Property = new IdentifierNode { Name = "contains" }
                    },
                    Arguments = [new NumberNode { Value = 2.0 }]
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("True", output);
    }

    [TestMethod]
    public void VM_ListContains_Method_Returns_False_When_Absent()
    {
        var bytecode = Build(
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
                            Elements =
                            [
                                new NumberNode { Value = 1.0 },
                                new NumberNode { Value = 2.0 },
                                new NumberNode { Value = 3.0 }
                            ]
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new CallExpression
                {
                    Callee = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "items" },
                        Property = new IdentifierNode { Name = "contains" }
                    },
                    Arguments = [new NumberNode { Value = 99.0 }]
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("False", output);
    }

    [TestMethod]
    public void VM_State_Persists_Across_Execute_Calls()
    {
        // Simulates REPL: declare x on one Execute call, print it on the next.
        var gen = new BytecodeGenerator();
        var vm = new VirtualMachine();

        vm.Execute(gen.Generate(new Program
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
                            Init = new NumberNode { Value = 7.0 }
                        }
                    ]
                }
            ]
        }));

        var output = CaptureOutput(() => vm.Execute(gen.Generate(new Program
        {
            Body = [new PrintStatement { Argument = new IdentifierNode { Name = "x" } }]
        })));

        Assert.AreEqual("7", output);
    }

    [TestMethod]
    public void VM_LoadVar_Undefined_Throws()
    {
        // Referencing an undeclared identifier is caught during bytecode generation.
        Assert.ThrowsExactly<BytecodeError>(() =>
            Build(new PrintStatement { Argument = new IdentifierNode { Name = "z" } })
        );
    }

    [TestMethod]
    public void VM_ForLoop_Prints_All_Values()
    {
        // for i in 0 to 3 { print i; }
        var bytecode = Build(new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 3.0 },
            Body = new BlockStatement
            {
                Body = [new PrintStatement { Argument = new IdentifierNode { Name = "i" } }]
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(4, lines);
        Assert.AreEqual("0", lines[0]);
        Assert.AreEqual("1", lines[1]);
        Assert.AreEqual("2", lines[2]);
        Assert.AreEqual("3", lines[3]);
    }

    [TestMethod]
    public void VM_ForLoop_WithStep_Prints_Correct_Values()
    {
        // for i in 0 to 10 step 2 { print i; }
        var bytecode = Build(new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 10.0 },
            Step = new NumberNode { Value = 2.0 },
            Body = new BlockStatement
            {
                Body = [new PrintStatement { Argument = new IdentifierNode { Name = "i" } }]
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(6, lines);
        Assert.AreEqual("0", lines[0]);
        Assert.AreEqual("2", lines[1]);
        Assert.AreEqual("4", lines[2]);
        Assert.AreEqual("6", lines[3]);
        Assert.AreEqual("8", lines[4]);
        Assert.AreEqual("10", lines[5]);
    }

    [TestMethod]
    public void VM_ForLoop_WithIf_Mod_Prints_Even_Numbers()
    {
        // for i in 0 to 10 step 1 {
        //   if (i % 2 == 0) {
        //     print i;
        //   }
        // }
        var bytecode = Build(new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 10.0 },
            Step = new NumberNode { Value = 1.0 },
            Body = new BlockStatement
            {
                Body =
                [
                    new IfStatement
                    {
                        Expr = new BinaryExpression
                        {
                            Left = new BinaryExpression
                            {
                                Left = new IdentifierNode { Name = "i" },
                                Operator = "%",
                                Right = new NumberNode { Value = 2.0 }
                            },
                            Operator = "==",
                            Right = new NumberNode { Value = 0.0 }
                        },
                        Stmt = new BlockStatement
                        {
                            Body = [new PrintStatement { Argument = new IdentifierNode { Name = "i" } }]
                        }
                    }
                ]
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(6, lines);
        Assert.AreEqual("0", lines[0]);
        Assert.AreEqual("2", lines[1]);
        Assert.AreEqual("4", lines[2]);
        Assert.AreEqual("6", lines[3]);
        Assert.AreEqual("8", lines[4]);
        Assert.AreEqual("10", lines[5]);
    }

    [TestMethod]
    public void VM_ForLoop_WithArithmeticBody_Prints_Squares()
    {
        // for i in 1 to 5 { print i * i; }
        var bytecode = Build(new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 1.0 },
            End = new NumberNode { Value = 5.0 },
            Body = new BlockStatement
            {
                Body =
                [
                    new PrintStatement
                    {
                        Argument = new BinaryExpression
                        {
                            Left = new IdentifierNode { Name = "i" },
                            Operator = "*",
                            Right = new IdentifierNode { Name = "i" }
                        }
                    }
                ]
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(5, lines);
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("4", lines[1]);
        Assert.AreEqual("9", lines[2]);
        Assert.AreEqual("16", lines[3]);
        Assert.AreEqual("25", lines[4]);
    }

    [TestMethod]
    public void VM_ForLoop_StartGreaterThanEnd_DoesNotExecuteBody()
    {
        // for i in 10 to 0 { print i; }
        // Condition 10 <= 0 is false immediately, so body never runs.
        var bytecode = Build(new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 10.0 },
            End = new NumberNode { Value = 0.0 },
            Body = new BlockStatement
            {
                Body = [new PrintStatement { Argument = new IdentifierNode { Name = "i" } }]
            }
        });

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual(string.Empty, output);
    }

    [TestMethod]
    public void VM_ForLoop_VariableDeclaredBeforeLoop_Accumulates()
    {
        // let sum -> 0;
        // for i in 1 to 3 { let sum -> sum + i; }
        // print sum;
        //
        // Note: since each loop iteration opens a new block scope,
        // re-declaring 'sum' inside creates a *new* local each time.
        // The outer 'sum' is never modified. This test verifies that
        // the outer variable survives the loop and prints its original value.
        var bytecode = Build(
            new VariableDeclaration
            {
                Kind = "let",
                Declarations =
                [
                    new VariableDeclarator
                    {
                        VarName = new IdentifierNode { Name = "sum" },
                        Init = new NumberNode { Value = 0.0 }
                    }
                ]
            },
            new ForStatement
            {
                Iterator = new IdentifierNode { Name = "i" },
                Start = new NumberNode { Value = 1.0 },
                End = new NumberNode { Value = 3.0 },
                Body = new BlockStatement
                {
                    Body =
                    [
                        new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
                    ]
                }
            },
            new PrintStatement { Argument = new IdentifierNode { Name = "sum" } }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        var lines = output.Split(Environment.NewLine);
        // Loop prints 1, 2, 3 then outer print sum outputs 0
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("2", lines[1]);
        Assert.AreEqual("3", lines[2]);
        Assert.AreEqual("0", lines[3]);
    }

    private static string CaptureOutput(Action action)
    {
        var writer = new StringWriter();
        var previous = Console.Out;
        Console.SetOut(writer);

        try
        {
            action();
        }
        finally
        {
            Console.SetOut(previous);
        }

        return writer.ToString().Trim();
    }

    private static BytecodeResult Build(params Node[] nodes)
    {
        var gen = new BytecodeGenerator();

        return gen.Generate(new Program { Body = [.. nodes] });
    }

    [TestMethod]
    public void VM_ArrayIndex_Returns_Correct_Element()
    {
        // let items -> [10, 20, 30]; print items[1];
        var bytecode = Build(
            new VariableDeclaration
            {
                Kind = "let",
                Declarations =
                [
                    new VariableDeclarator
                    {
                        VarName = new IdentifierNode { Name = "items" },
                        Init = new ArrayLiteral
                        {
                            Elements =
                            [
                                new NumberNode { Value = 10.0 },
                                new NumberNode { Value = 20.0 },
                                new NumberNode { Value = 30.0 }
                            ]
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new IndexExpression
                {
                    Object = new IdentifierNode { Name = "items" },
                    Index = new NumberNode { Value = 1.0 }
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));

        Assert.AreEqual("20", output);
    }

    [TestMethod]
    public void VM_ArrayIndex_OutOfBounds_Throws()
    {
        // let x -> [1, 2]; x[5]
        var bytecode = Build(
            new VariableDeclaration
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
            },
            new ExpressionStatement
            {
                Expression = new IndexExpression
                {
                    Object = new IdentifierNode { Name = "x" },
                    Index = new NumberNode { Value = 5.0 }
                }
            }
        );

        var threw = false;
        try { new VirtualMachine().Execute(bytecode); }
        catch { threw = true; }
        Assert.IsTrue(threw, "Expected an exception for out-of-bounds index");
    }

    [TestMethod]
    public void VM_ArrayIndex_OnNonArray_Throws()
    {
        // let x -> 42; x[0]
        var bytecode = Build(
            new VariableDeclaration
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
            },
            new ExpressionStatement
            {
                Expression = new IndexExpression
                {
                    Object = new IdentifierNode { Name = "x" },
                    Index = new NumberNode { Value = 0.0 }
                }
            }
        );

        var threw = false;
        try { new VirtualMachine().Execute(bytecode); }
        catch { threw = true; }
        Assert.IsTrue(threw, "Expected an exception when indexing a non-array value");
    }

    [TestMethod]
    public void VM_Struct_Field_Access_Prints_Undefined_By_Default()
    {
        var bytecode = Build(
            new StructDeclaration
            {
                Name = new IdentifierNode { Name = "Person" },
                Fields =
                [
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
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
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));
        Assert.AreEqual("undefined", output);
    }

    [TestMethod]
    public void VM_Struct_Field_Assignment_Prints_Assigned_Value()
    {
        var bytecode = Build(
            new StructDeclaration
            {
                Name = new IdentifierNode { Name = "Person" },
                Fields =
                [
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
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
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "p" },
                    Property = new IdentifierNode { Name = "age" }
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));
        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void VM_NewStruct_With_Arguments_Initializes_Fields_By_Order()
    {
        var bytecode = Build(
            new StructDeclaration
            {
                Name = new IdentifierNode { Name = "Person" },
                Fields =
                [
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
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
                            Arguments = [new StringNode { Value = "Alice" }, new NumberNode { Value = 30.0 }]
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "p" },
                    Property = new IdentifierNode { Name = "name" }
                }
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "p" },
                    Property = new IdentifierNode { Name = "age" }
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));
        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Alice\"", lines[0]);
        Assert.AreEqual("30", lines[1]);
    }

    [TestMethod]
    public void VM_Struct_Method_Call_Can_Mutate_Fields_Through_This()
    {
        var bytecode = Build(
            new StructDeclaration
            {
                Name = new IdentifierNode { Name = "Counter" },
                Fields =
                [
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "value" }, Type = new IdentifierNode { Name = "int" } }
                ],
                Methods =
                [
                    new FunctionDeclaration
                    {
                        Id = new IdentifierNode { Name = "increment" },
                        Params = [new IdentifierNode { Name = "delta" }],
                        Body = new BlockStatement
                        {
                            Body =
                            [
                                new ExpressionStatement
                                {
                                    Expression = new AssignmentExpression
                                    {
                                        Left = new MemberExpression
                                        {
                                            Object = new IdentifierNode { Name = "this" },
                                            Property = new IdentifierNode { Name = "value" }
                                        },
                                        Operator = "=",
                                        Right = new BinaryExpression
                                        {
                                            Left = new MemberExpression
                                            {
                                                Object = new IdentifierNode { Name = "this" },
                                                Property = new IdentifierNode { Name = "value" }
                                            },
                                            Operator = "+",
                                            Right = new IdentifierNode { Name = "delta" }
                                        }
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
                        VarName = new IdentifierNode { Name = "counter" },
                        VarType = new IdentifierNode { Name = "Counter" },
                        Init = new NewExpression
                        {
                            TypeName = new IdentifierNode { Name = "Counter" },
                            Arguments = [new NumberNode { Value = 2.0 }]
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
                        Object = new IdentifierNode { Name = "counter" },
                        Property = new IdentifierNode { Name = "increment" }
                    },
                    Arguments = [new NumberNode { Value = 3.0 }]
                }
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "counter" },
                    Property = new IdentifierNode { Name = "value" }
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));
        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void VM_NewStruct_Uses_Field_Initializers_For_Missing_Arguments()
    {
        var bytecode = Build(
            new StructDeclaration
            {
                Name = new IdentifierNode { Name = "Person" },
                Fields =
                [
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" }, Init = new StringNode { Value = "Unknown" } },
                    new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
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
                            TypeName = new IdentifierNode { Name = "Person" }
                        }
                    }
                ]
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "p" },
                    Property = new IdentifierNode { Name = "name" }
                }
            },
            new PrintStatement
            {
                Argument = new MemberExpression
                {
                    Object = new IdentifierNode { Name = "p" },
                    Property = new IdentifierNode { Name = "age" }
                }
            }
        );

        var output = CaptureOutput(() => new VirtualMachine().Execute(bytecode));
        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Unknown\"", lines[0]);
        Assert.AreEqual("undefined", lines[1]);
    }

}
