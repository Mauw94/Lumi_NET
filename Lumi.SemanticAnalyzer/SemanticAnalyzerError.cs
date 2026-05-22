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
    public static SemanticAnalyzerError InvalidMethodCall()
        => new("Invalid method call. Expected a member call like 'object.method(...)'");
    public static SemanticAnalyzerError ArgumentCountMismatch(string functionName, int expected, int actual)
        => new($"Function '{functionName}' expects {expected} argument(s) but was called with {actual}");
    public static SemanticAnalyzerError UnknownListMethod(string methodName)
        => new($"Unknown list method: '{methodName}'");
    public static SemanticAnalyzerError MethodNotSupportedOnType(string methodName, TypeKind type)
        => new($"Method '{methodName}' is not supported on values of type '{type}'");
    public static SemanticAnalyzerError MethodArgumentCountMismatch(string methodName, int expected, int actual)
        => new($"Method '{methodName}' expects {expected} argument(s) but was called with {actual}");
    public static SemanticAnalyzerError FunctionNameIsKeyword(string name)
        => new($"Function name '{name}' is a reserved keyword");
    public static SemanticAnalyzerError VarNameIsKeyword(string name)
        => new($"Variable name '{name}' is a reserved keyword");
    public static SemanticAnalyzerError UndefinedStruct(string name)
        => new($"Undefined struct: '{name}'");
    public static SemanticAnalyzerError InvalidStructDeclaration()
        => new("Invalid struct declaration. Expected a struct name (identifier)");
    public static SemanticAnalyzerError StructNameIsKeyword(string name)
        => new($"Struct name '{name}' is a reserved keyword");
    public static SemanticAnalyzerError DuplicateStructField(string structName, string fieldName)
        => new($"Struct '{structName}' has duplicate field '{fieldName}'");
    public static SemanticAnalyzerError DuplicateStructMethod(string structName, string methodName)
        => new($"Struct '{structName}' has duplicate method '{methodName}'");
    public static SemanticAnalyzerError DuplicateStructMember(string structName, string memberName)
        => new($"Struct '{structName}' already contains a member named '{memberName}'");
    public static SemanticAnalyzerError UnknownStructField(string structName, string fieldName)
        => new($"Struct '{structName}' does not contain field '{fieldName}'");
    public static SemanticAnalyzerError UnknownStructMethod(string structName, string methodName)
        => new($"Struct '{structName}' does not contain method '{methodName}'");
    public static SemanticAnalyzerError MemberAccessNotSupportedOnType(TypeKind type)
        => new($"Member access is not supported on values of type '{type}'");
    public static SemanticAnalyzerError TypeMismatch(string variableName, string expected, string actual)
        => new($"Variable '{variableName}' expects type '{expected}' but got '{actual}'");
    public static SemanticAnalyzerError StructConstructorArgumentCountMismatch(string structName, int maxExpected, int actual)
        => new($"Struct '{structName}' constructor accepts up to {maxExpected} argument(s) but was called with {actual}");
    public static SemanticAnalyzerError InvalidTypeAnnotation(string baseTypeName)
        => new($"Only 'list' type can have type parameters, not '{baseTypeName}'");
    public static SemanticAnalyzerError InvalidStructConstructorArgumentsMixing(string structName)
        => new($"Struct '{structName}' constructor cannot mix named and positional arguments.");
    public static SemanticAnalyzerError DuplicateStructFieldInitializer(string structName, string fieldName)
        => new($"Struct '{structName}' constructor received multiple initializers for field '{fieldName}'.");
}
