using LoxLexer;

namespace LoxAst;

public interface IStmtVisitor<TRetType>
{
    TRetType VisitExpressionStmt(ExpressionStmt node);
    TRetType VisitPrintStmt(PrintStmt node);
    TRetType VisitVariableDeclarationStmt(VariableDeclarationStmt node);
    TRetType VisitBlockStmt(BlockStmt node);
    TRetType VisitIfStmt(IfStmt node);
    TRetType VisitWhileStmt(WhileStmt node);
    TRetType VisitFunctionStmt(FunctionStmt node);
    TRetType VisitReturnStmt(ReturnStmt node);
}

public interface IStmt
{
    TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor);
}

public class ExpressionStmt : IStmt
{
    public ExpressionStmt(IExpr Expression)
    {
        this.Expression = Expression;
    }

    public IExpr Expression { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}

public class PrintStmt : IStmt
{
    public PrintStmt(IExpr Expression)
    {
        this.Expression = Expression;
    }

    public IExpr Expression { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitPrintStmt(this);
    }
}

public class VariableDeclarationStmt : IStmt
{
    public VariableDeclarationStmt(Token Name, IExpr? Initializer)
    {
        this.Name = Name;
        this.Initializer = Initializer;
    }

    public Token Name { get; set; }
    public IExpr? Initializer { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitVariableDeclarationStmt(this);
    }
}

public class BlockStmt : IStmt
{
    public BlockStmt(List<IStmt?> Block)
    {
        this.Block = Block;
    }

    public List<IStmt?> Block { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitBlockStmt(this);
    }
}

public class IfStmt : IStmt
{
    public IfStmt(IExpr Condition, IStmt ThenBranch, IStmt? ElseBranch)
    {
        this.Condition = Condition;
        this.ThenBranch = ThenBranch;
        this.ElseBranch = ElseBranch;
    }

    public IExpr Condition { get; set; }
    public IStmt ThenBranch { get; set; }
    public IStmt? ElseBranch { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitIfStmt(this);
    }
}

public class WhileStmt : IStmt
{
    public WhileStmt(IExpr Condition, IStmt Body)
    {
        this.Condition = Condition;
        this.Body = Body;
    }

    public IExpr Condition { get; set; }
    public IStmt Body { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitWhileStmt(this);
    }
}

public class FunctionStmt : IStmt
{
    public FunctionStmt(Token Name, List<Token> Params, List<IStmt?> Body)
    {
        this.Name = Name;
        this.Params = Params;
        this.Body = Body;
    }

    public Token Name { get; set; }
    public List<Token> Params { get; set; }
    public List<IStmt?> Body { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitFunctionStmt(this);
    }
}

public class ReturnStmt : IStmt
{
    public ReturnStmt(Token Keyword, IExpr? Value)
    {
        this.Keyword = Keyword;
        this.Value = Value;
    }

    public Token Keyword { get; set; }
    public IExpr? Value { get; set; }

    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor)
    {
        return visitor.VisitReturnStmt(this);
    }
}