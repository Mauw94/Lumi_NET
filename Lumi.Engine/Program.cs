using Lumi.Bytecode;
using Lumi.Engine;
using Lumi.Engine.ExecutionSteps;
using Lumi.VM;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;

// TODO: this needs to be split into two separate projects

while (true)
{
    Console.WriteLine("Execute S or R? (script/repl)");
    var choice = Console.ReadLine()?.ToLower().Trim();

    switch (choice)
    {
        case "s":
            Console.WriteLine("Enter script name (no extension): ");
            var scriptName = Console.ReadLine()?.ToLower().Trim();

            if (string.IsNullOrEmpty(scriptName))
            {
                break;
            }

            ExecuteScript(await LoadScript(scriptName));
            break;

        case "r":
            Console.WriteLine("Entering REPL mode. Type your code below:");
            Repl();
            break;
    }
}

static async Task<string> LoadScript(string scriptName)
{
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Examples"));
    var path = Path.Combine(root, scriptName + ".lumi");

    if (!File.Exists(path))
    {
        throw new FileNotFoundException($"Script not found: {path}", path);
    }

    return await File.ReadAllTextAsync(path);
}

static void ExecuteScript(string source)
{
    var vm = new VirtualMachine();
    var bytecodeGenerator = new BytecodeGenerator();
    var semanticAnalyzer = new SemanticAnalyzerType();

    Console.Clear();

    try
    {
        Console.WriteLine("Result: ");

        var pipeline = new ExecutionPipeline(
        [
            new ParsingStep(),
            new SemanticAnalysisStep(),
            new BytecodeExecutionStep()
        ]);

        pipeline.TryExecute(source, vm, bytecodeGenerator, semanticAnalyzer, Console.Out);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

static void Repl()
{
    var vm = new VirtualMachine();
    var bytecodeGenerator = new BytecodeGenerator();
    var semanticAnalyzer = new SemanticAnalyzerType();

    while (true)
    {
        Console.Write("> ");
        var source = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(source))
        {
            continue;
        }

        try
        {
            var pipeline = new ExecutionPipeline(
            [
                new ParsingStep(),
                new SemanticAnalysisStep(),
                new BytecodeExecutionStep()
            ]);

            pipeline.TryExecute(source, vm, bytecodeGenerator, semanticAnalyzer, Console.Out);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}