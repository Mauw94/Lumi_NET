namespace Lumi.Parser;

/// <summary>
/// Specifies the context in which parsing occurs within a programming language structure.
/// </summary>
/// <remarks>Use this enumeration to indicate the current parsing context, such as whether code is being parsed at
/// the top level, within a statement, block, function, class, module, expression, or declaration. The parsing context
/// can influence how code is interpreted and processed during parsing operations.</remarks>
internal enum ParsingContext
{
    TopLevel,
    Statement,
    Block,
    Function,
    Class,
    Module,
    Expression,
    Declaration,
}