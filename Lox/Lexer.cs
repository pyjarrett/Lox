using System.Diagnostics;

namespace LoxLexer;

/// <summary>
/// Produces a sequence of tokens from a block of text.
/// </summary>
public class Lexer
{
    static Lexer()
    {
        keywordsToTokenKinds = new Dictionary<string, TokenKind>()
        {
            { "and", TokenKind.And },
            { "class", TokenKind.Class },
            { "else", TokenKind.Else },
            { "false", TokenKind.False },
            { "fun", TokenKind.Fun },
            { "for", TokenKind.For },
            { "if", TokenKind.If },
            { "nil", TokenKind.Nil },
            { "or", TokenKind.Or },
            { "print", TokenKind.Print },
            { "return", TokenKind.Return },
            { "super", TokenKind.Super },
            { "this", TokenKind.This },
            { "true", TokenKind.True },
            { "var", TokenKind.Var },
            { "while", TokenKind.While },
        };
    }

    /// <summary>
    /// Creates a new Lexer to break the given text into tokens.
    /// </summary>
    public Lexer(string text, bool supportNestedBlockComments = true)
    {
        Text = text;
        this.supportNestedBlockComments = supportNestedBlockComments;
    }

    /// <summary>
    /// Produces a list of all tokens from the text.
    /// </summary>
    public List<Token> ScanTokens()
    {
        // Don't keep appending to the token list if this function gets called
        // multiple times.
        if (Tokens.Count > 0)
        {
            return Tokens;
        }

        // Process until there's no text left.
        while (!IsAtEnd())
        {
            // Move the current slice ahead.
            startIndex = currentIndex;
            ScanNextToken();
        }

        Tokens.Add(new Token(TokenKind.EndOfFile, "", lineNumber));
        return Tokens;
    }

    /// <summary>
    /// Tracks whether or not an error occurred when scanning tokens.
    /// </summary>
    public bool HasError { get; private set; } = false;

    /// <summary>
    /// Produces the slice give by the current slice cursor positions.
    /// </summary>
    private string CurrentSlice()
    {
        var length = currentIndex - startIndex;

        Debug.Assert(startIndex >= 0);
        Debug.Assert(length >= 0);

        return Text.Substring(startIndex, length);
    }

    /// <summary>
    /// Has the entire text been parsed?
    /// </summary>
    private bool IsAtEnd() => currentIndex >= Text.Length;

    /// <summary>
    /// Return the next character or a null character.
    /// </summary>
    private char Peek() => IsAtEnd() ? '\0' : Text[currentIndex];

    /// <summary>
    /// The character after the current one, or a null character.
    /// </summary>
    private char PeekNext() => currentIndex + 1 >= Text.Length ? '\0' : Text[currentIndex + 1];

    /// <summary>
    /// Moves to the next character.
    /// </summary>
    private char Advance() => Text[currentIndex++];

    /// <summary>
    /// A conditional version of Advance, which only moves forward when the
    /// given character is matched.
    /// </summary>
    private bool Match(char c)
    {
        if (IsAtEnd() || Text[currentIndex] != c)
        {
            return false;
        }

        ++currentIndex;
        return true;
    }

    /// <summary>
    /// Attempts to try to scan the next token.
    /// </summary>
    private void ScanNextToken()
    {
        // Get the first character, then figure out what to do.
        char c = Advance();
        switch (c)
        {
            // Single character tokens.
            case '(':
                AddToken(TokenKind.LeftParen);
                break;
            case ')':
                AddToken(TokenKind.RightParen);
                break;
            case '{':
                AddToken(TokenKind.LeftBrace);
                break;
            case '}':
                AddToken(TokenKind.RightBrace);
                break;
            case ',':
                AddToken(TokenKind.Comma);
                break;
            case '.':
                AddToken(TokenKind.Dot);
                break;
            case ';':
                AddToken(TokenKind.Semicolon);
                break;

            // Operate and assign in-place operators are not supported in Lox.
            case '+':
                AddToken(TokenKind.Plus);
                break;
            case '-':
                AddToken(TokenKind.Minus);
                break;
            case '*':
                AddToken(TokenKind.Star);
                break;

            // Multiple character tokens.
            case '=':
                AddToken(Match('=') ? TokenKind.EqualEqual : TokenKind.Equal);
                break;
            case '!':
                AddToken(Match('=') ? TokenKind.NotEqual : TokenKind.Not);
                break;
            case '<':
                AddToken(Match('=') ? TokenKind.LessThanOrEqual : TokenKind.LessThan);
                break;
            case '>':
                AddToken(Match('=') ? TokenKind.GreaterThanOrEqual : TokenKind.GreaterThan);
                break;

            case '/':
                if (Match('/'))
                {
                    // This is a line comment, proceed until an end of line or
                    // end of the text.
                    while (!IsAtEnd() && Peek() != '\n')
                    {
                        Advance();
                    }
                }
                else if (Peek() == '*' && supportNestedBlockComments)
                {
                    // Eat "*"
                    Advance();
                    ScanNestedComment();
                }
                else
                {
                    AddToken(TokenKind.Slash);
                }

                break;

            // Skip whitespace
            case ' ':
            case '\r':
            case '\t':
                break;

            // Skip newlines
            case '\n':
                ++lineNumber;
                break;

            case '"':
                ScanString();
                break;

            default:
                if (Char.IsDigit(c))
                {
                    ScanNumber();
                }
                else if (Char.IsLetter(c))
                {
                    while (char.IsLetterOrDigit(Peek()))
                    {
                        Advance();
                    }

                    if (keywordsToTokenKinds.ContainsKey(CurrentSlice()))
                    {
                        AddToken(keywordsToTokenKinds[CurrentSlice()]);
                    }
                    else
                    {
                        AddToken(TokenKind.Identifier);
                    }
                }
                else
                {
                    // Unhandled character type.
                    Error($"Unable to parse token, at character {c} at {lineNumber}");
                }

                break;
        }
    }

    /// <summary>
    /// Scan a string token, assuming that the starting index is currently at
    /// an open double quote.  Note that escaped characters are not handled.
    /// </summary>
    private void ScanString()
    {
        // Continue scanning until end of input or finding a '"'.
        while (Peek() != '\0')
        {
            // String terminator.
            if (Advance() == '"')
            {
                var slice = CurrentSlice();
                AddToken(TokenKind.String, slice.Substring(1, slice.Length - 2));
                return;
            }
        }

        // Could not find the end of the string.
        Error($"Unterminated string starting at {currentIndex}");
    }

    /// <summary>
    /// Scans a possible integer or decimal.
    /// </summary>
    ///
    /// Numbers can look like:
    /// * `[:digit:]+`
    /// * `[:digit:]+.[:digit:]+`
    private void ScanNumber()
    {
        Debug.Assert(char.IsDigit(Text[startIndex]));

        // Scan leading digits.  Continue until something else is found.
        while (char.IsDigit(Peek()))
        {
            Advance();
        }

        // Look for the decimal part followed by a number.
        if (Peek() == '.' && char.IsDigit(PeekNext()))
        {
            // Skip the '.'
            Advance();

            while (char.IsDigit(Peek()))
            {
                Advance();
            }
        }

        // Parse and add the value for use as the literal.
        double value = 0;
        if (double.TryParse(CurrentSlice(), out value))
        {
            AddToken(TokenKind.Number, value);
        }
        else
        {
            Error($"Unable to parse number literal {CurrentSlice()}");
        }
    }

    /// <summary>
    /// Scan potentially nested C-style block comments. e.g. "/* .. /* ... /* ... */ .. /* ... */ ... */ ...*/".
    /// </summary>
    private void ScanNestedComment()
    {
        var nestedDepth = 1;
        while (nestedDepth > 0)
        {
            // Lox Extension (4.4): Support for C block comments
            while (!IsAtEnd() && !(Peek() == '*' && PeekNext() == '/'))
            {
                // Block comments can span multiple lines.
                if (Peek() == '\n')
                {
                    ++lineNumber;
                    Advance();
                }
                else if (Peek() == '/' && PeekNext() == '*')
                {
                    // Descend into another nested block comment.
                    ++nestedDepth;

                    // Eat "/*"
                    Advance();
                    Advance();
                }
                else
                {
                    // Consume comment internals.
                    Advance();
                }
            }

            // Terminated before backing out of all nested comments.
            if (IsAtEnd())
            {
                Error("Unterminated block comment.");
                break;
            }

            // Eat "*/"
            Advance();
            Advance();

            --nestedDepth;
        }
    }

    /// <summary>
    /// Reports a parse error.
    /// </summary>
    private void Error(string message)
    {
        HasError = true;
        Console.Error.WriteLine(message);
    }

    /// <summary>
    /// Adds a new token with the current parse state with the current slice
    /// and line number.
    /// </summary>
    private void AddToken(TokenKind kind, object? literal = null)
    {
        Tokens.Add(new Token(kind, CurrentSlice(), lineNumber, literal));
    }

    /// <summary>
    /// Maps between lexemes of keywords and their associated token kinds.
    /// </summary>
    private static Dictionary<string, TokenKind> keywordsToTokenKinds;

    private string Text { get; init; } = "";
    private bool supportNestedBlockComments = true;
    private List<Token> Tokens { get; } = new();

    private int lineNumber = 1;

    /// <summary>
    /// The lexer produces a slice of text which gets put into tokens.  This
    /// is more efficient that copying one character at a time. 
    /// </summary>
    private int startIndex = 0;

    /// <summary>
    /// Ending index of the slice.
    /// </summary>
    private int currentIndex = 0;
}