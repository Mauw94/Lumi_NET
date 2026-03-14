namespace Lumi.Bytecode.Constants;

/// <summary>
/// ConstantPool manages a list of constants used in the bytecode.
/// Duplicate number and string constants are deduplicated via index dictionaries so that
/// repeated literals in source code only occupy a single constant pool slot.
/// </summary>
internal sealed class ConstantPool
{
    private readonly List<Constant> _values = new(capacity: 16);
    private readonly Dictionary<double, int> _numberIndex = [];
    private readonly Dictionary<string, int> _stringIndex = new(StringComparer.Ordinal);

    public int Add(Constant constant)
    {
        switch (constant.Kind)
        {
            case ConstantKind.Number:
                return StoreNumberConstant(constant);

            case ConstantKind.String:
                return StoreStringConstant(constant);

            default:
                _values.Add(constant);
                return _values.Count - 1;
        }
    }

    // Return the list directly — IReadOnlyList<T> prevents mutation without the
    // extra allocation that AsReadOnly() creates on every access.
    public IReadOnlyList<Constant> Values => _values;

    private int StoreNumberConstant(Constant constant)
    {
        var num = constant.Number!.Value;

        if (!_numberIndex.TryGetValue(num, out var ni))
        {
            ni = _values.Count;
            _values.Add(constant);
            _numberIndex[num] = ni;
        }

        return ni;
    }

    private int StoreStringConstant(Constant constant)
    {
        var str = constant.String!;

        if (!_stringIndex.TryGetValue(str, out var si))
        {
            si = _values.Count;
            _values.Add(constant);
            _stringIndex[str] = si;
        }

        return si;
    }
}