using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader;

/// <summary>
/// Токен Wiki-разметки
/// </summary>
public enum TokenType
{
    EscapeStart,
    ObjectStart,
    ObjectEnd,
    LinkStart,
    LinkEnd,
    ExtLinkStart,
    ExtLinkEnd,
    ThreeEqual,
    TwoEqual,
    Bar,
    Star,
    Quote,
    Text,
    NewLine,
    EndOfFile // ?
}

/// <summary>
/// Информация о токене Wiki-разметки
/// </summary>
public record TokenInfo(string text, bool atStart = false)
{
    public TokenType Type { get; init; } = TokenType.Text;

    public string Text { get; init; } = text;

    public bool AtStartOfLine { get; init; } = atStart;

    public bool IsText => Type == TokenType.Text;

    public TokenInfo(TokenType type, string text, bool atStart = false) : this(text, atStart)
    {
        Type = type;
        AtStartOfLine = atStart;
    }
}