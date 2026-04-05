using Lumi.Bytecode;
using Lumi.Parser;
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
        if (parser.HasErrors)
        {
            Console.WriteLine("Errors encountered during parsing: ");
            foreach (var error in parser.Errors)
            {
                Console.WriteLine(error);
            }
            return;
        }
        var vm = new VirtualMachine();
        vm.Execute(new BytecodeGenerator().Generate(new Parser(source).Parse()));
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }
}

static void Repl()
{
    var bytecodeGenerator = new BytecodeGenerator();
    var vm = new VirtualMachine();

    while (true)
    {
        Console.Write("> ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input)) continue;

        try
        {
            var parser = new Parser(input.Trim());
            var ast = parser.Parse();
            var bytecodeResult = bytecodeGenerator.Generate(ast);

            vm.Execute(bytecodeResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}