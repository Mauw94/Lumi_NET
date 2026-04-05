namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Manages a stack of scopes for tracking variable declarations during semantic analysis.
/// </summary>
internal sealed class ScopeManager
{
    private readonly List<Dictionary<string, Symbol>> _scopes = new(capacity: 4);

    private Dictionary<string, Symbol> CurrentScope =>
        _scopes.Count > 0 ? _scopes[^1] : throw SemanticAnalyzerError.NoActiveScope();

    /// <summary>
    /// Returns a flat view of all symbols registered across all scopes.
    /// </summary>
    public IReadOnlyList<Symbol> AllSymbols => [.. _scopes.SelectMany(s => s.Values)];

    /// <summary>
    /// Enter a new scope.
    /// </summary>
    public void EnterScope()
    {
        _scopes.Add([]);
    }

    /// <summary>
    /// Exit the current scope, discarding all symbols declared within it.
    /// </summary>
    public void ExitScope()
    {
        if (_scopes.Count == 0)
            throw SemanticAnalyzerError.NoActiveScope();

        _scopes.RemoveAt(_scopes.Count - 1);
    }

    /// <summary>
    /// Registers a symbol in the current scope.
    /// </summary>
    /// <param name="symbol">The symbol to register.</param>
    /// <exception cref="SemanticAnalyzerError">Thrown if the symbol is already defined in the current scope.</exception>
    public void RegisterSymbol(Symbol symbol)
    {
        var scope = CurrentScope;

        if (scope.ContainsKey(symbol.Name))
            throw SemanticAnalyzerError.RedefinedVariable(symbol.Name);

        scope[symbol.Name] = symbol;
    }

    /// <summary>
    /// Looks up a symbol by name, searching from the innermost scope outward.
    /// </summary>
    /// <param name="name">The name of the symbol to find.</param>
    /// <returns>The symbol if found; otherwise, null.</returns>
    public Symbol? LookupSymbol(string name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].TryGetValue(name, out var symbol))
                return symbol;
        }
        return null;
    }
}
