namespace Lumi.AST;

public class NodeSpan
{
    public Position Start { get; set; }
    public Position End { get; set; }

    public NodeSpan(Position start, Position end)
    {
        Start = start ?? new Position();
        End = end ?? new Position();
    }
}
