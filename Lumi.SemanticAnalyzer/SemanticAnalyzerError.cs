namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Represents errors that occur during semantic analysis of an abstract syntax tree.
/// </summary>
/// <param name="message">The error message that describes the semantic issue.</param>
public sealed class SemanticAnalyzerError(string message) : Exception(message)
{
    public static SemanticAnalyzerError UndefinedVariable(string name) => new($"Undefined variable: '{name}'");
    public static SemanticAnalyzerError UndefinedFunction(string name) => new($"Undefined function: '{name}'");
    public static SemanticAnalyzerError RedefinedVariable(string name) => new($"Variable '{name}' is already defined in this scope");
    public static SemanticAnalyzerError InvalidOperandType(string operatorSymbol, string leftType, string rightType)
        => new($"Cannot apply operator '{operatorSymbol}' to operands of type '{leftType}' and '{rightType}'");
    public static SemanticAnalyzerError InvalidUnaryOperandType(string operatorSymbol, string operandType)
        => new($"Cannot apply unary operator '{operatorSymbol}' to operand of type '{operandType}'");
    public static SemanticAnalyzerError AssignmentToReadOnlyVariable(string name)
        => new($"Cannot assign to read-only variable '{name}'");
    public static SemanticAnalyzerError InvalidAssignmentTarget()
        => new($"Invalid assignment target. Expected a variable");
    public static SemanticAnalyzerError NoActiveScope()
        => new("No active scope");
    public static SemanticAnalyzerError InvalidFunctionDeclaration()
        => new("Invalid function declaration. Expected a function name (identifier)");
    public static SemanticAnalyzerError InvalidFunctionParameter()
        => new("Invalid function parameter. Expected a parameter name (identifier)");
    public static SemanticAnalyzerError InvalidFunctionCall()
        => new("Invalid function call. Expected a function name (identifier)");
    public static SemanticAnalyzerError ArgumentCountMismatch(string functionName, int expected, int actual)
        => new($"Function '{functionName}' expects {expected} argument(s) but was called with {actual}");
}