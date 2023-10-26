using LoxAst;
using LoxLexer;

namespace LoxInterpreter;

/// <summary>
/// Errors reporting when executing the AST.
/// </summary>
public class RuntimeError : Exception
{
    public Token Source { get; }

    public RuntimeError(Token token, string message)
        : base(message)
    {
        Source = token;
    }
}

public class Interpreter : IExprVisitor<object?>
{
    public void Interpret(Expr expr)
    {
        try
        {
            var value = expr.Accept(this);
            Console.WriteLine(value);
        }
        catch (RuntimeError runtimeError)
        {
            Console.Error.WriteLine($"Runtime Error at {runtimeError.Source}: {runtimeError.Message}");
        }
    }

    public object? Evaluate(Expr expr)
    {
        return expr.Accept(this);
    }

    public object? VisitBinaryExpr(BinaryExpr node)
    {
        var left = node.Left.Accept(this);
        var right = node.Right.Accept(this);

        switch (node.Operator.Kind)
        {
            case TokenKind.Plus:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left + (double)right;
            case TokenKind.Minus:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left - (double)right;
            case TokenKind.Star:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left * (double)right;
            case TokenKind.Slash:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left / (double)right;
            case TokenKind.LessThan:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left < (double)right;
            case TokenKind.LessThanOrEqual:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left <= (double)right;
            case TokenKind.GreaterThan:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left > (double)right;
            case TokenKind.GreaterThanOrEqual:
                CheckNumberOperands(node.Operator, left, right);
                return (double)left >= (double)right;
            case TokenKind.NotEqual:
                return !AreEqual(left, right);
            case TokenKind.EqualEqual:
                return AreEqual(left, right);
        }

        throw new NotImplementedException();
    }

    public object? VisitGroupingExpr(GroupingExpr node)
    {
        return node.Expression.Accept(this);
    }

    public object? VisitLiteralExpr(LiteralExpr node)
    {
        return node.Value;
    }

    public object? VisitUnaryExpr(UnaryExpr node)
    {
        var value = node.Right.Accept(this);
        switch (node.Operator.Kind)
        {
            case TokenKind.Plus:
            {
                if (value is double d)
                {
                    return d;
                }

                break;
            }

            case TokenKind.Minus:
            {
                if (value is double d)
                {
                    return -d;
                }
            }

                break;
            case TokenKind.Not:
                return !IsTruthy(value);
        }

        throw new RuntimeError(node.Operator, "Invalid unary expression.");
    }

    private bool IsTruthy(object? value)
    {
        if (value == null)
        {
            return false;
        }

        if (value is Boolean b)
        {
            return b;
        }

        return true;
    }

    private bool AreEqual(object? left, object? right)
    {
        if (left == null && right == null)
        {
            return true;
        }

        if (left == null)
        {
            return false;
        }

        return left.Equals(right);
    }

    private static void CheckNumberOperands(Token op, params object?[] operands)
    {
        foreach (object? obj in operands)
        {
            if (obj is not double)
            {
                throw new RuntimeError(op, "Operand must be a number.");
            }
        }
    }
}