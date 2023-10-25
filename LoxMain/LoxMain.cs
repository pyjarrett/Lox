using LoxInterpreter;
using LoxLexer;
using LoxParser;

/// <summary>
/// LoxMain interpreter written in concert with reading "Crafting Interpreters"
/// by Robert Nystrom.
/// </summary>
public static class LoxMain
{
    public static void Main(String[] args)
    {
        const Int32 invalidUsage = 64;

        Console.WriteLine("LoxMain interpreter");
        if (args.Length > 1)
        {
            Console.Error.WriteLine("Usage: cslox [script]");
            System.Environment.Exit(invalidUsage);
        }
        else if (args.Length == 1)
        {
            RunFile(args[0]);
        }
        else
        {
            RunPrompt();
        }
    }

    /// Interprets the given text.
    private static void Run(string text)
    {
        Lexer lexer = new Lexer(text);
        Parser parser = new Parser(lexer.ScanTokens());
        var expr = parser.Expression();
        interpreter.Interpret(expr);
    }

    /// Runs the contents of a file as script.
    private static void RunFile(string fileName)
    {
        string text = File.ReadAllText(fileName);
        Run(text);
    }

    /// <summary>
    /// Runs an interactive prompt, interpreting each line as it is entered.
    /// </summary>
    private static void RunPrompt()
    {
        while (true)
        {
            string? line = ReadPrompt();
            if (line == null)
            {
                break;
            }
            Run(line);
        }
    }

    /// Reads text from a prompt.
    private static string? ReadPrompt()
    {
        Console.Write(" > ");
        return Console.ReadLine();
    }

    private static Interpreter interpreter = new Interpreter();
}