using LoxAst;

namespace LoxInterpreter;

public class LoxFunction : LoxCallable
{
    public LoxFunction(FunctionStmt decl, Environment env)
    {
        declaration = decl;
        closure = env;
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment environment = new Environment(closure);
        for (int i = 0; i < declaration.Params.Count; ++i)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
            return null;
        }
        catch (ReturnJump jump)
        {
            return jump.Value;
        }
    }

    public int Arity()
    {
        return declaration.Params.Count;
    }

    public override string ToString()
    {
        return $"<fn {declaration.Name.Lexeme}>";
    }

    private FunctionStmt declaration;
    private Environment closure;
}