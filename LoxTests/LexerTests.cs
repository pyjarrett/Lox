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

    [TestMethod]
    public void TestScanNumberLiteral()
    {
        VerifyTokens("0 123 0.5 2.0", new()
        {
            new(TokenKind.Number, "0", 1, 0.0),
            new(TokenKind.Number, "123", 1, 123.0),
            new(TokenKind.Number, "0.5", 1, 0.5),
            new(TokenKind.Number, "2.0", 1, 2.0),
            new(TokenKind.EndOfFile, "", 1),
        });
    }
    
    /// <summary>
    /// Ensure that multiple scans return the same set of tokens.
    /// </summary>
    [TestMethod]
     public void TestMultipleScanTokens()
    {
        Lexer lexer = new("0 100 200.0");
        List<Token> actual = lexer.ScanTokens();
        List<Token> expected = new()
        {
            new(TokenKind.Number, "0", 1, 0.0),
            new (TokenKind.Number, "100", 1, 100.0),
            new (TokenKind.Number, "200.0", 1, 200.0),
            new (TokenKind.EndOfFile, "", 1),
        };
        Assert.IsFalse(lexer.HasError);
        CollectionAssert.AreEqual(expected, actual);
        CollectionAssert.AreEqual(actual, lexer.ScanTokens());
    }

    private void VerifyTokens(string text, List<Token> expected)
    {
        Lexer lexer = new(text);
        List<Token> actual = lexer.ScanTokens();
        Assert.IsFalse(lexer.HasError);
        CollectionAssert.AreEqual(expected, actual);
    }
}