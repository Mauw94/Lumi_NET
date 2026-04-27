namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Represents a symbol (variable or function) in the current scope.
/// </summary>
/// <param name="Name">The name of the symbol.</param>
/// <param name="Kind">Whether the symbol is a variable, constant, or function.</param>
/// <param name="Type">The inferred or declared type of the symbol.</param>
/// <param name="IsReadOnly">True if the symbol cannot be reassigned (const).</param>
/// <param name="ParameterCount">For function symbols, the number of declared parameters. Null for non-function symbols.</param>
/// <param name="StructName">For struct-typed symbols, the concrete struct name.</param>
public readonly record struct Symbol(string Name, SymbolKind Kind, TypeKind Type, bool IsReadOnly = false, int? ParameterCount = null, string? StructName = null);