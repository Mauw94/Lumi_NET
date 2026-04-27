using System.Collections.Frozen;

namespace Lumi.Language;

/// <summary>
/// Provides the canonical list of language keywords used across the solution.
/// </summary>
public static class KeywordCatalog
{
    private static readonly FrozenSet<string> _all =
    new[]
    {
        "let", "const", "var", "fn", "if", "else", "return", "async", "await", "yield",
        "import", "export", "new", "class", "extends", "static", "get", "set",
        "try", "catch", "finally", "throw", "break", "continue", "switch", "case",
        "default", "for", "while", "do", "in", "of", "with", "delete",
        "instanceof", "typeof", "void", "debugger", "enum", "interface", "package",
        "private", "protected", "public", "implements", "abstract", "bool", "byte",
        "char", "double", "final", "float", "goto", "int", "long", "str",
        "native", "short", "synchronized", "throws", "transient", "volatile", "to",
        "step", "print", "list", "struct"
    }.ToFrozenSet(StringComparer.Ordinal);

    public static IReadOnlySet<string> All => _all;

    public static bool Contains(string? value) => value is not null && _all.Contains(value);
}
