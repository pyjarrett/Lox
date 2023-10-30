using LoxLexer;
using LoxInterpreter;
using LoxParser;

namespace LoxInterpreterTests;

public class InterpreterTests
{
    [Fact]
    public void TestArithmetic()
    {
        VerifyEvaluation(7.0, "1 + 2 * 3");
        VerifyEvaluation(2.0, "1 + 8 * 2 / 4 - (3.0 * 1.0)");
    }

    [Fact]
    public void TestLogicalOperators()
    {
        // Boolean versions
        VerifyEvaluation(true, "true or false");
        VerifyEvaluation(true, "false or true");
        VerifyEvaluation(false, "false or false");
        VerifyEvaluation(false, "true and false");
        VerifyEvaluation(true, "true and true");

        // Object return versions (like Ruby's ||)
        VerifyEvaluation(20.0, "nil or 20.0");
        VerifyEvaluation(10.0, "10.0 or 20.0");

        VerifyEvaluation(null, "nil and 20.0");
        VerifyEvaluation(20.0, "10.0 and 20.0");
    }

    [Fact]
    public void TestComparisonOperators()
    {
        VerifyEvaluation(true, "1 != 2");
        VerifyEvaluation(true, "1 != 2");
        VerifyEvaluation(false, "1 == 2");
        VerifyEvaluation(true, "1 == 1");
        VerifyEvaluation(true, "1 < 2");
        VerifyEvaluation(false, "2 < 1");
        VerifyEvaluation(true, "1 <= 2");
        VerifyEvaluation(true, "2 > 1");
        VerifyEvaluation(true, "2 >= 1");

        VerifyEvaluation(true, "nil == nil");
        VerifyEvaluation(false, "nil == 10");
    }

    [Fact]
    public void TestUnaryEvaluation()
    {
        VerifyEvaluation(10.0, "+10.0");
        VerifyEvaluation(-10.0, "-10.0");
        VerifyEvaluation(-20.0, "-(5 * 4)");

        VerifyEvaluation(true, "!nil");
        VerifyEvaluation(false, "!!nil");
        VerifyEvaluation(false, "!5.0");
    }

    [Fact]
    public void TestTruthiness()
    {
        VerifyEvaluation(null, "nil");

        VerifyEvaluation(true, "true");
        VerifyEvaluation(false, "false");
    }

    [Fact]
    public void TestRuntimeErrors()
    {
        VerifyThrows(typeof(RuntimeError), "-true");
        VerifyThrows(typeof(RuntimeError), "-false");
        VerifyThrows(typeof(RuntimeError), "false + true");
    }

    [Fact]
    public void TestStringIsNotCallable()
    {
        VerifyThrows(typeof(RuntimeError), "\"notCallable\"(1, nil, \"param\")");
    }

    [Fact]
    public void TestParseError()
    {
        VerifyThrows(typeof(ParseError), "(5.0");
    }

    [Fact]
    public void TestScopes()
    {
        VerifyOutput(@"20
10
",
            @"var a = 10;
{
    var a = 20.0;
    print a;
}
print a;
");
    }

    [Fact]
    public void TestWhileLoop()
    {
        VerifyOutput(@"1
3
5
",
            @"var a = 1;
while (a <= 5) {
    print a;
    a = a + 2;
}
");
    }

    [Fact]
    public void TestForLoop()
    {
        VerifyOutput(@"1
3
5
",
            @"
for (var a = 1; a <= 5; a = a + 2) {
    print a;
}
");
    }

    private void VerifyThrows(Type exception, string text)
    {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer.ScanTokens());
        Interpreter interpreter = new Interpreter();
        Assert.Throws(exception, () => interpreter.Evaluate(parser.Expression()));
    }

    private void VerifyEvaluation(object? expected, string text)
    {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer.ScanTokens());
        Interpreter interpreter = new Interpreter();
        Assert.Equal(expected, interpreter.Evaluate(parser.Expression()));
    }

    private void VerifyOutput(string expected, string input)
    {
        StringBufferOutput testLog = new StringBufferOutput();
        Lexer lexer = new Lexer(input);
        Parser parser = new Parser(lexer.ScanTokens());
        Interpreter interpreter = new Interpreter(testLog);
        interpreter.Interpret(parser.Parse());
        Assert.Equal(expected, testLog.OutputLog);
    }
}