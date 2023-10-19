namespace LoxTests;

using Lox;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void SingleTokenParse()
    {
        VerifyTokens(", -", new() {
                new Token(TokenKind.Comma, ",", 1),
                new Token(TokenKind.Minus, "-", 1),
                new Token(TokenKind.EndOfFile, "", 1),
            }
        );
    }

    [TestMethod]
    public void UnknownTokenParse()
    {
        Lexer lexer = new Lexer("this is a bad parse");
        var actual = lexer.ScanTokens();
        var expected = new List<Token>();
        Assert.IsTrue(lexer.HasError);
    }

    [TestMethod]
    public void TestString()
    {
        VerifyTokens("\"A string\"", new()
        {
            new(TokenKind.String, "\"A string\"", 1, "A string"),
            new(TokenKind.EndOfFile, "", 1),
        });
    }

    [TestMethod]
    public void TestUnterminatedString()
    {
        Lexer lexer = new("\"unterminated string!");
        lexer.ScanTokens();
        Assert.IsTrue(lexer.HasError);
    }

    [TestMethod]
    public void TestLineComment()
    {
        VerifyTokens("    //  this is a test", new()
        {
            new(TokenKind.EndOfFile, "", 1),
        });
    }

    private void VerifyTokens(string text, List<Token> expected)
    {
        Lexer lexer = new(text);
        List<Token> actual = lexer.ScanTokens();
        Assert.IsFalse(lexer.HasError);
        CollectionAssert.AreEqual(expected, actual);
    }
}