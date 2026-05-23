using Lumi.AST;
using Lumi.Bytecode;
using Lumi.VM;

namespace Lumi.Engine.Tests;

/// <summary>
/// End-to-end tests for structs through the full pipeline: Lexer → Parser → SemanticAnalyzer → Bytecode → VM.
/// </summary>
[TestClass]
[DoNotParallelize]
public sealed class StructTests
{
    [TestMethod]
    public void Test_Struct_Field_Access_Prints_Initialized_Values()
    {
        var source = """
            struct Person {
                name: str;
                age: int;
            }

            let person: Person -> new Person("Alice", 30);
            print person.name;
            print person.age;
            """;

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Alice\"", lines[0]);
        Assert.AreEqual("30", lines[1]);
    }

    [TestMethod]
    public void Test_Struct_Field_Assignment_Mutates_Field()
    {
        var source = """
            struct Person {
                age: int;
                name: str;
            }

            let person: Person -> new Person(1, "test");
            person.age = 5;
            print person.age;
            """;

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void Test_Struct_Named_Constructor_Arguments_Assign_By_Name()
    {
        var source = """
            struct Person {
                name: str;
                age: int;
            }

            let person: Person -> new Person(age: 5, name: "test");
            print person.name;
            print person.age;
            """;

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("\"test\"", lines[0]);
        Assert.AreEqual("5", lines[1]);
    }

    [TestMethod]
    public void Test_Struct_Field_Initializers_Are_Used_When_Constructor_Omits_Arguments()
    {
        var source = """
            struct Person {
                name: str -> "Harry";
                age: int -> 275;
            }

            let person: Person -> new Person();
            print person.name;
            print person.age;
            """;

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Harry\"", lines[0]);
        Assert.AreEqual("275", lines[1]);
    }

    [TestMethod]
    public void Test_Struct_Method_Call_Can_Use_This_And_Mutate_State()
    {
        var source = """
            struct Counter {
                value: int;

                fn increment(delta) {
                    this.value = this.value + delta;
                }
            }

            let counter: Counter -> new Counter(2);
            counter.increment(3);
            print counter.value;
            """;

        var output = ExecuteAndCapture(source);

        Assert.AreEqual("5", output);
    }

    [TestMethod]
    public void Test_Struct_Method_Can_Mutate_Default_Initialized_State()
    {
        var source = """
            struct Person {
                name: str -> "Harry";
                age: int -> 275;

                fn increaseAge(amount) {
                    this.age = this.age + amount;
                }
            }

            let person: Person -> new Person();
            person.increaseAge(2);

            print person.name;
            print person.age;
            """;

        var lines = ExecuteAndCapture(source).Split(Environment.NewLine);

        Assert.HasCount(2, lines);
        Assert.AreEqual("\"Harry\"", lines[0]);
        Assert.AreEqual("277", lines[1]);
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

            return writer.ToString().Trim();
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}
