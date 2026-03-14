using Lumi.Bytecode;
using Lumi.Parser;
using Lumi.VM;

do
{
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;

    try
    {
        var parser = new Parser(input.Trim());
        var ast = parser.Parse();
        var bytecodeGenerator = new BytecodeGenerator();
        var bytecodeResult = bytecodeGenerator.Generate(ast);

        var vm = new VirtualMachine();
        vm.Execute(bytecodeResult);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
    }

} while (true);
