using LoxInterpreter;
using LoxLexer;
using Environment = LoxInterpreter.Environment;

namespace LoxInterpreterTests;

public class EnvironmentTest
{
    // Single environment tests.

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
    
    // Multiple environment tests
    
    [Fact]
    public void TestDefineWithEnclosing()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);

        Environment global = new();
        Environment inner = new(global);
        
        // The inner scope doesn't have an "a".
        global.Define("a", 10.0);
        Assert.Equal(10.0, global.Get(a));
        Assert.Equal(10.0, inner.Get(a));
        
        // Shadow the "a", and expect the new value.
        inner.Define("a", 20.0);
        Assert.Equal(10.0, global.Get(a));
        Assert.Equal(20.0, inner.Get(a));
    }

    [Fact]
    public void TestAssignWithEnclosing()
    {
        var a = new Token(TokenKind.Identifier, "a", 1);
        var b = new Token(TokenKind.Identifier, "b", 1);

        Environment global = new();
        Environment inner = new(global);
        
        // The inner scope doesn't have an "a".
        global.Define("a", 10.0);
        Assert.Equal(10.0, global.Get(a));
        Assert.Equal(10.0, inner.Get(a));
        
        // Shadow the "a", and then assign to it.
        inner.Define("a", 20.0);
        inner.Assign(a, 30.0);
        Assert.Equal(10.0, global.Get(a));
        Assert.Equal(30.0, inner.Get(a));
        
        global.Define("b", 100.0);
        inner.Assign(b, 200.0);
        Assert.Equal(200.0, global.Get(b));
        Assert.Equal(200.0, inner.Get(b));
    }
}