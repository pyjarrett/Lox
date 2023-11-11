namespace LoxInterpreter;

public class LoxClass : LoxCallable
{
    private LoxClass? Superclass { get; init; }
    
    public String Name { get; init; }

    public Dictionary<string, LoxFunction> Methods { get; init; }

    public LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction>? methods)
    {
        Name = name;
        Superclass = superclass;
        Methods = methods;
    }
    
    public override string ToString()
    {
        return $"<class {Name}>";
    }

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        LoxInstance instance = new(this);
        
        // Call the constructor.
        LoxFunction? ctor = FindMethod("init");
        if (ctor != null)
        {
            return ctor.Bind(instance).Call(interpreter, arguments);
        }
        
        return instance;
    }

    public int Arity()
    {
        LoxFunction? ctor = FindMethod("init");
        if (ctor == null)
        {
            return 0;
        }
        return ctor.Arity();
    }

    /// <summary>
    /// Hides the details of looking up a method to enable looking up methods
    /// on the class or  higher in the inheritance hierarchy.
    /// </summary>
    public LoxFunction? FindMethod(string name)
    {
        if (Methods.ContainsKey(name))
        {
            return Methods[name];
        }

        // The method doesn't exist locally, so forward it upward the
        // inheritance chain.
        if (Superclass != null)
        {
            return Superclass.FindMethod(name);
        }
        
        return null;
    }
}