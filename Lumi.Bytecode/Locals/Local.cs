namespace Lumi.Bytecode.Locals;

/// <summary>
/// Represents a local variable with its name, kind, associated label, and optional declared type.
/// </summary>
/// <param name="Name">The name of the local variable.</param>
/// <param name="Kind">The kind of the local variable, indicating its role or classification.</param>
/// <param name="Label">The label associated with the local variable, used for identification or control flow.</param>
/// <param name="Type">The declared type of the local variable, or <see langword="null"/> if none was specified.</param>
public readonly record struct Local(string Name, LocalKind Kind, Label Label, VarType Type = VarType.Unknown);