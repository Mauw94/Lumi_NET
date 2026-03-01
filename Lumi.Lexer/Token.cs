namespace Lumi.Lexer;

/// <summary>
/// Represents a token produced by the lexer, encapsulating its type, value (if applicable), and precise source code location (line and column information).
/// </summary>
/// <param name="kind">Kind of token.</param>
/// <param name="value">String value of the token.</param>
/// <param name="number">Number value of the token.</param>
/// <param name="startLine">Position of starting line.</param>
/// <param name="startColumn">Position of starting column.</param>
/// <param name="endLine">Positing of end line.</param>
/// <param name="endColumn">Position of end column.</param>
public class Token(TokenKind kind, string? value, double? number, int startLine, int startColumn, int endLine, int endColumn)
{
    public TokenKind Kind { get; } = kind;
    public string? Value { get; } = value;
    public double? Number { get; } = number;
    public int StartLine { get; } = startLine;
    public int StartColumn { get; } = startColumn;
    public int EndLine { get; } = endLine;
    public int EndColumn { get; } = endColumn;

    public static Token WithPositions(TokenKind type, int startLine, int startColumn, int endLine, int endColumn)
    {
        return new Token(type, null, null, startLine, startColumn, endLine, endColumn);
    }

    public static Token WithValue(TokenKind type, string value, int startLine, int startColumn, int endLine, int endColumn)
    {
        return new Token(type, value, null, startLine, startColumn, endLine, endColumn);
    }

    public static Token WithNumber(double number, int startLine, int startColumn, int endLine, int endColumn)
    {
        return new Token(TokenKind.Number, null, number, startLine, startColumn, endLine, endColumn);
    }

    public override bool Equals(object? obj)
    {
        if (obj is Token t)
        {
            if (Kind != t.Kind) return false;
            if (Number.HasValue || t.Number.HasValue)
                return Number == t.Number;
            return string.Equals(Value, t.Value, StringComparison.Ordinal);
        }
        return false;
    }

    public override int GetHashCode() => HashCode.Combine(Kind, Value, Number);
}