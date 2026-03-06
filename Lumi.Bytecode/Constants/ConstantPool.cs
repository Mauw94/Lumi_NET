namespace Lumi.Bytecode.Constants;

/// <summary>
/// ConstantPool manages a list of constants used in the bytecode.
/// </summary>
public class ConstantPool
{
    private readonly List<Constant> _values = [];

    public int Add(Constant constant)
    {
        _values.Add(constant);
        return _values.Count - 1;
    }

    public IReadOnlyList<Constant> Values => _values.AsReadOnly();
}