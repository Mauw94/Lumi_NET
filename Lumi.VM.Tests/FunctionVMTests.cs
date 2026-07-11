using Lumi.AST;
using Lumi.Bytecode;

namespace Lumi.VM.Tests;

// Console.SetOut is global state; disabling parallelism prevents race conditions when capturing output across tests.
[DoNotParallelize]
[TestClass]
public sealed class FunctionVMTests
{
    [TestMethod]
    public void VM_Function_NoParams_PrintsValue()
    {
        // fn greet() { print 42; }
        // greet();
        var output = RunAndCapture(
            Fn("greet", [], Print(Num(42))),
            Call("greet"));

        Assert.AreEqual("42", output);
    }

    [TestMethod]
    public void VM_Function_SingleParam_PrintsDoubled()
    {
        // fn double(x) { print x * 2; }
        // double(7);
        var output = RunAndCapture(
            Fn("double", ["x"],
                Print(Bin(Id("x"), "*", Num(2)))),
            Call("double", Num(7)));

        Assert.AreEqual("14", output);
    }

    [TestMethod]
    public void VM_Function_TwoParams_PrintsSum()
    {
        // fn add(a, b) { print a + b; }
        // add(3, 5);
        var output = RunAndCapture(
            Fn("add", ["a", "b"],
                Print(Bin(Id("a"), "+", Id("b")))),
            Call("add", Num(3), Num(5)));

        Assert.AreEqual("8", output);
    }

    [TestMethod]
    public void VM_Function_ThreeParams_PrintsProduct()
    {
        // fn mul3(x, y, z) { print x * y * z; }
        // mul3(2, 3, 4);
        var output = RunAndCapture(
            Fn("mul3", ["x", "y", "z"],
                Print(Bin(Bin(Id("x"), "*", Id("y")), "*", Id("z")))),
            Call("mul3", Num(2), Num(3), Num(4)));

        Assert.AreEqual("24", output);
    }

    [TestMethod]
    public void VM_Function_CalledMultipleTimes_PrintsEachResult()
    {
        // fn square(n) { print n * n; }
        // square(3); square(5); square(10);
        var output = RunAndCapture(
            Fn("square", ["n"],
                Print(Bin(Id("n"), "*", Id("n")))),
            Call("square", Num(3)),
            Call("square", Num(5)),
            Call("square", Num(10)));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(3, lines);
        Assert.AreEqual("9", lines[0]);
        Assert.AreEqual("25", lines[1]);
        Assert.AreEqual("100", lines[2]);
    }

    [TestMethod]
    public void VM_Function_WithLocalVariable()
    {
        // fn compute(x) { let r -> x + 10; print r; }
        // compute(5);
        var output = RunAndCapture(
            Fn("compute", ["x"],
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = Id("r"),
                            Init = Bin(Id("x"), "+", Num(10))
                        }
                    ]
                },
                Print(Id("r"))),
            Call("compute", Num(5)));

        Assert.AreEqual("15", output);
    }

    [TestMethod]
    public void VM_Function_WithIfStatement()
    {
        // fn sign(n) { if (n > 0) { print 1; } else { print 0; } }
        // sign(5); sign(-3);
        var output = RunAndCapture(
            Fn("sign", ["n"],
                new IfStatement
                {
                    Expr = Bin(Id("n"), ">", Num(0)),
                    Stmt = new BlockStatement { Body = [Print(Num(1))] },
                    ElsePart = new BlockStatement { Body = [Print(Num(0))] }
                }),
            Call("sign", Num(5)),
            Call("sign", Num(-3)));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(2, lines);
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("0", lines[1]);
    }

    [TestMethod]
    public void VM_Function_NestedCalls()
    {
        // fn inner() { print 2; }
        // fn outer() { print 1; inner(); }
        // outer();
        var output = RunAndCapture(
            Fn("inner", [], Print(Num(2))),
            Fn("outer", [], Print(Num(1)),
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = Id("inner"),
                        Arguments = []
                    }
                }),
            Call("outer"));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(2, lines);
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("2", lines[1]);
    }

    [TestMethod]
    public void VM_Function_LocalClosure_Call_UsesCapturedValue()
    {
        var output = RunAndCapture(
            Fn("outer", [],
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = Id("captured"),
                            Init = Num(1)
                        }
                    ]
                },
                Fn("inner", [], Print(Id("captured"))),
                new ExpressionStatement
                {
                    Expression = new AssignmentExpression
                    {
                        Left = Id("captured"),
                        Operator = "=",
                        Right = Num(2)
                    }
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = Id("inner"),
                        Arguments = []
                    }
                }),
            Call("outer"));

        Assert.AreEqual("2", output);
    }

    [TestMethod]
    public void VM_Function_ReturnedClosure_SeesLiveCapturedMutation()
    {
        var output = RunAndCapture(
            Fn("outer", [],
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = Id("captured"),
                            Init = Num(1)
                        }
                    ]
                },
                Fn("inner", [], Print(Id("captured"))),
                new ExpressionStatement
                {
                    Expression = new AssignmentExpression
                    {
                        Left = Id("captured"),
                        Operator = "=",
                        Right = Num(2)
                    }
                },
                new ReturnStatement
                {
                    Argument = Id("inner")
                }),
            new VariableDeclaration
            {
                Kind = "let",
                Declarations =
                [
                    new VariableDeclarator
                    {
                        VarName = Id("printer"),
                        Init = new CallExpression
                        {
                            Callee = Id("outer"),
                            Arguments = []
                        }
                    }
                ]
            },
            new ExpressionStatement
            {
                Expression = new CallExpression
                {
                    Callee = Id("printer"),
                    Arguments = []
                }
            });

        Assert.AreEqual("2", output);
    }

    [TestMethod]
    public void VM_Function_MultipleFunctions_CalledInOrder()
    {
        // fn a() { print 10; }
        // fn b() { print 20; }
        // b(); a();
        var output = RunAndCapture(
            Fn("a", [], Print(Num(10))),
            Fn("b", [], Print(Num(20))),
            Call("b"),
            Call("a"));

        var lines = output.Split(Environment.NewLine);
        Assert.HasCount(2, lines);
        Assert.AreEqual("20", lines[0]);
        Assert.AreEqual("10", lines[1]);
    }

    [TestMethod]
    public void VM_Function_UndefinedFunction_Throws()
    {
        // Calling a function that was never declared should throw at runtime.
        var bytecode = Build(Call("missing"));
        var vm = new VirtualMachine();

        var threw = false;
        try
        {
            vm.Execute(bytecode);
        }
        catch
        {
            threw = true;
        }
        Assert.IsTrue(threw, "Expected an exception for calling an undefined function");
    }

    // ── AST builder helpers ─────────────────────────────────────────

    private static IdentifierNode Id(string name) => new() { Name = name };
    private static NumberNode Num(double v) => new() { Value = v };

    private static BinaryExpression Bin(Node left, string op, Node right)
        => new() { Left = left, Operator = op, Right = right };

    private static PrintStatement Print(Node arg) => new() { Argument = arg };

    private static FunctionDeclaration Fn(string name, string[] parameters, params Node[] bodyStatements)
        => new()
        {
            Id = Id(name),
            Params = [.. parameters.Select(Id)],
            Body = new BlockStatement { Body = [.. bodyStatements] }
        };

    private static ExpressionStatement Call(string name, params Node[] args)
        => new()
        {
            Expression = new CallExpression
            {
                Callee = Id(name),
                Arguments = [.. args]
            }
        };

    private static BytecodeResult Build(params Node[] nodes)
        => new BytecodeGenerator().Generate(new Program { Body = [.. nodes] });

    private static string RunAndCapture(params Node[] nodes)
    {
        var bytecode = Build(nodes);
        return CaptureOutput(() => new VirtualMachine().Execute(bytecode));
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
}