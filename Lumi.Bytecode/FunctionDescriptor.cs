namespace Lumi.Bytecode;

/// <summary>
/// Describes a compiled function body independently of how it is invoked at runtime.
/// </summary>
/// <param name="FunctionId">The stable compiler-assigned identifier for this function.</param>
/// <param name="Name">The source-level function name.</param>
/// <param name="EntryPoint">The bytecode instruction index where the function body begins.</param>
/// <param name="ParameterCount">The declared parameter count, excluding any implicit receiver.</param>
/// <param name="ParentFunctionId">The enclosing function descriptor id when this function is nested; otherwise null.</param>
/// <param name="OwningStructName">The struct name when this descriptor represents a struct method; otherwise null.</param>
/// <param name="CaptureNames">The outer-scope local names referenced by this function.</param>
public sealed record FunctionDescriptor(
    int FunctionId,
    string Name,
    int EntryPoint,
    int ParameterCount,
    int? ParentFunctionId,
    string? OwningStructName,
    IReadOnlyList<string> CaptureNames)
{
    public bool HasCaptures() => CaptureNames.Count > 0;
}
