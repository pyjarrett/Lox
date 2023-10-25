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
        var value = expr.Accept(this);
        Console.WriteLine(value);
    }
    
    public object? VisitBinaryExpr(BinaryExpr node)
    {
        var left = node.Left.Accept(this);
        var right = node.Right.Accept(this);

        if (left == null || right == null)
        {
            throw new Exception("Binary operation with nullable operand.");
        }

        switch (node.Operator.Kind)
        {
            case TokenKind.Plus:
                checkNumberOperands(node.Operator, left, right);
                return (double)left + (double)right;
            case TokenKind.Minus:
                checkNumberOperands(node.Operator, left, right);
                return (double)left - (double)right;
            case TokenKind.Star:
                checkNumberOperands(node.Operator, left, right);
                return (double)left * (double)right;
            case TokenKind.Slash:
                checkNumberOperands(node.Operator, left, right);
                return (double)left / (double)right;
            case TokenKind.LessThan:
                checkNumberOperands(node.Operator, left, right);
                return (double)left < (double)right;
            case TokenKind.LessThanOrEqual:
                checkNumberOperands(node.Operator, left, right);
                return (double)left <= (double)right;
            case TokenKind.GreaterThan:
                checkNumberOperands(node.Operator, left, right);
                return (double)left > (double)right;
            case TokenKind.GreaterThanOrEqual:
                checkNumberOperands(node.Operator, left, right);
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
        return node.Accept(this);
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
                if (value != null && value is double d)
                {
                    return d;
                }

                throw new NotImplementedException();
            }

            case TokenKind.Minus:
            {
                if (value != null && value is double d)
                {
                    return -d;
                }

                throw new NotImplementedException();
            }
            case TokenKind.Not:
                return !IsTruthy(value);
        }

        throw new NotImplementedException();
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

    private void checkNumberOperands(Token op, params object?[] operands)
    {
        foreach (object? obj in operands)
        {
            if (!(obj is Double))
            {
                throw new RuntimeError(op, "Operand must be a number.");
            }
        }
    }
}