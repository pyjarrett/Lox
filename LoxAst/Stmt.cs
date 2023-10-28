using LoxLexer;

namespace LoxAst;

public interface IStmtVisitor<TRetType> {
    TRetType VisitExpressionStmt(ExpressionStmt node);
    TRetType VisitPrintStmt(PrintStmt node);
    TRetType VisitVariableDeclarationStmt(VariableDeclarationStmt node);
    TRetType VisitBlockStmt(BlockStmt node);
    TRetType VisitIfStmt(IfStmt node);
    TRetType VisitWhileStmt(WhileStmt node);
}

public interface IStmt {
    TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor);
}

public readonly record struct ExpressionStmt (IExpr Expression) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitExpressionStmt(this);
    }
}
public readonly record struct PrintStmt (IExpr Expression) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitPrintStmt(this);
    }
}
public readonly record struct VariableDeclarationStmt (Token Name, IExpr? Initializer) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitVariableDeclarationStmt(this);
    }
}
public readonly record struct BlockStmt (List<IStmt?> Block) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitBlockStmt(this);
    }
}
public readonly record struct IfStmt (IExpr Condition, IStmt ThenBranch, IStmt? ElseBranch) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitIfStmt(this);
    }
}
public readonly record struct WhileStmt (IExpr Condition, IStmt Body) : IStmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitWhileStmt(this);
    }
}
