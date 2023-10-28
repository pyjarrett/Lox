using LoxLexer;

namespace LoxAst;

public interface IExprVisitor<TRetType> {
    TRetType VisitBinaryExpr(BinaryExpr node);
    TRetType VisitGroupingExpr(GroupingExpr node);
    TRetType VisitLiteralExpr(LiteralExpr node);
    TRetType VisitVariableExpr(VariableExpr node);
    TRetType VisitUnaryExpr(UnaryExpr node);
    TRetType VisitAssignmentExpr(AssignmentExpr node);
    TRetType VisitLogicalExpr(LogicalExpr node);
}

public interface IExpr {
    TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor);
}

public readonly record struct BinaryExpr (IExpr Left, Token Operator, IExpr Right) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitBinaryExpr(this);
    }
}
public readonly record struct GroupingExpr (IExpr Expression) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitGroupingExpr(this);
    }
}
public readonly record struct LiteralExpr (object Value) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitLiteralExpr(this);
    }
}
public readonly record struct VariableExpr (Token Name) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitVariableExpr(this);
    }
}
public readonly record struct UnaryExpr (Token Operator, IExpr Right) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitUnaryExpr(this);
    }
}
public readonly record struct AssignmentExpr (Token Name, IExpr Value) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitAssignmentExpr(this);
    }
}
public readonly record struct LogicalExpr (IExpr Left, Token Operator, IExpr Right) : IExpr {
    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitLogicalExpr(this);
    }
}
