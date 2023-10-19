using System.Diagnostics;
using System.IO.Compression;

namespace Lox;

/// <summary>
/// Produces a sequence of tokens from a block of text.
/// </summary>
public class Lexer
{
    /// <summary>
    /// Creates a new Lexer to break the given text into tokens.
    /// </summary>
    public Lexer(string text)
    {
        Text = text;
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
    /// Moves to the next character.
    /// </summary>
    private char Advance() => Text[currentIndex++];

    /// <summary>
    /// Attempts to try to scan the next token.
    /// </summary>
    private void ScanNextToken()
    {
        // Get the first character, then figure out what to do.
        char c = Advance();
        switch (c)
        {
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
            case '+':
                AddToken(TokenKind.Plus);
                break;
            case '-':
                AddToken(TokenKind.Minus);
                break;
            case '*':
                AddToken(TokenKind.Star);
                break;
            case '/':
                AddToken(TokenKind.Slash);
                break;
            case '=':
                AddToken(TokenKind.Equal);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Adds a new token with the current parse state with the current slice
    /// and line number.
    /// </summary>
    private void AddToken(TokenKind kind)
    {
        Tokens.Add(new Token(kind, CurrentSlice(), lineNumber));
    }

    private string Text { get; init; } = "";
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