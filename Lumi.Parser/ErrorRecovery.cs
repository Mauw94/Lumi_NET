namespace Lumi.Parser;

/// <summary>
/// Provides functionality to track and manage parsing errors, allowing recovery up to a specified maximum number of
/// errors.
/// </summary>
/// <remarks>Use this class to accumulate and inspect parsing errors during parsing operations. Recovery is
/// considered possible as long as the number of recorded errors does not exceed the specified maximum. This is useful
/// for parsers that need to tolerate a limited number of errors before aborting or reporting failure.</remarks>
/// <param name="maxErrors">The maximum number of errors that can be recorded before recovery is no longer possible. Must be a non-negative
/// integer.</param>
public sealed class ErrorRecovery(int maxErrors)
{
    private readonly int _maxErrors = maxErrors;
    private int _errorCount = 0;
    private readonly List<ParserError> _errors = [];

    public static ErrorRecovery Default() => new(10);

    public bool CanRecover() => _errorCount < _maxErrors;

    public void RecordError(ParserError error)
    {
        if (_errorCount < _maxErrors)
        {
            _errors.Add(error);
            _errorCount++;
        }
    }

    public bool HasErrors() => _errors.Count > 0;

    public void ClearErrors()
    {
        _errors.Clear();
        _errorCount = 0;
    }

    public IReadOnlyList<ParserError> Errors => _errors.AsReadOnly();

    public int ErrorCount => _errorCount;
}
