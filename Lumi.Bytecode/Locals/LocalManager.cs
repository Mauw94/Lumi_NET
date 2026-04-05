namespace Lumi.Bytecode.Locals;

/// <summary>
/// Manages a stack of local variable scopes and provides methods to create, retrieve, and look up local variables
/// within those scopes.
/// </summary>
internal sealed class LocalManager
{
    private readonly List<Dictionary<string, Local>> _scopes = new(capacity: 4);
    private int _nextSlotId = 0;
    private Dictionary<string, Local> CurrentScope => _scopes.Count > 0 ? _scopes[^1] : throw BytecodeError.NoActiveScope();

    /// <summary>
    /// Returns a flat view of every local registered across all scopes.
    /// </summary>
    public IReadOnlyList<Local> AllLocals => [.. _scopes.SelectMany(s => s.Values)];

    /// <summary>
    /// Enter a new scope.
    /// </summary>
    public void EnterScope()
    {
        _scopes.Add([]);
    }

    /// <summary>
    /// Exit the current scope, discarding all local variables declared within it. If there are no active scopes, an exception is thrown.
    /// </summary>
    public void ExitScope()
    {
        if (_scopes.Count == 0) throw BytecodeError.NoActiveScope();
        _scopes.RemoveAt(_scopes.Count - 1);
    }

    /// <summary>
    /// Saves the current slot counter and resets it to zero.
    /// Used when entering a function so its locals are numbered from 0 (relative to the function's base pointer).
    /// </summary>
    /// <returns>The saved slot counter to pass to <see cref="RestoreSlotCounter"/>.</returns>
    public int SaveAndResetSlotCounter()
    {
        var saved = _nextSlotId;
        _nextSlotId = 0;
        return saved;
    }

    /// <summary>
    /// Restores a previously saved slot counter (returned by <see cref="SaveAndResetSlotCounter"/>).
    /// Called after a function body has been compiled so global-scope numbering resumes correctly.
    /// </summary>
    public void RestoreSlotCounter(int savedCounter)
    {
        _nextSlotId = savedCounter;
    }

    /// <summary>
    /// Gets the existing label for the specified local variable name and kind in the current scope, or creates a new
    /// one if it does not exist.
    /// </summary>
    /// <param name="name">The name of the local variable for which to get or create a label. Cannot be null.</param>
    /// <param name="kind">The kind of the local variable to associate with the label.</param>
    /// <param name="type">The declared type, or <see langword="null"/> if no type annotation was given.</param>
    /// <returns>A label associated with the specified local variable name and kind. If the local variable does not exist in the
    /// current scope, a new label is created and returned.</returns>
    public Label GetOrCreateLocal(string name, LocalKind kind, VarType type = VarType.Unknown)
    {
        var current = CurrentScope;
        if (current.TryGetValue(name, out var existing))
            return existing.Label;

        var newLabel = new Label(_nextSlotId++);
        var newLocal = new Local(name, kind, newLabel, type);
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