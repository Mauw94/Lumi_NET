using Lumi.Ast;
using Lumi.AST;
using Lumi.Lexer;

namespace Lumi.Parser
{
    public class Parser
    {
        private string _source;
        private bool _strictMode;
        private readonly Lexer.Lexer lexer;
        private Token current;
        private Token previous;
        private ErrorRecovery errorRecovery;
        private ParsingContext context;

        public Parser(string source)
        {
            _source = source;
            _strictMode = false;

            lexer = new Lexer.Lexer(source);

            try
            {
                current = lexer.NextToken();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lexer error: {ex}");
                current = null;
            }

            previous = null;
            errorRecovery = ErrorRecovery.Default();
            context = ParsingContext.TopLevel;
        }

        public Node Parse()
        {
            return ParseProgram();
        }

        private Node ParseProgram()
        {
            var body = new List<Node>();
            var startPos = CurrentPosition();

            if (IsEof())
            {
                var endPos = PreviousPosition();
                var span = CreateSpan(startPos, endPos);
                return new Program { Body = body, Span = span } as Node;
            }

            while (!IsEof())
            {
                try
                {
                    var stmt = ParseStatement();
                    body.Add(stmt);
                }
                catch (ParserError error)
                {
                    if (!TryRecoverFromError(error))
                    {
                        throw;
                    }

                    if (IsEof())
                        break;
                }
            }

            var end = PreviousPosition();
            var programSpan = CreateSpan(startPos, end);
            return new Program { Body = body, Span = programSpan } as Node;
        }

        private Node ParseStatement()
        {
            var oldContext = context;
            context = ParsingContext.Statement;

            Node result;

            if (current != null)
            {
                if (IsKeyword(current, out var kw))
                {
                    switch (kw)
                    {
                        case "let":
                        case "const":
                            result = ParseVariableDeclaration();
                            break;
                        case "if":
                            result = ParseIfStatement();
                            break;
                        case "print":
                            result = ParsePrintStatement();
                            break;
                        case "fn":
                            result = ParseFunctionStatement();
                            break;
                        case "for":
                            result = ParseForStatement();
                            break;
                        default:
                            // temporary placeholder to avoid infinite loop
                            Advance();
                            result = Node.Null;
                            break;
                    }
                }
                else if (Check(TokenKind.LeftBrace))
                {
                    result = ParseBlockStatement();
                }
                else
                {
                    result = ParseExpressionStatement();
                }
            }
            else
            {
                context = oldContext;
                throw ParserError.UnexpectedEndOfFile(null);
            }

            context = oldContext;
            return result;
        }

        private Node ParseBlockStatement()
        {
            Advance(); // consume '{'

            var oldContext = context;
            context = ParsingContext.Block;

            var body = new List<Node>();
            while (!Check(TokenKind.RightBrace) && !IsEof())
            {
                try
                {
                    var stmt = ParseStatement();
                    body.Add(stmt);
                }
                catch (ParserError err)
                {
                    if (!TryRecoverFromError(err))
                    {
                        context = oldContext;
                        throw;
                    }
                }
            }

            Expect(TokenKind.RightBrace);

            context = oldContext;
            var span = CreateSpanFromTokens();
            return new BlockStatement { Body = body, Span = span } as Node;
        }

        private Node ParseForStatement()
        {
            // for i in 1 to 10 step 5 { }
            Advance();
            var id = ParseIdentifier();
            ExpectKeyword("in");
            var start = ParseExpression();
            ExpectKeyword("to");
            var end = ParseExpression();
            Node step = null;
            if (CheckKeyword("step"))
            {
                Advance();
                step = ParseExpression();
            }
            var body = ParseStatement();
            var span = CreateSpanFromTokens();
            return new ForStatement
            {
                Iterator = id as Node,
                Start = start as Node,
                End = end as Node,
                Step = step as Node,
                Body = body as Node,
                Span = span
            } as Node;
        }

        private Node ParseFunctionStatement()
        {
            Advance(); // consume 'fn'
            Node id = null;
            if (CheckIdentifier())
            {
                id = ParseIdentifier();
            }

            Expect(TokenKind.LeftParen);
            var parameters = ParseParameters();
            Expect(TokenKind.RightParen);

            var body = ParseFunctionBody();
            var span = CreateSpanFromTokens();

            return new FunctionDeclaration
            {
                Id = id,
                Params = parameters,
                Body = body,
                IsAsync = false,
                Span = span
            } as Node;
        }

        private List<Node> ParseParameters()
        {
            var parameters = new List<Node>();
            while (!Check(TokenKind.RightParen) && !IsEof())
            {
                parameters.Add(ParseIdentifier());
                if (Check(TokenKind.Comma))
                    Advance();
            }
            return parameters;
        }

        private List<Node> ParseArguments()
        {
            var args = new List<Node>();
            while (!Check(TokenKind.RightParen) && !IsEof())
            {
                args.Add(ParseExpression());
                if (Check(TokenKind.Comma))
                    Advance();
            }
            return args;
        }

        private Node ParseFunctionBody()
        {
            return ParseBlockStatement();
        }

        private Node ParsePrintStatement()
        {
            Advance(); // consume 'print'
            var expr = ParseExpression();
            if (Check(TokenKind.Semicolon))
                Advance();
            var span = CreateSpanFromTokens();
            return new PrintStatement { Argument = expr, Span = span } as Node;
        }

        private Node ParseIfStatement()
        {
            Advance(); // consume 'if'
            Expect(TokenKind.LeftParen);
            var expr = ParseExpression();
            Expect(TokenKind.RightParen);

            var stmt = ParseStatement();

            Node elsePart = null;
            if (current != null && IsKeyword(current, out var kw) && kw == "else")
            {
                Advance();
                elsePart = ParseStatement();
            }

            var span = CreateSpanFromTokens();
            return new IfStatement { Expr = expr, Stmt = stmt, ElsePart = elsePart, Span = span } as Node;
        }

        private Node ParseVariableDeclaration()
        {
            var kind = "let";
            if (current != null && IsKeyword(current, out var kw))
            {
                kind = kw;
            }

            Advance(); // consume keyword

            var declarations = new List<VariableDeclarator>();

            while (true)
            {
                var id = ParseIdentifier();
                var varType = TryParseIdentifierType();
                Node init = null;
                if (Check(TokenKind.Arrow))
                {
                    Advance();
                    init = ParseExpression();
                }

                var span = CreateSpanFromTokens();
                declarations.Add(new VariableDeclarator { VarName = id, VarType = varType, Init = init, Span = span });

                if (!Check(TokenKind.Comma))
                    break;
                Advance();
            }

            if (Check(TokenKind.Semicolon))
                Advance();

            var pan = CreateSpanFromTokens();
            return new VariableDeclaration { Kind = kind, Declarations = declarations, Span = pan } as Node;
        }

        private Node ParseIdentifier()
        {
            if (current == null)
                throw ParserError.UnexpectedEndOfFile(null);

            if (IsIdentifier(current, out var name))
            {
                Advance();
                return new IdentifierNode { Name = name } as Node;
            }

            throw ParserError.InvalidSyntax("Expected identifier", CurrentPosition() ?? new Position());
        }

        private Node ParseExpressionStatement()
        {
            var expr = ParseExpression();
            if (Check(TokenKind.Semicolon))
                Advance();
            var span = CreateSpanFromTokens();
            return new ExpressionStatement { Expression = expr, Span = span } as Node;
        }

        private Node ParseExpression()
        {
            return ParseAssignmentExpression();
        }

        private Node ParseAssignmentExpression()
        {
            var left = ParseLogicalOrExpression();
            if (IsAssignmentOperator())
            {
                var operatorStr = CurrentTokenString();
                Advance();
                var right = ParseAssignmentExpression();
                var span = CreateSpanFromTokens();
                return new AssignmentExpression { Left = left, Operator = operatorStr, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseLogicalOrExpression()
        {
            var left = ParseLogicalAndExpression();
            while (Check(TokenKind.LogicalOr))
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseLogicalAndExpression();
                var span = CreateSpanFromTokens();
                left = new LogicalExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseLogicalAndExpression()
        {
            var left = ParseEqualityExpression();
            while (Check(TokenKind.LogicalAnd))
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseEqualityExpression();
                var span = CreateSpanFromTokens();
                left = new LogicalExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseEqualityExpression()
        {
            var left = ParseRelationalExpression();
            while (IsEqualityOperator())
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseRelationalExpression();
                var span = CreateSpanFromTokens();
                left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseRelationalExpression()
        {
            var left = ParseAdditiveExpression();
            while (IsRelationalOperator())
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseAdditiveExpression();
                var span = CreateSpanFromTokens();
                left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseAdditiveExpression()
        {
            var left = ParseMultiplicativeExpression();
            while (IsAdditiveOperator())
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseMultiplicativeExpression();
                var span = CreateSpanFromTokens();
                left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseMultiplicativeExpression()
        {
            var left = ParseUnaryExpression();
            while (IsMultiplicativeOperator())
            {
                var op = CurrentTokenString();
                Advance();
                var right = ParseUnaryExpression();
                var span = CreateSpanFromTokens();
                left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span } as Node;
            }
            return left;
        }

        private Node ParseUnaryExpression()
        {
            if (IsUnaryOperator())
            {
                var op = CurrentTokenString();
                var prefix = true;
                Advance();
                var argument = ParseUnaryExpression();
                var span = CreateSpanFromTokens();
                return new UnaryExpression { Operator = op, Argument = argument, Prefix = prefix, Span = span } as Node;
            }
            return ParsePostfixExpression();
        }

        private Node ParsePostfixExpression()
        {
            var expr = ParsePrimaryExpression();
            while (true)
            {
                if (current == null) break;
                if (Check(TokenKind.LeftParen))
                {
                    Advance();
                    var args = ParseArguments();
                    Expect(TokenKind.RightParen);
                    var span = CreateSpanFromTokens();
                    expr = new CallExpression { Callee = expr, Arguments = args, Span = span } as Node;
                    continue;
                }
                break;
            }
            return expr;
        }

        private void Expect(TokenKind kind)
        {
            if (Check(kind))
            {
                Advance();
                return;
            }

            var currentToken = CurrentToken();
            throw ParserError.UnexpectedToken(currentToken ?? throw new InvalidOperationException("No current token"), kind.ToString());
        }

        private bool IsUnaryOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.Plus) || Check(TokenKind.Minus) || Check(TokenKind.Increment) || Check(TokenKind.Decrement);
        }

        private bool IsMultiplicativeOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.Star) || Check(TokenKind.Slash) || Check(TokenKind.Percent);
        }

        private bool IsAdditiveOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.Plus) || Check(TokenKind.Minus);
        }

        private bool IsRelationalOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.LessThan) || Check(TokenKind.GreaterThan) || Check(TokenKind.LessThanEqual) || Check(TokenKind.GreaterThanEqual);
        }

        private bool IsAssignmentOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.Arrow) || Check(TokenKind.Assign) || Check(TokenKind.PlusAssign) || Check(TokenKind.MinusAssign);
        }

        private bool IsEqualityOperator()
        {
            if (current == null) return false;
            return Check(TokenKind.EqualEqual) || Check(TokenKind.NotEqual);
        }

        private Node ParsePrimaryExpression()
        {
            if (current == null)
                throw ParserError.UnexpectedEndOfFile(null);

            var kindStr = current.Kind.ToString();
            if (Check(TokenKind.Number))
            {
                var value = current.Number ?? 0.0;
                Advance();
                return new NumberNode { Value = value } as Node;
            }
            if (IsIdentifier(current, out var name))
            {
                Advance();
                return new IdentifierNode { Name = name } as Node;
            }
            if (Check(TokenKind.String))
            {
                var s = current.Value ?? string.Empty;
                Advance();
                return new StringNode { Value = s } as Node;
            }
            if (Check(TokenKind.Boolean))
            {
                var b = (current.Value ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                Advance();
                return new BooleanNode { Value = b } as Node;
            }
            if (Check(TokenKind.Null))
            {
                Advance();
                return Node.Null;
            }
            if (Check(TokenKind.Undefined))
            {
                Advance();
                return Node.Undefined;
            }

            if (CheckIdentifier())
            {
                return ParseIdentifier();
            }

            throw ParserError.UnexpectedToken(current, "Expected primary expression");
        }

        private Node TryParseIdentifierType()
        {
            if (Check(TokenKind.Colon))
            {
                Advance();
                if (current != null && IsKeyword(current, out var kw))
                {
                    Advance();
                    return new IdentifierNode { Name = kw } as Node;
                }
            }
            return null;
        }

        private bool CheckIdentifier()
        {
            return current != null && IsIdentifier(current, out _);
        }

        private string CurrentTokenString()
        {
            if (current == null) return "EOF";
            // Try common token kinds
            if (Check(TokenKind.Plus)) return "+";
            if (Check(TokenKind.Minus)) return "-";
            if (Check(TokenKind.Star)) return "*";
            if (Check(TokenKind.Slash)) return "/";
            if (Check(TokenKind.Percent)) return "%";
            if (Check(TokenKind.Equal)) return "=";
            if (Check(TokenKind.EqualEqual)) return "==";
            if (Check(TokenKind.LessThan)) return "<";
            if (Check(TokenKind.GreaterThan)) return ">";
            if (Check(TokenKind.LessThanEqual)) return "<=";
            if (Check(TokenKind.GreaterThanEqual)) return ">=";
            if (Check(TokenKind.PlusAssign)) return "+=";
            if (Check(TokenKind.MinusAssign)) return "-=";
            if (Check(TokenKind.Increment)) return "++";
            if (Check(TokenKind.Decrement)) return "--";
            if (IsIdentifier(current, out var name)) return name;
            if (Check(TokenKind.String)) return current.Value ?? string.Empty;
            if (Check(TokenKind.Boolean)) return (current.Value ?? "false").ToString();
            if (Check(TokenKind.Number)) return current.Number?.ToString() ?? "0";
            if (Check(TokenKind.Eof)) return "EOF";
            return current.Kind.ToString();
        }

        private bool Check(TokenKind kind)
        {
            if (current == null) return false;
            return current.Kind.Equals(kind);
        }

        private void Advance()
        {
            previous = current;
            try
            {
                current = lexer.NextToken();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Lexer error: {ex}");
                current = null;
            }
        }

        private bool IsEof()
        {
            return current == null || Check(TokenKind.Eof);
        }

        private Position CurrentPosition()
        {
            if (current == null) return null;
            return new Position(current.StartLine, current.StartColumn);
        }

        private Position PreviousPosition()
        {
            if (previous == null) return null;
            return new Position(previous.EndLine, previous.EndColumn);
        }

        private Token CurrentToken()
        {
            return current;
        }

        private NodeSpan CreateSpanFromTokens()
        {
            var start = PreviousPosition() ?? new Position();
            var end = CurrentPosition() ?? new Position();
            return new NodeSpan(start, end);
        }

        private NodeSpan CreateSpan(Position start, Position end)
        {
            var s = start ?? new Position();
            var e = end ?? new Position();
            return new NodeSpan(s, e);
        }

        private bool TryRecoverFromError(ParserError error)
        {
            if (!errorRecovery.CanRecover()) return false;

            errorRecovery.RecordError(error);

            var ctx = new RecoveryContext(CurrentToken(), previous, context);
            var strategy = ctx.DetermineStrategy();

            if (strategy is RecoveryStrategy.SkipUntil skip)
            {
                var tokens = skip.Tokens;
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t != null)
                    {
                        foreach (var tok in tokens)
                        {
                            if (t.Kind.ToString().Contains(tok))
                            {
                                return true;
                            }
                        }
                    }
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.SkipUntilStatement)
            {
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t == null) break;
                    if (t.Kind == TokenKind.Semicolon || t.Kind == TokenKind.RightBrace) break;
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.SkipUntilBlock)
            {
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t != null && t.Kind == TokenKind.RightBrace) break;
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.SkipUntilFunction)
            {
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t == null) break;
                    if (t.Kind == TokenKind.RightBrace || t.Kind == TokenKind.Semicolon) break;
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.SkipUntilClass)
            {
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t != null && t.Kind == TokenKind.RightBrace) break;
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.SkipUntilModule)
            {
                while (!IsEof())
                {
                    var t = CurrentToken();
                    if (t != null)
                    {
                        if (t.Kind == TokenKind.RightBrace || (IsKeyword(t, out var k) && (k == "import" || k == "export"))) break;
                    }
                    Advance();
                }
                return true;
            }

            if (strategy is RecoveryStrategy.InsertToken || strategy is RecoveryStrategy.ReplaceToken || strategy is RecoveryStrategy.DeleteToken)
            {
                Advance();
                return true;
            }

            return false;
        }

        // Helper keyword/identifier checks - adapt to your Token implementation
        private bool IsKeyword(Token token, out string keyword)
        {
            keyword = null;
            if (token == null) return false;
            if (token.Kind == TokenKind.Keyword)
            {
                keyword = token.Value ?? string.Empty;
                return true;
            }
            return false;
        }

        private bool CheckKeyword(string kw)
        {
            return current != null && IsKeyword(current, out var k) && k == kw;
        }

        private void ExpectKeyword(string kw)
        {
            if (!CheckKeyword(kw))
                throw ParserError.UnexpectedToken(current, $"Keyword {kw}");
            Advance();
        }

        private bool IsIdentifier(Token token, out string name)
        {
            name = null;
            if (token == null) return false;
            if (token.Kind == TokenKind.Identifier)
            {
                name = token.Value ?? string.Empty;
                return true;
            }
            return false;
        }
    }
}
