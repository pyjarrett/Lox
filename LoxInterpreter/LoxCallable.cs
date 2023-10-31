namespace LoxInterpreter;

/// <summary>
/// A callable might be a Lox-defined function, or a wrapper around
/// a foreign function.
/// </summary>
public interface LoxCallable
{
    object? Call(Interpreter interpreter, List<object?> arguments);
    
    /// <summary>
    /// The number of parameters to this callable object.
    /// </summary>
    int Arity();
}