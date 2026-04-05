using System.Globalization;

namespace Lumi.Lexer;

/// <summary>
/// Represents a lexical analyzer that tokenizes a source string into a sequence of tokens for further processing.  
/// </summary>
/// <param name="source">The source string to be tokenized. This value cannot be null.</param>
public sealed class Lexer(string source)
{
    private static readonly HashSet<string> _keywords = new(StringComparer.Ordinal)
    {
        "let","const","var","fn","if","else","return","async","await","yield",
        "import","export","new","class","extends","static","get","set",
        "try","catch","finally","throw","break","continue","switch","case",
        "default","for","while","do","in","of","with","delete",
        "instanceof","typeof","void","debugger","enum","interface","package",
        "private","protected","public","implements","abstract","bool","byte",
        "char","double","final","float","goto","int","long","str",
        "native","short","synchronized","throws","transient","volatile","to",
        "step","print"
    };

    private readonly string _source = source ?? string.Empty;
    private int _pos = 0;
    private int _line = 1;
    private int _column = 1;

    /// <summary>
    /// Extracts and returns a read-only list of tokens parsed from the source input.
    /// </summary>
    /// <remarks>If the source input contains no tokens, the returned list will consist solely of an
    /// end-of-file (EOF) token. The returned list preserves the order in which tokens appear in the source.</remarks>
    /// <returns>A read-only list of tokens representing the lexical elements found in the source input. The list always includes
    /// an end-of-file (EOF) token as the last element.</returns>
    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (_pos < _source.Length)
        {
            var token = NextToken();

            if (token.Kind == TokenKind.Eof)
            {
                tokens.Add(token);
                break;
            }

            tokens.Add(token);
        }

        if (tokens.Count == 0 || tokens[tokens.Count - 1].Kind != TokenKind.Eof)
        {
            tokens.Add(Token.WithPositions(TokenKind.Eof, _line, _column, _line, _column));
        }

        return tokens;
    }

    /// <summary>
    /// Reads and returns the next token from the source input, advancing the current position past any whitespace and
    /// the recognized token.
    /// </summary>
    /// <remarks>This method recognizes and returns tokens for identifiers, keywords, numbers, strings,
    /// comments, and operators. It updates the current line and column positions as tokens are read.</remarks>
    /// <returns>A <see cref="Token"/> representing the next token in the source input. Returns an end-of-file token if the end
    /// of the source is reached.</returns>
    public Token NextToken()
    {
        SkipWhitespace();

        if (_pos >= _source.Length)
        {
            return Token.WithPositions(TokenKind.Eof, _line, _column, _line, _column);
        }

        var startLine = _line;
        var startCol = _column;
        var c = _source[_pos];

        Token token;

        if (char.IsLetter(c) || c == '_' || c == '$' || c > 127)
        {
            token = ReadIdentifierOrKeyword(startLine, startCol);
        }
        else if (char.IsDigit(c))
        {
            token = ReadNumber(startLine, startCol);
        }
        else if (c == '"' || c == '\'')
        {
            token = ReadString(startLine, startCol);
        }
        else if (c == '/')
        {
            var next = PeekChar(1);
            if (next == '/') token = ReadLineComment(startLine, startCol);
            else if (next == '*') token = ReadBlockComment(startLine, startCol);
            else token = ReadOperator(startLine, startCol);
        }
        else
        {
            token = ReadOperator(startLine, startCol);
        }

        return token;
    }

    private Token ReadIdentifierOrKeyword(int startLine, int startCol)
    {
        var start = _pos;
        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            if (char.IsLetterOrDigit(c) || c > 127 || c == '_')
                Advance();
            else break;
        }

        var identifier = _source[start.._pos];

        switch (identifier)
        {
            case "true": return Token.WithValue(TokenKind.Boolean, "true", startLine, startCol, _line, _column);
            case "false": return Token.WithValue(TokenKind.Boolean, "false", startLine, startCol, _line, _column);
            case "null": return Token.WithPositions(TokenKind.Null, startLine, startCol, _line, _column);
            case "undefined": return Token.WithPositions(TokenKind.Undefined, startLine, startCol, _line, _column);
            case "this": return Token.WithValue(TokenKind.Keyword, "this", startLine, startCol, _line, _column);
            case "super": return Token.WithValue(TokenKind.Keyword, "super", startLine, startCol, _line, _column);
            default:
                if (_keywords.Contains(identifier))
                    return Token.WithValue(TokenKind.Keyword, identifier, startLine, startCol, _line, _column);
                return Token.WithValue(TokenKind.Identifier, identifier, startLine, startCol, _line, _column);
        }
    }

    private Token ReadNumber(int startLine, int startCol)
    {
        var numStart = _pos;
        bool isHex = false, isBinary = false, isOctal = false;

        if (_source[_pos] == '0' && _pos + 1 < _source.Length)
        {
            var c1 = _source[_pos + 1];
            if (c1 == 'x' || c1 == 'X') { isHex = true; Advance(); Advance(); }
            else if (c1 == 'b' || c1 == 'B') { isBinary = true; Advance(); Advance(); }
            else if (c1 == 'o' || c1 == 'O') { isOctal = true; Advance(); Advance(); }
        }

        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            bool keep = isHex ? Uri.IsHexDigit(c)
                      : isBinary ? c == '0' || c == '1'
                      : isOctal ? c >= '0' && c <= '7'
                      : char.IsDigit(c) || c == '.' || c == 'e' || c == 'E';
            if (keep) Advance(); else break;
        }

        var span = _source.AsSpan(numStart, _pos - numStart);

        if (isHex)
        {
            if (ulong.TryParse(span.Slice(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var hex))
                return Token.WithNumber(hex, startLine, startCol, _line, _column);
        }
        else if (isBinary)
        {
            try { return Token.WithNumber(Convert.ToUInt64(span.Slice(2).ToString(), 2), startLine, startCol, _line, _column); }
            catch { }
        }
        else if (isOctal)
        {
            try { return Token.WithNumber(Convert.ToUInt64(span.Slice(2).ToString(), 8), startLine, startCol, _line, _column); }
            catch { }
        }
        else
        {
            // Common decimal path: TryParse accepts ReadOnlySpan<char> — zero string allocation
            if (double.TryParse(span, NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                return Token.WithNumber(d, startLine, startCol, _line, _column);
        }

        throw LexError.InvalidNumber(span.ToString());
    }

    private Token ReadString(int startLine, int startCol)
    {
        var quote = _source[_pos];
        Advance(); // skip opening
        var sb = new System.Text.StringBuilder();
        bool found = false;

        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            if (c == quote)
            {
                Advance(); found = true; break;
            }
            else if (c == '\\')
            {
                Advance();
                if (_pos < _source.Length)
                {
                    var esc = _source[_pos];
                    switch (esc)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case '\\': sb.Append('\\'); break;
                        case '"': sb.Append('"'); break;
                        case '\'': sb.Append('\''); break;
                        default: sb.Append(esc); break;
                    }
                    Advance();
                }
            }
            else
            {
                sb.Append(c);
                Advance();
            }
        }

        if (!found) throw LexError.UnterminatedString;

        return Token.WithValue(TokenKind.String, sb.ToString(), startLine, startCol, _line, _column);
    }

    private Token ReadLineComment(int startLine, int startCol)
    {
        Advance(); // /
        Advance(); // /
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            if (c == '\n') break;
            sb.Append(c);
            Advance();
        }
        return Token.WithValue(TokenKind.Comment, sb.ToString(), startLine, startCol, _line, _column);
    }

    private Token ReadBlockComment(int startLine, int startCol)
    {
        Advance(); // /
        Advance(); // *
        var sb = new System.Text.StringBuilder();
        bool found = false;
        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            if (c == '*' && PeekChar(1) == '/')
            {
                Advance(); Advance(); found = true; break;
            }
            sb.Append(c);
            Advance();
        }
        if (!found) throw LexError.UnterminatedComment;
        return Token.WithValue(TokenKind.Comment, sb.ToString(), startLine, startCol, _line, _column);
    }

    private Token ReadOperator(int startLine, int startCol)
    {
        var c = _source[_pos];

        if (_pos + 1 < _source.Length)
        {
            var next = _source[_pos + 1];
            switch (c)
            {
                case '=' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.EqualEqual, startLine, startCol, _line, _column);
                case '!' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.NotEqual, startLine, startCol, _line, _column);
                case '<' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.LessThanEqual, startLine, startCol, _line, _column);
                case '>' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.GreaterThanEqual, startLine, startCol, _line, _column);
                case '+' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.PlusAssign, startLine, startCol, _line, _column);
                case '-' when next == '=': Advance(); Advance(); return Token.WithPositions(TokenKind.MinusAssign, startLine, startCol, _line, _column);
                case '+' when next == '+': Advance(); Advance(); return Token.WithPositions(TokenKind.Increment, startLine, startCol, _line, _column);
                case '-' when next == '-': Advance(); Advance(); return Token.WithPositions(TokenKind.Decrement, startLine, startCol, _line, _column);
                case '-' when next == '>': Advance(); Advance(); return Token.WithPositions(TokenKind.Arrow, startLine, startCol, _line, _column);
            }
        }

        switch (c)
        {
            case '(': Advance(); return Token.WithPositions(TokenKind.LeftParen, startLine, startCol, _line, _column);
            case ')': Advance(); return Token.WithPositions(TokenKind.RightParen, startLine, startCol, _line, _column);
            case '{': Advance(); return Token.WithPositions(TokenKind.LeftBrace, startLine, startCol, _line, _column);
            case '}': Advance(); return Token.WithPositions(TokenKind.RightBrace, startLine, startCol, _line, _column);
            case '[': Advance(); return Token.WithPositions(TokenKind.LeftBracket, startLine, startCol, _line, _column);
            case ']': Advance(); return Token.WithPositions(TokenKind.RightBracket, startLine, startCol, _line, _column);
            case '.': Advance(); return Token.WithPositions(TokenKind.Dot, startLine, startCol, _line, _column);
            case ';': Advance(); return Token.WithPositions(TokenKind.Semicolon, startLine, startCol, _line, _column);
            case ',': Advance(); return Token.WithPositions(TokenKind.Comma, startLine, startCol, _line, _column);
            case ':': Advance(); return Token.WithPositions(TokenKind.Colon, startLine, startCol, _line, _column);
            case '?': Advance(); return Token.WithPositions(TokenKind.Question, startLine, startCol, _line, _column);
            case '!': Advance(); return Token.WithPositions(TokenKind.Exclamation, startLine, startCol, _line, _column);
            case '=': Advance(); return Token.WithPositions(TokenKind.Assign, startLine, startCol, _line, _column);
            case '+': Advance(); return Token.WithPositions(TokenKind.Plus, startLine, startCol, _line, _column);
            case '-': Advance(); return Token.WithPositions(TokenKind.Minus, startLine, startCol, _line, _column);
            case '*': Advance(); return Token.WithPositions(TokenKind.Star, startLine, startCol, _line, _column);
            case '/': Advance(); return Token.WithPositions(TokenKind.Slash, startLine, startCol, _line, _column);
            case '%': Advance(); return Token.WithPositions(TokenKind.Percent, startLine, startCol, _line, _column);
            case '<': Advance(); return Token.WithPositions(TokenKind.LessThan, startLine, startCol, _line, _column);
            case '>': Advance(); return Token.WithPositions(TokenKind.GreaterThan, startLine, startCol, _line, _column);
            default: throw LexError.UnexpectedCharacter(c);
        }
    }

    private void SkipWhitespace()
    {
        while (_pos < _source.Length)
        {
            var c = _source[_pos];
            if (char.IsWhiteSpace(c))
            {
                if (c == '\n') { _line++; _column = 1; }
                else { _column++; }
                Advance();
            }
            else break;
        }
    }

    private void Advance()
    {
        if (_pos < _source.Length)
        {
            _pos++;
            _column++;
        }
    }

    private char? PeekChar(int offset)
    {
        if (_pos + offset < _source.Length) return _source[_pos + offset];
        return null;
    }
}