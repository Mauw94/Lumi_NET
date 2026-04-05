using Lumi.AST;
using Lumi.Bytecode;
using Lumi.Parser;
using Lumi.SemanticAnalyzer;
using Lumi.VM;

while (true)
{
    Console.WriteLine("Execute S or R? (script/repl)");
    var choice = Console.ReadLine()?.ToLower().Trim();

    switch (choice?.ToLower())
    {
        case "s":
            Console.WriteLine("Enter script name (no extension): ");
            var scriptName = Console.ReadLine()?.ToLower().Trim();
            if (string.IsNullOrEmpty(scriptName)) break;
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
    var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
    var path = Path.Combine(root, scriptName + ".lumi");

    if (!File.Exists(path)) throw new FileNotFoundException($"Script not found: {path}", path);

    return await File.ReadAllTextAsync(path);
}

static void ExecuteScript(string source)
{
    Console.Clear();
    try
    {
        Console.WriteLine("Result: ");
        var parser = new Parser(source);
        var ast = parser.Parse();

        PrintParseErrors(parser);
        SemanticAnalysis(ast);
        ExecuteBytecode(ast);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

static void Repl()
{
    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) continue;

        try
        {
            var parser = new Parser(input.Trim());
            var ast = parser.Parse();

            PrintParseErrors(parser);
            SemanticAnalysis(ast);
            ExecuteBytecode(ast);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}

static void PrintParseErrors(Parser parser)
{
    if (parser.HasErrors)
    {
        Console.WriteLine("Errors encountered during parsing: ");
        foreach (var error in parser.Errors)
        {
            Console.WriteLine(error);
        }
        return;
    }
}

static void SemanticAnalysis(Node? ast)
{
    var semanticAnalyzer = new SemanticAnalyzer();

    if (ast is Lumi.AST.Program program)
    {
        var analysisResult = semanticAnalyzer.Analyze(program);
        if (!analysisResult.IsValid)
        {
            foreach (var error in analysisResult.Errors)
            {
                Console.WriteLine($"Semantic error: {error.Message}");
            }
        }
    }
}

static void ExecuteBytecode(Node? ast)
{
    if (ast == null)
    {
        Console.WriteLine("Error: AST is null");
        return;
    }

    var vm = new VirtualMachine();
    vm.Execute(new BytecodeGenerator().Generate(ast));
}