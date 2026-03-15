namespace Lumi.AST;

/// <summary>
/// Represents a zero-based position within a text document, defined by line and column numbers.
/// </summary>
/// <remarks>Use this struct to specify locations in text files, such as for diagnostics, parsing, or editor
/// features. Line and column values are one-based, with the first line and column being 1.</remarks>
/// <param name="Line">The line number of the position. Must be greater than or equal to 1.</param>
/// <param name="Column">The column number of the position. Must be greater than or equal to 1.</param>
public readonly record struct Position(int Line, int Column)
{
    public Position() : this(1, 1) { }

    public override string ToString() => $"{Line}:{Column}";
}