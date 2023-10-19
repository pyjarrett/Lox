namespace LoxTests;

using Lox;

[TestClass]
public class LexerTests
{
    [TestMethod]
    public void SingleTokenParse()
    {
        Lexer lexer = new Lexer(",-");
        var actual = lexer.ScanTokens();
        var expected = new List<Token>()
        {
            new Token(TokenKind.Comma, ",", 1),
            new Token(TokenKind.Minus, "-", 1),
            new Token(TokenKind.EndOfFile, "", 1),
        };
        CollectionAssert.AreEqual(expected:expected, actual:actual);
    }
}