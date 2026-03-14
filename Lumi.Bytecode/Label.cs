namespace Lumi.Bytecode;

/// <summary>
/// Represents a unique label identified by an integer ID.
/// </summary>
/// <param name="Id">The unique identifier for the label. Must be a positive integer.</param>
public readonly record struct Label(int Id)
{
    public override string ToString() => $"L{Id}";
}