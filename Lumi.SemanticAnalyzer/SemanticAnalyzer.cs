using Lumi.AST;

namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Performs semantic analysis on an abstract syntax tree (AST) using the visitor pattern.
/// Validates variable definitions, type compatibility, and symbol references.
/// </summary>
public sealed class SemanticAnalyzer
{
    private readonly ScopeManager _scopes = new();

    /// <summary>
    /// Analyzes the given program node and returns a semantic analysis result.
    /// </summary>
    /// <param name="program">The root node of the syntax tree to analyze.</param>
    /// <returns>A SemanticAnalysisResult containing any errors found.</returns>
    public SemanticAnalysisResult Analyze(Program program)
    {
        var errors = new List<SemanticAnalyzerError>();

        _scopes.EnterScope(); // Global scope

        try
        {
            VisitProgram(program);
        }
        catch (SemanticAnalyzerError err)
        {
            errors.Add(err);
        }

        _scopes.ExitScope();

        return new SemanticAnalysisResult(errors);
    }

    private void Visit(Node node)
    {
        switch (node)
        {
            case Program program:
                VisitProgram(program);
                break;

            case BlockStatement block:
                VisitBlockStatement(block);
                break;

            case VariableDeclaration varDecl:
                VisitVariableDeclaration(varDecl);
                break;

            case IdentifierNode identifier:
                VisitIdentifierNode(identifier);
                break;

            case PrintStatement printStmt:
                VisitPrintStatement(printStmt);
                break;

            case IfStatement ifStmt:
                VisitIfStatement(ifStmt);
                break;

            case ForStatement forStmt:
                VisitForStatement(forStmt);
                break;

            case ExpressionStatement exprStmt:
                VisitExpressionStatement(exprStmt);
                break;

            case BinaryExpression binExpr:
                VisitBinaryExpression(binExpr);
                break;

            case AssignmentExpression assignExpr:
                VisitAssignmentExpression(assignExpr);
                break;

            case UnaryExpression unaryExpr:
                VisitUnaryExpression(unaryExpr);
                break;

            // Literal nodes require no semantic analysis
            case NumberNode:
            case StringNode:
            case BooleanNode:
                break;
        }
    }

    private void VisitProgram(Program program)
    {
        foreach (var statement in program.Body)
            Visit(statement);
    }

    private void VisitBlockStatement(BlockStatement block)
    {
        _scopes.EnterScope();
        foreach (var statement in block.Body)
            Visit(statement);
        _scopes.ExitScope();
    }

    private void VisitVariableDeclaration(VariableDeclaration varDecl)
    {
        foreach (var declarator in varDecl.Declarations)
        {
            if (declarator.VarName is not IdentifierNode varName)
                throw SemanticAnalyzerError.InvalidAssignmentTarget();

            var isReadOnly = varDecl.Kind == "const";
            var inferred = TypeKind.Unknown;

            // If an initializer is provided, analyze it and infer type
            if (declarator.Init is not null)
            {
                Visit(declarator.Init);
                inferred = InferType(declarator.Init);
            }

            var symbol = new Symbol(
                varName.Name,
                varDecl.Kind == "const" ? SymbolKind.Constant : SymbolKind.Variable,
                inferred,
                isReadOnly
            );

            _scopes.RegisterSymbol(symbol);
        }
    }

    private void VisitIdentifierNode(IdentifierNode identifier)
    {
        var symbol = _scopes.LookupSymbol(identifier.Name);
        if (symbol is null)
            throw SemanticAnalyzerError.UndefinedVariable(identifier.Name);
    }

    private void VisitPrintStatement(PrintStatement printStmt)
    {
        Visit(printStmt.Argument);
    }

    private void VisitIfStatement(IfStatement ifStmt)
    {
        Visit(ifStmt.Expr);

        _scopes.EnterScope();
        Visit(ifStmt.Stmt);
        _scopes.ExitScope();

        if (ifStmt.ElsePart is not null)
        {
            _scopes.EnterScope();
            Visit(ifStmt.ElsePart);
            _scopes.ExitScope();
        }
    }

    private void VisitForStatement(ForStatement forStmt)
    {
        _scopes.EnterScope();

        // Iterator must be an identifier
        if (forStmt.Iterator is not IdentifierNode iteratorName)
            throw SemanticAnalyzerError.InvalidAssignmentTarget();

        // Register the iterator variable
        var symbol = new Symbol(iteratorName.Name, SymbolKind.Variable, TypeKind.Number);
        _scopes.RegisterSymbol(symbol);

        // Analyze bounds and step
        Visit(forStmt.Start);
        Visit(forStmt.End);
        if (forStmt.Step is not null)
            Visit(forStmt.Step);

        // Analyze the body
        Visit(forStmt.Body);

        _scopes.ExitScope();
    }

    private void VisitExpressionStatement(ExpressionStatement exprStmt)
    {
        Visit(exprStmt.Expression);
    }

    private void VisitBinaryExpression(BinaryExpression binExpr)
    {
        Visit(binExpr.Left);
        Visit(binExpr.Right);
    }

    private void VisitAssignmentExpression(AssignmentExpression assignExpr)
    {
        if (assignExpr.Left is not IdentifierNode targetIdentifier)
            throw SemanticAnalyzerError.InvalidAssignmentTarget();

        var target = _scopes.LookupSymbol(targetIdentifier.Name);
        if (target is null)
            throw SemanticAnalyzerError.UndefinedVariable(targetIdentifier.Name);

        if (target.Value.IsReadOnly)
            throw SemanticAnalyzerError.AssignmentToReadOnlyVariable(targetIdentifier.Name);

        Visit(assignExpr.Right);
    }

    private void VisitUnaryExpression(UnaryExpression unaryExpr)
    {
        Visit(unaryExpr.Argument);
    }

    /// <summary>
    /// Infers the type of a node based on its structure and content.
    /// </summary>
    private static TypeKind InferType(Node node)
    {
        return node switch
        {
            NumberNode => TypeKind.Number,
            StringNode => TypeKind.String,
            BooleanNode => TypeKind.Boolean,
            BinaryExpression => TypeKind.Number, // Simplified: assume arithmetic results in numbers
            _ => TypeKind.Unknown
        };
    }
}