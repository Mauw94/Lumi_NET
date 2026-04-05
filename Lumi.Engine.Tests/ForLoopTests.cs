using Lumi.AST;
using Lumi.Bytecode;
using Lumi.VM;

namespace Lumi.Engine.Tests;

/// <summary>
/// Functional tests for FOR loops with semantic analysis and variable declarations,
/// covering the complete pipeline from source to execution.
/// </summary>
[TestClass]
[DoNotParallelize] // FOR loop tests capture console output
public sealed class ForLoopTests
{
    /// <summary>
    /// Test: Simple FOR loop printing all values (0-9).
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_Simple_Iteration()
    {
        // Arrange
        var source = "for i in 0 to 9 { print i; }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(10, lines, "Should print 10 values (0-9)");
        for (int i = 0; i < 10; i++)
        {
            Assert.AreEqual(i.ToString(), lines[i].Trim(), $"Line {i} should be {i}");
        }
    }

    /// <summary>
    /// Test: FOR loop with explicit step value.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_Step()
    {
        // Arrange
        var source = "for i in 0 to 8 step 2 { print i; }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(5, lines, "Should print 5 values (0, 2, 4, 6, 8)");
        var expected = new[] { "0", "2", "4", "6", "8" };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], lines[i].Trim(), $"Expected {expected[i]} at position {i}");
        }
    }

    /// <summary>
    /// Test: FOR loop with IF statement filtering even numbers.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_If_Even_Numbers()
    {
        // Arrange
        var source = "for i in 0 to 9 { if (i % 2 == 0) { print i; } }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(5, lines, "Should print 5 even numbers");
        var expected = new[] { "0", "2", "4", "6", "8" };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], lines[i].Trim(), $"Expected {expected[i]} at position {i}");
        }
    }

    /// <summary>
    /// Test: FOR loop with IF statement using less-than condition.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_If_Less_Than()
    {
        // Arrange
        var source = "for i in 0 to 9 { if (i < 5) { print i; } }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(5, lines, "Should print 5 numbers (0-4)");
        var expected = new[] { "0", "1", "2", "3", "4" };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], lines[i].Trim(), $"Expected {expected[i]} at position {i}");
        }
    }

    /// <summary>
    /// Test: FOR loop with variable declaration before and usage within loop.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_Variable_Declaration_Before()
    {
        // Arrange
        var source = "let sum -> 0; for i in 0 to 4 { sum = sum + i; } print sum;";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        // Sum of 0+1+2+3+4 = 10
        Assert.AreEqual("10", output.Trim(), "Sum should be 10");
    }

    /// <summary>
    /// Test: FOR loop with variable declaration and IF statement with arithmetic.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_Variable_And_If_Accumulate()
    {
        // Arrange
        var source = "let sum -> 0; for i in 0 to 9 { if (i % 2 == 0) { sum = sum + i; } } print sum;";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        // Sum of even numbers 0+2+4+6+8 = 20
        Assert.AreEqual("20", output.Trim(), "Sum of even numbers should be 20");
    }

    /// <summary>
    /// Test: FOR loop with IF/ELSE statement.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_If_Else()
    {
        // Arrange
        var source = "for i in 0 to 4 { if (i < 2) { print i; } else { print 100; } }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(5, lines, "Should print 5 lines");
        Assert.AreEqual("0", lines[0].Trim(), "First line should be 0");
        Assert.AreEqual("1", lines[1].Trim(), "Second line should be 1");
        Assert.AreEqual("100", lines[2].Trim(), "Third line should be 100");
        Assert.AreEqual("100", lines[3].Trim(), "Fourth line should be 100");
        Assert.AreEqual("100", lines[4].Trim(), "Fifth line should be 100");
    }

    /// <summary>
    /// Test: Nested FOR loops.
    /// </summary>
    [TestMethod]
    public void Test_Nested_ForLoops()
    {
        // Arrange
        var source = "for i in 0 to 2 { for j in 0 to 1 { print i; } }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(6, lines, "Should print 6 values (3 outer iterations × 2 inner iterations)");
        // Each outer iteration prints inner loop value twice
        Assert.AreEqual("0", lines[0].Trim());
        Assert.AreEqual("0", lines[1].Trim());
        Assert.AreEqual("1", lines[2].Trim());
        Assert.AreEqual("1", lines[3].Trim());
        Assert.AreEqual("2", lines[4].Trim());
        Assert.AreEqual("2", lines[5].Trim());
    }

    /// <summary>
    /// Test: FOR loop with multiple variable declarations and IF conditions.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_Multiple_Variables_And_If()
    {
        // Arrange
        var source = @"
            let count -> 0;
            let sum -> 0;
            for i in 0 to 9 {
                if (i > 3) {
                    count = count + 1;
                    sum = sum + i;
                }
            }
            print count;
            print sum;
            ";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(2, lines, "Should print 2 values (count and sum)");
        Assert.AreEqual("6", lines[0].Trim(), "Count of values > 3 should be 6 (4,5,6,7,8,9)");
        // Sum of 4+5+6+7+8+9 = 39
        Assert.AreEqual("39", lines[1].Trim(), "Sum should be 39");
    }

    /// <summary>
    /// Test: FOR loop with const variable (read-only) before loop.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_Const_Variable()
    {
        // Arrange
        var source = "const multiplier -> 2; for i in 0 to 4 { print i * multiplier; }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(5, lines, "Should print 5 values");

        var expected = new[] { "0", "2", "4", "6", "8" };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], lines[i].Trim(), $"Expected {expected[i]} * 2 = {expected[i]}");
        }
    }

    /// <summary>
    /// Test: FOR loop computing factorial.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_Factorial()
    {
        // Arrange
        var source = @"
            let n -> 5;
            let result -> 1;
            for i in 1 to n {
                result = result * i;
            }
            print result;
            ";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        // Factorial of 5 = 120
        Assert.AreEqual("120", output.Trim(), "Factorial of 5 should be 120");
    }

    /// <summary>
    /// Test: FOR loop with IF statement and value replacement.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_With_If_Skip_Values()
    {
        // Arrange
        var source = @"
            for i in 0 to 9 {
                if (i == 5) {
                    print 999;
                } else {
                    print i;
                }
            }
            ";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(10, lines);

        // Check that 999 appears at position 5 (when i == 5)
        Assert.AreEqual("999", lines[5].Trim(), "Position 5 should print 999");
        // Check other values
        Assert.AreEqual("0", lines[0].Trim());
        Assert.AreEqual("9", lines[9].Trim());
    }

    /// <summary>
    /// Test: FOR loop range larger than iteration count.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_Large_Range()
    {
        // Arrange
        var source = "for i in 0 to 99 step 10 { print i; }";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        var lines = output.Trim().Split([Environment.NewLine], StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();

        Assert.HasCount(10, lines, "Should print 10 values (0, 10, 20, ..., 90)");

        var expected = new[] { "0", "10", "20", "30", "40", "50", "60", "70", "80", "90" };
        for (int i = 0; i < expected.Length; i++)
        {
            Assert.AreEqual(expected[i], lines[i].Trim());
        }
    }

    /// <summary>
    /// Test: FOR loop with variable reassignment in IF block.
    /// </summary>
    [TestMethod]
    public void Test_ForLoop_Variable_Reassignment_In_If()
    {
        // Arrange
        var source = @"
            let x -> 0;
            for i in 0 to 4 {
                if (i % 2 == 0) {
                    x = i;
                }
            }
            print x;
            ";

        // Act
        var output = ExecuteAndCapture(source);

        // Assert
        // Last even number before 5 is 4
        Assert.AreEqual("4", output.Trim(), "x should be reassigned to the last even number (4)");
    }

    /// <summary>
    /// Helper method to execute source code and capture console output.
    /// Includes full pipeline: Lexer → Parser → SemanticAnalyzer → BytecodeGenerator → VM
    /// </summary>
    private static string ExecuteAndCapture(string source)
    {
        var originalOut = Console.Out;
        try
        {
            using (var writer = new StringWriter())
            {
                Console.SetOut(writer);

                // Parse
                var parser = new Lumi.Parser.Parser(source);
                var ast = parser.Parse();

                if (parser.HasErrors)
                {
                    return $"Parse errors: {string.Join(", ", parser.Errors)}";
                }

                // Semantic analysis
                if (ast is Program program)
                {
                    var semanticAnalyzer = new Lumi.SemanticAnalyzer.SemanticAnalyzer();
                    var analysisResult = semanticAnalyzer.Analyze(program);
                    if (!analysisResult.IsValid)
                    {
                        return $"Semantic errors: {string.Join(", ", analysisResult.Errors.Select(e => e.Message))}";
                    }
                }

                // Generate bytecode
                var bytecodeGenerator = new BytecodeGenerator();
                var bytecodeResult = bytecodeGenerator.Generate(ast);

                // Execute
                var vm = new VirtualMachine();
                vm.Execute(bytecodeResult);

                return writer.ToString();
            }
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}