namespace Lumi.AST;

public readonly record struct Position(int Line, int Column)
{
    public Position() : this(1, 1) { }

    public override string ToString() => $"{Line}:{Column}";
}
