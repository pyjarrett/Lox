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

    /// <summary>
    /// Test a simple function definition.
    /// </summary>
    [TestMethod]
    public void TestFunctionDefinition()
    {
        VerifyTokens(@"fun sum(a, b) {
    return a + b;
}", new()
        {
            new(TokenKind.Fun, "fun", 1),
            new(TokenKind.Identifier, "sum", 1, "sum"),
            new(TokenKind.LeftParen, "(", 1), 
            new(TokenKind.Identifier, "a", 1, "a"),
            new(TokenKind.Comma, ",", 1),
            new(TokenKind.Identifier, "b", 1, "b"),
            new(TokenKind.RightParen, ")", 1),
            new(TokenKind.LeftBrace, "{", 1),
            
            new(TokenKind.Return, "return", 2),
            new(TokenKind.Identifier, "a", 2, "a"),
            new(TokenKind.Plus, "+", 2),
            new(TokenKind.Identifier, "b", 2, "b"),
            new(TokenKind.Semicolon, ";", 2),
            
            new(TokenKind.RightBrace, "}", 3),
            
            new(TokenKind.EndOfFile, "", 3),
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