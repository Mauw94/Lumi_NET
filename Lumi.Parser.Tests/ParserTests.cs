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
    public void Test_Parsing_List_Declaration_Without_Explicit_Type()
    {
        var source = "let x -> [1, 2, 3];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsNull(declarator.VarType);
        Assert.IsInstanceOfType<ArrayLiteral>(declarator.Init);

        var array = (ArrayLiteral)declarator.Init;
        Assert.HasCount(3, array.Elements);
        Assert.AreEqual(1, ((NumberNode)array.Elements[0]).Value);
        Assert.AreEqual(2, ((NumberNode)array.Elements[1]).Value);
        Assert.AreEqual(3, ((NumberNode)array.Elements[2]).Value);
    }

    [TestMethod]
    public void Test_Parsing_List_Declaration_With_Explicit_Type()
    {
        var source = "let x: list -> [1,2,3];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<IdentifierNode>(declarator.VarType);
        Assert.AreEqual("list", ((IdentifierNode)declarator.VarType!).Name);

        Assert.IsInstanceOfType<ArrayLiteral>(declarator.Init);
        var array = (ArrayLiteral)declarator.Init!;
        Assert.HasCount(3, array.Elements);
    }

    [TestMethod]
    public void Test_Parsing_List_With_Parameterized_Type()
    {
        var source = "let items: list<int> -> [1,2,3];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<ParameterizedTypeNode>(declarator.VarType);

        var paramType = (ParameterizedTypeNode)declarator.VarType!;
        Assert.AreEqual("list", paramType.BaseTypeName);
        Assert.IsInstanceOfType<IdentifierNode>(paramType.TypeArgument);
        Assert.AreEqual("int", ((IdentifierNode)paramType.TypeArgument).Name);

        Assert.IsInstanceOfType<ArrayLiteral>(declarator.Init);
        var array = (ArrayLiteral)declarator.Init!;
        Assert.HasCount(3, array.Elements);
    }

    [TestMethod]
    public void Test_Parsing_List_Of_Struct_With_Parameterized_Type()
    {
        var source = "let cars: list<Car> -> [];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<ParameterizedTypeNode>(declarator.VarType);

        var paramType = (ParameterizedTypeNode)declarator.VarType!;
        Assert.AreEqual("list", paramType.BaseTypeName);
        Assert.IsInstanceOfType<IdentifierNode>(paramType.TypeArgument);
        Assert.AreEqual("Car", ((IdentifierNode)paramType.TypeArgument).Name);

        Assert.IsInstanceOfType<ArrayLiteral>(declarator.Init);
    }

    [TestMethod]
    public void Test_Parsing_List_Of_Struct_With_Parameterized_Type_Case_Insensitive()
    {
        var source = "let cars: List<Car> -> [];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<ParameterizedTypeNode>(declarator.VarType);

        var paramType = (ParameterizedTypeNode)declarator.VarType!;
        Assert.AreEqual("List", paramType.BaseTypeName);
        Assert.IsInstanceOfType<IdentifierNode>(paramType.TypeArgument);
        Assert.AreEqual("Car", ((IdentifierNode)paramType.TypeArgument).Name);

        Assert.IsInstanceOfType<ArrayLiteral>(declarator.Init);
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

    [TestMethod]
    public void Test_Parsing_Array_Index_Simple()
    {
        // let x -> [1,2,3]; print x[1];
        var source = "let x -> [1,2,3]; print x[1];";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        var printStmt = (PrintStatement)program.Body[1];

        Assert.IsInstanceOfType<IndexExpression>(printStmt.Argument);
        var indexExpr = (IndexExpression)printStmt.Argument;

        Assert.IsInstanceOfType<IdentifierNode>(indexExpr.Object);
        Assert.AreEqual("x", ((IdentifierNode)indexExpr.Object).Name);
        Assert.IsInstanceOfType<NumberNode>(indexExpr.Index);
        Assert.AreEqual(1, ((NumberNode)indexExpr.Index).Value);
    }

    [TestMethod]
    public void Test_Parsing_Array_Index_With_Expression()
    {
        // let x -> [1,2,3]; print x[1 + 1];
        var source = "let x -> [1,2,3]; print x[1 + 1];";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        var printStmt = (PrintStatement)program.Body[1];

        Assert.IsInstanceOfType<IndexExpression>(printStmt.Argument);
        var indexExpr = (IndexExpression)printStmt.Argument;

        Assert.IsInstanceOfType<IdentifierNode>(indexExpr.Object);
        Assert.IsInstanceOfType<BinaryExpression>(indexExpr.Index);
        Assert.AreEqual("+", ((BinaryExpression)indexExpr.Index).Operator);
    }

    [TestMethod]
    public void Test_Parsing_Inline_Array_Literal_Index()
    {
        // print [10,20,30][0];
        var source = "print [10,20,30][0];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        var printStmt = (PrintStatement)program.Body[0];

        Assert.IsInstanceOfType<IndexExpression>(printStmt.Argument);
        var indexExpr = (IndexExpression)printStmt.Argument;

        Assert.IsInstanceOfType<ArrayLiteral>(indexExpr.Object);
        Assert.HasCount(3, ((ArrayLiteral)indexExpr.Object).Elements);
        Assert.IsInstanceOfType<NumberNode>(indexExpr.Index);
        Assert.AreEqual(0, ((NumberNode)indexExpr.Index).Value);
    }

    [TestMethod]
    public void Test_Parsing_List_Method_Call()
    {
        var source = "let items: list -> [1,2,3]; items.add(1);";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        Assert.IsInstanceOfType<ExpressionStatement>(program.Body[1]);

        var exprStmt = (ExpressionStatement)program.Body[1];
        Assert.IsInstanceOfType<CallExpression>(exprStmt.Expression);

        var callExpr = (CallExpression)exprStmt.Expression;
        Assert.IsInstanceOfType<MemberExpression>(callExpr.Callee);

        var memberExpr = (MemberExpression)callExpr.Callee;
        Assert.IsInstanceOfType<IdentifierNode>(memberExpr.Object);
        Assert.AreEqual("items", ((IdentifierNode)memberExpr.Object).Name);
        Assert.AreEqual("add", memberExpr.Property.Name);
        Assert.HasCount(1, callExpr.Arguments);
        Assert.AreEqual(1, ((NumberNode)callExpr.Arguments[0]).Value);
    }

    [TestMethod]
    public void Test_Parsing_List_Remove_Method_Call()
    {
        var source = "let items: list -> [1,2,3]; items.remove(2);";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        Assert.IsInstanceOfType<ExpressionStatement>(program.Body[1]);

        var exprStmt = (ExpressionStatement)program.Body[1];
        Assert.IsInstanceOfType<CallExpression>(exprStmt.Expression);

        var callExpr = (CallExpression)exprStmt.Expression;
        Assert.IsInstanceOfType<MemberExpression>(callExpr.Callee);

        var memberExpr = (MemberExpression)callExpr.Callee;
        Assert.IsInstanceOfType<IdentifierNode>(memberExpr.Object);
        Assert.AreEqual("items", ((IdentifierNode)memberExpr.Object).Name);
        Assert.AreEqual("remove", memberExpr.Property.Name);
        Assert.HasCount(1, callExpr.Arguments);
        Assert.AreEqual(2, ((NumberNode)callExpr.Arguments[0]).Value);
    }

    [TestMethod]
    public void Test_Parsing_List_Length_Method_Call()
    {
        var source = "let items: list -> [1,2,3]; items.length();";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        Assert.IsInstanceOfType<ExpressionStatement>(program.Body[1]);

        var exprStmt = (ExpressionStatement)program.Body[1];
        Assert.IsInstanceOfType<CallExpression>(exprStmt.Expression);

        var callExpr = (CallExpression)exprStmt.Expression;
        Assert.IsInstanceOfType<MemberExpression>(callExpr.Callee);

        var memberExpr = (MemberExpression)callExpr.Callee;
        Assert.IsInstanceOfType<IdentifierNode>(memberExpr.Object);
        Assert.AreEqual("items", ((IdentifierNode)memberExpr.Object).Name);
        Assert.AreEqual("length", memberExpr.Property.Name);
        Assert.IsEmpty(callExpr.Arguments);
    }

    [TestMethod]
    public void Test_Parsing_List_Contains_Method_Call()
    {
        var source = "let items: list -> [1,2,3]; items.contains(2);";
        var program = ParseProgram(source);

        Assert.HasCount(2, program.Body);
        Assert.IsInstanceOfType<ExpressionStatement>(program.Body[1]);

        var exprStmt = (ExpressionStatement)program.Body[1];
        Assert.IsInstanceOfType<CallExpression>(exprStmt.Expression);

        var callExpr = (CallExpression)exprStmt.Expression;
        Assert.IsInstanceOfType<MemberExpression>(callExpr.Callee);

        var memberExpr = (MemberExpression)callExpr.Callee;
        Assert.IsInstanceOfType<IdentifierNode>(memberExpr.Object);
        Assert.AreEqual("items", ((IdentifierNode)memberExpr.Object).Name);
        Assert.AreEqual("contains", memberExpr.Property.Name);
        Assert.HasCount(1, callExpr.Arguments);
        Assert.AreEqual(2, ((NumberNode)callExpr.Arguments[0]).Value);
    }

    [TestMethod]
    public void Test_Parsing_Struct_Declaration()
    {
        var source = "struct Person { name: str; age: int; }";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<StructDeclaration>(program.Body[0]);

        var structDecl = (StructDeclaration)program.Body[0];
        Assert.AreEqual("Person", structDecl.Name.Name);
        Assert.HasCount(2, structDecl.Fields);
        Assert.AreEqual("name", structDecl.Fields[0].Name.Name);
        Assert.AreEqual("str", structDecl.Fields[0].Type.Name);
        Assert.AreEqual("age", structDecl.Fields[1].Name.Name);
        Assert.AreEqual("int", structDecl.Fields[1].Type.Name);
    }

    [TestMethod]
    public void Test_Parsing_New_Expression()
    {
        var source = "let person: Person -> new Person;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<IdentifierNode>(declarator.VarType);
        Assert.AreEqual("Person", ((IdentifierNode)declarator.VarType!).Name);
        Assert.IsInstanceOfType<NewExpression>(declarator.Init);
        Assert.AreEqual("Person", ((NewExpression)declarator.Init!).TypeName.Name);
    }

    [TestMethod]
    public void Test_Parsing_Struct_Field_Access()
    {
        var source = "print person.name;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<PrintStatement>(program.Body[0]);

        var printStmt = (PrintStatement)program.Body[0];
        Assert.IsInstanceOfType<MemberExpression>(printStmt.Argument);

        var memberExpr = (MemberExpression)printStmt.Argument;
        Assert.IsInstanceOfType<IdentifierNode>(memberExpr.Object);
        Assert.AreEqual("person", ((IdentifierNode)memberExpr.Object).Name);
        Assert.AreEqual("name", memberExpr.Property.Name);
    }

    [TestMethod]
    public void Test_Parsing_New_Expression_With_Arguments()
    {
        var source = "let person: Person -> new Person(\"Alice\", 30);";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<NewExpression>(declarator.Init);

        var newExpr = (NewExpression)declarator.Init!;
        Assert.AreEqual("Person", newExpr.TypeName.Name);
        Assert.HasCount(2, newExpr.Arguments);
        Assert.IsInstanceOfType<StringNode>(newExpr.Arguments[0]);
        Assert.IsInstanceOfType<NumberNode>(newExpr.Arguments[1]);
    }

    [TestMethod]
    public void Test_Parsing_Struct_Field_Assignment()
    {
        var source = "person.age = 5;";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<ExpressionStatement>(program.Body[0]);

        var exprStmt = (ExpressionStatement)program.Body[0];
        Assert.IsInstanceOfType<AssignmentExpression>(exprStmt.Expression);

        var assignment = (AssignmentExpression)exprStmt.Expression;
        Assert.IsInstanceOfType<MemberExpression>(assignment.Left);

        var left = (MemberExpression)assignment.Left;
        Assert.AreEqual("age", left.Property.Name);
        Assert.IsInstanceOfType<IdentifierNode>(left.Object);
        Assert.AreEqual("person", ((IdentifierNode)left.Object).Name);
        Assert.IsInstanceOfType<NumberNode>(assignment.Right);
        Assert.AreEqual(5, ((NumberNode)assignment.Right).Value);
    }

    [TestMethod]
    public void Test_Parsing_List_With_Parameterized_Type_Is_Valid()
    {
        var source = "let items: list<Car> -> [];";
        var program = ParseProgram(source);

        Assert.HasCount(1, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);

        var declarator = ((VariableDeclaration)program.Body[0]).Declarations[0];
        Assert.IsInstanceOfType<ParameterizedTypeNode>(declarator.VarType);

        var paramType = (ParameterizedTypeNode)declarator.VarType!;
        Assert.AreEqual("list", paramType.BaseTypeName);
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
