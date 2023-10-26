using LoxLexer;

namespace LoxAst;

public interface IStmtVisitor<TRetType> {
    TRetType VisitExpressionStmt(ExpressionStmt node);
    TRetType VisitPrintStmt(PrintStmt node);
}

public interface Stmt {
    TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor);
}

public readonly record struct ExpressionStmt (Expr Expression) : Stmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitExpressionStmt(this);
    }
}
public readonly record struct PrintStmt (Expr Expression) : Stmt {
    public TRetType Accept<TRetType>(IStmtVisitor<TRetType> visitor) {
        return visitor.VisitPrintStmt(this);
    }
}
