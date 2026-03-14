using System.Collections;

namespace Lumi.Parser;

/// <summary>
/// Provides an abstract base class for defining error recovery strategies used during parsing operations.
/// </summary>
/// <remarks>Derived classes represent specific recovery strategies, such as skipping tokens, inserting or
/// replacing tokens, or halting recovery. These strategies enable parsers to handle syntax errors and continue
/// processing input in a controlled manner. Use a concrete implementation of this class to specify how the parser
/// should attempt to recover from an error.</remarks>
internal abstract class RecoveryStrategy
{
    private RecoveryStrategy() { }

    public sealed class SkipUntil(List<string> tokens) : RecoveryStrategy, IEnumerable<string>
    {
        public List<string> Tokens { get; } = tokens ?? [];

        public IEnumerator<string> GetEnumerator() => Tokens.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public sealed class SkipUntilStatement : RecoveryStrategy { public SkipUntilStatement() { } }
    public sealed class SkipUntilBlock : RecoveryStrategy { public SkipUntilBlock() { } }
    public sealed class SkipUntilFunction : RecoveryStrategy { public SkipUntilFunction() { } }
    public sealed class SkipUntilClass : RecoveryStrategy { public SkipUntilClass() { } }
    public sealed class SkipUntilModule : RecoveryStrategy { public SkipUntilModule() { } }
    public sealed class InsertToken(string token) : RecoveryStrategy
    {
        public string Token { get; } = token;
    }
    public sealed class ReplaceToken(string token) : RecoveryStrategy
    {
        public string Token { get; } = token;
    }
    public sealed class DeleteToken : RecoveryStrategy { public DeleteToken() { } }
    public sealed class NoRecovery : RecoveryStrategy { public NoRecovery() { } }
}