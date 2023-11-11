using LoxLexer;

namespace LoxAst;

public interface IExprVisitor<TRetType> {
    TRetType VisitBinaryExpr(BinaryExpr node);
    TRetType VisitGroupingExpr(GroupingExpr node);
    TRetType VisitLiteralExpr(LiteralExpr node);
    TRetType VisitVariableExpr(VariableExpr node);
    TRetType VisitUnaryExpr(UnaryExpr node);
    TRetType VisitCallExpr(CallExpr node);
    TRetType VisitGetExpr(GetExpr node);
    TRetType VisitSetExpr(SetExpr node);
    TRetType VisitThisExpr(ThisExpr node);
    TRetType VisitAssignmentExpr(AssignmentExpr node);
    TRetType VisitLogicalExpr(LogicalExpr node);
}

public interface IExpr {
    TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor);
}

public class BinaryExpr : IExpr {

    public BinaryExpr(IExpr Left, Token Operator, IExpr Right)
    {
        this.Left = Left;
        this.Operator = Operator;
        this.Right = Right;
    }

    public IExpr Left { get; set; }
    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitBinaryExpr(this);
    }
}

public class GroupingExpr : IExpr {

    public GroupingExpr(IExpr Expression)
    {
        this.Expression = Expression;
    }

    public IExpr Expression { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitGroupingExpr(this);
    }
}

public class LiteralExpr : IExpr {

    public LiteralExpr(object Value)
    {
        this.Value = Value;
    }

    public object Value { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitLiteralExpr(this);
    }
}

public class VariableExpr : IExpr {

    public VariableExpr(Token Name)
    {
        this.Name = Name;
    }

    public Token Name { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitVariableExpr(this);
    }
}

public class UnaryExpr : IExpr {

    public UnaryExpr(Token Operator, IExpr Right)
    {
        this.Operator = Operator;
        this.Right = Right;
    }

    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitUnaryExpr(this);
    }
}

public class CallExpr : IExpr {

    public CallExpr(IExpr Callee, Token Paren, List<IExpr> Arguments)
    {
        this.Callee = Callee;
        this.Paren = Paren;
        this.Arguments = Arguments;
    }

    public IExpr Callee { get; set; }
    public Token Paren { get; set; }
    public List<IExpr> Arguments { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitCallExpr(this);
    }
}

public class GetExpr : IExpr {

    public GetExpr(IExpr Object, Token Name)
    {
        this.Object = Object;
        this.Name = Name;
    }

    public IExpr Object { get; set; }
    public Token Name { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitGetExpr(this);
    }
}

public class SetExpr : IExpr {

    public SetExpr(IExpr Object, Token Name, IExpr Value)
    {
        this.Object = Object;
        this.Name = Name;
        this.Value = Value;
    }

    public IExpr Object { get; set; }
    public Token Name { get; set; }
    public IExpr Value { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitSetExpr(this);
    }
}

public class ThisExpr : IExpr {

    public ThisExpr(Token Keyword)
    {
        this.Keyword = Keyword;
    }

    public Token Keyword { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitThisExpr(this);
    }
}

public class AssignmentExpr : IExpr {

    public AssignmentExpr(Token Name, IExpr Value)
    {
        this.Name = Name;
        this.Value = Value;
    }

    public Token Name { get; set; }
    public IExpr Value { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitAssignmentExpr(this);
    }
}

public class LogicalExpr : IExpr {

    public LogicalExpr(IExpr Left, Token Operator, IExpr Right)
    {
        this.Left = Left;
        this.Operator = Operator;
        this.Right = Right;
    }

    public IExpr Left { get; set; }
    public Token Operator { get; set; }
    public IExpr Right { get; set; }

    public TRetType Accept<TRetType>(IExprVisitor<TRetType> visitor) {
        return visitor.VisitLogicalExpr(this);
    }
}

