namespace LoxInterpreter;

public class LoxClass
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
}