namespace LoxAst;

public class AstPrinterVisitor : IExprVisitor<string>, IStmtVisitor<string>
{
    /// Print statements as
    ///     stmt1
    ///     stmt2
    ///     stmt3
    public string Visit(List<IStmt?> block)
    {
        return Indent + string.Join($"\n{Indent}", block.Select((x) => x.Accept(this)));
    }

    public string VisitBinaryExpr(BinaryExpr node)
    {
        return $"({node.Operator.Lexeme} {node.Left.Accept(this)} {node.Right.Accept(this)})";
    }

    public string VisitLogicalExpr(LogicalExpr node)
    {
        return $"({node.Operator.Lexeme} {node.Left.Accept(this)} {node.Right.Accept(this)})";
    }

    public string VisitGroupingExpr(GroupingExpr node)
    {
        return $"(group {node.Expression.Accept(this)})";
    }

    public string VisitLiteralExpr(LiteralExpr node)
    {
        return $"{node.Value}";
    }

    public string VisitVariableExpr(VariableExpr node)
    {
        return $"VAR:{node.Name.Lexeme}";
    }

    public string VisitUnaryExpr(UnaryExpr node)
    {
        return $"({node.Operator.Lexeme} {node.Right.Accept(this)})";
    }

    public string VisitCallExpr(CallExpr node)
    {
        var args = string.Join(',', node.Arguments.Select(arg => $"{arg}"));
        return $"(Call {args})";
    }

    public string VisitAssignmentExpr(AssignmentExpr node)
    {
        return $"({node.Name.Lexeme} = {node.Value.Accept(this)})";
    }

    public string VisitExpressionStmt(ExpressionStmt node)
    {
        return $"{Indent}{node.Expression.Accept(this)};\n";
    }

    public string VisitPrintStmt(PrintStmt node)
    {
        return $"{Indent}print {node.Expression.Accept(this)};\n";
    }

    public string VisitVariableDeclarationStmt(VariableDeclarationStmt node)
    {
        return $"{Indent}vardecl {node.Name.Lexeme} {node.Initializer?.Accept(this)};\n";
    }

    public string VisitBlockStmt(BlockStmt node)
    {
        var s = $"block\n{Indent}{string.Join($"{Indent}", node.Block.Select((x) => x.Accept(this)))}\n";
        return s;
    }

    public string VisitIfStmt(IfStmt node)
    {
        Push();
        var thenBranch = node.ThenBranch.Accept(this);
        var elseBranch = "";
        if (node.ElseBranch != null)
        {
            elseBranch = node.ElseBranch.Accept(this);
        }
        Pop();
        return $"if\n{thenBranch}\nelse\n{elseBranch}\n";
    }

    public string VisitWhileStmt(WhileStmt node)
    {
        Push();
        var loop = $"{Indent} {node.Body.Accept(this)}";
        Pop();
        return $"while {node.Condition.Accept(this)}\n{loop}\n";
    }

    public string VisitFunctionStmt(FunctionStmt node)
    {
        return $"{Indent}function {node.Name.Lexeme};\n";
    }

    public string VisitReturnStmt(ReturnStmt node)
    {
        return $"{Indent}return {node.Accept(this)};\n";
    }

    private string Indent = "";
    private string IndentAmount = "\t";

    private void Push()
    {
        Indent = Indent + IndentAmount;
    }

    private void Pop()
    {
        Indent = Indent.Substring(0, Indent.Length - IndentAmount.Length);
    }
}