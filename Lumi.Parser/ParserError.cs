using Lumi.AST;
using Lumi.Lexer;

namespace Lumi.Parser
{
    /// <summary>
    /// Represents an error that occurs during parsing, providing details about the error message, position in the
    /// source, and any expected tokens.
    /// </summary>
    /// <remarks>This class is used to encapsulate various parsing errors, allowing for detailed error
    /// reporting and handling in parsing operations.</remarks>
    /// <param name="message">The error message that describes the parsing error.</param>
    /// <param name="position">The position in the source code where the error occurred, represented by a line and column number.</param>
    /// <param name="expected">An optional string indicating the expected token or syntax that was not found during parsing.</param>
    public class ParserError(string message, Position position, string? expected = null) : Exception(message)
    {
        public Position Position { get; } = position;
        public string? Expected { get; } = expected;

        public static ParserError UnexpectedToken(Token token, string expected)
        {
            var pos = new Position(token.StartLine, token.StartColumn);
            return new ParserError($"Unexpected token: {token.Kind}", pos, expected);
        }

        public static ParserError UnexpectedEndOfFile(string? expected = null)
        {
            var pos = new Position(1, 1);
            return new ParserError("Unexpected end of file", pos, expected);
        }

        public static ParserError InvalidSyntax(string message, Position position)
        {
            return new ParserError(message, position);
        }

        public static ParserError NoCurrentTokenFound(Position position)
        {
            return new ParserError("No current token found", position);
        }

        public static ParserError NoFunctionIdentifierFound(Position position)
        {
            return new ParserError("No function identifier found", position);
        }
    }
}
