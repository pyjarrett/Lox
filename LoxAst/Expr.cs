using LoxLexer;

namespace LoxAst;

public interface IExprVisitor<TRetType> {
    TRetType VisitBinaryExpr(BinaryExpr node);
    TRetType VisitGroupingExpr(GroupingExpr node);
    TRetType VisitLiteralExpr(LiteralExpr node);
    TRetType VisitUnaryExpr(UnaryExpr node);
}

public interface Expr {
    TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor);
}

public readonly record struct BinaryExpr (Expr Left, Token Operator, Expr Right) : Expr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitBinaryExpr(this);
    }
}
public readonly record struct GroupingExpr (Expr Expression) : Expr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitGroupingExpr(this);
    }
}
public readonly record struct LiteralExpr (object Value) : Expr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitLiteralExpr(this);
    }
}
public readonly record struct UnaryExpr (Token Operator, Expr Right) : Expr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitUnaryExpr(this);
    }
}
