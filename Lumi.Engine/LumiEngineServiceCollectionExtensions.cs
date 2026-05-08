using Lumi.Engine.ExecutionSteps;
using Microsoft.Extensions.DependencyInjection;

namespace Lumi.Engine;

public static class LumiEngineServiceCollectionExtensions
{
    public static IServiceCollection AddLumiEngine(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<ExecutionPipeline>();
        services.AddTransient<IPipelineExecutionStep, ParsingStep>();
        services.AddTransient<IPipelineExecutionStep, SemanticAnalysisStep>();
        services.AddTransient<IPipelineExecutionStep, BytecodeExecutionStep>();

        return services;
    }
}