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

    private static IReadOnlyList<Token> Tokenize(string source)
    {
        var lexer = new Lexer(source);
        return lexer.Tokenize();
    }
}
