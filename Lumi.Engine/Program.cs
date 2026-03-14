using Lumi.Bytecode;
using Lumi.Parser;
using Lumi.VM;

do
{
    var input = Console.ReadLine();
    
    if (string.IsNullOrWhiteSpace(input)) continue;

    var parser = new Parser(input.Trim());
    var ast = parser.Parse();
    var bytecodeGenerator = new BytecodeGenerator();
    bytecodeGenerator.Generate(ast);

    var vm = new VirtualMachine();
    vm.Execute(bytecodeGenerator);

} while (true);
