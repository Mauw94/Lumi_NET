namespace Lumi.Lexer;

public class LexError : Exception
{
    public LexError(string message) : base(message) { }

    public static LexError InvalidNumber(string s) => new LexError($"Invalid number: {s}");
    public static LexError UnterminatedString => new LexError("Unterminated string");
    public static LexError UnterminatedComment => new LexError("Unterminated comment");
    public static LexError UnexpectedCharacter(char c) => new LexError($"Unexpected character: {c}");
}
