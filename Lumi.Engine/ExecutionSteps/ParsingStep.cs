namespace Lumi.Engine.ExecutionSteps;

public sealed class ParsingStep : IPipelineExecutionStep
{
    public int Order => ExecutionStepOrder.ParsingStep;

    public bool TryExecute(PipelineExecutionContext context)
    {
        var parser = context.CreateParser();
        context.SetAst(parser.Parse());

        if (!parser.HasErrors)
        {
            return true;
        }

        Console.WriteLine("Errors encountered during parsing: ");

        foreach (var error in parser.Errors)
        {
            Console.WriteLine(error);
        }

        return false;
    }
}
