using Lumi.AST;
using Lumi.Bytecode.Constants;
using Lumi.Bytecode.Instructions;

namespace Lumi.Bytecode.Tests;

[TestClass]
public sealed class FunctionBytecodeTests
{
    [TestMethod]
    public void Test_FunctionDeclaration_EmitsJumpOverBody()
    {
        // fn greet() { print 1; }
        //
        // Expected layout:
        //   0: Jump -> 5            skip over body
        //   1: PushConst (1)        body: push 1
        //   2: Print                body: print
        //   3: PushConst (undef)    implicit return value
        //   4: Return
        //   5: <end>                normal execution resumes

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "greet" },
                Params = [],
                Body = new BlockStatement
                {
                    Body = [new PrintStatement { Argument = new NumberNode { Value = 1.0 } }]
                }
            });

        var result = Generate(program);

        Assert.HasCount(5, result.Instructions);

        // Skip-jump
        Assert.AreEqual(InstructionKind.Jump, result.Instructions[0].Kind);
        Assert.AreEqual(5, result.Instructions[0].GetSafeIntOperand());

        // Body
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.Print, result.Instructions[2].Kind);

        // Implicit return
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[3].Kind);
        Assert.AreEqual(InstructionKind.Return, result.Instructions[4].Kind);

        // The undefined constant must be present
        var undefIndex = result.Instructions[3].GetSafeIntOperand();
        Assert.AreEqual(ConstantKind.Undefined, result.Constants[undefIndex].Kind);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_RegistersFunctionAddress()
    {
        // fn foo() { print 1; }
        // The function address should point to instruction index 1 (right after the skip-jump).

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "foo" },
                Params = [],
                Body = new BlockStatement
                {
                    Body = [new PrintStatement { Argument = new NumberNode { Value = 1.0 } }]
                }
            });

        var result = Generate(program);

        Assert.IsTrue(result.FunctionAddresses.ContainsKey("foo"));
        Assert.AreEqual(1, result.FunctionAddresses["foo"]);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_RegistersFunctionDescriptor()
    {
        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "foo" },
                Params = [new IdentifierNode { Name = "value" }],
                Body = new BlockStatement { Body = [] }
            });

        var result = Generate(program);
        var functionId = result.FunctionDescriptorIds["foo"];
        var descriptor = result.FunctionDescriptors[functionId];

        Assert.AreEqual(functionId, descriptor.FunctionId);
        Assert.AreEqual("foo", descriptor.Name);
        Assert.AreEqual(1, descriptor.EntryPoint);
        Assert.AreEqual(1, descriptor.ParameterCount);
        Assert.IsNull(descriptor.ParentFunctionId);
        Assert.IsNull(descriptor.OwningStructName);
        Assert.IsFalse(descriptor.HasCaptures());
        Assert.HasCount(0, descriptor.CaptureNames);
        Assert.AreEqual(result.FunctionAddresses["foo"], descriptor.EntryPoint);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_WithParameters_EmitsStoreVars()
    {
        // fn add(a, b) { print a + b; }
        //
        // Expected layout:
        //   0: Jump -> 8            skip over body
        //   1: StoreVar (a)         pop arg into a  ← entry point
        //   2: StoreVar (b)         pop arg into b  (reverse order — b first, then a — wait, no)
        //
        // Actually parameters are stored right-to-left, so b is stored first then a.
        // Wait, the code stores from parameterLabels.Count-1 down to 0.
        // For params [a, b] with labels [0, 1]:
        //   emit StoreVar(labels[1]) = StoreVar(1)   — stores b
        //   emit StoreVar(labels[0]) = StoreVar(0)   — stores a
        //
        //   1: StoreVar 1           store b
        //   2: StoreVar 0           store a
        //   3: LoadVar 0            load a
        //   4: LoadVar 1            load b
        //   5: Add
        //   6: Print
        //   7: PushConst (undef)
        //   8: Return
        //   9: <end>

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "add" },
                Params =
                [
                    new IdentifierNode { Name = "a" },
                    new IdentifierNode { Name = "b" }
                ],
                Body = new BlockStatement
                {
                    Body =
                    [
                        new PrintStatement
                        {
                            Argument = new BinaryExpression
                            {
                                Left = new IdentifierNode { Name = "a" },
                                Operator = "+",
                                Right = new IdentifierNode { Name = "b" }
                            }
                        }
                    ]
                }
            });

        var result = Generate(program);

        // Skip-jump
        Assert.AreEqual(InstructionKind.Jump, result.Instructions[0].Kind);

        // Parameter stores (right-to-left: b first, then a)
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[1].Kind);
        Assert.AreEqual(InstructionKind.StoreVar, result.Instructions[2].Kind);
        var slotB = result.Instructions[1].GetSafeIntOperand();
        var slotA = result.Instructions[2].GetSafeIntOperand();
        Assert.AreNotEqual(slotA, slotB);

        // Function entry is right after the skip-jump
        Assert.AreEqual(1, result.FunctionAddresses["add"]);
    }

    [TestMethod]
    public void Test_CallExpression_EmitsCallFnWithName()
    {
        // fn noop() { } noop();
        //
        // The call site should emit a CallFn instruction with the function name.

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "noop" },
                Params = [],
                Body = new BlockStatement { Body = [] }
            },
            new ExpressionStatement
            {
                Expression = new CallExpression
                {
                    Callee = new IdentifierNode { Name = "noop" },
                    Arguments = []
                }
            });

        var result = Generate(program);

        // Find the CallFn instruction (it's after the function body).
        var callFn = result.Instructions.First(i => i.Kind == InstructionKind.CallFn);
        Assert.AreEqual("noop", callFn.StringOperand);
    }

    [TestMethod]
    public void Test_CallExpression_WithArguments_PushesBeforeCall()
    {
        // fn add(a, b) { print a + b; }
        // add(10, 20);
        //
        // At the call site we expect: PushConst(10), PushConst(20), CallFn "add"

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "add" },
                Params =
                [
                    new IdentifierNode { Name = "a" },
                    new IdentifierNode { Name = "b" }
                ],
                Body = new BlockStatement
                {
                    Body =
                    [
                        new PrintStatement
                        {
                            Argument = new BinaryExpression
                            {
                                Left = new IdentifierNode { Name = "a" },
                                Operator = "+",
                                Right = new IdentifierNode { Name = "b" }
                            }
                        }
                    ]
                }
            },
            new ExpressionStatement
            {
                Expression = new CallExpression
                {
                    Callee = new IdentifierNode { Name = "add" },
                    Arguments =
                    [
                        new NumberNode { Value = 10.0 },
                        new NumberNode { Value = 20.0 }
                    ]
                }
            });

        var result = Generate(program);

        // Locate the CallFn instruction.
        int callIndex = -1;
        for (int i = 0; i < result.Instructions.Count; i++)
        {
            if (result.Instructions[i].Kind == InstructionKind.CallFn)
            {
                callIndex = i;
                break;
            }
        }

        Assert.IsTrue(callIndex >= 2, "CallFn should be preceded by argument pushes");

        // The two instructions before CallFn should be PushConst for the arguments.
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[callIndex - 2].Kind);
        Assert.AreEqual(InstructionKind.PushConst, result.Instructions[callIndex - 1].Kind);

        // Verify argument constants (10 and 20).
        var arg1Index = result.Instructions[callIndex - 2].GetSafeIntOperand();
        var arg2Index = result.Instructions[callIndex - 1].GetSafeIntOperand();
        Assert.AreEqual(10.0, result.Constants[arg1Index].Number);
        Assert.AreEqual(20.0, result.Constants[arg2Index].Number);
    }

    [TestMethod]
    public void Test_MultipleFunctionDeclarations_RegisterSeparateAddresses()
    {
        // fn a() { print 1; }
        // fn b() { print 2; }

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "a" },
                Params = [],
                Body = new BlockStatement
                {
                    Body = [new PrintStatement { Argument = new NumberNode { Value = 1.0 } }]
                }
            },
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "b" },
                Params = [],
                Body = new BlockStatement
                {
                    Body = [new PrintStatement { Argument = new NumberNode { Value = 2.0 } }]
                }
            });

        var result = Generate(program);

        Assert.IsTrue(result.FunctionAddresses.ContainsKey("a"));
        Assert.IsTrue(result.FunctionAddresses.ContainsKey("b"));
        Assert.AreNotEqual(result.FunctionAddresses["a"], result.FunctionAddresses["b"]);
    }

    [TestMethod]
    public void Test_NestedFunctionDeclaration_RegistersParentDescriptor()
    {
        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "outer" },
                Params = [],
                Body = new BlockStatement
                {
                    Body =
                    [
                        new FunctionDeclaration
                        {
                            Id = new IdentifierNode { Name = "inner" },
                            Params = [],
                            Body = new BlockStatement { Body = [] }
                        }
                    ]
                }
            });

        var result = Generate(program);
        var outerId = result.FunctionDescriptorIds["outer"];
        var outerDescriptor = result.FunctionDescriptors[outerId];
        var innerDescriptor = result.FunctionDescriptors.Values.Single(d => d.Name == "inner");

        Assert.IsNull(outerDescriptor.ParentFunctionId);
        Assert.AreEqual(outerId, innerDescriptor.ParentFunctionId);
        Assert.IsNull(innerDescriptor.OwningStructName);
        Assert.IsFalse(innerDescriptor.HasCaptures());
        Assert.AreNotEqual(outerDescriptor.EntryPoint, innerDescriptor.EntryPoint);
    }

    [TestMethod]
    public void Test_NestedFunctionDeclaration_WithOuterReference_RegistersCaptures()
    {
        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "outer" },
                Params = [],
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
                                    VarName = new IdentifierNode { Name = "captured" },
                                    Init = new NumberNode { Value = 1.0 }
                                }
                            ]
                        },
                        new FunctionDeclaration
                        {
                            Id = new IdentifierNode { Name = "inner" },
                            Params = [],
                            Body = new BlockStatement
                            {
                                Body =
                                [
                                    new PrintStatement
                                    {
                                        Argument = new IdentifierNode { Name = "captured" }
                                    }
                                ]
                            }
                        }
                    ]
                }
            });

        var result = Generate(program);
        var innerDescriptor = result.FunctionDescriptors.Values.Single(d => d.Name == "inner");

        Assert.IsTrue(innerDescriptor.HasCaptures());
        CollectionAssert.AreEqual(new[] { "captured" }, innerDescriptor.CaptureNames.ToArray());
    }

    [TestMethod]
    public void Test_FunctionDeclaration_BodyDoesNotLeakLocals()
    {
        // fn foo() { let x -> 1; }
        // let y -> 2;
        //
        // 'x' is scoped to the function; 'y' should get its own slot.
        // After bytecode generation the function's scope is exited,
        // so referencing 'x' outside should fail.

        var gen = new BytecodeGenerator();

        Assert.ThrowsExactly<BytecodeError>(() =>
            gen.Generate(BuildProgram(
                new FunctionDeclaration
                {
                    Id = new IdentifierNode { Name = "foo" },
                    Params = [],
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
                                        Init = new NumberNode { Value = 1.0 }
                                    }
                                ]
                            }
                        ]
                    }
                },
                new PrintStatement { Argument = new IdentifierNode { Name = "x" } }
            )));
    }

    [TestMethod]
    public void Test_FunctionDeclaration_ImplicitReturn_HasUndefinedConstant()
    {
        // Every function body ends with PushConst(undefined) + Return.
        // Verify the constant pool contains an Undefined entry.

        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "empty" },
                Params = [],
                Body = new BlockStatement { Body = [] }
            });

        var result = Generate(program);

        Assert.IsTrue(result.Constants.Any(c => c.Kind == ConstantKind.Undefined));

        // Last two instructions must be PushConst + Return.
        var last = result.Instructions[^1];
        var secondLast = result.Instructions[^2];
        Assert.AreEqual(InstructionKind.Return, last.Kind);
        Assert.AreEqual(InstructionKind.PushConst, secondLast.Kind);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_NonIdentifierId_Throws()
    {
        // Using a NumberNode as the function id should throw.
        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new NumberNode { Value = 0.0 },
                Params = [],
                Body = new BlockStatement { Body = [] }
            });

        Assert.ThrowsExactly<BytecodeError>(() => Generate(program));
    }

    [TestMethod]
    public void Test_FunctionDeclaration_NonIdentifierParam_Throws()
    {
        // Using a NumberNode as a parameter should throw.
        var program = BuildProgram(
            new FunctionDeclaration
            {
                Id = new IdentifierNode { Name = "bad" },
                Params = [new NumberNode { Value = 0.0 }],
                Body = new BlockStatement { Body = [] }
            });

        Assert.ThrowsExactly<BytecodeError>(() => Generate(program));
    }

    // ── helpers ──────────────────────────────────────────────────────

    private static Program BuildProgram(params Node[] body)
        => new() { Body = [.. body] };

    private static BytecodeResult Generate(Program program)
        => new BytecodeGenerator().Generate(program);
}
