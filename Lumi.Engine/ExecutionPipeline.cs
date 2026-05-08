using Lumi.Bytecode;
using Lumi.Engine.ExecutionSteps;
using Lumi.VM;
using SemanticAnalyzerType = Lumi.SemanticAnalyzer.SemanticAnalyzer;

namespace Lumi.Engine;

public class ExecutionPipeline
{
    private readonly IEnumerable<IPipelineExecutionStep> _steps;

    public ExecutionPipeline(IEnumerable<IPipelineExecutionStep> steps)
    {
        _steps = [.. steps.OrderBy(step => step.Order)];
    }

    public bool TryExecute(string source, VirtualMachine vm, BytecodeGenerator bytecodeGenerator, SemanticAnalyzerType semanticAnalyzer, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(vm);
        ArgumentNullException.ThrowIfNull(bytecodeGenerator);
        ArgumentNullException.ThrowIfNull(semanticAnalyzer);
        ArgumentNullException.ThrowIfNull(output);

        var context = new PipelineExecutionContext
        {
            Source = source,
            VirtualMachine = vm,
            BytecodeGenerator = bytecodeGenerator,
            SemanticAnalyzer = semanticAnalyzer,
            Output = output
        };
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(output);

            foreach (var step in _steps)
            {
                if (!step.TryExecute(context))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return false;
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }
}