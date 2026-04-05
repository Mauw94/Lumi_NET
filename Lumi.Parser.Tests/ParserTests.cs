using Lumi.AST;

namespace Lumi.Parser.Tests;

[TestClass]
public sealed class ParserTests
{
    [TestMethod]
    public void Test_Parsing_Print_Statement()
    {
        var source = "print 42;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<PrintStatement>(program.Body[0]);
    }

    [TestMethod]
    public void Test_Binary_Expression_Parsing()
    {
        var source = "print 1 + 2 * 3;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<PrintStatement>(program.Body[0]);

        var printStmt = (PrintStatement)program.Body[0];
        Assert.IsInstanceOfType<BinaryExpression>(printStmt.Argument);

        var binaryExpr = (BinaryExpression)printStmt.Argument;
        Assert.AreEqual("+", binaryExpr.Operator);
        Assert.IsInstanceOfType<NumberNode>(binaryExpr.Left);
        Assert.AreEqual(1, ((NumberNode)binaryExpr.Left).Value);
        Assert.IsInstanceOfType<BinaryExpression>(binaryExpr.Right);

        var rightBinary = (BinaryExpression)binaryExpr.Right;
        Assert.AreEqual("*", rightBinary.Operator);
        Assert.IsInstanceOfType<NumberNode>(rightBinary.Left);
        Assert.AreEqual(2, ((NumberNode)rightBinary.Left).Value);
        Assert.IsInstanceOfType<NumberNode>(rightBinary.Right);
        Assert.AreEqual(3, ((NumberNode)rightBinary.Right).Value);
    }

    [TestMethod]
    public void Test_Parsing_Variable_Declaration()
    {
        var source = "let x: int -> 42;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);
        var varDecla = ((VariableDeclaration)program.Body[0]).Declarations[0];

        Assert.IsInstanceOfType<IdentifierNode>(varDecla.VarName);
        Assert.AreEqual("x", ((IdentifierNode)varDecla.VarName).Name);

        Assert.IsInstanceOfType<IdentifierNode>(varDecla.VarType);
        Assert.AreEqual("int", ((IdentifierNode)varDecla.VarType).Name);

        Assert.IsInstanceOfType<NumberNode>(varDecla.Init);
        Assert.AreEqual(42, ((NumberNode)varDecla.Init).Value);
    }

    [TestMethod]
    public void Test_If_Else_Statement()
    {
        var source = "if (1 < 2) { print 42; } else { print 0; }";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<IfStatement>(program.Body[0]);
        var ifStmt = (IfStatement)program.Body[0];

        Assert.IsInstanceOfType<BlockStatement>(ifStmt.ElsePart);
        var elsePart = (BlockStatement)ifStmt.ElsePart;

        Assert.IsInstanceOfType<PrintStatement>(elsePart.Body[0]);

        Assert.IsInstanceOfType<BinaryExpression>(ifStmt.Expr);
        Assert.IsInstanceOfType<BlockStatement>(ifStmt.Stmt);
        var block = (BlockStatement)ifStmt.Stmt;

        Assert.IsInstanceOfType<PrintStatement>(block.Body[0]);
        var printStmt = (PrintStatement)block.Body[0];

        Assert.IsInstanceOfType<NumberNode>(printStmt.Argument);
    }

    [TestMethod]
    public void Test_For_Statement()
    {
        var source = "for i in 0 to 10 { print i; }";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<ForStatement>(program.Body[0]);
        var forStmt = (ForStatement)program.Body[0];

        Assert.IsInstanceOfType<IdentifierNode>(forStmt.Iterator);
        Assert.IsInstanceOfType<NumberNode>(forStmt.Start);
        Assert.IsInstanceOfType<NumberNode>(forStmt.End);
        Assert.IsInstanceOfType<BlockStatement>(forStmt.Body);
        var block = (BlockStatement)forStmt.Body;

        Assert.IsInstanceOfType<PrintStatement>(block.Body[0]);
        var printStmt = (PrintStatement)block.Body[0];

        Assert.IsInstanceOfType<IdentifierNode>(printStmt.Argument);
    }

    [TestMethod]
    public void Test_For_With_If_Block()
    {
        var source = "for i in 0 to 10 { if (i % 2 == 0) { print i; } }";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<ForStatement>(program.Body[0]);
        var forStmt = (ForStatement)program.Body[0];

        Assert.IsInstanceOfType<IdentifierNode>(forStmt.Iterator);
        Assert.IsInstanceOfType<NumberNode>(forStmt.Start);
        Assert.IsInstanceOfType<NumberNode>(forStmt.End);
        Assert.IsInstanceOfType<BlockStatement>(forStmt.Body);
        var block = (BlockStatement)forStmt.Body;

        Assert.IsInstanceOfType<IfStatement>(block.Body[0]);
        var ifStatement = (IfStatement)block.Body[0];

        Assert.IsInstanceOfType<BinaryExpression>(ifStatement.Expr);
    }

    private static Program ParseProgram(string source)
    {
        var parser = new Parser(source);
        var ast = parser.Parse();
        Assert.IsNotNull(ast);

        var program = ((Program)ast);
        Assert.IsNotNull(program);

        return program;
    }
}
