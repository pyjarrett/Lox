using System.Diagnostics;
using LoxLexer;

namespace LoxInterpreter;

/// <summary>
/// Value stored when a variable is uninitialized.  This helps differentiate
/// a failure to initialize from an initialized `nil.`
/// </summary>
public readonly record struct UninitializedValue;

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
        Enclosing = parent;
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

        if (Enclosing == null)
        {
            throw new RuntimeError(name, $"Unknown variable: '{identifier}'");
        }

        return Enclosing.Get(name);
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)!.values[name];
    }

    /// <summary>
    /// Ascend up through enclosing environments, looking for the Nth ancestor.
    /// This environment is considered the 0th ancestor, and `enclosing` is the
    /// 1st.
    /// </summary>
    private Environment? Ancestor(int distance)
    {
        Environment? ancestor = this;
        for (var i = 0; i < distance; ++i)
        {
            ancestor = ancestor!.Enclosing;
        }

        return ancestor;
    }

    /// <summary>
    /// Define and potentially redefine a variable with a given name, giving
    /// it the specified value.
    /// </summary>
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
        else if (Enclosing == null)
        {
            throw new RuntimeError(name, $"Undefined identifier: '{name.Lexeme}'");
        }
        else
        {
            Enclosing.Assign(name, value);
        }
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        Ancestor(distance)!.Assign(name, value);
    }

    /// <summary>
    /// The parent scope of this environment.
    /// </summary>
    public Environment? Enclosing { get; private set; } = null;

    private Dictionary<string, object?> values = new();
}