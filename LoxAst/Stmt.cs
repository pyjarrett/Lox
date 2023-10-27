using LoxLexer;

namespace LoxAst;

public interface IStmtVisitor<TRetType> {
    TRetType VisitExpressionStmt(ExpressionStmt node);
    TRetType VisitPrintStmt(PrintStmt node);
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
