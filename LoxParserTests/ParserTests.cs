using LoxAst;
using LoxLexer;
using LoxParser;
using Xunit.Abstractions;

namespace LoxParserTests;

public class ParserTests
{
    private ITestOutputHelper output;

    public ParserTests(ITestOutputHelper helper)
    {
        output = helper;
    }

    [Fact]
    public void PlainArithmeticExpression()
    {
        // 1 + 2.0 / 3.0 * 4 - --5
        // 1 + (2.0 / 3.0) * 4 - --5
        // 1 + ((2.0 / 3.0) * 4) - --5
        // (1 + ((2.0 / 3.0) * 4)) - --5
        // (1 + ((2.0 / 3.0) * 4)) - (-(-5)))
        //
        //              "-"
        //           /      \
        //         "+"      "-"
        //        /   \        \
        //       1    "*"      "-"
        //           /   \       \
        //         "/"    4       5
        //        /   \
        //      2.0    3.0
        Lexer lexer = new Lexer("1 + 2.0 / 3.0 * 4 - --5");
        Parser parser = new Parser(lexer.ScanTokens());

        IExpr expr = parser.Expression();
        AstPrinterVisitor astVisitor = new();
        Assert.Equal("(- (+ 1 (* (/ 2 3) 4)) (- (- 5)))", expr.Accept(astVisitor));
    }

    [Fact]
    public void ParenthesizedArithmeticExpression()
    {
        Lexer lexer = new Lexer("(1 + 2.0) / ((3.0 * 4) - --5)");
        Parser parser = new Parser(lexer.ScanTokens());

        IExpr expr = parser.Expression();
        AstPrinterVisitor astVisitor = new();
        Assert.Equal("(/ (group (+ 1 2)) (group (- (group (* 3 4)) (- (- 5)))))", expr.Accept(astVisitor));
    }
}