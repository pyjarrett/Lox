using LoxLexer;
using LoxInterpreter;
using LoxParser;
using NuGet.Frameworks;

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
    public void TestParseError()
    {
        VerifyThrows(typeof(ParseError), "(5.0");
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
}