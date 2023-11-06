namespace LoxInterpreter;

public class LoxClass : LoxCallable
{
    public String Name { get; init; }

    public LoxClass(string name)
    {
        Name = name;
    }
    
    public override string ToString()
    {
        return $"<class {Name}>";
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        LoxInstance instance = new(this);
        return instance;
    }

    public int Arity()
    {
        // TODO: Add support for adding init parameters.
        return 0;
    }
}