using LoxLexer;

namespace LoxInterpreter;

public class LoxInstance
{
    public LoxInstance(LoxClass klass)
    {
        this.klass = klass;
    }

    public override string ToString()
    {
        return $"{klass.Name} instance";
    }

    public object? Get(Token name)
    {
        if (fields.ContainsKey(name.Lexeme))
        {
            return fields[name.Lexeme];
        }

        LoxFunction? method = klass.FindMethod(name.Lexeme);
        if (method != null)
        {
            return method.Bind(this);
        }
        throw new RuntimeError(name, $"Unknown property: '{name.Lexeme}'");
    }

    public void Set(Token name, object? value)
    {
        fields[name.Lexeme] = value;
    }

    private LoxClass klass;
    private Dictionary<string, object?> fields = new();
}