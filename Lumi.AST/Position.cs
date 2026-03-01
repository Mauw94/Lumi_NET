namespace Lumi.AST;

public class Position
{
    public int Line { get; set; } = 1;
    public int Column { get; set; } = 1;

    public Position() { }
    public Position(int line, int column)
    {
        Line = line;
        Column = column;
    }

    public override string ToString() => $"{Line}:{Column}";
}
