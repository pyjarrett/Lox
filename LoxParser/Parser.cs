using System.Data;
using System.Diagnostics;
using LoxLexer;
using LoxAst;

namespace LoxParser;

/// <summary>
/// The parser uses exceptions to bail out of the call stack since it's a
/// recursive descent parser and that's where parse state is stored.
/// </summary>
public class ParseError : Exception
{
    public ParseError()
    {
    }

    public ParseError(string message)
        : base(message)
    {
    }

    public ParseError(string message, Exception inner)
        : base(message, inner)
    {
    }
}

/// <summary>
/// Parser for Lox.
/// </summary>
public class Parser
{
    public Parser(List<Token> tokens)
    {
        Tokens = tokens;
    }

    public List<Token> Tokens { get; init; }

    /// <summary>
    /// Parses the tokens into statements.
    /// </summary>
    public List<IStmt> Parse()
    {
        List<IStmt> stmts = new();

        while (!IsAtEnd())
        {
            stmts.Add(Statement());
        }

        return stmts;
    }

    /// <summary>
    /// Parses the next statement.
    /// </summary>
    ///
    /// Statement types:
    /// * `print $expression;`
    /// * `$expression`;
    public IStmt? Statement()
    {
        if (Match(TokenKind.Print))
        {
            IExpr expr = Expression();
            Consume(TokenKind.Semicolon, "Expected ';' after print statement.");
            return new PrintStmt(expr);
        }
        else
        {
            IExpr expr = Expression();
            Consume(TokenKind.Semicolon, "Expected ';' after expression statement.");
            return new ExpressionStmt(expr);
        }
    }

    public IExpr Expression()
    {
        return Equality();
    }

    public IExpr Equality()
    {
        var expr = Comparison();
        while (Match(TokenKind.EqualEqual, TokenKind.NotEqual))
        {
            var op = Previous();
            var right = Comparison();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// A left-associative binding.
    /// `comparison := term (( ">" | ">=" | "<=" | "<" )term)*`
    public IExpr Comparison()
    {
        var expr = Term();
        while (Match(TokenKind.GreaterThan, TokenKind.GreaterThanOrEqual, TokenKind.LessThan,
                   TokenKind.LessThanOrEqual))
        {
            var op = Previous();
            var right = Term();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// 
    /// </summary>
    /// `term := factor (("+" | "-") factor)*
    public IExpr Term()
    {
        var expr = Factor();
        while (Match(TokenKind.Plus, TokenKind.Minus))
        {
            var op = Previous();
            var right = Factor();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Multiplication is below addition in precedence and has left
    /// associativity.
    /// </summary>
    public IExpr Factor()
    {
        // Descend into next lower rule to prevent infinite recursion.
        IExpr expr = Unary();
        while (Match(TokenKind.Star, TokenKind.Slash))
        {
            var op = Previous();
            var right = Unary();
            expr = new BinaryExpr(expr, op, right);
        }

        return expr;
    }

    /// <summary>
    /// Unary expression parse.
    /// </summary>
    ///
    /// Of the form `(! | + | -)unary | primary`
    public IExpr Unary()
    {
        if (Match(TokenKind.Plus, TokenKind.Minus, TokenKind.Not))
        {
            Token op = Previous();
            IExpr expr = Unary();
            return new LoxAst.UnaryExpr(op, expr);
        }

        return Primary();
    }

    public IExpr Primary()
    {
        if (Match(TokenKind.False))
        {
            return new LoxAst.LiteralExpr(false);
        }

        if (Match(TokenKind.True))
        {
            return new LoxAst.LiteralExpr(true);
        }

        if (Match(TokenKind.Nil))
        {
            return new LoxAst.LiteralExpr();
        }

        if (Match(TokenKind.String, TokenKind.Number))
        {
            return new LoxAst.LiteralExpr(Previous().Literal!);
        }

        // Parenthesized expression
        if (Match(TokenKind.LeftParen))
        {
            IExpr expr = Expression();
            Consume(TokenKind.RightParen, "Expected ')' after expression");
            return new LoxAst.GroupingExpr(expr);
        }

        throw Error(Peek(), "Expected an expression");
    }

    private ParseError Error(Token token, string message)
    {
        if (token.Kind == TokenKind.EndOfFile)
        {
            Console.Error.WriteLine($"Error at end of file: {message}");
        }
        else
        {
            Console.Error.WriteLine($"Error at token '{token.Lexeme}': {message}");
        }

        return new ParseError(message);
    }

    /// <summary>
    /// Looks at the current token, but doesn't move onto the next one.
    /// </summary>
    private Token Peek()
    {
        return Tokens[current];
    }

    /// <summary>
    /// See if the current token matches a given kind, but don't move to the
    /// next token.
    /// </summary>
    private bool Check(TokenKind kind)
    {
        if (IsAtEnd())
        {
            return false;
        }

        return Peek().Kind == kind;
    }

    /// <summary>
    /// It's easier to match a token and then check it later on, than to store it.
    /// </summary>
    private Token Previous()
    {
        Debug.Assert(current > 0);
        return Tokens[current - 1];
    }

    /// <summary>
    /// Have all tokens been processed?
    /// </summary>
    private bool IsAtEnd()
    {
        return Peek().Kind == TokenKind.EndOfFile;
    }

    /// <summary>
    /// Move to the next token if possible.
    /// </summary>
    private Token Advance()
    {
        if (!IsAtEnd())
        {
            ++current;
        }

        return Previous();
    }

    /// <summary>
    /// See if the current token kind matches any of the given list. 
    /// </summary>
    private bool Match(params TokenKind[] kinds)
    {
        foreach (var kind in kinds)
        {
            if (Check(kind))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private Token Consume(TokenKind kind, String message)
    {
        if (Check(kind))
        {
            return Advance();
        }

        throw Error(Peek(), message);
    }

    private int current = 0;
}