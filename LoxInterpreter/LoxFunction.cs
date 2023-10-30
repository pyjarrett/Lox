using LoxAst;

namespace LoxInterpreter;

public class LoxFunction : LoxCallable
{
    public LoxFunction(FunctionStmt decl)
    {
        declaration = decl;
    }

    public object Call(Interpreter interpreter, List<object?> arguments)
    {
        Environment environment = new Environment(interpreter.globals);
        for (int i = 0; i < declaration.Params.Count; ++i)
        {
            environment.Define(declaration.Params[i].Lexeme, arguments[i]);
        }

        interpreter.ExecuteBlock(declaration.Body, environment);

        // TODO: Weird this isn't a return of a value.
        return null;
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
}