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
        return Context switch
        {
            ParsingContext.TopLevel => CurrentToken.Kind switch
            {
                TokenKind.Semicolon or TokenKind.RightBrace => new RecoveryStrategy.SkipUntil([";", "}"]),
                _ => new RecoveryStrategy.SkipUntilStatement(),
            },
            ParsingContext.Statement => new RecoveryStrategy.SkipUntil([";", "}", ")"]),
            ParsingContext.Block => new RecoveryStrategy.SkipUntil(["}"]),
            ParsingContext.Function => new RecoveryStrategy.SkipUntil(["}", ";"]),
            ParsingContext.Class => new RecoveryStrategy.SkipUntil(["}"]),
            ParsingContext.Module => new RecoveryStrategy.SkipUntil(["}", "import", "export"]),
            ParsingContext.Expression => new RecoveryStrategy.SkipUntil([";", ",", ")", "]", "}"]),
            ParsingContext.Declaration => new RecoveryStrategy.SkipUntil([";", "}"]),
            _ => new RecoveryStrategy.NoRecovery(),
        };
    }

    public bool IsRecoveryToken(Token token)
    {
        var tokenStr = token.Kind.ToString();
        return RecoveryTokens.Contains(tokenStr);
    }

    public Position? CurrentPosition()
    {
        return new Position(CurrentToken.StartLine, CurrentToken.StartColumn);
    }
}