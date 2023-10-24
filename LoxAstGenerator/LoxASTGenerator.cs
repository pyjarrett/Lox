using System.Diagnostics;
using System.Xml.Serialization;

public static class LoxASTGenerator
{
    /// <summary>
    /// Writes out the definitions for AST node family types and the associated
    /// Visitor interface for visiting all nodes types.
    /// </summary>
    public static void Main(string[] args)
    {
        if (args.Length != 2)
        {
            PrintUsageAndExit();
        }

        string defnFile = args[0];
        if (!File.Exists(defnFile))
        {
            Console.Error.WriteLine($"Input type definition file {defnFile} does not exist.");
            System.Environment.Exit(1);
        }

        // This could create the directory, but require that it exists.
        string dirName = args[1];
        if (!Directory.Exists(dirName))
        {
            Console.Error.Write($"Output directory {dirName} does not exist.");
            System.Environment.Exit(1);
        }

        GenerateAST(defnFile, dirName);
    }

    private static void PrintUsageAndExit()
    {
        Console.WriteLine("Usage: generate_ast <output_directory>");
        System.Environment.Exit(1);
    }

    public readonly record struct Variable(
        [property: XmlAttribute] string Type,
        [property: XmlAttribute] string Name);

    public readonly record struct SubType(
        [property: XmlAttribute] string Name,
        Variable[] Variables);

    public readonly record struct Group(
        [property: XmlAttribute] string Base,
        [property: XmlArrayItem("Using")] string[] Usings,
        SubType[] SubTypes);

    public readonly record struct ASTDefn(Group[] Groups);

    /// <summary>
    /// The AST requires a potentially large set of related types to be created,
    /// different in what data they contain.  The set of types which needs to be
    /// visited can be similarly generated from this list, which results in the
    /// visitor interface being kept up-to-date with the current node types.
    /// </summary>
    private static void GenerateAST(string defnFile, string dirName)
    {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(ASTDefn));
        FileStream fileStream = new FileStream(defnFile, System.IO.FileMode.Open);
        ASTDefn? ast = (ASTDefn?)xmlSerializer.Deserialize(fileStream);
        if (ast == null)
        {
            Console.Error.WriteLine("Unable to load AST definition");
            System.Environment.Exit(1);
        }

        foreach (var group in ast?.Groups)
        {
            DefineGroup(dirName, group);
        }
    }

    private static void DefineGroup(string dirName, Group group)
    {
        var groupFileName = Path.Join(dirName, group.Base + ".cs");
        Console.WriteLine($"Writing group {group.Base} to {groupFileName}.");

        var visitorName = $"I{group.Base}Visitor<TRetType>";

        using (StreamWriter file = new StreamWriter(groupFileName))
        {
            if (group.Usings.Length > 0)
            {
                foreach (var dependency in group.Usings)
                {
                    file.WriteLine($"using {dependency};");
                }

                file.WriteLine();
            }

            file.WriteLine("namespace LoxAst;");
            file.WriteLine();

            // Write the visitor to visit all node types.
            file.WriteLine($"public interface {visitorName} {{");
            foreach (var subType in group.SubTypes)
            {
                file.WriteLine($"    TRetType Visit{subType.Name}{group.Base}({subType.Name}{group.Base} node);");
            }

            file.WriteLine("}");
            file.WriteLine();

            file.WriteLine($"public interface {group.Base} {{");
            file.WriteLine($"    TRetType Accept<TRetType>({visitorName} visitor);");
            file.WriteLine("}");
            file.WriteLine();

            foreach (var subType in group.SubTypes)
            {
                var variables = subType.Variables.Select(variable => $"{variable.Type} {variable.Name}").ToArray();
                file.WriteLine(
                    $"public readonly record struct {subType.Name}{group.Base} ({string.Join(", ", variables)}) : {group.Base} {{");

                file.WriteLine($"    public TRetType Accept<TRetType>({visitorName} visitor) {{");
                file.WriteLine($"        return visitor.Visit{subType.Name}{group.Base}(this);");
                file.WriteLine("    }");
                file.WriteLine("}");
            }
        }
    }
}