using LoxAst;
using LoxLexer;
using LoxParser;
using Xunit.Abstractions;

namespace LoxParserTests;

public class AstVisitor : IExprVisitor<string>
{
    public string VisitBinaryExpr(BinaryExpr node)
    {
        return $"({node.Operator.Lexeme} {node.Left.Accept(this)} {node.Right.Accept(this)})";
    }

    public string VisitGroupingExpr(GroupingExpr node)
    {
        return $"(group {node.Expression.Accept(this)})";
    }

    public string VisitLiteralExpr(LiteralExpr node)
    {
        return $"{node.Value}";
    }

    public string VisitVariableExpr(VariableExpr node)
    {
        return $"VARIABLE: {node.Name.Lexeme}";
    }

    public string VisitUnaryExpr(UnaryExpr node)
    {
        return $"({node.Operator.Lexeme} {node.Right.Accept(this)})";
    }

    public string VisitAssignmentExpr(AssignmentExpr node)
    {
        return $"{node.Name} = {node.Value.Accept(this)}";
    }
}

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
        AstVisitor astVisitor = new AstVisitor();
        Assert.Equal("(- (+ 1 (* (/ 2 3) 4)) (- (- 5)))", expr.Accept(astVisitor));
    }

    [Fact]
    public void ParenthesizedArithmeticExpression()
    {
        Lexer lexer = new Lexer("(1 + 2.0) / ((3.0 * 4) - --5)");
        Parser parser = new Parser(lexer.ScanTokens());

        IExpr expr = parser.Expression();
        AstVisitor astVisitor = new AstVisitor();
        Assert.Equal("(/ (group (+ 1 2)) (group (- (group (* 3 4)) (- (- 5)))))", expr.Accept(astVisitor));
    }
}