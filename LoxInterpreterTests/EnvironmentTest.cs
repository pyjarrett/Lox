using LoxInterpreter;
using LoxLexer;
using Environment = LoxInterpreter.Environment;

namespace LoxInterpreterTests;

public class EnvironmentTest
{
    [Fact]
    public void TestDefine()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);

        Environment env = new();
        env.Define("a", 10.0);
        Assert.Equal(10.0, env.Get(a));
    }

    [Fact]
    public void TestAssign()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);

        Environment env = new();
        env.Define("a", 10.0);
        Assert.Equal(10.0, env.Get(a));
        env.Assign(a, 20.0);
        Assert.Equal(20.0, env.Get(a));
    }

    [Fact]
    public void TestGetUndefined()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);

        Environment env = new();
        Assert.Throws<RuntimeError>(() => env.Get(a));
    }

    [Fact]
    public void TestAssignToUndefined()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);

        Environment env = new();
        Assert.Throws<RuntimeError>(() => env.Assign(a, 20.0));
    }
}