namespace LoxTests;

using LoxLexer;

public class TokenTests
{
    /// <summary>
    /// I wasn't sure if the boxing of primitive values was affecting the
    /// equality function, so I wrote this. 
    /// </summary>
    [Fact]
    public void TestTokenEquality()
    {
        var a = "123";
        var b = "123";
        var aLiteral = (object)123.0;
        var bLiteral = (object)123.0;
        Assert.Equal(
            new Token(TokenKind.Number, a, 1, aLiteral),
            new Token(TokenKind.Number, b, 1, bLiteral)
            );
    }
    
}