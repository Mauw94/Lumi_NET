namespace Lumi.AST;

/// <summary>
/// Represents a node in the abstract syntax tree (AST) for the Lumi programming language. 
/// </summary>
public abstract class Node
{
    public NodeSpan Span { get; set; }

    // Convenience singletons used by the parser
    public static readonly Node Null = new NullNode();
    public static readonly Node Undefined = new UndefinedNode();
}

public sealed class NullNode : Node { }
public sealed class UndefinedNode : Node { }

// Program
public class Program : Node
{
    public List<Node> Body { get; set; } = [];
}

// Declarations
public class VariableDeclaration : Node
{
    public string Kind { get; set; } = string.Empty;
    public List<VariableDeclarator> Declarations { get; set; } = [];
}

public class VariableDeclarator : Node
{
    public required Node VarName { get; set; }
    public Node? VarType { get; set; }
    public Node? Init { get; set; }
}

public class FunctionDeclaration : Node
{
    public required Node Id { get; set; }
    public List<Node> Params { get; set; } = [];
    public required Node Body { get; set; }
    public bool IsAsync { get; set; }
    public string? OwningStructName { get; set; }
}

public class StructDeclaration : Node
{
    public required IdentifierNode Name { get; set; }
    public List<StructFieldDeclaration> Fields { get; set; } = [];
    public List<FunctionDeclaration> Methods { get; set; } = [];
}

public class StructFieldDeclaration : Node
{
    public required IdentifierNode Name { get; set; }
    public required IdentifierNode Type { get; set; }
    public Node? Init { get; set; }
}

public class CallExpression : Node
{
    public required Node Callee { get; set; }
    public List<Node> Arguments { get; set; } = [];
}

public class NewExpression : Node
{
    public required IdentifierNode TypeName { get; set; }
    public List<Node> Arguments { get; set; } = [];
}

public class StructFieldInitializerArgument : Node
{
    public required IdentifierNode Name { get; set; }
    public required Node Value { get; set; }
}

public class MemberExpression : Node
{
    public required Node Object { get; set; }
    public required IdentifierNode Property { get; set; }
}

// Expressions
public class BinaryExpression : Node
{
    public required Node Left { get; set; }
    public string Operator { get; set; } = string.Empty; // TODO: replace with enum
    public required Node Right { get; set; }
}

public class LogicalExpression : Node
{
    public required Node Left { get; set; }
    public string Operator { get; set; } = string.Empty;
    public required Node Right { get; set; }
}

public class AssignmentExpression : Node
{
    public required Node Left { get; set; }
    public string Operator { get; set; } = string.Empty;
    public required Node Right { get; set; }
}

public class UnaryExpression : Node
{
    public string Operator { get; set; } = string.Empty;
    public required Node Argument { get; set; }
    public bool Prefix { get; set; }
}

// Statements
public class PrintStatement : Node
{
    public required Node Argument { get; set; }
}

public class ExpressionStatement : Node
{
    public required Node Expression { get; set; }
}

public class BlockStatement : Node
{
    public List<Node> Body { get; set; } = [];
}

public class IfStatement : Node
{
    public required Node Expr { get; set; }
    public required Node Stmt { get; set; }
    public Node? ElsePart { get; set; }
}

public class ForStatement : Node
{
    public required Node Iterator { get; set; }
    public required Node Start { get; set; }
    public required Node End { get; set; }
    public Node? Step { get; set; }
    public required Node Body { get; set; }
}

public class ReturnStatement : Node
{
    public Node? Argument { get; set; }
}

// Literals and misc
public class ArrayLiteral : Node
{
    public List<Node> Elements { get; set; } = [];
}

public class IndexExpression : Node
{
    public required Node Object { get; set; }
    public required Node Index { get; set; }
}

public class IdentifierNode : Node
{
    public string Name { get; set; } = string.Empty;
}

public class ParameterizedTypeNode : Node
{
    public required string BaseTypeName { get; set; }
    public required Node TypeArgument { get; set; }
}

public class NumberNode : Node
{
    public double Value { get; set; }
}

public class StringNode : Node
{
    public string Value { get; set; } = string.Empty;
}

public class BooleanNode : Node
{
    public bool Value { get; set; }
}
