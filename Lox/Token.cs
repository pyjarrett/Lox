namespace Lox;

/// <summary>
/// Tokens have an associated Kind which is used to efficiently assign and
/// compare tokens without comparing their full lexemes.
/// </summary>
public enum TokenKind
{
    // Single character token kinds (types).
    LeftParen,
    RightParen,
    LeftBrace,
    RightBrace,
    Comma,
    Dot,
    Minus,
    Plus,
    Semicolon,
    Slash,
    Star,

    // Multiple character token kinds (types).
    Not,
    NotEqual,
    Equal,
    EqualEqual,
    GreaterThan,
    GreaterThanOrEqual,
    LessThan,
    LessThanOrEqual,

    // Tokens which contain literals.
    Identifier,
    String,

    // Keywords.
    And,
    Class,
    Else,
    False,
    Fun,
    For,
    If,
    Nil,
    Or,
    Print,
    Return,
    Super,
    This,
    True,
    Var,
    While,

    // End of file
    EndOfFile,
}

/// <summary>
/// The tokens produced by the lexer.
/// </summary>
/// <param name="Kind"></param>
/// <param name="Lexeme"></param>
/// <param name="LineNumber"></param>
/// <param name="Literal">The literal value of the token if it has one, such as the value of an integer literal.</param>
public readonly record struct Token(TokenKind Kind, string Lexeme, long LineNumber, object? Literal = null);
