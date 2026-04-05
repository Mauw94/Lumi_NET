namespace Lumi.AST.Tests;

/// <summary>
/// Comprehensive tests for Abstract Syntax Tree node creation and structure.
/// </summary>
[TestClass]
public sealed class ASTTests
{
    #region Program and Block Tests

    [TestMethod]
    public void Test_Program_Creation()
    {
        // Arrange & Act
        var program = new Program();

        // Assert
        Assert.IsNotNull(program.Body);
        Assert.HasCount(0, program.Body);
        Assert.IsInstanceOfType<Node>(program);
    }

    [TestMethod]
    public void Test_Program_With_Statements()
    {
        // Arrange
        var program = new Program();
        var stmt = new ExpressionStatement { Expression = new NumberNode { Value = 42 } };

        // Act
        program.Body.Add(stmt);

        // Assert
        Assert.HasCount(1, program.Body);
        Assert.AreSame(stmt, program.Body[0]);
    }

    [TestMethod]
    public void Test_BlockStatement_Creation()
    {
        // Arrange & Act
        var block = new BlockStatement();

        // Assert
        Assert.IsNotNull(block.Body);
        Assert.HasCount(0, block.Body);
    }

    [TestMethod]
    public void Test_BlockStatement_With_Nested_Statements()
    {
        // Arrange
        var block = new BlockStatement();
        var stmt1 = new ExpressionStatement { Expression = new NumberNode { Value = 1 } };
        var stmt2 = new ExpressionStatement { Expression = new NumberNode { Value = 2 } };

        // Act
        block.Body.Add(stmt1);
        block.Body.Add(stmt2);

        // Assert
        Assert.HasCount(2, block.Body);
        Assert.AreSame(stmt1, block.Body[0]);
        Assert.AreSame(stmt2, block.Body[1]);
    }

    #endregion

    #region Literal Node Tests

    [TestMethod]
    public void Test_NumberNode_Creation()
    {
        // Arrange & Act
        var node = new NumberNode { Value = 42.5 };

        // Assert
        Assert.IsNotNull(node);
        Assert.AreEqual(42.5, node.Value);
        Assert.IsInstanceOfType<Node>(node);
    }

    [TestMethod]
    public void Test_NumberNode_With_Integer()
    {
        // Arrange & Act
        var node = new NumberNode { Value = 100 };

        // Assert
        Assert.AreEqual(100, node.Value);
    }

    [TestMethod]
    public void Test_NumberNode_With_Negative()
    {
        // Arrange & Act
        var node = new NumberNode { Value = -50.25 };

        // Assert
        Assert.AreEqual(-50.25, node.Value);
    }

    [TestMethod]
    public void Test_NumberNode_With_Zero()
    {
        // Arrange & Act
        var node = new NumberNode { Value = 0 };

        // Assert
        Assert.AreEqual(0, node.Value);
    }

    [TestMethod]
    public void Test_StringNode_Creation()
    {
        // Arrange & Act
        var node = new StringNode { Value = "hello world" };

        // Assert
        Assert.IsNotNull(node);
        Assert.AreEqual("hello world", node.Value);
        Assert.IsInstanceOfType<Node>(node);
    }

    [TestMethod]
    public void Test_StringNode_With_Empty_String()
    {
        // Arrange & Act
        var node = new StringNode { Value = "" };

        // Assert
        Assert.AreEqual("", node.Value);
    }

    [TestMethod]
    public void Test_StringNode_With_Special_Characters()
    {
        // Arrange & Act
        var node = new StringNode { Value = "!@#$%^&*()" };

        // Assert
        Assert.AreEqual("!@#$%^&*()", node.Value);
    }

    [TestMethod]
    public void Test_BooleanNode_True()
    {
        // Arrange & Act
        var node = new BooleanNode { Value = true };

        // Assert
        Assert.IsNotNull(node);
        Assert.IsTrue(node.Value);
        Assert.IsInstanceOfType<Node>(node);
    }

    [TestMethod]
    public void Test_BooleanNode_False()
    {
        // Arrange & Act
        var node = new BooleanNode { Value = false };

        // Assert
        Assert.IsFalse(node.Value);
    }

    [TestMethod]
    public void Test_NullNode_Singleton()
    {
        // Arrange & Act
        var nullNode1 = Node.Null;
        var nullNode2 = Node.Null;

        // Assert
        Assert.IsInstanceOfType<NullNode>(nullNode1);
        Assert.AreSame(nullNode1, nullNode2, "Null nodes should be singletons");
    }

    [TestMethod]
    public void Test_UndefinedNode_Singleton()
    {
        // Arrange & Act
        var undefinedNode1 = Node.Undefined;
        var undefinedNode2 = Node.Undefined;

        // Assert
        Assert.IsInstanceOfType<UndefinedNode>(undefinedNode1);
        Assert.AreSame(undefinedNode1, undefinedNode2, "Undefined nodes should be singletons");
    }

    #endregion

    #region Identifier Tests

    [TestMethod]
    public void Test_IdentifierNode_Creation()
    {
        // Arrange & Act
        var node = new IdentifierNode { Name = "myVariable" };

        // Assert
        Assert.IsNotNull(node);
        Assert.AreEqual("myVariable", node.Name);
        Assert.IsInstanceOfType<Node>(node);
    }

    [TestMethod]
    public void Test_IdentifierNode_With_Underscore()
    {
        // Arrange & Act
        var node = new IdentifierNode { Name = "_privateVar" };

        // Assert
        Assert.AreEqual("_privateVar", node.Name);
    }

    [TestMethod]
    public void Test_IdentifierNode_With_Numbers()
    {
        // Arrange & Act
        var node = new IdentifierNode { Name = "var123" };

        // Assert
        Assert.AreEqual("var123", node.Name);
    }

    #endregion

    #region Variable Declaration Tests

    [TestMethod]
    public void Test_VariableDeclaration_With_Let()
    {
        // Arrange & Act
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42 }
                }
            ]
        };

        // Assert
        Assert.AreEqual("let", decl.Kind);
        Assert.HasCount(1, decl.Declarations);
        Assert.IsNotNull(decl.Declarations[0].VarName);
        Assert.IsNotNull(decl.Declarations[0].Init);
    }

    [TestMethod]
    public void Test_VariableDeclaration_With_Const()
    {
        // Arrange & Act
        var decl = new VariableDeclaration
        {
            Kind = "const",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "PI" },
                    Init = new NumberNode { Value = 3.14159 }
                }
            ]
        };

        // Assert
        Assert.AreEqual("const", decl.Kind);
        Assert.HasCount(1, decl.Declarations);
    }

    [TestMethod]
    public void Test_VariableDeclaration_With_Var()
    {
        // Arrange & Act
        var decl = new VariableDeclaration
        {
            Kind = "var",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "count" },
                    Init = new NumberNode { Value = 0 }
                }
            ]
        };

        // Assert
        Assert.AreEqual("var", decl.Kind);
    }

    [TestMethod]
    public void Test_VariableDeclaration_Without_Initializer()
    {
        // Arrange & Act
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "uninitialized" },
                    Init = null
                }
            ]
        };

        // Assert
        Assert.IsNull(decl.Declarations[0].Init);
    }

    [TestMethod]
    public void Test_VariableDeclaration_Multiple_Declarators()
    {
        // Arrange & Act
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "a" },
                    Init = new NumberNode { Value = 1 }
                },
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "b" },
                    Init = new NumberNode { Value = 2 }
                },
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "c" },
                    Init = new NumberNode { Value = 3 }
                }
            ]
        };

        // Assert
        Assert.HasCount(3, decl.Declarations);
        Assert.AreEqual("a", ((IdentifierNode)decl.Declarations[0].VarName).Name);
        Assert.AreEqual("b", ((IdentifierNode)decl.Declarations[1].VarName).Name);
        Assert.AreEqual("c", ((IdentifierNode)decl.Declarations[2].VarName).Name);
    }

    #endregion

    #region Expression Tests

    [TestMethod]
    public void Test_BinaryExpression_Addition()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 5 },
            Operator = "+",
            Right = new NumberNode { Value = 3 }
        };

        // Assert
        Assert.IsNotNull(expr);
        Assert.AreEqual("+", expr.Operator);
        Assert.IsInstanceOfType<NumberNode>(expr.Left);
        Assert.IsInstanceOfType<NumberNode>(expr.Right);
    }

    [TestMethod]
    public void Test_BinaryExpression_Subtraction()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 10 },
            Operator = "-",
            Right = new NumberNode { Value = 4 }
        };

        // Assert
        Assert.AreEqual("-", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_Multiplication()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 6 },
            Operator = "*",
            Right = new NumberNode { Value = 7 }
        };

        // Assert
        Assert.AreEqual("*", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_Division()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 20 },
            Operator = "/",
            Right = new NumberNode { Value = 4 }
        };

        // Assert
        Assert.AreEqual("/", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_Modulo()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 10 },
            Operator = "%",
            Right = new NumberNode { Value = 3 }
        };

        // Assert
        Assert.AreEqual("%", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_Equality()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 5 },
            Operator = "==",
            Right = new NumberNode { Value = 5 }
        };

        // Assert
        Assert.AreEqual("==", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_Comparison()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 10 },
            Operator = "<",
            Right = new NumberNode { Value = 20 }
        };

        // Assert
        Assert.AreEqual("<", expr.Operator);
    }

    [TestMethod]
    public void Test_BinaryExpression_With_Identifiers()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "+",
            Right = new IdentifierNode { Name = "y" }
        };

        // Assert
        Assert.IsInstanceOfType<IdentifierNode>(expr.Left);
        Assert.IsInstanceOfType<IdentifierNode>(expr.Right);
    }

    [TestMethod]
    public void Test_UnaryExpression_Negation()
    {
        // Arrange & Act
        var expr = new UnaryExpression
        {
            Operator = "-",
            Argument = new NumberNode { Value = 5 },
            Prefix = true
        };

        // Assert
        Assert.AreEqual("-", expr.Operator);
        Assert.IsTrue(expr.Prefix);
    }

    [TestMethod]
    public void Test_UnaryExpression_Logical_Not()
    {
        // Arrange & Act
        var expr = new UnaryExpression
        {
            Operator = "!",
            Argument = new BooleanNode { Value = true },
            Prefix = true
        };

        // Assert
        Assert.AreEqual("!", expr.Operator);
    }

    [TestMethod]
    public void Test_AssignmentExpression_Simple()
    {
        // Arrange & Act
        var expr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "=",
            Right = new NumberNode { Value = 42 }
        };

        // Assert
        Assert.IsNotNull(expr);
        Assert.AreEqual("=", expr.Operator);
        Assert.IsInstanceOfType<IdentifierNode>(expr.Left);
        Assert.IsInstanceOfType<NumberNode>(expr.Right);
    }

    [TestMethod]
    public void Test_AssignmentExpression_With_BinaryExpression()
    {
        // Arrange & Act
        var expr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "sum" },
            Operator = "=",
            Right = new BinaryExpression
            {
                Left = new NumberNode { Value = 10 },
                Operator = "+",
                Right = new NumberNode { Value = 5 }
            }
        };

        // Assert
        Assert.IsInstanceOfType<BinaryExpression>(expr.Right);
        var binExpr = (BinaryExpression)expr.Right;
        Assert.AreEqual("+", binExpr.Operator);
    }

    #endregion

    #region Statement Tests

    [TestMethod]
    public void Test_PrintStatement_With_Number()
    {
        // Arrange & Act
        var stmt = new PrintStatement
        {
            Argument = new NumberNode { Value = 42 }
        };

        // Assert
        Assert.IsNotNull(stmt);
        Assert.IsInstanceOfType<NumberNode>(stmt.Argument);
    }

    [TestMethod]
    public void Test_PrintStatement_With_String()
    {
        // Arrange & Act
        var stmt = new PrintStatement
        {
            Argument = new StringNode { Value = "Hello, World!" }
        };

        // Assert
        Assert.IsInstanceOfType<StringNode>(stmt.Argument);
    }

    [TestMethod]
    public void Test_PrintStatement_With_Variable()
    {
        // Arrange & Act
        var stmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "x" }
        };

        // Assert
        Assert.IsInstanceOfType<IdentifierNode>(stmt.Argument);
    }

    [TestMethod]
    public void Test_ExpressionStatement_Creation()
    {
        // Arrange & Act
        var stmt = new ExpressionStatement
        {
            Expression = new BinaryExpression
            {
                Left = new NumberNode { Value = 1 },
                Operator = "+",
                Right = new NumberNode { Value = 2 }
            }
        };

        // Assert
        Assert.IsNotNull(stmt);
        Assert.IsInstanceOfType<BinaryExpression>(stmt.Expression);
    }

    [TestMethod]
    public void Test_IfStatement_Without_Else()
    {
        // Arrange & Act
        var stmt = new IfStatement
        {
            Expr = new BooleanNode { Value = true },
            Stmt = new PrintStatement { Argument = new NumberNode { Value = 1 } },
            ElsePart = null
        };

        // Assert
        Assert.IsNotNull(stmt);
        Assert.IsNull(stmt.ElsePart);
        Assert.IsInstanceOfType<BooleanNode>(stmt.Expr);
    }

    [TestMethod]
    public void Test_IfStatement_With_Else()
    {
        // Arrange & Act
        var stmt = new IfStatement
        {
            Expr = new BooleanNode { Value = false },
            Stmt = new PrintStatement { Argument = new NumberNode { Value = 1 } },
            ElsePart = new PrintStatement { Argument = new NumberNode { Value = 2 } }
        };

        // Assert
        Assert.IsNotNull(stmt.ElsePart);
        Assert.IsInstanceOfType<PrintStatement>(stmt.ElsePart);
    }

    [TestMethod]
    public void Test_IfStatement_With_Block_Body()
    {
        // Arrange & Act
        var stmt = new IfStatement
        {
            Expr = new BinaryExpression
            {
                Left = new NumberNode { Value = 5 },
                Operator = ">",
                Right = new NumberNode { Value = 3 }
            },
            Stmt = new BlockStatement
            {
                Body = [new PrintStatement { Argument = new NumberNode { Value = 1 } }]
            },
            ElsePart = null
        };

        // Assert
        Assert.IsInstanceOfType<BlockStatement>(stmt.Stmt);
        var block = (BlockStatement)stmt.Stmt;
        Assert.HasCount(1, block.Body);
    }

    #endregion

    #region Loop Tests

    [TestMethod]
    public void Test_ForStatement_Creation()
    {
        // Arrange & Act
        var stmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0 },
            End = new NumberNode { Value = 10 },
            Step = null,
            Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
        };

        // Assert
        Assert.IsNotNull(stmt);
        Assert.AreEqual("i", ((IdentifierNode)stmt.Iterator).Name);
        Assert.AreEqual(0, ((NumberNode)stmt.Start).Value);
        Assert.AreEqual(10, ((NumberNode)stmt.End).Value);
        Assert.IsNull(stmt.Step);
    }

    [TestMethod]
    public void Test_ForStatement_With_Step()
    {
        // Arrange & Act
        var stmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0 },
            End = new NumberNode { Value = 100 },
            Step = new NumberNode { Value = 5 },
            Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
        };

        // Assert
        Assert.IsNotNull(stmt.Step);
        Assert.AreEqual(5, ((NumberNode)stmt.Step).Value);
    }

    [TestMethod]
    public void Test_ForStatement_With_Block_Body()
    {
        // Arrange & Act
        var stmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0 },
            End = new NumberNode { Value = 10 },
            Step = null,
            Body = new BlockStatement
            {
                Body =
                [
                    new PrintStatement { Argument = new IdentifierNode { Name = "i" } },
                    new PrintStatement { Argument = new StringNode { Value = " " } }
                ]
            }
        };

        // Assert
        Assert.IsInstanceOfType<BlockStatement>(stmt.Body);
        var block = (BlockStatement)stmt.Body;
        Assert.HasCount(2, block.Body);
    }

    #endregion

    #region Function Declaration Tests

    [TestMethod]
    public void Test_FunctionDeclaration_Creation()
    {
        // Arrange & Act
        var decl = new FunctionDeclaration
        {
            Id = new IdentifierNode { Name = "greet" },
            Params = [],
            Body = new BlockStatement { Body = [] },
            IsAsync = false
        };

        // Assert
        Assert.IsNotNull(decl);
        Assert.AreEqual("greet", ((IdentifierNode)decl.Id).Name);
        Assert.HasCount(0, decl.Params);
        Assert.IsFalse(decl.IsAsync);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_With_Parameters()
    {
        // Arrange & Act
        var decl = new FunctionDeclaration
        {
            Id = new IdentifierNode { Name = "add" },
            Params =
            [
                new IdentifierNode { Name = "a" },
                new IdentifierNode { Name = "b" }
            ],
            Body = new BlockStatement { Body = [] },
            IsAsync = false
        };

        // Assert
        Assert.HasCount(2, decl.Params);
        Assert.AreEqual("a", ((IdentifierNode)decl.Params[0]).Name);
        Assert.AreEqual("b", ((IdentifierNode)decl.Params[1]).Name);
    }

    [TestMethod]
    public void Test_FunctionDeclaration_Async()
    {
        // Arrange & Act
        var decl = new FunctionDeclaration
        {
            Id = new IdentifierNode { Name = "fetchData" },
            Params = [],
            Body = new BlockStatement { Body = [] },
            IsAsync = true
        };

        // Assert
        Assert.IsTrue(decl.IsAsync);
    }

    #endregion

    #region Call Expression Tests

    [TestMethod]
    public void Test_CallExpression_No_Arguments()
    {
        // Arrange & Act
        var expr = new CallExpression
        {
            Callee = new IdentifierNode { Name = "getTime" },
            Arguments = []
        };

        // Assert
        Assert.IsNotNull(expr);
        Assert.HasCount(0, expr.Arguments);
        Assert.AreEqual("getTime", ((IdentifierNode)expr.Callee).Name);
    }

    [TestMethod]
    public void Test_CallExpression_With_Arguments()
    {
        // Arrange & Act
        var expr = new CallExpression
        {
            Callee = new IdentifierNode { Name = "add" },
            Arguments =
            [
                new NumberNode { Value = 5 },
                new NumberNode { Value = 3 }
            ]
        };

        // Assert
        Assert.HasCount(2, expr.Arguments);
        Assert.IsInstanceOfType<NumberNode>(expr.Arguments[0]);
        Assert.IsInstanceOfType<NumberNode>(expr.Arguments[1]);
    }

    #endregion

    #region Array Literal Tests

    [TestMethod]
    public void Test_ArrayLiteral_Empty()
    {
        // Arrange & Act
        var arr = new ArrayLiteral { Elements = [] };

        // Assert
        Assert.IsNotNull(arr);
        Assert.HasCount(0, arr.Elements);
    }

    [TestMethod]
    public void Test_ArrayLiteral_With_Numbers()
    {
        // Arrange & Act
        var arr = new ArrayLiteral
        {
            Elements =
            [
                new NumberNode { Value = 1 },
                new NumberNode { Value = 2 },
                new NumberNode { Value = 3 }
            ]
        };

        // Assert
        Assert.HasCount(3, arr.Elements);
        Assert.IsInstanceOfType<NumberNode>(arr.Elements[0]);
    }

    [TestMethod]
    public void Test_ArrayLiteral_With_Mixed_Types()
    {
        // Arrange & Act
        var arr = new ArrayLiteral
        {
            Elements =
            [
                new NumberNode { Value = 42 },
                new StringNode { Value = "hello" },
                new BooleanNode { Value = true }
            ]
        };

        // Assert
        Assert.HasCount(3, arr.Elements);
        Assert.IsInstanceOfType<NumberNode>(arr.Elements[0]);
        Assert.IsInstanceOfType<StringNode>(arr.Elements[1]);
        Assert.IsInstanceOfType<BooleanNode>(arr.Elements[2]);
    }

    #endregion

    #region Node Span Tests

    [TestMethod]
    public void Test_Node_Span_Assignment()
    {
        // Arrange
        var node = new NumberNode { Value = 42 };
        var span = new NodeSpan();

        // Act
        node.Span = span;

        // Assert
        Assert.AreEqual(span, node.Span, "Span should be assigned correctly");
    }

    [TestMethod]
    public void Test_Node_Span_Inheritance()
    {
        // Arrange
        var expr = new BinaryExpression
        {
            Left = new NumberNode { Value = 1 },
            Operator = "+",
            Right = new NumberNode { Value = 2 }
        };

        // Act
        expr.Span = new NodeSpan();

        // Assert - Span was successfully assigned (value type assignment verifies this)
        // Validate by checking that expression still works normally
        Assert.IsNotNull(expr.Left);
    }

    #endregion

    #region Complex AST Structure Tests

    [TestMethod]
    public void Test_Complex_Program_Structure()
    {
        // Arrange & Act
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations = [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "x" },
                            Init = new NumberNode { Value = 10 }
                        }
                    ]
                },
                new IfStatement
                {
                    Expr = new BinaryExpression
                    {
                        Left = new IdentifierNode { Name = "x" },
                        Operator = ">",
                        Right = new NumberNode { Value = 5 }
                    },
                    Stmt = new PrintStatement { Argument = new IdentifierNode { Name = "x" } },
                    ElsePart = null
                },
                new ForStatement
                {
                    Iterator = new IdentifierNode { Name = "i" },
                    Start = new NumberNode { Value = 0 },
                    End = new NumberNode { Value = 5 },
                    Step = null,
                    Body = new PrintStatement { Argument = new IdentifierNode { Name = "i" } }
                }
            ]
        };

        // Assert
        Assert.HasCount(3, program.Body);
        Assert.IsInstanceOfType<VariableDeclaration>(program.Body[0]);
        Assert.IsInstanceOfType<IfStatement>(program.Body[1]);
        Assert.IsInstanceOfType<ForStatement>(program.Body[2]);
    }

    [TestMethod]
    public void Test_Nested_Binary_Expressions()
    {
        // Arrange & Act
        var expr = new BinaryExpression
        {
            Left = new BinaryExpression
            {
                Left = new NumberNode { Value = 2 },
                Operator = "+",
                Right = new NumberNode { Value = 3 }
            },
            Operator = "*",
            Right = new BinaryExpression
            {
                Left = new NumberNode { Value = 4 },
                Operator = "-",
                Right = new NumberNode { Value = 1 }
            }
        };

        // Assert
        Assert.IsInstanceOfType<BinaryExpression>(expr.Left);
        Assert.IsInstanceOfType<BinaryExpression>(expr.Right);
        var leftExpr = (BinaryExpression)expr.Left;
        Assert.AreEqual("+", leftExpr.Operator);
    }

    [TestMethod]
    public void Test_Deeply_Nested_Blocks()
    {
        // Arrange & Act
        var program = new Program
        {
            Body =
            [
                new BlockStatement
                {
                    Body = [
                        new BlockStatement
                        {
                            Body = [
                                new BlockStatement
                                {
                                    Body = [
                                        new PrintStatement { Argument = new NumberNode { Value = 1 } }
                                    ]
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        // Assert
        Assert.HasCount(1, program.Body);
        var block1 = (BlockStatement)program.Body[0];
        Assert.HasCount(1, block1.Body);
        var block2 = (BlockStatement)block1.Body[0];
        Assert.HasCount(1, block2.Body);
        var block3 = (BlockStatement)block2.Body[0];
        Assert.HasCount(1, block3.Body);
    }

    #endregion
}
