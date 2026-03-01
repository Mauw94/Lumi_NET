using Lumi.AST;
using Lumi.Lexer;

namespace Lumi.Parser
{
    public class ParserError : Exception
    {
        public Position Position { get; }
        public string Expected { get; }

        public ParserError(string message, Position position, string expected = null) : base(message)
        {
            Position = position;
            Expected = expected;
        }

        public static ParserError UnexpectedToken(Token token, string expected = null)
        {
            var pos = new Position(token.StartLine, token.StartColumn);
            return new ParserError($"Unexpected token: {token.Kind}", pos, expected);
        }

        public static ParserError UnexpectedEndOfFile(string expected = null)
        {
            var pos = new Position(1, 1);
            return new ParserError("Unexpected end of file", pos, expected);
        }

        public static ParserError InvalidSyntax(string message, Position position)
        {
            return new ParserError(message, position);
        }
    }
}
