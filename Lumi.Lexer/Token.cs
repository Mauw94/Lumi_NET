namespace Lumi.Lexer;

public class Token(TokenKind type, string? value, double? number, int startLine, int startColumn, int endLine, int endColumn)
{
    public TokenKind Kind { get; } = type;
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
