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
    
    [Fact]
    public void TestParseError()
    {
        VerifyParserThrows(typeof(ParseError), "(5.0");
    }
    
    private void VerifyParserThrows(Type exception, string text)
    {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer.ScanTokens());
        Assert.Throws(exception, () => parser.Parse());
    }

}