namespace Lumi.Lexer;

/// <summary>
/// Represents an error that occurs during lexical analysis, providing detailed information about specific lexical
/// issues encountered while processing input.
/// </summary>
/// <remarks>Use this exception to signal errors such as invalid numbers, unterminated strings or comments, and
/// unexpected characters during the tokenization phase of parsing. The class includes static factory methods for
/// creating common lexical error instances with descriptive messages.</remarks>
internal sealed class LexError(string message) : Exception(message)
{
    public static LexError InvalidNumber(string s) => new($"Invalid number: {s}");
    public static LexError UnterminatedString => new("Unterminated string");
    public static LexError UnterminatedComment => new("Unterminated comment");
    public static LexError UnexpectedCharacter(char c) => new($"Unexpected character: {c}");
}