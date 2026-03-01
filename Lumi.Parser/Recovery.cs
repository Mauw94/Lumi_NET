using Lumi.AST;
using Lumi.Lexer;
using System.Collections;

namespace Lumi.Parser
{
    // Parsing context enum
    public enum ParsingContext
    {
        TopLevel,
        Statement,
        Block,
        Function,
        Class,
        Module,
        Expression,
        Declaration,
    }

    // Recovery strategy discriminated type
    public abstract class RecoveryStrategy
    {
        private RecoveryStrategy() { }

        public sealed class SkipUntil : RecoveryStrategy, IEnumerable<string>
        {
            public List<string> Tokens { get; }
            public SkipUntil(List<string> tokens)
            {
                Tokens = tokens ?? new List<string>();
            }

            public IEnumerator<string> GetEnumerator() => Tokens.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public sealed class SkipUntilStatement : RecoveryStrategy { public SkipUntilStatement() { } }
        public sealed class SkipUntilBlock : RecoveryStrategy { public SkipUntilBlock() { } }
        public sealed class SkipUntilFunction : RecoveryStrategy { public SkipUntilFunction() { } }
        public sealed class SkipUntilClass : RecoveryStrategy { public SkipUntilClass() { } }
        public sealed class SkipUntilModule : RecoveryStrategy { public SkipUntilModule() { } }
        public sealed class InsertToken : RecoveryStrategy { public string Token { get; } public InsertToken(string token) { Token = token; } }
        public sealed class ReplaceToken : RecoveryStrategy { public string Token { get; } public ReplaceToken(string token) { Token = token; } }
        public sealed class DeleteToken : RecoveryStrategy { public DeleteToken() { } }
        public sealed class NoRecovery : RecoveryStrategy { public NoRecovery() { } }
    }

    // Recovery context that holds parsing state for recovery decisions
    public class RecoveryContext
    {
        public Token CurrentToken { get; }
        public Token PreviousToken { get; }
        public List<string> RecoveryTokens { get; set; }
        public ParsingContext Context { get; }

        public RecoveryContext(Token currentToken, Token previousToken, ParsingContext context)
        {
            CurrentToken = currentToken;
            PreviousToken = previousToken;
            RecoveryTokens = new List<string>();
            Context = context;
        }

        public RecoveryContext WithRecoveryTokens(List<string> tokens)
        {
            RecoveryTokens = tokens ?? new List<string>();
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
                                return new RecoveryStrategy.SkipUntil(new List<string> { ";", "}" });
                            default:
                                return new RecoveryStrategy.SkipUntilStatement();
                        }
                    }
                    return new RecoveryStrategy.NoRecovery();

                case ParsingContext.Statement:
                    return new RecoveryStrategy.SkipUntil(new List<string> { ";", "}", ")" });

                case ParsingContext.Block:
                    return new RecoveryStrategy.SkipUntil(new List<string> { "}" });

                case ParsingContext.Function:
                    return new RecoveryStrategy.SkipUntil(new List<string> { "}", ";" });

                case ParsingContext.Class:
                    return new RecoveryStrategy.SkipUntil(new List<string> { "}" });

                case ParsingContext.Module:
                    return new RecoveryStrategy.SkipUntil(new List<string> { "}", "import", "export" });

                case ParsingContext.Expression:
                    return new RecoveryStrategy.SkipUntil(new List<string> { ";", ",", ")", "]", "}" });

                case ParsingContext.Declaration:
                    return new RecoveryStrategy.SkipUntil(new List<string> { ";", "}" });

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

        public Position CurrentPosition()
        {
            if (CurrentToken == null) return null;
            return new Position(CurrentToken.StartLine, CurrentToken.StartColumn);
        }
    }

    // Error recovery manager
    public class ErrorRecovery
    {
        private readonly int _maxErrors;
        private int _errorCount;
        private readonly List<ParserError> _errors;

        public ErrorRecovery(int maxErrors)
        {
            _maxErrors = maxErrors;
            _errorCount = 0;
            _errors = [];
        }

        public static ErrorRecovery Default() => new(10);

        public bool CanRecover() => _errorCount < _maxErrors;

        public void RecordError(ParserError error)
        {
            if (_errorCount < _maxErrors)
            {
                _errors.Add(error);
                _errorCount++;
            }
        }

        public bool HasErrors() => _errors.Count > 0;

        public void ClearErrors()
        {
            _errors.Clear();
            _errorCount = 0;
        }

        public IReadOnlyList<ParserError> Errors => _errors.AsReadOnly();

        public int ErrorCount => _errorCount;
    }
}
