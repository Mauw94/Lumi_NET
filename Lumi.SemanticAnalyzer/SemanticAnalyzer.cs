using Lumi.AST;
using Lumi.Language;

namespace Lumi.SemanticAnalyzer;

/// <summary>
/// Performs semantic analysis on an abstract syntax tree (AST) using the visitor pattern.
/// Validates variable definitions, type compatibility, and symbol references.
/// </summary>
public sealed class SemanticAnalyzer
{
    private readonly record struct StructMethodSignature(int ParameterCount);

    private readonly ScopeManager _scopes = new();
    private readonly Dictionary<string, List<string>> _structFieldOrder = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, (TypeKind Type, string? StructName)>> _structFieldTypes = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Dictionary<string, StructMethodSignature>> _structMethods = new(StringComparer.Ordinal);
    private readonly Dictionary<string, (TypeKind Type, string? StructName)> _listElementTypes = new(StringComparer.Ordinal);
    private static readonly Dictionary<string, int> ListMethodParameterCounts = new(StringComparer.Ordinal)
    {
        ["add"] = 1,
        ["remove"] = 1,
        ["length"] = 0,
        ["contains"] = 1
    };

    public SemanticAnalyzer()
    {
        _scopes.EnterScope(); // Global scope
    }

    /// <summary>
    /// Analyzes the given program node and returns a semantic analysis result.
    /// </summary>
    /// <param name="program">The root node of the syntax tree to analyze.</param>
    /// <returns>A SemanticAnalysisResult containing any errors found.</returns>
    public SemanticAnalysisResult Analyze(Program program)
    {
        var errors = new List<SemanticAnalyzerError>();

        try
        {
            VisitProgram(program);
        }
        catch (SemanticAnalyzerError err)
        {
            errors.Add(err);
        }

        return new SemanticAnalysisResult(errors);
    }

    public void Dispose()
    {
        _scopes.ExitScope();
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

            case FunctionDeclaration funcDecl:
                VisitFunctionDeclaration(funcDecl);
                break;

            case StructDeclaration structDecl:
                VisitStructDeclaration(structDecl);
                break;

            case CallExpression callExpr:
                VisitCallExpression(callExpr);
                break;

            case MemberExpression memberExpr:
                VisitMemberExpression(memberExpr);
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

            case ArrayLiteral arrayLiteral:
                VisitArrayLiteral(arrayLiteral);
                break;

            case IndexExpression indexExpr:
                Visit(indexExpr.Object);
                Visit(indexExpr.Index);
                break;

            case NewExpression newExpression:
                VisitNewExpression(newExpression);
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
        // First pass: register all function declarations at the global level
        foreach (var statement in program.Body)
        {
            if (statement is FunctionDeclaration funcDecl)
            {
                if (funcDecl.Id is not IdentifierNode functionName)
                    throw SemanticAnalyzerError.InvalidFunctionDeclaration();

                var symbol = new Symbol(functionName.Name, SymbolKind.Function, TypeKind.Unknown, ParameterCount: funcDecl.Params.Count);
                _scopes.RegisterSymbol(symbol);
            }

            if (statement is StructDeclaration structDecl)
            {
                if (KeywordCatalog.Contains(structDecl.Name.Name))
                    throw SemanticAnalyzerError.StructNameIsKeyword(structDecl.Name.Name);

                var fields = new HashSet<string>(StringComparer.Ordinal);
                var methods = new HashSet<string>(StringComparer.Ordinal);
                var fieldOrder = new List<string>(structDecl.Fields.Count);
                var fieldTypes = new Dictionary<string, (TypeKind Type, string? StructName)>(StringComparer.Ordinal);
                var methodSignatures = new Dictionary<string, StructMethodSignature>(StringComparer.Ordinal);

                foreach (var field in structDecl.Fields)
                {
                    if (!fields.Add(field.Name.Name))
                        throw SemanticAnalyzerError.DuplicateStructField(structDecl.Name.Name, field.Name.Name);

                    fieldOrder.Add(field.Name.Name);
                    fieldTypes[field.Name.Name] = InferDeclaredTypeInfo(field.Type);
                }

                foreach (var method in structDecl.Methods)
                {
                    if (method.Id is not IdentifierNode methodName)
                        throw SemanticAnalyzerError.InvalidFunctionDeclaration();

                    if (KeywordCatalog.Contains(methodName.Name))
                        throw SemanticAnalyzerError.FunctionNameIsKeyword(methodName.Name);

                    if (fieldTypes.ContainsKey(methodName.Name))
                        throw SemanticAnalyzerError.DuplicateStructMember(structDecl.Name.Name, methodName.Name);

                    if (!methods.Add(methodName.Name))
                        throw SemanticAnalyzerError.DuplicateStructMethod(structDecl.Name.Name, methodName.Name);

                    methodSignatures[methodName.Name] = new StructMethodSignature(method.Params.Count);
                }

                _structFieldOrder[structDecl.Name.Name] = fieldOrder;
                _structFieldTypes[structDecl.Name.Name] = fieldTypes;
                _structMethods[structDecl.Name.Name] = methodSignatures;
                _scopes.RegisterSymbol(new Symbol(structDecl.Name.Name, SymbolKind.Struct, TypeKind.Struct, IsReadOnly: true, StructName: structDecl.Name.Name));
            }
        }

        // Second pass: analyze all statements (including function bodies)
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

            if (KeywordCatalog.Contains(varName.Name))
                throw SemanticAnalyzerError.VarNameIsKeyword(varName.Name);

            var isReadOnly = varDecl.Kind == "const";
            var (inferred, structName) = InferDeclaredTypeInfo(declarator.VarType);

            // If an initializer is provided, analyze it and infer type
            if (declarator.Init is not null)
            {
                Visit(declarator.Init);
                var (initializerType, initializerStructName) = InferTypeInfo(declarator.Init);

                if (inferred != TypeKind.Unknown && initializerType != TypeKind.Unknown)
                {
                    var isDifferentStruct = inferred == TypeKind.Struct
                        && initializerType == TypeKind.Struct
                        && !string.Equals(structName, initializerStructName, StringComparison.Ordinal);

                    if (inferred != initializerType || isDifferentStruct)
                    {
                        var expected = inferred == TypeKind.Struct ? structName ?? "struct" : inferred.ToString();
                        var actual = initializerType == TypeKind.Struct ? initializerStructName ?? "struct" : initializerType.ToString();
                        throw SemanticAnalyzerError.TypeMismatch(varName.Name, expected, actual);
                    }
                }

                if (inferred == TypeKind.Unknown)
                {
                    inferred = initializerType;
                    structName = initializerStructName;
                }
            }

            if (inferred == TypeKind.Array)
            {
                // Extract element type from explicit parameterized type annotation (e.g., list<Car>)
                if (declarator.VarType is ParameterizedTypeNode paramType)
                {
                    var elementTypeInfo = InferDeclaredTypeInfo(paramType.TypeArgument);

                    if (declarator.Init is ArrayLiteral arrayLiteral && arrayLiteral.Elements.Count > 0)
                    {
                        var initializerElementTypeInfo = InferArrayElementTypeInfo(arrayLiteral);
                        var isDifferentElementStruct = elementTypeInfo.Type == TypeKind.Struct
                            && initializerElementTypeInfo.Type == TypeKind.Struct
                            && !string.Equals(elementTypeInfo.StructName, initializerElementTypeInfo.StructName, StringComparison.Ordinal);

                        if (elementTypeInfo.Type != TypeKind.Unknown
                            && initializerElementTypeInfo.Type != TypeKind.Unknown
                            && (elementTypeInfo.Type != initializerElementTypeInfo.Type || isDifferentElementStruct))
                        {
                            var expected = elementTypeInfo.Type == TypeKind.Struct
                                ? elementTypeInfo.StructName ?? "struct"
                                : elementTypeInfo.Type.ToString();
                            var actual = initializerElementTypeInfo.Type == TypeKind.Struct
                                ? initializerElementTypeInfo.StructName ?? "struct"
                                : initializerElementTypeInfo.Type.ToString();
                            throw SemanticAnalyzerError.TypeMismatch(varName.Name, expected, actual);
                        }
                    }
                    if (elementTypeInfo.Type != TypeKind.Unknown)
                        _listElementTypes[varName.Name] = elementTypeInfo;
                }
                // Or infer element type from initializer array literal
                else if (declarator.Init is ArrayLiteral arrayLiteral)
                {
                    var elementTypeInfo = InferArrayElementTypeInfo(arrayLiteral);
                    if (elementTypeInfo.Type != TypeKind.Unknown)
                        _listElementTypes[varName.Name] = elementTypeInfo;
                }
            }

            var symbol = new Symbol(
                Name: varName.Name,
                Kind: varDecl.Kind == "const" ? SymbolKind.Constant : SymbolKind.Variable,
                Type: inferred,
                IsReadOnly: isReadOnly,
                StructName: structName
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
        var symbol = new Symbol(iteratorName.Name, SymbolKind.Variable, TypeKind.Int);
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
        if (assignExpr.Left is IdentifierNode targetIdentifier)
        {
            var target = _scopes.LookupSymbol(targetIdentifier.Name);
            if (target is null)
                throw SemanticAnalyzerError.UndefinedVariable(targetIdentifier.Name);

            if (target.Value.IsReadOnly)
                throw SemanticAnalyzerError.AssignmentToReadOnlyVariable(targetIdentifier.Name);

            Visit(assignExpr.Right);
            return;
        }

        if (assignExpr.Left is MemberExpression memberAssignment)
        {
            VisitMemberExpression(memberAssignment);
            Visit(assignExpr.Right);

            var (objectType, structName) = InferTypeInfo(memberAssignment.Object);
            if (objectType != TypeKind.Struct || string.IsNullOrWhiteSpace(structName))
                throw SemanticAnalyzerError.InvalidAssignmentTarget();

            var expectedTypeInfo = GetStructFieldType(structName, memberAssignment.Property.Name);
            var actualTypeInfo = InferTypeInfo(assignExpr.Right);

            if (expectedTypeInfo.Type != TypeKind.Unknown && actualTypeInfo.Type != TypeKind.Unknown)
            {
                var isDifferentStruct = expectedTypeInfo.Type == TypeKind.Struct
                    && actualTypeInfo.Type == TypeKind.Struct
                    && !string.Equals(expectedTypeInfo.StructName, actualTypeInfo.StructName, StringComparison.Ordinal);

                if (expectedTypeInfo.Type != actualTypeInfo.Type || isDifferentStruct)
                {
                    var expected = expectedTypeInfo.Type == TypeKind.Struct ? expectedTypeInfo.StructName ?? "struct" : expectedTypeInfo.Type.ToString();
                    var actual = actualTypeInfo.Type == TypeKind.Struct ? actualTypeInfo.StructName ?? "struct" : actualTypeInfo.Type.ToString();

                    throw SemanticAnalyzerError.TypeMismatch($"{structName}.{memberAssignment.Property.Name}", expected, actual);
                }
            }

            return;
        }

        throw SemanticAnalyzerError.InvalidAssignmentTarget();
    }

    private void VisitUnaryExpression(UnaryExpression unaryExpr)
    {
        Visit(unaryExpr.Argument);
    }

    private void VisitArrayLiteral(ArrayLiteral arrayLiteral)
    {
        foreach (var element in arrayLiteral.Elements)
        {
            Visit(element);
        }
    }

    private void VisitFunctionDeclaration(FunctionDeclaration funcDecl)
    {
        VisitFunctionDeclaration(funcDecl, receiverStructName: null);
    }

    private void VisitFunctionDeclaration(FunctionDeclaration funcDecl, string? receiverStructName)
    {
        if (funcDecl.Id is not IdentifierNode functionName)
            throw SemanticAnalyzerError.InvalidFunctionDeclaration();

        if (receiverStructName is null && KeywordCatalog.Contains(functionName.Name))
            throw SemanticAnalyzerError.FunctionNameIsKeyword(functionName.Name);

        // Function is already registered by VisitProgram's first pass,
        // so we only need to analyze the body here.

        // Create a new scope for the function body
        _scopes.EnterScope();

        if (receiverStructName is not null)
        {
            var thisSymbol = new Symbol("this", SymbolKind.Variable, TypeKind.Struct, IsReadOnly: true, StructName: receiverStructName);
            _scopes.RegisterSymbol(thisSymbol);
        }

        // Register parameters as variables in the function scope
        foreach (var param in funcDecl.Params)
        {
            if (param is not IdentifierNode paramName)
                throw SemanticAnalyzerError.InvalidFunctionParameter();

            var paramSymbol = new Symbol(paramName.Name, SymbolKind.Variable, TypeKind.Unknown);
            _scopes.RegisterSymbol(paramSymbol);
        }

        // Analyze the function body
        Visit(funcDecl.Body);

        _scopes.ExitScope();
    }

    private void VisitStructDeclaration(StructDeclaration structDecl)
    {
        if (KeywordCatalog.Contains(structDecl.Name.Name))
            throw SemanticAnalyzerError.StructNameIsKeyword(structDecl.Name.Name);

        foreach (var method in structDecl.Methods)
            VisitFunctionDeclaration(method, structDecl.Name.Name);
    }

    private void VisitMemberExpression(MemberExpression memberExpr)
    {
        Visit(memberExpr.Object);

        var (objectType, structName) = InferTypeInfo(memberExpr.Object);

        if (objectType == TypeKind.Struct)
        {
            if (string.IsNullOrWhiteSpace(structName) || !_structFieldTypes.TryGetValue(structName, out var fields))
                throw SemanticAnalyzerError.UndefinedStruct(structName ?? "");

            if (!fields.ContainsKey(memberExpr.Property.Name))
                throw SemanticAnalyzerError.UnknownStructField(structName, memberExpr.Property.Name);

            return;
        }

        if (objectType == TypeKind.Array)
            return;

        throw SemanticAnalyzerError.MemberAccessNotSupportedOnType(objectType);
    }

    private void VisitCallExpression(CallExpression callExpr)
    {
        if (callExpr.Callee is IdentifierNode functionName)
        {
            // Look up the function
            var function = _scopes.LookupSymbol(functionName.Name);
            if (function is null || function.Value.Kind != SymbolKind.Function)
                throw SemanticAnalyzerError.UndefinedFunction(functionName.Name);

            // Validate argument count matches the declared parameter count
            if (function.Value.ParameterCount.HasValue && callExpr.Arguments.Count != function.Value.ParameterCount.Value)
                throw SemanticAnalyzerError.ArgumentCountMismatch(functionName.Name, function.Value.ParameterCount.Value, callExpr.Arguments.Count);

            // Analyze arguments
            foreach (var arg in callExpr.Arguments)
            {
                Visit(arg);
            }

            return;
        }

        if (callExpr.Callee is not MemberExpression memberExpr)
            throw SemanticAnalyzerError.InvalidMethodCall();

        Visit(memberExpr.Object);

        foreach (var arg in callExpr.Arguments)
        {
            Visit(arg);
        }

        var objectType = InferType(memberExpr.Object);

        if (objectType == TypeKind.Array)
        {
            if (!ListMethodParameterCounts.TryGetValue(memberExpr.Property.Name, out var expectedArgumentCount))
                throw SemanticAnalyzerError.UnknownListMethod(memberExpr.Property.Name);

            if (callExpr.Arguments.Count != expectedArgumentCount)
                throw SemanticAnalyzerError.MethodArgumentCountMismatch(memberExpr.Property.Name, expectedArgumentCount, callExpr.Arguments.Count);

            return;
        }

        var (_, structName) = InferTypeInfo(memberExpr.Object);
        if (objectType == TypeKind.Struct)
        {
            if (string.IsNullOrWhiteSpace(structName) || !_structMethods.TryGetValue(structName, out var methods))
                throw SemanticAnalyzerError.UndefinedStruct(structName ?? "");

            if (!methods.TryGetValue(memberExpr.Property.Name, out var methodSignature))
                throw SemanticAnalyzerError.UnknownStructMethod(structName, memberExpr.Property.Name);

            if (callExpr.Arguments.Count != methodSignature.ParameterCount)
                throw SemanticAnalyzerError.MethodArgumentCountMismatch(memberExpr.Property.Name, methodSignature.ParameterCount, callExpr.Arguments.Count);

            return;
        }

        throw SemanticAnalyzerError.MethodNotSupportedOnType(memberExpr.Property.Name, objectType);
    }

    private void VisitNewExpression(NewExpression newExpression)
    {
        var structName = newExpression.TypeName.Name;
        if (!_structFieldOrder.TryGetValue(structName, out var fieldOrder))
            throw SemanticAnalyzerError.UndefinedStruct(structName);

        foreach (var argument in newExpression.Arguments)
            Visit(argument);

        if (newExpression.Arguments.Count > fieldOrder.Count)
            throw SemanticAnalyzerError.StructConstructorArgumentCountMismatch(structName, fieldOrder.Count, newExpression.Arguments.Count);

        for (int i = 0; i < newExpression.Arguments.Count; i++)
        {
            var fieldName = fieldOrder[i];
            var expectedTypeInfo = GetStructFieldType(structName, fieldName);
            var actualTypeInfo = InferTypeInfo(newExpression.Arguments[i]);

            if (expectedTypeInfo.Type == TypeKind.Unknown || actualTypeInfo.Type == TypeKind.Unknown)
                continue;

            var isDifferentStruct = expectedTypeInfo.Type == TypeKind.Struct
                && actualTypeInfo.Type == TypeKind.Struct
                && !string.Equals(expectedTypeInfo.StructName, actualTypeInfo.StructName, StringComparison.Ordinal);

            if (expectedTypeInfo.Type != actualTypeInfo.Type || isDifferentStruct)
            {
                var expected = expectedTypeInfo.Type == TypeKind.Struct ? expectedTypeInfo.StructName ?? "struct" : expectedTypeInfo.Type.ToString();
                var actual = actualTypeInfo.Type == TypeKind.Struct ? actualTypeInfo.StructName ?? "struct" : actualTypeInfo.Type.ToString();
                throw SemanticAnalyzerError.TypeMismatch($"{structName}.{fieldName}", expected, actual);
            }
        }
    }

    private (TypeKind Type, string? StructName) InferDeclaredTypeInfo(Node? typeNode)
    {
        if (typeNode is ParameterizedTypeNode paramType)
        {
            var baseTypeName = paramType.BaseTypeName.ToLowerInvariant();
            if (baseTypeName != "list")
                throw SemanticAnalyzerError.InvalidTypeAnnotation(paramType.BaseTypeName);

            _ = InferDeclaredTypeInfo(paramType.TypeArgument);
            return (TypeKind.Array, null);
        }

        if (typeNode is not IdentifierNode typeIdentifier)
            return (TypeKind.Unknown, null);

        var primitiveType = typeIdentifier.Name.ToLowerInvariant() switch
        {
            "int" or "float" or "number" => TypeKind.Int,
            "string" or "str" => TypeKind.String,
            "bool" or "boolean" => TypeKind.Boolean,
            "list" or "array" => TypeKind.Array,
            _ => TypeKind.Unknown,
        };

        if (primitiveType != TypeKind.Unknown)
            return (primitiveType, null);

        if (_structFieldOrder.ContainsKey(typeIdentifier.Name))
            return (TypeKind.Struct, typeIdentifier.Name);

        return (TypeKind.Unknown, null);
    }

    private TypeKind InferType(Node node)
    {
        var (type, _) = InferTypeInfo(node);

        return type;
    }

    private (TypeKind Type, string? StructName) InferTypeInfo(Node node)
    {
        if (node is IdentifierNode identifier)
        {
            var symbol = _scopes.LookupSymbol(identifier.Name);

            if (symbol.HasValue)
                return (symbol.Value.Type, symbol.Value.StructName);
        }

        if (node is IndexExpression indexExpr)
        {
            var objectTypeInfo = InferTypeInfo(indexExpr.Object);
            if (objectTypeInfo.Type == TypeKind.Array)
            {
                if (indexExpr.Object is IdentifierNode objectIdentifier
                    && _listElementTypes.TryGetValue(objectIdentifier.Name, out var inferredElementType))
                {
                    return inferredElementType;
                }

                if (indexExpr.Object is ArrayLiteral arrayLiteral)
                    return InferArrayElementTypeInfo(arrayLiteral);

                return (TypeKind.Unknown, null);
            }
        }

        return InferTypeFromNode(node);
    }

    private (TypeKind Type, string? StructName) InferArrayElementTypeInfo(ArrayLiteral arrayLiteral)
    {
        if (arrayLiteral.Elements.Count == 0)
            return (TypeKind.Unknown, null);

        var inferred = InferTypeInfo(arrayLiteral.Elements[0]);
        if (inferred.Type == TypeKind.Unknown)
            return (TypeKind.Unknown, null);

        for (int i = 1; i < arrayLiteral.Elements.Count; i++)
        {
            var current = InferTypeInfo(arrayLiteral.Elements[i]);
            var isDifferentStruct = inferred.Type == TypeKind.Struct
                && current.Type == TypeKind.Struct
                && !string.Equals(inferred.StructName, current.StructName, StringComparison.Ordinal);

            if (current.Type != inferred.Type || isDifferentStruct)
                return (TypeKind.Unknown, null);
        }

        return inferred;
    }

    /// <summary>
    /// Infers the type of a node based on its structure and content.
    /// </summary>
    private (TypeKind Type, string? StructName) InferTypeFromNode(Node node)
    {
        return node switch
        {
            NumberNode => (TypeKind.Int, null),
            StringNode => (TypeKind.String, null),
            BooleanNode => (TypeKind.Boolean, null),
            ArrayLiteral => (TypeKind.Array, null),
            BinaryExpression => (TypeKind.Int, null), // Simplified: assume arithmetic results in numbers
            NewExpression newExpression when _structFieldOrder.ContainsKey(newExpression.TypeName.Name) => (TypeKind.Struct, newExpression.TypeName.Name),
            MemberExpression memberExpression => InferMemberExpressionType(memberExpression),
            _ => (TypeKind.Unknown, null)
        };
    }

    private (TypeKind Type, string? StructName) InferMemberExpressionType(MemberExpression memberExpression)
    {
        var (objectType, structName) = InferTypeInfo(memberExpression.Object);
        if (objectType != TypeKind.Struct || string.IsNullOrWhiteSpace(structName))
            return (TypeKind.Unknown, null);

        return GetStructFieldType(structName, memberExpression.Property.Name);
    }

    private (TypeKind Type, string? StructName) GetStructFieldType(string structName, string fieldName)
    {
        if (_structFieldTypes.TryGetValue(structName, out var fields)
            && fields.TryGetValue(fieldName, out var fieldType))
        {
            return fieldType;
        }

        return (TypeKind.Unknown, null);
    }
}
