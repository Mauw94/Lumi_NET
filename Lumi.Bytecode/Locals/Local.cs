namespace Lumi.Bytecode.Locals;

/// <summary>
/// Represents a local variable with its name, kind, and associated label information.
/// </summary>
/// <param name="Name">The name of the local variable.</param>
/// <param name="Kind">The kind of the local variable, indicating its role or classification.</param>
/// <param name="Label">The label associated with the local variable, used for identification or control flow.</param>
internal sealed record Local(string Name, LocalKind Kind, Label Label);