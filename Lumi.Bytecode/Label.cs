namespace Lumi.Bytecode;

/// <summary>
/// Represents a unique label identified by an integer ID.
/// </summary>
/// <param name="id">The unique identifier for the label. Must be a positive integer.</param>
public readonly struct Label(int id)
{
    public int Id { get; } = id;

    public override string ToString() => $"L{Id}";
}