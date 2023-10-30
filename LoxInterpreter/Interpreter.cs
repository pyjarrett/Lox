﻿using System.Text;
using LoxAst;
using LoxLexer;

namespace LoxInterpreter;

/// There's no Void or Unit type to be used in generics for C#, so provide on.
public record Unit;

/// <summary>
/// Errors reporting when executing the AST.
/// </summary>
public class RuntimeError : Exception
{
    public Token Location { get; }

    public RuntimeError(Token token, string message)
        : base(message)
    {
        Location = token;
    }
}

public interface IInterpreterOutput
{
    void Output(string message);
    void Error(string message);
}

public class ConsoleOutput : IInterpreterOutput
{
    public void Output(string message)
    {
        Console.WriteLine(message);
    }

    public void Error(string message)
    {
        Console.Error.WriteLine(message);
    }
}

public class StringBufferOutput : IInterpreterOutput
{
    public void Output(string message)
    {
        outputLog.AppendLine(message);
    }

    public void Error(string message)
    {
        errorLog.AppendLine(message);
    }

    public string OutputLog => outputLog.ToString();
    public string ErrorLog => errorLog.ToString();

    private StringBuilder outputLog = new();
    private StringBuilder errorLog = new();
}

internal class ClockNativeCallable : LoxCallable
{
    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public int Arity()
    {
        return 0;
    }

    public override string ToString()
    {
        return "<native fn>";
    }
}

public class Interpreter : IExprVisitor<object?>, IStmtVisitor<Unit>
{
    public Interpreter(IInterpreterOutput? targetOutput = null)
    {
        output = targetOutput ?? new ConsoleOutput();

        globals.Define("clock", new ClockNativeCallable());

        environment = globals;
    }

    public void Interpret(List<IStmt?> stmts)
    {
        try
        {
            foreach (IStmt? stmt in stmts)
            {
                if (stmt != null)
                {
                    Execute(stmt);
                }
            }
        }
        catch (RuntimeError runtimeError)
        {
            output.Error(
                $"Runtime Error at {runtimeError.Location.LineNumber} '{runtimeError.Location.Lexeme}': {runtimeError.Message}");
            if (runtimeError.StackTrace != null)
            {
                output.Error(runtimeError.StackTrace);
            }
        }
    }

    public object? Evaluate(IExpr expr)
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

    public object? VisitLogicalExpr(LogicalExpr node)
    {
        object? left = Evaluate(node.Left);
        if (node.Operator.Kind == TokenKind.Or)
        {
            if (!IsTruthy(left))
            {
                return Evaluate(node.Right);
            }
        }
        else if (node.Operator.Kind == TokenKind.And)
        {
            // This looks weird to return the right value or an `and`...
            if (IsTruthy(left))
            {
                return Evaluate(node.Right);
            }
        }

        // Left of `OR`
        return left;
    }

    public object? VisitGroupingExpr(GroupingExpr node)
    {
        return node.Expression.Accept(this);
    }

    public object? VisitLiteralExpr(LiteralExpr node)
    {
        return node.Value;
    }

    public object? VisitVariableExpr(VariableExpr node)
    {
        var value = environment.Get(node.Name);
        if (value is UninitializedValue)
        {
            throw new RuntimeError(node.Name, "Accessing uninitialized value.");
        }

        return value;
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

    public object? VisitCallExpr(CallExpr node)
    {
        object? callee = Evaluate(node.Callee);

        // Evaluate all of the arguments for the node.
        List<object?> arguments = new();

        foreach (var arg in node.Arguments)
        {
            arguments.Add(Evaluate(arg));
        }

        if (callee is LoxCallable loxCallable)
        {
            if (arguments.Count != loxCallable.Arity())
            {
                throw new RuntimeError(node.Paren,
                    $"Expected {loxCallable.Arity()} arguments but found {arguments.Count} arguments.");
            }

            return loxCallable.Call(this, arguments);
        }

        throw new RuntimeError(node.Paren, "Can only call functions and classes.");
    }

    public object? VisitAssignmentExpr(AssignmentExpr node)
    {
        object? value = node.Value.Accept(this);
        environment.Assign(node.Name, value);
        return value;
    }

    public Unit VisitExpressionStmt(ExpressionStmt node)
    {
        Evaluate(node.Expression);
        return new();
    }

    public Unit VisitPrintStmt(PrintStmt node)
    {
        object? value = Evaluate(node.Expression);
        output.Output($"{value}");
        return new();
    }

    public Unit VisitVariableDeclarationStmt(VariableDeclarationStmt node)
    {
        object? value = new UninitializedValue();
        if (node.Initializer != null)
        {
            value = Evaluate(node.Initializer);
        }

        environment.Define(node.Name.Lexeme, value);
        return new();
    }

    public Unit VisitFunctionStmt(FunctionStmt node)
    {
        LoxFunction fn = new LoxFunction(node);
        environment.Define(node.Name.Lexeme, fn);
        return new();
    }

    public Unit VisitBlockStmt(BlockStmt node)
    {
        ExecuteBlock(node.Block, new Environment(environment));
        return new();
    }

    public void ExecuteBlock(List<IStmt?> stmts, Environment scope)
    {
        Environment previous = environment;

        try
        {
            environment = scope;

            foreach (IStmt? stmt in stmts)
            {
                if (stmt != null)
                {
                    Execute(stmt);
                }
            }
        }
        finally
        {
            // Pop the local scope as the block is exited.
            // Do this inside a `finally` block to always do this, even if
            // there was a runtime error.
            environment = previous;
        }
    }

    public Unit VisitIfStmt(IfStmt node)
    {
        if (IsTruthy(Evaluate(node.Condition)))
        {
            Execute(node.ThenBranch);
        }
        else if (node.ElseBranch != null)
        {
            Execute(node.ElseBranch);
        }

        return new();
    }

    public Unit VisitWhileStmt(WhileStmt node)
    {
        while (IsTruthy(Evaluate(node.Condition)))
        {
            Execute(node.Body);
        }

        return new();
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

    private void Execute(IStmt stmt)
    {
        stmt.Accept(this);
    }

    public readonly Environment globals = new();

    private Environment environment;
    private IInterpreterOutput output;
}