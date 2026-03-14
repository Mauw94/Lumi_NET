using Lumi.Bytecode;
using Lumi.Parser;
using Lumi.VM;

// Create VM and bytecode generator once to maintain state across input lines
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