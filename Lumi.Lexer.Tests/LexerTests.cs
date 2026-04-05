namespace Lumi.Lexer.Tests;

using Lumi.Lexer;

[TestClass]
public sealed class LexerTests
{
    [TestMethod]
    public void Test_Number_Lexing()
    {
        var tokens = Tokenize("42");
        Assert.AreEqual(TokenKind.Number, tokens[0].Kind);
        Assert.AreEqual(42, tokens[0].Number);
    }

    [TestMethod]
    public void Test_String_Lexing()
    {
        var tokens = Tokenize("\"42\"");
        Assert.AreEqual(TokenKind.String, tokens[0].Kind);
        Assert.AreEqual("42", tokens[0].Value);
    }

    [TestMethod]
    public void Test_String_Number_Lexing()
    {
        var tokens = Tokenize("22 \"42\"");

        Assert.AreEqual(TokenKind.Number, tokens[0].Kind);
        Assert.AreEqual(22, tokens[0].Number);

        Assert.AreEqual(TokenKind.String, tokens[1].Kind);
        Assert.AreEqual("42", tokens[1].Value);
    }

    [TestMethod]
    public void Test_Line_Comment_Lexing()
    {
        var tokens = Tokenize("// This is a comment\n42");

        Assert.AreEqual(TokenKind.Comment, tokens[0].Kind);
        Assert.AreEqual(" This is a comment", tokens[0].Value);

        Assert.AreEqual(TokenKind.Number, tokens[1].Kind);
        Assert.AreEqual(42, tokens[1].Number);
    }

    [TestMethod]
    public void Test_Block_Comment_Lexing()
    {
        var tokens = Tokenize("/* This is a comment " +
            "More comments " +
            "And here */ \n42");

        Assert.AreEqual(TokenKind.Comment, tokens[0].Kind);
        Assert.AreEqual(" This is a comment More comments And here ", tokens[0].Value);

        Assert.AreEqual(TokenKind.Number, tokens[1].Kind);
        Assert.AreEqual(42, tokens[1].Number);
    }

    [TestMethod]
    public void Test_Boolean_Identifier_Lexing()
    {
        var tokens = Tokenize("false true");

        Assert.AreEqual(TokenKind.Boolean, tokens[0].Kind);
        Assert.AreEqual("false", tokens[0].Value);

        Assert.AreEqual(TokenKind.Boolean, tokens[1].Kind);
        Assert.AreEqual("true", tokens[1].Value);
    }

    [TestMethod]
    public void Test_Keyword_Identifier_Lexing()
    {
        var tokens = Tokenize("let print const");

        Assert.AreEqual(TokenKind.Keyword, tokens[0].Kind);
        Assert.AreEqual("let", tokens[0].Value);

        Assert.AreEqual(TokenKind.Keyword, tokens[1].Kind);
        Assert.AreEqual("print", tokens[1].Value);

        Assert.AreEqual(TokenKind.Keyword, tokens[2].Kind);
        Assert.AreEqual("const", tokens[2].Value);
    }

    [TestMethod]
    public void Test_Operator_Lexing()
    {
        var tokens = Tokenize("== -> ;");

        Assert.AreEqual(TokenKind.EqualEqual, tokens[0].Kind);
        Assert.AreEqual(TokenKind.Arrow, tokens[1].Kind);
        Assert.AreEqual(TokenKind.Semicolon, tokens[2].Kind);
    }

    [TestMethod]
    public void Test_Lexer_Error_Unterminated_String()
    {
        Assert.Throws<LexError>(() => Tokenize("\"Unterminated string"));
    }

    [TestMethod]
    public void Test_Lexer_Error_Unterminated_Comment()
    {
        Assert.Throws<LexError>(() => Tokenize("/* This is a comment."));
    }

    private static IReadOnlyList<Token> Tokenize(string source)
    {
        var lexer = new Lexer(source);
        return lexer.Tokenize();
    }
}
