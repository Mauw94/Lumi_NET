namespace Lumi.AST;

/// <summary>
/// Represents a span of text in the source code, defined by a start and end position.
/// </summary>
/// <param name="Start">The starting position of the span.</param>
/// <param name="End">The ending position of the span.</param>
public readonly record struct NodeSpan(Position Start, Position End);