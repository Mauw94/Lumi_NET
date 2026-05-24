using Lumi.AST;
using Lumi.Bytecode;
using Lumi.VM;

namespace Lumi.Engine.Tests;

/// <summary>
/// End-to-end tests for array indexing through the full pipeline: Lexer → Parser → SemanticAnalyzer → Bytecode → VM.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class ArrayTests
{
    [TestMethod]
    public void Test_Array_Index_Print_First_Element()
    {
        var source = "let x -> [1, 2, 3]; print x[0];";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("1", output.Trim());
    }

    [TestMethod]
    public void Test_Array_Index_Print_Middle_Element()
    {
        var source = "let x -> [10, 20, 30]; print x[1];";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("20", output.Trim());
    }

    [TestMethod]
    public void Test_Array_Index_Variable_As_Index()
    {
        var source = "let x -> [10, 20, 30]; let i -> 2; print x[i];";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("30", output.Trim());
    }

    [TestMethod]
    public void Test_List_Add_Method_Mutates_List()
    {
        var source = "let items: list -> [1, 2, 3]; items.add(4); print items;";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("[1, 2, 3, 4]", output.Trim());
    }

    [TestMethod]
    public void Test_List_Remove_Method_Mutates_List()
    {
        var source = "let items: list -> [1, 2, 3]; items.remove(2); print items;";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("[1, 3]", output.Trim());
    }

    [TestMethod]
    public void Test_List_Length_Method_Returns_Count()
    {
        var source = "let items: list -> [1, 2, 3]; print items.length();";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("3", output.Trim());
    }

    [TestMethod]
    public void Test_List_Contains_Method_Returns_True_When_Present()
    {
        var source = "let items: list -> [1, 2, 3]; print items.contains(2);";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("True", output.Trim());
    }

    [TestMethod]
    public void Test_List_Contains_Method_Returns_False_When_Absent()
    {
        var source = "let items: list -> [1, 2, 3]; print items.contains(99);";

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("False", output.Trim());
    }

    [TestMethod]
    public void Test_List_Add_Method_In_Loop_Does_Not_Overflow_Stack()
    {
        var source = """
            let items: list -> [];
            for i in 0 to 1499 step 1 {
                items.add(i);
            }
            print items.length();
            """;

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("1500", output.Trim());
    }

    private static string ExecuteAndCapture(string source)
    {
        var originalOut = Console.Out;
        try
        {
            using var writer = new StringWriter();
            Console.SetOut(writer);

            var parser = new Lumi.Parser.Parser(source);
            var ast = parser.Parse();

            if (parser.HasErrors)
                return $"Parse errors: {string.Join(", ", parser.Errors)}";

            if (ast is Program program)
            {
                var semanticAnalyzer = new Lumi.SemanticAnalyzer.SemanticAnalyzer();
                var analysisResult = semanticAnalyzer.Analyze(program);
                if (!analysisResult.IsValid)
                    return $"Semantic errors: {string.Join(", ", analysisResult.Errors.Select(e => e.Message))}";
            }

            var bytecodeResult = new BytecodeGenerator().Generate(ast);
            new VirtualMachine().Execute(bytecodeResult);

            return writer.ToString();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}