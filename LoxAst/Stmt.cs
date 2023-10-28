using LoxLexer;

namespace LoxAst;

public interface IStmtVisitor<TRetType> {
    TRetType VisitExpressionStmt(ExpressionStmt node);
    TRetType VisitPrintStmt(PrintStmt node);
    TRetType VisitVariableDeclarationStmt(VariableDeclarationStmt node);
    TRetType VisitBlockStmt(BlockStmt node);
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
