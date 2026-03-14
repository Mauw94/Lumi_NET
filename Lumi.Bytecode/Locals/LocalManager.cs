namespace Lumi.Bytecode.Locals;

/// <summary>
/// Manages a stack of local variable scopes and provides methods to create, retrieve, and look up local variables
/// within those scopes.
/// </summary>
internal sealed class LocalManager
{
    private readonly List<Dictionary<string, Local>> _scopes = new(capacity: 4);
    private int _nextLabelId = 0;
    private Dictionary<string, Local> CurrentScope => _scopes.Count > 0 ? _scopes[^1] : throw BytecodeError.NoActiveScope();

    public void EnterScope()
    {
        _scopes.Add([]);
    }

    public void ExitScope()
    {
        if (_scopes.Count == 0) throw BytecodeError.NoActiveScope();
        _scopes.RemoveAt(_scopes.Count - 1);
    }

    /// <summary>
    /// Gets the existing label for the specified local variable name and kind in the current scope, or creates a new
    /// one if it does not exist.
    /// </summary>
    /// <param name="name">The name of the local variable for which to get or create a label. Cannot be null.</param>
    /// <param name="kind">The kind of the local variable to associate with the label.</param>
    /// <returns>A label associated with the specified local variable name and kind. If the local variable does not exist in the
    /// current scope, a new label is created and returned.</returns>
    public Label GetOrCreateLocal(string name, LocalKind kind)
    {
        var current = CurrentScope;
        if (current.TryGetValue(name, out var existing))
            return existing.Label;

        var newLabel = new Label(_nextLabelId++);
        var newLocal = new Local(name, kind, newLabel);
        current[name] = newLocal;

        return newLabel;
    }

    public Label GetOrCreateLocal(string name) => GetOrCreateLocal(name, LocalKind.Let);

    /// <summary>
    /// Searches for a local variable with the specified name in the current and enclosing scopes.
    /// </summary>
    /// <remarks>The search begins in the innermost scope and proceeds outward through enclosing scopes. If
    /// multiple variables share the same name in different scopes, the one in the closest (most nested) scope is
    /// returned.</remarks>
    /// <param name="name">The name of the local variable to locate. Cannot be null.</param>
    /// <returns>A <see cref="Local"/> instance representing the found local variable if one exists; otherwise, <see
    /// langword="null"/>.</returns>
    public Local? LookupLocal(string name)
    {
        for (int i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].TryGetValue(name, out var local))
                return local;
        }

        return null;
    }
}