using Lumi.Bytecode;
using Lumi.Engine;
using Lumi.VM;
using Microsoft.Extensions.DependencyInjection;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;

var serviceProvider = new ServiceCollection()
    .AddLumiEngine()
    .BuildServiceProvider();

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

            ExecuteScript(await LoadScript(scriptName), serviceProvider);
            break;

        case "r":
            Console.WriteLine("Entering REPL mode. Type your code below:");
            Repl(serviceProvider);
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

static void ExecuteScript(string source, IServiceProvider serviceProvider)
{
    var vm = new VirtualMachine();
    var bytecodeGenerator = new BytecodeGenerator();
    var semanticAnalyzer = new SemanticAnalyzerType();

    Console.Clear();

    try
    {
        Console.WriteLine("Result: ");

        var pipeline = serviceProvider.GetRequiredService<ExecutionPipeline>();
        pipeline.TryExecute(source, vm, bytecodeGenerator, semanticAnalyzer, Console.Out);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

static void Repl(IServiceProvider serviceProvider)
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
            var pipeline = serviceProvider.GetRequiredService<ExecutionPipeline>();
            pipeline.TryExecute(source, vm, bytecodeGenerator, semanticAnalyzer, Console.Out);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}