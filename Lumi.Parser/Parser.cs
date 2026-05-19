using Lumi.AST;
using Lumi.Language;
using Lumi.Lexer;

namespace Lumi.Parser;

/// <summary>
/// Parser class responsible for converting a stream of tokens produced by the lexer into an Abstract Syntax Tree (AST) representing the structure of the source code. 
/// It implements a recursive descent parsing strategy, handling various language constructs such as expressions, statements, and declarations. 
/// The parser also includes error recovery mechanisms to gracefully handle syntax errors and continue parsing where possible.
/// </summary>
public sealed class Parser
{
    //private string _source;
    //private bool _strictMode;
    public bool HasErrors => errorRecovery.HasErrors();
    public IReadOnlyList<ParserError> Errors => errorRecovery.Errors;

    private readonly Lexer.Lexer lexer;
    private Token? current;
    private Token? previous;
    private readonly ErrorRecovery errorRecovery;
    private ParsingContext context;

    public Parser(string source)
    {
        //_source = source;
        //_strictMode = false;

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
        return ParseProgram;
    }

    private Node ParseProgram
    {
        get
        {
            var body = new List<Node>();
            var startPos = CurrentPosition();

            if (IsEof())
            {
                var endPos = PreviousPosition();
                var span = CreateSpan(startPos, endPos);
                return new Program { Body = body, Span = span };
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
            return new Program { Body = body, Span = programSpan };
        }
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
                    case "var":
                        result = ParseVariableDeclaration;
                        break;
                    case "if":
                        result = ParseIfStatement;
                        break;
                    case "print":
                        result = ParsePrintStatement;
                        break;
                    case "fn":
                        result = ParseFunctionStatement;
                        break;
                    case "struct":
                        result = ParseStructStatement;
                        break;
                    case "for":
                        result = ParseForStatement;
                        break;
                    case "return":
                        result = ParseReturnStatement;
                        break;
                    default:
                        result = ParseExpressionStatement;
                        break;
                }
            }
            else if (Check(TokenKind.LeftBrace))
            {
                result = ParseBlockStatement;
            }
            else
            {
                result = ParseExpressionStatement;
            }
        }
        else
        {
            context = oldContext;
            throw ParserError.UnexpectedEndOfFile();
        }

        context = oldContext;
        return result;
    }

    private Node ParseBlockStatement
    {
        get
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
            return new BlockStatement { Body = body, Span = span };
        }
    }

    private Node ParseForStatement
    {
        get
        {
            // for i in 1 to 10 step 5 { }
            Advance();
            var id = ParseIdentifier;

            ExpectKeyword("in");
            var start = ParseExpression();

            ExpectKeyword("to");
            var end = ParseExpression();

            Node? step = null;

            if (CheckKeyword("step"))
            {
                Advance();
                step = ParseExpression();
            }

            var body = ParseStatement();
            var span = CreateSpanFromTokens();

            return new ForStatement
            {
                Iterator = id,
                Start = start,
                End = end,
                Step = step,
                Body = body,
                Span = span
            };
        }
    }

    private Node ParseFunctionStatement
    {
        get
        {
            Advance(); // consume 'fn'
            Node id;

            if (CheckIdentifier())
            {
                id = ParseIdentifier;
            }
            else
            {
                throw ParserError.NoFunctionIdentifierFound(CurrentPosition() ?? new Position());
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
            };
        }
    }

    private Node ParseStructStatement
    {
        get
        {
            Advance(); // consume 'struct'

            if (!CheckIdentifier())
                throw ParserError.InvalidSyntax("Expected struct name", CurrentPosition() ?? new Position());

            var name = (IdentifierNode)ParseIdentifier;

            Expect(TokenKind.LeftBrace);

            var fields = new List<StructFieldDeclaration>();
            var methods = new List<FunctionDeclaration>();

            while (!Check(TokenKind.RightBrace) && !IsEof())
            {
                if (CheckKeyword("fn"))
                {
                    var method = (FunctionDeclaration)ParseFunctionStatement;
                    method.OwningStructName = name.Name;
                    methods.Add(method);

                    continue;
                }

                if (!CheckIdentifier())
                    throw ParserError.InvalidSyntax("Expected struct field name or method declaration", CurrentPosition() ?? new Position());

                var fieldName = (IdentifierNode)ParseIdentifier;

                Expect(TokenKind.Colon);

                IdentifierNode fieldType;
                if (current != null && IsKeyword(current, out var keywordType))
                {
                    Advance();
                    fieldType = new IdentifierNode { Name = keywordType };
                }
                else if (current != null && IsIdentifier(current, out var identifierType))
                {
                    Advance();
                    fieldType = new IdentifierNode { Name = identifierType };
                }
                else
                {
                    throw ParserError.InvalidSyntax("Expected struct field type", CurrentPosition() ?? new Position());
                }

                if (Check(TokenKind.Semicolon))
                    Advance();

                fields.Add(new StructFieldDeclaration { Name = fieldName, Type = fieldType, Span = CreateSpanFromTokens() });
            }

            Expect(TokenKind.RightBrace);

            if (Check(TokenKind.Semicolon))
                Advance();

            var span = CreateSpanFromTokens();

            return new StructDeclaration { Name = name, Fields = fields, Methods = methods, Span = span };
        }
    }

    private List<Node> ParseParameters()
    {
        var parameters = new List<Node>();

        while (!Check(TokenKind.RightParen) && !IsEof())
        {
            parameters.Add(ParseIdentifier);
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
        return ParseBlockStatement;
    }

    private Node ParsePrintStatement
    {
        get
        {
            Advance(); // consume 'print'
            var expr = ParseExpression();

            if (Check(TokenKind.Semicolon))
                Advance();

            var span = CreateSpanFromTokens();

            return new PrintStatement { Argument = expr, Span = span };
        }
    }

    private Node ParseReturnStatement
    {
        get
        {
            Advance(); // consume 'return'

            // Return statement can have an optional expression
            Node? argument = null;

            // Check if there's an expression to return
            if (!Check(TokenKind.Semicolon) && !Check(TokenKind.RightBrace) && !IsEof())
            {
                argument = ParseExpression();
            }

            if (Check(TokenKind.Semicolon))
                Advance();

            var span = CreateSpanFromTokens();

            return new ReturnStatement { Argument = argument, Span = span };
        }
    }

    private Node ParseIfStatement
    {
        get
        {
            Advance(); // consume 'if'

            Expect(TokenKind.LeftParen);
            var expr = ParseExpression();
            Expect(TokenKind.RightParen);

            var stmt = ParseStatement();

            Node? elsePart = null;

            if (current != null && IsKeyword(current, out var kw) && kw == "else")
            {
                Advance();
                elsePart = ParseStatement();
            }

            var span = CreateSpanFromTokens();

            return new IfStatement { Expr = expr, Stmt = stmt, ElsePart = elsePart, Span = span };
        }
    }

    private Node ParseVariableDeclaration
    {
        get
        {
            IsKeyword(current, out var kw);
            var kind = kw;

            Advance(); // consume keyword

            var declarations = new List<VariableDeclarator>();

            while (true)
            {
                var id = ParseIdentifier;
                var varType = TryParseIdentifierType;
                Node? init = null;

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

            return new VariableDeclaration { Kind = kind, Declarations = declarations, Span = pan };
        }
    }

    private Node ParseIdentifier
    {
        get
        {
            if (current == null)
                throw ParserError.UnexpectedEndOfFile();

            if (IsIdentifier(current, out var name))
            {
                Advance();
                return new IdentifierNode { Name = name };
            }

            throw ParserError.InvalidSyntax("Expected identifier", CurrentPosition() ?? new Position());
        }
    }

    private Node ParseExpressionStatement
    {
        get
        {
            var expr = ParseExpression();

            if (Check(TokenKind.Semicolon))
                Advance();

            var span = CreateSpanFromTokens();

            return new ExpressionStatement { Expression = expr, Span = span };
        }
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

            return new AssignmentExpression { Left = left, Operator = operatorStr, Right = right, Span = span };
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

            left = new LogicalExpression { Left = left, Operator = op, Right = right, Span = span };
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

            left = new LogicalExpression { Left = left, Operator = op, Right = right, Span = span };
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

            left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span };
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

            left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span };
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

            left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span };
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

            left = new BinaryExpression { Left = left, Operator = op, Right = right, Span = span };
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

            return new UnaryExpression { Operator = op, Argument = argument, Prefix = prefix, Span = span };
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

                expr = new CallExpression { Callee = expr, Arguments = args, Span = span };

                continue;
            }

            if (Check(TokenKind.Dot))
            {
                Advance();

                if (current is null || current.Value.Kind != TokenKind.Identifier)
                    throw ParserError.UnexpectedToken(CurrentToken(), "Expected identifier after '.'");

                var property = new IdentifierNode { Name = current.Value.Value ?? string.Empty };
                Advance();

                var span = CreateSpanFromTokens();
                expr = new MemberExpression { Object = expr, Property = property, Span = span };

                continue;
            }

            if (Check(TokenKind.LeftBracket))
            {
                Advance();
                var index = ParseExpression();
                Expect(TokenKind.RightBracket);

                var span = CreateSpanFromTokens();

                expr = new IndexExpression { Object = expr, Index = index, Span = span };

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

        throw ParserError.UnexpectedToken(CurrentToken(), kind.ToString());
    }

    private bool IsUnaryOperator() =>
        current?.Kind is TokenKind.Plus or TokenKind.Minus or TokenKind.Increment or TokenKind.Decrement;

    private bool IsMultiplicativeOperator() =>
        current?.Kind is TokenKind.Star or TokenKind.Slash or TokenKind.Percent;

    private bool IsAdditiveOperator() =>
        current?.Kind is TokenKind.Plus or TokenKind.Minus;

    private bool IsRelationalOperator() =>
        current?.Kind is TokenKind.LessThan or TokenKind.GreaterThan or TokenKind.LessThanEqual or TokenKind.GreaterThanEqual;

    private bool IsAssignmentOperator() =>
        current?.Kind is TokenKind.Arrow or TokenKind.Assign or TokenKind.PlusAssign or TokenKind.MinusAssign;

    private bool IsEqualityOperator() =>
        current?.Kind is TokenKind.EqualEqual or TokenKind.NotEqual or TokenKind.Equal;

    private Node ParsePrimaryExpression()
    {
        var tok = current ?? throw ParserError.UnexpectedEndOfFile();

        if (tok.Kind == TokenKind.Keyword && string.Equals(tok.Value, "new", StringComparison.Ordinal))
        {
            Advance(); // consume 'new'

            if (current == null || !IsIdentifier(current, out var typeName))
                throw ParserError.InvalidSyntax("Expected type name after 'new'", CurrentPosition() ?? new Position());

            Advance();

            var arguments = new List<Node>();
            if (Check(TokenKind.LeftParen))
            {
                Advance();
                arguments = ParseArguments();
                Expect(TokenKind.RightParen);
            }

            return new NewExpression { TypeName = new IdentifierNode { Name = typeName }, Arguments = arguments };
        }

        if (tok.Kind == TokenKind.Keyword && string.Equals(tok.Value, "this", StringComparison.Ordinal))
        {
            Advance();
            return new IdentifierNode { Name = "this" };
        }

        switch (tok.Kind)
        {
            case TokenKind.LeftBracket:
                return ParseArrayLiteral();

            case TokenKind.Number:
                var value = tok.Number ?? 0.0;
                Advance();
                return new NumberNode { Value = value };

            case TokenKind.Identifier:
                var name = tok.Value ?? string.Empty;
                Advance();
                return new IdentifierNode { Name = name };

            case TokenKind.String:
                var s = tok.Value ?? string.Empty;
                Advance();
                return new StringNode { Value = s };

            case TokenKind.Boolean:
                var b = (tok.Value ?? "false").Equals("true", StringComparison.OrdinalIgnoreCase);
                Advance();
                return new BooleanNode { Value = b };

            case TokenKind.Null:
                Advance();
                return Node.Null;

            case TokenKind.Undefined:
                Advance();
                return Node.Undefined;

            default:
                throw ParserError.UnexpectedToken(tok, "Expected primary expression");
        }
    }

    private Node ParseArrayLiteral()
    {
        Expect(TokenKind.LeftBracket);

        var elements = new List<Node>();
        while (!Check(TokenKind.RightBracket) && !IsEof())
        {
            elements.Add(ParseExpression());

            if (Check(TokenKind.Comma))
            {
                Advance();
                continue;
            }

            if (!Check(TokenKind.RightBracket))
                throw ParserError.UnexpectedToken(CurrentToken(), "Expected ',' or ']' in array literal");
        }

        Expect(TokenKind.RightBracket);

        var span = CreateSpanFromTokens();
        return new ArrayLiteral { Elements = elements, Span = span };
    }

    private Node? TryParseIdentifierType
    {
        get
        {
            if (Check(TokenKind.Colon))
            {
                Advance();
                if (current != null && IsKeyword(current, out var kw))
                {
                    var baseTypeName = kw;
                    Advance();

                    // Check for parameterized type syntax: only list supports type parameters
                    if (Check(TokenKind.LessThan))
                    {
                        // Only allow parameterized types for list
                        if (!string.Equals(baseTypeName, "list", StringComparison.OrdinalIgnoreCase))
                        {
                            throw ParserError.InvalidSyntax("Only 'list' types can have type parameters", CurrentPosition() ?? new Position());
                        }

                        Advance(); // consume '<'

                        // Parse the type argument
                        Node? typeArg = null;
                        if (current != null && IsKeyword(current, out var paramKw))
                        {
                            typeArg = new IdentifierNode { Name = paramKw };
                            Advance();
                        }
                        else if (current != null && IsIdentifier(current, out var paramId))
                        {
                            typeArg = new IdentifierNode { Name = paramId };
                            Advance();
                        }

                        if (typeArg == null)
                            throw ParserError.InvalidSyntax("Expected type parameter in generic type", CurrentPosition() ?? new Position());

                        Expect(TokenKind.GreaterThan);

                        return new ParameterizedTypeNode { BaseTypeName = baseTypeName, TypeArgument = typeArg };
                    }

                    return new IdentifierNode { Name = baseTypeName };
                }

                if (current != null && IsIdentifier(current, out var id))
                {
                    var baseTypeName = id;
                    Advance();

                    // Check for parameterized type syntax: only list supports type parameters
                    if (Check(TokenKind.LessThan))
                    {
                        if (!string.Equals(baseTypeName, "list", StringComparison.OrdinalIgnoreCase))
                        {
                            throw ParserError.InvalidSyntax("Only 'list' types can have type parameters", CurrentPosition() ?? new Position());
                        }

                        Advance(); // consume '<'

                        // Parse the type argument
                        Node? typeArg = null;
                        if (current != null && IsKeyword(current, out var paramKw))
                        {
                            typeArg = new IdentifierNode { Name = paramKw };
                            Advance();
                        }
                        else if (current != null && IsIdentifier(current, out var paramId))
                        {
                            typeArg = new IdentifierNode { Name = paramId };
                            Advance();
                        }

                        if (typeArg == null)
                            throw ParserError.InvalidSyntax("Expected type parameter in generic type", CurrentPosition() ?? new Position());

                        Expect(TokenKind.GreaterThan);

                        return new ParameterizedTypeNode { BaseTypeName = baseTypeName, TypeArgument = typeArg };
                    }

                    return new IdentifierNode { Name = baseTypeName };
                }

                throw ParserError.UnexpectedToken(CurrentToken(), "Expected a type name after ':'");
            }

            return null;
        }
    }

    private bool CheckIdentifier()
    {
        return current != null && IsIdentifier(current, out _);
    }

    private string CurrentTokenString()
    {
        if (current == null) return "EOF";
        var tok = current.Value;
        return tok.Kind switch
        {
            TokenKind.Plus => "+",
            TokenKind.Minus => "-",
            TokenKind.Star => "*",
            TokenKind.Slash => "/",
            TokenKind.Percent => "%",
            TokenKind.Equal => "=",
            TokenKind.EqualEqual => "==",
            TokenKind.LessThan => "<",
            TokenKind.GreaterThan => ">",
            TokenKind.LessThanEqual => "<=",
            TokenKind.GreaterThanEqual => ">=",
            TokenKind.PlusAssign => "+=",
            TokenKind.MinusAssign => "-=",
            TokenKind.Increment => "++",
            TokenKind.Decrement => "--",
            TokenKind.Identifier => tok.Value ?? string.Empty,
            TokenKind.String => tok.Value ?? string.Empty,
            TokenKind.Boolean => tok.Value ?? "false",
            TokenKind.Number => tok.Number?.ToString() ?? "0",
            TokenKind.Eof => "EOF",
            _ => tok.Kind.ToString()
        };
    }

    private bool Check(TokenKind kind) => current?.Kind == kind;

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

    private Position? CurrentPosition() =>
        current is { } tok ? new Position(tok.StartLine, tok.StartColumn) : null;

    private Position? PreviousPosition() =>
        previous is { } tok ? new Position(tok.EndLine, tok.EndColumn) : null;

    private Token CurrentToken()
        => current ?? throw ParserError.NoCurrentTokenFound(CurrentPosition() ?? new Position());

    private NodeSpan CreateSpanFromTokens()
    {
        var start = PreviousPosition() ?? new Position();
        var end = CurrentPosition() ?? new Position();

        return new NodeSpan(start, end);
    }

    private static NodeSpan CreateSpan(Position? start, Position? end)
    {
        var s = start ?? new Position();
        var e = end ?? new Position();

        return new NodeSpan(s, e);
    }

    private bool TryRecoverFromError(ParserError error)
    {
        if (!errorRecovery.CanRecover()) return false;

        errorRecovery.RecordError(error);

        var ctx = new RecoveryContext(CurrentToken(), previous ?? default, context);
        var strategy = ctx.DetermineStrategy();

        if (strategy is RecoveryStrategy.SkipUntil skip)
        {
            var tokens = skip.Tokens;
            while (!IsEof())
            {
                var t = CurrentToken();
                foreach (var tok in tokens)
                {
                    if (t.Kind.ToString().Contains(tok))
                        return true;
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
                if (t.Kind == TokenKind.Semicolon || t.Kind == TokenKind.RightBrace) break;
                Advance();
            }
            return true;
        }

        if (strategy is RecoveryStrategy.SkipUntilBlock)
        {
            while (!IsEof())
            {
                if (CurrentToken().Kind == TokenKind.RightBrace) break;
                Advance();
            }
            return true;
        }

        if (strategy is RecoveryStrategy.SkipUntilFunction)
        {
            while (!IsEof())
            {
                var t = CurrentToken();
                if (t.Kind == TokenKind.RightBrace || t.Kind == TokenKind.Semicolon) break;
                Advance();
            }
            return true;
        }

        if (strategy is RecoveryStrategy.SkipUntilClass)
        {
            while (!IsEof())
            {
                if (CurrentToken().Kind == TokenKind.RightBrace) break;
                Advance();
            }
            return true;
        }

        if (strategy is RecoveryStrategy.SkipUntilModule)
        {
            while (!IsEof())
            {
                var t = CurrentToken();
                if (t.Kind == TokenKind.RightBrace || (IsKeyword(t, out var k) && (k == "import" || k == "export"))) break;
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

    private static bool IsKeyword(Token? token, out string keyword)
    {
        keyword = string.Empty;
        if (!token.HasValue) return false;

        var tok = token.Value;
        if (tok.Kind != TokenKind.Keyword) return false;

        keyword = tok.Value ?? string.Empty;

        return IdentifierClassifier.IsKeywordLike(keyword);
    }

    private bool CheckKeyword(string kw)
    {
        return current != null && IsKeyword(current, out var k) && k == kw;
    }

    private void ExpectKeyword(string kw)
    {
        if (!CheckKeyword(kw))
            throw ParserError.UnexpectedToken(CurrentToken(), $"Keyword {kw}");

        Advance();
    }

    private static bool IsIdentifier(Token? token, out string name)
    {
        name = string.Empty;
        if (!token.HasValue) return false;
        var tok = token.Value;
        if (tok.Kind != TokenKind.Identifier) return false;
        name = tok.Value ?? string.Empty;
        return true;
    }
}
