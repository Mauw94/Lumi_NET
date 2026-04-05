using Lumi.AST;
using Lumi.Bytecode;
using Lumi.Parser;
using Lumi.SemanticAnalyzer;
using Lumi.VM;

namespace Lumi.Engine.Tests;

[TestClass]
public sealed class FunctionTests
{
    [TestMethod]
    public void Test_Simple_Function_Call_No_Parameters()
    {
        // Arrange
        var source = @"
            fn sayHello() {
                print 42;
            }
            sayHello();
        ";

        // Act
        ExecuteProgram(source);

        // Assert - should print 42 without errors
    }

    [TestMethod]
    public void Test_Nested_Function_Calls_No_Parameters()
    {
        // Arrange
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

        // Act
        ExecuteProgram(source);

        // Assert - should print 1 then 2
    }

    [TestMethod]
    public void Test_Multiple_Function_Calls()
    {
        // Arrange
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

        // Act
        ExecuteProgram(source);

        // Assert - should print 10, 20, 10
    }

    [TestMethod]
    public void Test_Function_With_Variable()
    {
        // Arrange
        var source = @"
            fn printValue() {
                let x -> 99;
                print x;
            }
            printValue();
        ";

        // Act
        ExecuteProgram(source);

        // Assert - should print 99
    }

    [TestMethod]
    public void Test_Function_With_If_No_Parameters()
    {
        // Arrange
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

        // Act
        ExecuteProgram(source);

        // Assert - should print 1
    }

    [TestMethod]
    public void Test_Function_With_Single_Parameter()
    {
        // Arrange
        var source = @"
            fn double(x) {
                print x * 2;
            }
            double(7);
        ";

        // Act & Assert - should print 14 without errors
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_With_Multiple_Parameters()
    {
        // Arrange
        var source = @"
            fn add(a, b) {
                print a + b;
            }
            add(3, 5);
        ";

        // Act & Assert - should print 8
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_With_Three_Parameters()
    {
        // Arrange
        var source = @"
            fn multiply(x, y, z) {
                print x * y * z;
            }
            multiply(2, 3, 4);
        ";

        // Act & Assert - should print 24
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_Parameter_Used_In_If()
    {
        // Arrange
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

        // Act & Assert - should print 1
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_Called_Multiple_Times_With_Different_Args()
    {
        // Arrange
        var source = @"
            fn square(n) {
                print n * n;
            }
            square(3);
            square(5);
        ";

        // Act & Assert - should print 9 then 25
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_With_Local_Variable_And_Parameter()
    {
        // Arrange
        var source = @"
            fn compute(x) {
                let result -> x * x + 1;
                print result;
            }
            compute(4);
        ";

        // Act & Assert - should print 17
        ExecuteProgram(source);
    }

    [TestMethod]
    public void Test_Function_Called_With_Negative_Argument()
    {
        // Arrange — this is the exact scenario that was crashing with a stack underflow
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

        // Act & Assert - should print 0 without errors
        ExecuteProgram(source);
    }

    private static void ExecuteProgram(string source)
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

        var vm = new VirtualMachine();
        vm.Execute(bytecodeResult);
    }
}

