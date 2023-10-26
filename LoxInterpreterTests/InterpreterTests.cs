using LoxLexer;
using LoxInterpreter;
using LoxParser;
using NuGet.Frameworks;

namespace LoxInterpreterTests;

public class InterpreterTests
{
    [Fact]
    public void Test1()
    {
        VerifyEvaluation(7.0, "1 + 2 * 3");
        VerifyEvaluation(2.0, "1 + 8 * 2 / 4 - (3.0 * 1.0)");

        VerifyEvaluation(true, "1 != 2");
        VerifyEvaluation(false, "1 == 2");
        VerifyEvaluation(true, "1 < 2");
        VerifyEvaluation(false, "2 < 1");
        VerifyEvaluation(true, "1 <= 2");
        VerifyEvaluation(true, "2 > 1");
        VerifyEvaluation(true, "2 >= 1");

        VerifyEvaluation(10.0, "+10.0");
        VerifyEvaluation(-10.0, "-10.0");
        VerifyEvaluation(-20.0, "-(5 * 4)");

        VerifyEvaluation(null, "nil");
        VerifyEvaluation(true, "nil == nil");
        VerifyEvaluation(false, "nil == 10");
        VerifyEvaluation(true, "!nil");
        VerifyEvaluation(false, "!!nil");

        VerifyEvaluation(true, "true");
        VerifyEvaluation(false, "false");

        VerifyThrows(typeof(RuntimeError), "-true");
        VerifyThrows(typeof(RuntimeError), "-false");
        VerifyThrows(typeof(RuntimeError), "false + true");

        VerifyThrows(typeof(ParseError), "(5.0");

        VerifyEvaluation(false, "!5.0");
    }

    private void Evaluate(string text)
    {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer.ScanTokens());
        Interpreter interpreter = new Interpreter();
        interpreter.Evaluate(parser.Expression());
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