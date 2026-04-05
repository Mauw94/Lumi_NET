namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Contains the results of semantic analysis.
/// </summary>
/// <param name="Errors">The list of semantic errors found during analysis.</param>
public readonly record struct SemanticAnalysisResult(IReadOnlyList<SemanticAnalyzerError> Errors)
{
    /// <summary>
    /// Returns true if the analysis found no errors.
    /// </summary>
    public bool IsValid => Errors.Count == 0;

    /// <summary>
    /// Throws the first error if any errors were found.
    /// </summary>
    public void ThrowIfErrors()
    {
        if (Errors.Count > 0)
            throw Errors[0];
    }
}