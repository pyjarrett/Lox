using LoxLexer;

namespace LoxInterpreter;

/// <summary>
/// A container for variables and their values.
/// </summary>
public class Environment
{
    /// <summary>
    /// An environment, with an optional environment in which it exists in.
    /// This parent environment could be the global scope, or the next higher
    /// block.
    /// </summary>
    public Environment(Environment? parent = null)
    {
        enclosing = parent;
    }

    /// <summary>
    /// Look up a variable, throwing a RuntimeError if it can't find it.
    /// </summary>
    public object Get(Token name)
    {
        var identifier = name.Lexeme;
        if (values.ContainsKey(identifier))
        {
            return values[identifier];
        }

        if (enclosing == null)
        {
            throw new RuntimeError(name, $"Unknown variable: '{identifier}'");
        }

        return enclosing.Get(name);
    }

    public void Define(string name, object? value)
    {
        values[name] = value;
    }

    public void Assign(Token name, object? value)
    {
        var identifier = name.Lexeme;
        if (values.ContainsKey(identifier))
        {
            values[identifier] = value;
        }
        else if (enclosing == null)
        {
            throw new RuntimeError(name, $"Undefined identifier: '{name.Lexeme}'");
        }
        else
        {
            enclosing.Assign(name, value);
        }
    }

    private Environment? enclosing = null;
    private Dictionary<string, object?> values = new();
}