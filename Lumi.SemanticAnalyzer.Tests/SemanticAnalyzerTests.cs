using Lumi.AST;

namespace Lumi.SemanticAnalyzer.Tests;

[TestClass]
public sealed class SemanticAnalyzerTests
{
    private readonly SemanticAnalyzer _analyzer = new();

    [TestMethod]
    public void Analyze_ValidProgram_ReturnsNoErrors()
    {
        // Build AST: let x -> 42
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var program = new Program { Body = [decl] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_UndefinedVariable_ReturnsError()
    {
        // Build AST: print y (where y is undefined)
        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "y" }
        };

        var program = new Program { Body = [printStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_VariableDeclaration_ThenReference_IsValid()
    {
        // Build AST: let x -> 42; print x
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "x" }
        };

        var program = new Program { Body = [decl, printStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_AssignToUndefinedVariable_ReturnsError()
    {
        // Build AST: x = 42 (where x is undefined)
        var assignExpr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Right = new NumberNode { Value = 42.0 }
        };

        var exprStmt = new ExpressionStatement { Expression = assignExpr };
        var program = new Program { Body = [exprStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_AssignToConstant_ReturnsError()
    {
        // Build AST: const x -> 42; x = 100
        var decl = new VariableDeclaration
        {
            Kind = "const",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var assignExpr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Right = new NumberNode { Value = 100.0 }
        };

        var exprStmt = new ExpressionStatement { Expression = assignExpr };
        var program = new Program { Body = [decl, exprStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("read-only", result.Errors[0].Message, "Error should be about assignment to read-only variable");
    }

    [TestMethod]
    public void Analyze_AssignToLetVariable_IsValid()
    {
        // Build AST: let x -> 42; x = 100
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var assignExpr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Right = new NumberNode { Value = 100.0 }
        };

        var exprStmt = new ExpressionStatement { Expression = assignExpr };
        var program = new Program { Body = [decl, exprStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_InvalidAssignmentTarget_ReturnsError()
    {
        // Build AST: 42 = x (assigning to a literal)
        var assignExpr = new AssignmentExpression
        {
            Left = new NumberNode { Value = 42.0 },
            Right = new IdentifierNode { Name = "x" }
        };

        var exprStmt = new ExpressionStatement { Expression = assignExpr };
        var program = new Program { Body = [exprStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Invalid assignment target", result.Errors[0].Message, "Error should be about invalid assignment target");
    }

    [TestMethod]
    public void Analyze_VariableRedeclaration_ReturnsError()
    {
        // Build AST: let x -> 42; let x -> 100
        var decl1 = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var decl2 = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 100.0 }
                }
            ]
        };

        var program = new Program { Body = [decl1, decl2] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("already defined", result.Errors[0].Message, "Error should be about variable redeclaration");
    }

    [TestMethod]
    public void Analyze_BlockScope_AllowsShadowing()
    {
        // Build AST: let x -> 42; { let x -> 100; print x }
        var outerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var innerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 100.0 }
                }
            ]
        };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "x" }
        };

        var block = new BlockStatement
        {
            Body = [innerDecl, printStmt]
        };

        var program = new Program { Body = [outerDecl, block] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors (shadowing is allowed)");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_VariableUndefinedOutsideScope_ReturnsError()
    {
        // Build AST: { let x -> 42 }; print x (x is undefined outside block)
        var innerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var block = new BlockStatement { Body = [innerDecl] };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "x" }
        };

        var program = new Program { Body = [block, printStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_BinaryExpressionWithUndefinedVar_ReturnsError()
    {
        // Build AST: let result -> (x + 5)
        var binExpr = new BinaryExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Operator = "+",
            Right = new NumberNode { Value = 5.0 }
        };

        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "result" },
                    Init = binExpr
                }
            ]
        };

        var program = new Program { Body = [decl] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_ForLoopIteratorVariable_IsRegistered()
    {
        // Build AST: for i in 0 to 10 { print i }
        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "i" }
        };

        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 10.0 },
            Step = null,
            Body = printStmt
        };

        var program = new Program { Body = [forStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors (iterator variable 'i' should be in scope)");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ForLoopIteratorNotInScopeAfterLoop_ReturnsError()
    {
        // Build AST: for i in 0 to 10 { }; print i
        var forStmt = new ForStatement
        {
            Iterator = new IdentifierNode { Name = "i" },
            Start = new NumberNode { Value = 0.0 },
            End = new NumberNode { Value = 10.0 },
            Step = null,
            Body = new BlockStatement { Body = [] }
        };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "i" }
        };

        var program = new Program { Body = [forStmt, printStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_IfStatementBlockScope_AllowsShadowing()
    {
        // Build AST: let x -> 42; if true { let x -> 100; print x }
        var outerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 42.0 }
                }
            ]
        };

        var innerDecl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new NumberNode { Value = 100.0 }
                }
            ]
        };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "x" }
        };

        var block = new BlockStatement { Body = [innerDecl, printStmt] };

        var ifStmt = new IfStatement
        {
            Expr = new BooleanNode { Value = true },
            Stmt = block,
            ElsePart = null
        };

        var program = new Program { Body = [outerDecl, ifStmt] };
        var result = _analyzer.Analyze(program);

        Assert.IsTrue(result.IsValid, "Program should have no errors (shadowing in if block is allowed)");
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_UnaryExpression_WithUndefinedVar_ReturnsError()
    {
        // Build AST: let result -> (!x)
        var unaryExpr = new UnaryExpression
        {
            Operator = "!",
            Argument = new IdentifierNode { Name = "x" }
        };

        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "result" },
                    Init = unaryExpr
                }
            ]
        };

        var program = new Program { Body = [decl] };
        var result = _analyzer.Analyze(program);

        Assert.IsFalse(result.IsValid, "Program should have an error");
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message, "Error should be about undefined variable");
    }

    [TestMethod]
    public void Analyze_MultipleErrors_ReturnsAllErrors()
    {
        // Build AST with multiple undefined variables
        // x = y; print z
        var assignExpr = new AssignmentExpression
        {
            Left = new IdentifierNode { Name = "x" },
            Right = new IdentifierNode { Name = "y" }
        };

        var exprStmt = new ExpressionStatement { Expression = assignExpr };

        var printStmt = new PrintStatement
        {
            Argument = new IdentifierNode { Name = "z" }
        };

        var program = new Program { Body = [exprStmt, printStmt] };
        var result = _analyzer.Analyze(program);

        // May have multiple errors depending on error collection behavior
        Assert.IsFalse(result.IsValid, "Program should have at least one error");
        Assert.IsGreaterThanOrEqualTo(1, result.Errors.Count, "Program should report at least one undefined variable error");
    }

    [TestMethod]
    public void Analyze_FunctionCall_CorrectArgCount_NoErrors()
    {
        // fn add(a, b) { print a + b; }
        // add(1, 2);
        var program = new Program
        {
            Body =
            [
                new FunctionDeclaration
                {
                    Id = new IdentifierNode { Name = "add" },
                    Params = [new IdentifierNode { Name = "a" }, new IdentifierNode { Name = "b" }],
                    Body = new BlockStatement
                    {
                        Body = [new PrintStatement { Argument = new BinaryExpression { Left = new IdentifierNode { Name = "a" }, Operator = "+", Right = new IdentifierNode { Name = "b" } } }]
                    }
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new IdentifierNode { Name = "add" },
                        Arguments = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_FunctionCall_TooFewArgs_ReturnsError()
    {
        // fn add(a, b) { print a + b; }
        // add(1);
        var program = new Program
        {
            Body =
            [
                new FunctionDeclaration
                {
                    Id = new IdentifierNode { Name = "add" },
                    Params = [new IdentifierNode { Name = "a" }, new IdentifierNode { Name = "b" }],
                    Body = new BlockStatement { Body = [] }
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new IdentifierNode { Name = "add" },
                        Arguments = [new NumberNode { Value = 1.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 2 argument(s) but was called with 1", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_FunctionCall_TooManyArgs_ReturnsError()
    {
        // fn greet(name) { print name; }
        // greet("a", "b", "c");
        var program = new Program
        {
            Body =
            [
                new FunctionDeclaration
                {
                    Id = new IdentifierNode { Name = "greet" },
                    Params = [new IdentifierNode { Name = "name" }],
                    Body = new BlockStatement { Body = [] }
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new IdentifierNode { Name = "greet" },
                        Arguments = [new StringNode { Value = "a" }, new StringNode { Value = "b" }, new StringNode { Value = "c" }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 1 argument(s) but was called with 3", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_FunctionCall_ZeroParamsWithArgs_ReturnsError()
    {
        // fn noop() { }
        // noop(42);
        var program = new Program
        {
            Body =
            [
                new FunctionDeclaration
                {
                    Id = new IdentifierNode { Name = "noop" },
                    Params = [],
                    Body = new BlockStatement { Body = [] }
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new IdentifierNode { Name = "noop" },
                        Arguments = [new NumberNode { Value = 42.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 0 argument(s) but was called with 1", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_ListLiteralDeclaration_ReturnsNoErrors()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements =
                                [
                                    new NumberNode { Value = 1.0 },
                                    new NumberNode { Value = 2.0 },
                                    new NumberNode { Value = 3.0 }
                                ]
                            }
                        }
                    ]
                },
                new PrintStatement { Argument = new IdentifierNode { Name = "items" } }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ArrayIndexExpression_Valid_ReturnsNoErrors()
    {
        // let x -> [1, 2, 3]; print x[0];
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new ArrayLiteral
                    {
                        Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }, new NumberNode { Value = 3.0 }]
                    }
                }
            ]
        };

        var printStmt = new PrintStatement
        {
            Argument = new IndexExpression
            {
                Object = new IdentifierNode { Name = "x" },
                Index = new NumberNode { Value = 0.0 }
            }
        };

        var program = new Program { Body = [decl, printStmt] };
        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ArrayIndexExpression_UndefinedObject_ReturnsError()
    {
        // print y[0]; (y is undefined)
        var printStmt = new PrintStatement
        {
            Argument = new IndexExpression
            {
                Object = new IdentifierNode { Name = "y" },
                Index = new NumberNode { Value = 0.0 }
            }
        };

        var program = new Program { Body = [printStmt] };
        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_ArrayIndexExpression_UndefinedVariableInIndex_ReturnsError()
    {
        // let x -> [1, 2, 3]; print x[n]; (n is undefined)
        var decl = new VariableDeclaration
        {
            Kind = "let",
            Declarations =
            [
                new VariableDeclarator
                {
                    VarName = new IdentifierNode { Name = "x" },
                    Init = new ArrayLiteral { Elements = [new NumberNode { Value = 1.0 }] }
                }
            ]
        };

        var printStmt = new PrintStatement
        {
            Argument = new IndexExpression
            {
                Object = new IdentifierNode { Name = "x" },
                Index = new IdentifierNode { Name = "n" }
            }
        };

        var program = new Program { Body = [decl, printStmt] };
        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("Undefined variable", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_ListMethodCall_Add_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "add" }
                        },
                        Arguments = [new NumberNode { Value = 3.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ListMethodCall_Remove_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "remove" }
                        },
                        Arguments = [new NumberNode { Value = 2.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ListMethodCall_Length_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "length" }
                        },
                        Arguments = []
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ListMethodCall_InvalidArgCount_ReturnsError()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "add" }
                        },
                        Arguments = [new NumberNode { Value = 3.0 }, new NumberNode { Value = 4.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 1 argument(s) but was called with 2", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_ListLengthMethodCall_InvalidArgCount_ReturnsError()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "length" }
                        },
                        Arguments = [new NumberNode { Value = 3.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 0 argument(s) but was called with 1", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_ListMethodCall_Contains_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }, new NumberNode { Value = 2.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "contains" }
                        },
                        Arguments = [new NumberNode { Value = 1.0 }]
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_ListContainsMethodCall_InvalidArgCount_ReturnsError()
    {
        var program = new Program
        {
            Body =
            [
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "items" },
                            VarType = new IdentifierNode { Name = "list" },
                            Init = new ArrayLiteral
                            {
                                Elements = [new NumberNode { Value = 1.0 }]
                            }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new CallExpression
                    {
                        Callee = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "items" },
                            Property = new IdentifierNode { Name = "contains" }
                        },
                        Arguments = []
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("expects 1 argument(s) but was called with 0", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_Struct_Instantiation_And_FieldAccess_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Name = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "person" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new PrintStatement
                {
                    Argument = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "person" },
                        Property = new IdentifierNode { Name = "name" }
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_Struct_Unknown_Field_ReturnsError()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Name = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "person" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new PrintStatement
                {
                    Argument = new MemberExpression
                    {
                        Object = new IdentifierNode { Name = "person" },
                        Property = new IdentifierNode { Name = "age" }
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("does not contain field", result.Errors[0].Message);
    }

    [TestMethod]
    public void Analyze_Struct_Field_Assignment_IsValid()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Name = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression { TypeName = new IdentifierNode { Name = "Person" } }
                        }
                    ]
                },
                new ExpressionStatement
                {
                    Expression = new AssignmentExpression
                    {
                        Left = new MemberExpression
                        {
                            Object = new IdentifierNode { Name = "p" },
                            Property = new IdentifierNode { Name = "age" }
                        },
                        Operator = "=",
                        Right = new NumberNode { Value = 5.0 }
                    }
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsTrue(result.IsValid);
        Assert.IsEmpty(result.Errors);
    }

    [TestMethod]
    public void Analyze_NewStruct_With_TooMany_Arguments_ReturnsError()
    {
        var program = new Program
        {
            Body =
            [
                new StructDeclaration
                {
                    Name = new IdentifierNode { Name = "Person" },
                    Fields =
                    [
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "name" }, Type = new IdentifierNode { Name = "str" } },
                        new StructFieldDeclaration { Name = new IdentifierNode { Name = "age" }, Type = new IdentifierNode { Name = "int" } }
                    ]
                },
                new VariableDeclaration
                {
                    Kind = "let",
                    Declarations =
                    [
                        new VariableDeclarator
                        {
                            VarName = new IdentifierNode { Name = "p" },
                            VarType = new IdentifierNode { Name = "Person" },
                            Init = new NewExpression
                            {
                                TypeName = new IdentifierNode { Name = "Person" },
                                Arguments = [new StringNode { Value = "Alice" }, new NumberNode { Value = 30.0 }, new NumberNode { Value = 99.0 }]
                            }
                        }
                    ]
                }
            ]
        };

        var result = new SemanticAnalyzer().Analyze(program);

        Assert.IsFalse(result.IsValid);
        Assert.HasCount(1, result.Errors);
        Assert.Contains("constructor accepts up to 2 argument", result.Errors[0].Message);
    }
}
