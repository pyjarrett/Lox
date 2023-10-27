using LoxLexer;

namespace LoxInterpreter;

/// <summary>
/// A container for variables and their values.
/// </summary>
public class Environment
{
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

        throw new RuntimeError(name, $"Unknown variable: '{identifier}'");
    }

    public void Define(Token name, object? value)
    {
        values[name.Lexeme] = value;
    }

    public void Assign(Token name, object? value)
    {
        var identifier = name.Lexeme;
        if (values.ContainsKey(identifier))
        {
            values[identifier] = value;
        }
        else
        {
            throw new RuntimeError(name, $"Undefined identifier: '{name.Lexeme}'");
        }
        
    }
    
    private Dictionary<string, object?> values = new();
}