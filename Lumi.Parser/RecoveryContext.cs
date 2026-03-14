using Lumi.AST;
using Lumi.Lexer;

namespace Lumi.Parser;

/// <summary>
/// Represents the context used for error recovery during parsing, including the current and previous tokens and the
/// parsing context.
/// </summary>
/// <remarks>Use this class to manage recovery state and determine appropriate recovery strategies when a parsing
/// error occurs. The recovery context helps guide the parser in resuming operation after encountering unexpected or
/// invalid tokens.</remarks>
/// <param name="currentToken">The token that is currently being processed by the parser.</param>
/// <param name="previousToken">The token that was processed immediately before the current token.</param>
/// <param name="context">The parsing context in which recovery is being performed. Determines the applicable recovery strategy.</param>
internal sealed class RecoveryContext(Token currentToken, Token previousToken, ParsingContext context)
{
    public Token CurrentToken { get; } = currentToken;
    public Token PreviousToken { get; } = previousToken;
    public List<string> RecoveryTokens { get; set; } = [];
    public ParsingContext Context { get; } = context;

    public RecoveryContext WithRecoveryTokens(List<string> tokens)
    {
        RecoveryTokens = tokens ?? [];
        return this;
    }

    public RecoveryStrategy DetermineStrategy()
    {
        switch (Context)
        {
            case ParsingContext.TopLevel:
                if (CurrentToken != null)
                {
                    switch (CurrentToken.Kind)
                    {
                        case TokenKind.Semicolon:
                        case TokenKind.RightBrace:
                            return new RecoveryStrategy.SkipUntil([";", "}"]);
                        default:
                            return new RecoveryStrategy.SkipUntilStatement();
                    }
                }
                return new RecoveryStrategy.NoRecovery();

            case ParsingContext.Statement:
                return new RecoveryStrategy.SkipUntil([";", "}", ")"]);

            case ParsingContext.Block:
                return new RecoveryStrategy.SkipUntil(["}"]);

            case ParsingContext.Function:
                return new RecoveryStrategy.SkipUntil(["}", ";"]);

            case ParsingContext.Class:
                return new RecoveryStrategy.SkipUntil(["}"]);

            case ParsingContext.Module:
                return new RecoveryStrategy.SkipUntil(["}", "import", "export"]);

            case ParsingContext.Expression:
                return new RecoveryStrategy.SkipUntil([";", ",", ")", "]", "}"]);

            case ParsingContext.Declaration:
                return new RecoveryStrategy.SkipUntil([";", "}"]);

            default:
                return new RecoveryStrategy.NoRecovery();
        }
    }

    public bool IsRecoveryToken(Token token)
    {
        if (token == null) return false;
        var tokenStr = token.Kind.ToString();
        return RecoveryTokens.Contains(tokenStr);
    }

    public Position? CurrentPosition()
    {
        if (CurrentToken == null) return null;
        return new Position(CurrentToken.StartLine, CurrentToken.StartColumn);
    }
}

