using Lumi.AST;

namespace Lumi.Ast
{
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
        public List<Node> Body { get; set; } = new List<Node>();
    }

    // Declarations
    public class VariableDeclaration : Node
    {
        public string Kind { get; set; }
        public List<VariableDeclarator> Declarations { get; set; } = new List<VariableDeclarator>();
    }

    public class VariableDeclarator : Node
    {
        public Node VarName { get; set; }
        public Node VarType { get; set; }
        public Node Init { get; set; }
    }

    public class FunctionDeclaration : Node
    {
        public Node Id { get; set; }
        public List<Node> Params { get; set; } = new List<Node>();
        public Node Body { get; set; }
        public bool IsAsync { get; set; }
    }

    public class CallExpression : Node
    {
        public Node Callee { get; set; }
        public List<Node> Arguments { get; set; } = new List<Node>();
    }

    // Expressions
    public class BinaryExpression : Node
    {
        public Node Left { get; set; }
        public string Operator { get; set; }
        public Node Right { get; set; }
    }

    public class LogicalExpression : Node
    {
        public Node Left { get; set; }
        public string Operator { get; set; }
        public Node Right { get; set; }
    }

    public class AssignmentExpression : Node
    {
        public Node Left { get; set; }
        public string Operator { get; set; }
        public Node Right { get; set; }
    }

    public class UnaryExpression : Node
    {
        public string Operator { get; set; }
        public Node Argument { get; set; }
        public bool Prefix { get; set; }
    }

    // Statements
    public class PrintStatement : Node
    {
        public Node Argument { get; set; }
    }

    public class ExpressionStatement : Node
    {
        public Node Expression { get; set; }
    }

    public class BlockStatement : Node
    {
        public List<Node> Body { get; set; } = new List<Node>();
    }

    public class IfStatement : Node
    {
        public Node Expr { get; set; }
        public Node Stmt { get; set; }
        public Node ElsePart { get; set; }
    }

    public class ForStatement : Node
    {
        public Node Iterator { get; set; }
        public Node Start { get; set; }
        public Node End { get; set; }
        public Node Step { get; set; }
        public Node Body { get; set; }
    }

    // Literals and misc
    public class ArrayLiteral : Node
    {
        public List<Node> Elements { get; set; } = new List<Node>();
    }

    public class IdentifierNode : Node
    {
        public string Name { get; set; }
    }

    public class NumberNode : Node
    {
        public double Value { get; set; }
    }

    public class StringNode : Node
    {
        public string Value { get; set; }
    }

    public class BooleanNode : Node
    {
        public bool Value { get; set; }
    }
}
