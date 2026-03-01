using Lumi.Ast;

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
