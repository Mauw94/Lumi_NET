using Lumi.Ast;
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

}