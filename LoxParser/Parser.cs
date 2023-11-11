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
    public List<IStmt?> Parse()
    {
        List<IStmt?> stmts = new();

        while (!IsAtEnd())
        {
            stmts.Add(Declaration());
        }

        return stmts;
    }

    public IStmt? Declaration()
    {
        try
        {
            if (Match(TokenKind.Var))
            {
                return VariableDeclarationStatement();
            }

            if (Match(TokenKind.Class))
            {
                return ClassDeclarationStatement();
            }
            
            if (Match(TokenKind.Fun))
            {
                return FunctionDeclarationStatement("function");
            }

            return Statement();
        }
        catch (ParseError parseError)
        {
            Console.Error.WriteLine($"Parse error: {parseError}");

            // An error occurred and the Parser has panicked, so skip to a
            // known good position in the token stream.
            Synchronize();
            return null;
        }
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
            return PrintStatement();
        }

        if (Match(TokenKind.LeftBrace))
        {
            return new BlockStmt(Block());
        }

        if (Match(TokenKind.If))
        {
            return IfStatement();
        }

        if (Match(TokenKind.While))
        {
            return WhileStatement();
        }

        if (Match(TokenKind.For))
        {
            return ForStatement();
        }

        if (Match(TokenKind.Return))
        {
            return ReturnStatement();
        }

        return ExpressionStatement();
    }

    private IStmt? IfStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' before condition.");
        IExpr? condition = Expression();
        Consume(TokenKind.RightParen, "Expected ')' after condition.");

        IStmt? thenBranch = Statement();
        IStmt? elseBranch = null;
        if (Match(TokenKind.Else))
        {
            elseBranch = Statement();
        }

        return new IfStmt(condition, thenBranch!, elseBranch);
    }

    private IStmt? WhileStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' before condition.");
        IExpr expression = Expression();
        Consume(TokenKind.RightParen, "Expected ')' after condition.");
        IStmt statement = Statement()!;
        return new WhileStmt(expression, statement);
    }

    /// <summary>
    /// For statement syntactic sugar conversion into `while` loop.
    /// </summary>
    ///
    /// `for (initializer; condition; step) { ... }`
    /// 
    private IStmt? ForStatement()
    {
        Consume(TokenKind.LeftParen, "Expected '(' before initializer.");

        IStmt? initializer = null;
        IExpr? condition = null;
        IExpr? update = null;

        // Initializer
        if (Match(TokenKind.Semicolon))
        {
            initializer = null;
        }
        else if (Match(TokenKind.Var))
        {
            initializer = VariableDeclarationStatement();
        }
        else
        {
            initializer = ExpressionStatement();
        }

        // Condition
        if (!Check(TokenKind.Semicolon))
        {
            condition = Expression();
        }
        else
        {
            // No condition, so assume an infinite loop.
            condition = new LiteralExpr(true);
        }

        Consume(TokenKind.Semicolon, "Expected ';' after condition.");

        // Update (step)
        if (!Check(TokenKind.RightParen))
        {
            update = Expression();
        }

        Consume(TokenKind.RightParen, "Expected ')' after loop parameters.");
        IStmt? body = Statement();

        // Convert the parts of the `for` loop into a `while` loop.

        // initializer
        // loop
        //      body
        //      update

        // Wrap together the body and increment for the `while` loop.
        if (update != null)
        {
            body = new BlockStmt(new List<IStmt?>() { body, new ExpressionStmt(update) });
        }

        // Wrap the initializer with the sugared loop body.
        body = new BlockStmt(new List<IStmt?>() { initializer, new WhileStmt(condition, body!) });

        return body;
    }

    private IStmt? ReturnStatement()
    {
        Token keyword = Previous();
        IExpr value = null;
        if (!Check(TokenKind.Semicolon))
        {
            value = Expression();
        }
        Consume(TokenKind.Semicolon, "Expected ';' after return expression.");
        return new ReturnStmt(keyword, value);
    }

    /// <summary>
    /// Produce a list of statements in a block.  Doesn't produce a block type
    /// directly so this can be reused for function bodies and class definitions.
    /// </summary>
    private List<IStmt?> Block()
    {
        List<IStmt?> statements = new();
        while (!IsAtEnd() && !Check(TokenKind.RightBrace))
        {
            statements.Add(Declaration());
        }

        Consume(TokenKind.RightBrace, "Expected '}' after a block.");
        return statements;
    }

    private IStmt ExpressionStatement()
    {
        IExpr expr = Expression();
        Consume(TokenKind.Semicolon, "Expected ';' after expression statement.");
        return new ExpressionStmt(expr);
    }

    private IStmt VariableDeclarationStatement()
    {
        Consume(TokenKind.Identifier, "Expected an identifier.");
        Token identifier = Previous();
        IExpr? initializer = null;

        // A variable with an initializer.
        if (Match(TokenKind.Equal))
        {
            initializer = Expression();
        }

        Consume(TokenKind.Semicolon, "Expected a ';' after a variable declaration.");

        return new VariableDeclarationStmt(identifier, initializer);
    }

    // class_declaration := class IDENTIFIER '{' function* '}'
    private IStmt ClassDeclarationStatement()
    {
        Token className = Consume(TokenKind.Identifier, "Expected a class name.");

        Consume(TokenKind.LeftBrace, "Expected '{' after class name.");

        List<FunctionStmt> methods = new();
        
        // Keep parsing functions as long as they are more.
        while (!Check(TokenKind.RightBrace) && !IsAtEnd())
        {
            methods.Add(FunctionDeclarationStatement("method"));
        }
        
        Consume(TokenKind.RightBrace, "Expected '}' after class.");

        return new ClassStmt(className, methods);
    }

    /// <summary>
    /// Parses a function declaration, reporting errors using the given 'kind'
    /// of expected function.
    /// </summary>
    private FunctionStmt FunctionDeclarationStatement(string kind)
    {
        Token name = Consume(TokenKind.Identifier, $"Expected {kind} name.");

        Consume(TokenKind.LeftParen, "Expected '(' before parameters.");

        // Get the list of parameters.  This is similar to how arguments are parsed.

        List<Token> parameters = new();
        if (!Check(TokenKind.RightParen))
        {
            parameters.Add(Consume(TokenKind.Identifier, "Expected a parameter name."));
            while (Match(TokenKind.Comma))
            {
                parameters.Add(Consume(TokenKind.Identifier, "Expected a parameter name."));
            }
        }

        Consume(TokenKind.RightParen, "Expected ')' after function parameters.");
        Consume(TokenKind.LeftBrace, "Expected '{' after function parameters.");

        // Parse the body
        List<IStmt?> body = Block();

        return new FunctionStmt(name, parameters, body);
    }

    private IStmt PrintStatement()
    {
        IExpr expr = Expression();
        Consume(TokenKind.Semicolon, "Expected ';' after print statement.");
        return new PrintStmt(expr);
    }

    public IExpr Expression()
    {
        return Assignment();
    }

    /// ```
    /// assignment := ( call "." )? IDENTIFIER "=" assignment
    ///     | logic_or;
    /// ```
    public IExpr Assignment()
    {
        // The left side of an assignment, could be a chained expression,
        // like a.b.c.d.
        IExpr expr = Or();

        if (Match(TokenKind.Equal))
        {
            Token equals = Previous();
            IExpr rightHandSide = Assignment();
            if (expr is VariableExpr varExpr)
            {
                return new AssignmentExpr(varExpr.Name, rightHandSide);
            }

            if (expr is GetExpr getExpr)
            {
                return new SetExpr(getExpr.Object, getExpr.Name, rightHandSide);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    /// ```
    /// logic_or := logic_and ("or" logic_and)* ;
    /// logic_and := equality ("and" equality)* ;
    /// ```
    public IExpr Or()
    {
        IExpr left = And();
        while (Match(TokenKind.Or))
        {
            Token op = Previous();
            IExpr right = And();
            left = new LogicalExpr(left, op, right);
        }

        return left;
    }

    public IExpr And()
    {
        IExpr left = Equality();
        if (Match(TokenKind.And))
        {
            Token op = Previous();
            IExpr right = Equality();
            left = new LogicalExpr(left, op, right);
        }

        return left;
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
    /// Of the form `(! | + | -)unary | call`
    public IExpr Unary()
    {
        if (Match(TokenKind.Plus, TokenKind.Minus, TokenKind.Not))
        {
            Token op = Previous();
            IExpr expr = Unary();
            return new LoxAst.UnaryExpr(op, expr);
        }

        return Call();
    }

    // call := primary ( "(" arguments? ")" | "." IDENTIFIER)*
    public IExpr Call()
    {
        IExpr expr = Primary();

        while (true)
        {
            if (Match(TokenKind.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else if (Match(TokenKind.Dot))
            {
                Token name = Consume(TokenKind.Identifier, "Expected property after '.'");
                expr = new GetExpr(expr, name);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    public IExpr FinishCall(IExpr callee)
    {
        List<IExpr> arguments = new();

        // Parse any parameters.
        if (!Check(TokenKind.RightParen))
        {
            arguments.Add(Expression());

            // There are commas after each additional argument.
            while (Match(TokenKind.Comma))
            {
                // The bytecode interpreter is simplified by limiting the
                // number of arguments, so also limit the tree-walk interpreter.
                if (arguments.Count >= MaxArgumentsInFunctionCall)
                {
                    Error(Peek(), "Exceeded maximum number of arguments to function.");
                }

                arguments.Add(Expression());
            }
        }

        // Parenthesis after arguments, used for error reporting.
        Token paren = Consume(TokenKind.RightParen, "Expected ')' after function call.");

        return new CallExpr(callee, paren, arguments);
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
            return new LoxAst.LiteralExpr(null);
        }

        if (Match(TokenKind.String, TokenKind.Number))
        {
            return new LoxAst.LiteralExpr(Previous().Literal!);
        }

        if (Match(TokenKind.This))
        {
            return new LoxAst.ThisExpr(Previous());
        }
        
        if (Match(TokenKind.Identifier))
        {
            return new LoxAst.VariableExpr(Previous());
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

    /// <summary>
    /// Called when the parser runs into a problem where there's a syntax error,
    /// and it needs to find a place in the token stream where it can restart
    /// parsing.
    /// </summary>
    ///
    /// This allows the parser to emit multiple errors, rather than having to
    /// bail out of parsing after the first error.
    private void Synchronize()
    {
        // Skip the most recently encountered bad thing.
        Advance();

        while (!IsAtEnd())
        {
            // Reached the end of a statement, so start the next statement.
            if (Previous().Kind == TokenKind.Semicolon)
            {
                return;
            }

            // If the next token is something that is recognizable as a
            // statement or a declaration, then try to start parsing there.
            switch (Peek().Kind)
            {
                case TokenKind.Class: return;
                case TokenKind.Fun: return;
                case TokenKind.Print: return;
                case TokenKind.Return: return;
                case TokenKind.Var: return;
                case TokenKind.For: return;
                case TokenKind.If: return;
                case TokenKind.While: return;
            }

            // Move along and try again.
            Advance();
        }
    }

    private int current = 0;
    private const int MaxArgumentsInFunctionCall = 255;
}