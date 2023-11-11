using System.Diagnostics;
using System.Runtime.InteropServices.JavaScript;
using LoxAst;
using LoxLexer;

namespace LoxInterpreter;

/// <summary>
/// Counts the number of hops from the current scope (at zero) to the global
/// environment.
/// </summary>
public class Resolver : IExprVisitor<Unit>, IStmtVisitor<Unit>
{
    public Resolver(Interpreter interpreter)
    {
        _interpreter = interpreter;
    }

    public Unit VisitBinaryExpr(BinaryExpr node)
    {
        Resolve(node.Left);
        Resolve(node.Right);
        return new();
    }

    public Unit VisitGroupingExpr(GroupingExpr node)
    {
        Resolve(node.Expression);
        return new();
    }

    public Unit VisitLiteralExpr(LiteralExpr node)
    {
        return new();
    }

    public Unit VisitThisExpr(ThisExpr node)
    {
        if (classContext == ClassType.None)
        {
            _interpreter.Error(node.Keyword, "Cannot use 'this' outside of a class.");
        }
        ResolveLocal(node, node.Keyword);
        return new Unit();
    }

    /// <summary>
    /// Evaluates the value of a variable.
    /// </summary>
    public Unit VisitVariableExpr(VariableExpr node)
    {
        // Trying to use a variable before it has been defined.
        // C# difference from Java here, Java's Map returns null if the key doesn't exist.
        if (scopes.Count > 0 && CurrentScope().ContainsKey(node.Name.Lexeme) &&
            CurrentScope()[node.Name.Lexeme] == false)
        {
            _interpreter.Error(node.Name, "Can't usage a variable into its own initializer.");
        }

        ResolveLocal(node, node.Name);
        return new();
    }

    public Unit VisitUnaryExpr(UnaryExpr node)
    {
        Resolve(node.Right);
        return new();
    }

    public Unit VisitCallExpr(CallExpr node)
    {
        Resolve(node.Callee);
        foreach (var arg in node.Arguments)
        {
            Resolve(arg);
        }

        return new();
    }

    public Unit VisitGetExpr(GetExpr node)
    {
        Resolve(node.Object);
        return new();
    }

    public Unit VisitSetExpr(SetExpr node)
    {
        Resolve(node.Object);
        Resolve(node.Value);
        return new();
    }

    public Unit VisitAssignmentExpr(AssignmentExpr node)
    {
        Resolve(node.Value);
        ResolveLocal(node, node.Name);
        return new();
    }

    public Unit VisitLogicalExpr(LogicalExpr node)
    {
        Resolve(node.Left);
        Resolve(node.Right);
        return new();
    }

    public Unit VisitExpressionStmt(ExpressionStmt node)
    {
        Resolve(node.Expression);
        return new();
    }

    public Unit VisitPrintStmt(PrintStmt node)
    {
        Resolve(node.Expression);
        return new Unit();
    }

    public Unit VisitVariableDeclarationStmt(VariableDeclarationStmt node)
    {
        Declare(node.Name);
        if (node.Initializer != null)
        {
            Resolve(node.Initializer!);
        }

        Define(node.Name);
        return new();
    }

    public Unit VisitBlockStmt(BlockStmt node)
    {
        BeginScope();
        Resolve(node.Block);
        EndScope();
        return new();
    }

    public Unit VisitIfStmt(IfStmt node)
    {
        Resolve(node.Condition);
        Resolve(node.ThenBranch);

        if (node.ElseBranch != null)
        {
            Resolve(node.ElseBranch);
        }

        return new();
    }

    public Unit VisitWhileStmt(WhileStmt node)
    {
        Resolve(node.Condition);
        Resolve(node.Body);
        return new();
    }

    public Unit VisitClassStmt(ClassStmt node)
    {
        ClassType enclosingClass = classContext;
        classContext = ClassType.Class;
        
        Declare(node.Name);
        Define(node.Name);

        // Make a new scope for `this` and declare that it exists.
        BeginScope();
        CurrentScope()["this"] = true;
        
        foreach (var method in node.Methods)
        {
            var functionType = method.Name.Lexeme.Equals("init") ? FunctionType.Initializer : FunctionType.Method;
            ResolveFunction(method, functionType);
        }

        EndScope();

        classContext = ClassType.None;
        return new();
    }

    public Unit VisitFunctionStmt(FunctionStmt node)
    {
        Declare(node.Name);
        Define(node.Name);
        ResolveFunction(node, FunctionType.Function);
        return new();
    }

    public Unit VisitReturnStmt(ReturnStmt node)
    {
        // Can't have a `return` at the top level, outside of all functions.
        if (functionContext == FunctionType.None)
        {
            _interpreter.Error(node.Keyword, "Return must be inside a function.");
        }
        
        if (node.Value != null)
        {
            if (functionContext == FunctionType.Initializer)
            {
                _interpreter.Error(node.Keyword, "Cannot return from an initializer.");
            }
            Resolve(node.Value);
        }

        return new();
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0) return;

        if (CurrentScope().ContainsKey(name.Lexeme))
        {
            _interpreter.Error(name, "Already a variable with this name in this scope.");
        }
        CurrentScope()[name.Lexeme] = false;
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0) return;

        // Variable should already have been declared.
        Debug.Assert(CurrentScope().ContainsKey(name.Lexeme));
        Debug.Assert(CurrentScope()[name.Lexeme] == false);

        // Define this variable.
        CurrentScope()[name.Lexeme] = true;
    }

    public void Resolve(List<IStmt?> stmts)
    {
        foreach (var stmt in stmts)
        {
            Resolve(stmt);
        }
    }

    private void ResolveFunction(FunctionStmt node, FunctionType functionType)
    {
        // Preserve previous function type due to possibly nested functions.
        FunctionType enclosingFunction = functionContext;
        functionContext = functionType;
        
        BeginScope();
        foreach (var param in node.Params)
        {
            Declare(param);
            Define(param);
        }

        Resolve(node.Body);
        EndScope();
        
        // Restore previous function type.
        functionContext = enclosingFunction;
    }

    private void Resolve(IStmt? node)
    {
        if (node == null)
        {
            return;
        }
        node.Accept(this);
    }

    private void Resolve(IExpr node)
    {
        node.Accept(this);
    }

    private void ResolveLocal(IExpr expr, Token name)
    {
        // Starting the innermost scope, start pushing up the environments,
        // looking for a match to the given variable.
        for (var i = scopes.Count - 1; i >= 0; --i)
        {
            if (scopes[i].ContainsKey(name.Lexeme))
            {
                _interpreter.Resolve(expr, scopes.Count - i - 1);
                return;
            }
        }
    }

    private void BeginScope()
    {
        scopes.Add(new Dictionary<string, bool>());
    }

    private void EndScope()
    {
        scopes.RemoveAt(scopes.Count - 1);
    }

    private Dictionary<string, Boolean> CurrentScope()
    {
        Debug.Assert(scopes.Count > 0);
        return scopes[scopes.Count - 1];
    }

    private Interpreter _interpreter;

    /// A stack of scopes from the top global scope to this scope.
    ///
    /// Count the number of scopes, by describing whether or not a variable is
    /// defined.
    ///
    /// Key
    /// ----------------------------
    /// Missing = undefined variable
    /// False   = declared
    /// True    = defined
    private List<Dictionary<string, Boolean>> scopes = new();

    private enum FunctionType
    {
        None,
        Function,
        Method,
        Initializer,
    }

    private enum ClassType
    {
        None,
        Class,
    }

    private FunctionType functionContext = FunctionType.None;
    private ClassType classContext = ClassType.None;
}