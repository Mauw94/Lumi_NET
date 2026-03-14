namespace Lumi.Lexer;

/// <summary>
/// Represents a token produced by the lexer. A readonly struct so every token is an inline
/// value copy — no heap allocation, no GC pressure, no object header overhead.
/// </summary>
public readonly struct Token
{
    public TokenKind Kind { get; }
    public string? Value { get; }
    public double? Number { get; }
    public int StartLine { get; }
    public int StartColumn { get; }
    public int EndLine { get; }
    public int EndColumn { get; }

    private Token(TokenKind kind, string? value, double? number, int startLine, int startColumn, int endLine, int endColumn)
    {
        Kind = kind;
        Value = value;
        Number = number;
        StartLine = startLine;
        StartColumn = startColumn;
        EndLine = endLine;
        EndColumn = endColumn;
    }

    public static Token WithPositions(TokenKind type, int startLine, int startColumn, int endLine, int endColumn)
        => new(type, null, null, startLine, startColumn, endLine, endColumn);

    public static Token WithValue(TokenKind type, string value, int startLine, int startColumn, int endLine, int endColumn)
        => new(type, value, null, startLine, startColumn, endLine, endColumn);

    public static Token WithNumber(double number, int startLine, int startColumn, int endLine, int endColumn)
        => new(TokenKind.Number, null, number, startLine, startColumn, endLine, endColumn);
}