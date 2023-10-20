namespace LoxTests;

using Lox;

[TestClass]
public class TokenTests
{
    [TestMethod]
    public void TestTokenEquality()
    {
        var a = "123";
        var b = "123";
        var aLiteral = (object)123;
        var bLiteral = (object)123;
        Assert.AreEqual(
            new Token(TokenKind.Number, a, 1, aLiteral),
            new Token(TokenKind.Number, b, 1, bLiteral)
            );
    }
    
}