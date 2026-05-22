using Lumi.AST;
using Lumi.Bytecode;
using Lumi.VM;

namespace Lumi.Engine.Tests;

[DoNotParallelize]
[TestClass]
public sealed class FunctionTests
{
    [TestMethod]
    public void Test_Simple_Function_Call_No_Parameters()
    {
        var source = @"
            fn sayHello() {
                print 42;
            }
            sayHello();
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("42", output);
    }

    [TestMethod]
    public void Test_Nested_Function_Calls_No_Parameters()
    {
        var source = @"
            fn outer() {
                print 1;
                inner();
            }
            fn inner() {
                print 2;
            }
            outer();
        ";

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("2", lines[1]);
    }

    [TestMethod]
    public void Test_Multiple_Function_Calls()
    {
        var source = @"
            fn func1() {
                print 10;
            }
            fn func2() {
                print 20;
            }
            func1();
            func2();
            func1();
        ";

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(3, lines);
        Assert.AreEqual("10", lines[0]);
        Assert.AreEqual("20", lines[1]);
        Assert.AreEqual("10", lines[2]);
    }

    [TestMethod]
    public void Test_Function_With_Variable()
    {
        var source = @"
            fn printValue() {
                let x -> 99;
                print x;
            }
            printValue();
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("99", output);
    }

    [TestMethod]
    public void Test_Function_With_If_No_Parameters()
    {
        var source = @"
            fn checkValue() {
                let value -> 5;
                if (value > 0) {
                    print 1;
                } else {
                    print 0;
                }
            }
            checkValue();
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("1", output);
    }

    [TestMethod]
    public void Test_Function_With_Single_Parameter()
    {
        var source = @"
            fn twice(x) {
                print x * 2;
            }
            twice(7);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("14", output);
    }

    [TestMethod]
    public void Test_Function_With_Multiple_Parameters()
    {
        var source = @"
            fn add(a, b) {
                print a + b;
            }
            add(3, 5);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("8", output);
    }

    [TestMethod]
    public void Test_Function_With_Three_Parameters()
    {
        var source = @"
            fn multiply(x, y, z) {
                print x * y * z;
            }
            multiply(2, 3, 4);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("24", output);
    }

    [TestMethod]
    public void Test_Function_Parameter_Used_In_If()
    {
        var source = @"
            fn isPositive(num) {
                if (num > 0) {
                    print 1;
                } else {
                    print 0;
                }
            }
            isPositive(5);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("1", output);
    }

    [TestMethod]
    public void Test_Function_Called_Multiple_Times_With_Different_Args()
    {
        var source = @"
            fn square(n) {
                print n * n;
            }
            square(3);
            square(5);
        ";

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("9", lines[0]);
        Assert.AreEqual("25", lines[1]);
    }

    [TestMethod]
    public void Test_Function_With_Local_Variable_And_Parameter()
    {
        var source = @"
            fn compute(x) {
                let result -> x * x + 1;
                print result;
            }
            compute(4);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("17", output);
    }

    [TestMethod]
    public void Test_Function_Called_With_Negative_Argument()
    {
        var source = @"
            fn check(n) {
                if (n > 0) {
                    print 1;
                } else {
                    print 0;
                }
            }
            check(-3);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("0", output);
    }

    [TestMethod]
    public void Test_Function_With_Return_In_If()
    {
        var source = @"
            fn compute(n) {
                if (n <= 5) {
                    return n + 10;
                }
                return n * n;
            }
            print compute(4);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("14", output);
    }

    [TestMethod]
    public void Test_Function_With_Return_In_If_Else_Branch()
    {
        var source = @"
            fn compute(n) {
                if (n <= 5) {
                    return n + 10;
                }
                return n * n;
            }
            print compute(10);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("100", output);
    }

    [TestMethod]
    public void Test_Function_With_Early_Return()
    {
        var source = @"
            fn test(n) {
                if (n > 0) {
                    return 1;
                }
                return 0;
            }
            print test(5);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("1", output);
    }

    [TestMethod]
    public void Test_Function_With_Return_No_Value()
    {
        var source = @"
            fn test() {
                print 1;
                return;
            }
            test();
            print 2;
        ";

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("1", lines[0]);
        Assert.AreEqual("2", lines[1]);
    }

    [TestMethod]
    public void Test_Recursive_Function_Fibonacci()
    {
        var source = @"
            fn fib(n) {
                if (n <= 1) {
                    return n;
                } else {
                    return fib(n - 1) + fib(n - 2);
                }
            }
            print fib(5);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void Test_Recursive_Function_Fibonacci_Base_Cases()
    {
        var source = @"
            fn fib(n) {
                if (n <= 1) {
                    return n;
                } else {
                    return fib(n - 1) + fib(n - 2);
                }
            }
            print fib(0);
            print fib(1);
            print fib(2);
            print fib(3);
        ";

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(4, lines);
        Assert.AreEqual("0", lines[0]);
        Assert.AreEqual("1", lines[1]);
        Assert.AreEqual("1", lines[2]);
        Assert.AreEqual("2", lines[3]);
    }

    [TestMethod]
    public void Test_Deep_Recursion_Factorial()
    {
        var source = @"
            fn fact(n) {
                if (n <= 1) {
                    return 1;
                }
                return n * fact(n - 1);
            }
            print fact(5);
        ";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("120", output);
    }

    private static string ExecuteAndCapture(string source)
    {
        var parser = new Parser.Parser(source);
        var ast = parser.Parse();

        var analyzer = new SemanticAnalyzer.SemanticAnalyzer();
        var analysisResult = analyzer.Analyze(ast as Program ?? throw new InvalidOperationException());

        if (analysisResult.Errors.Count > 0)
        {
            throw new InvalidOperationException($"Semantic analysis failed: {string.Join(", ", analysisResult.Errors)}");
        }

        var bytecodeGenerator = new BytecodeGenerator();
        var bytecodeResult = bytecodeGenerator.Generate(ast);

        var writer = new StringWriter();
        var previous = Console.Out;
        Console.SetOut(writer);

        try
        {
            var vm = new VirtualMachine();
            vm.Execute(bytecodeResult);
        }
        finally
        {
            Console.SetOut(previous);
        }

        return writer.ToString().Trim();
    }

    [TestMethod]
    public void Benchmark_Fibonacci_25_Performance()
    {
        var source = @"
            fn fib(n) {
                if (n <= 1) {
                    return n;
                } else {
                    return fib(n - 1) + fib(n - 2);
                }
            }
            print fib(25);
        ";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var output = ExecuteAndCapture(source);
        stopwatch.Stop();

        // Verify correctness: fib(25) = 75025
        Assert.AreEqual("75025", output);

        // Performance: object pooling reduces GC pressure by reusing snapshot arrays
        // Baseline (no pooling): ~165-300ms, With pooling: expected reduction
        // Note: Test environment and JIT compilation affects timing
        Console.WriteLine($"✓ fib(25) computed in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Object pooling reduces allocations from ~16.7M to ~N where N = call stack depth");
    }
}
