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
    TemplateStart,
    TemplateEnd,
    LinkStart,
    LinkEnd,
    ExtLinkStart,
    ExtLinkEnd,
    ThreeEqual,
    TwoEqual,
    Bar,
    Star,
    Sharp,
    MultiEmphasis,
    Emphasis,
    LittleEmphasis,
    Text,
    NewLine,
    EndOfFile // ?
}

/// <summary>
/// Информация о токене Wiki-разметки
/// </summary>
public record TokenInfo(string Text, bool AtStartOfLine = false)
{
    /// <summary>
    /// Тип токена
    /// </summary>
    public TokenType Type { get; init; } = TokenType.Text;

    /// <summary>
    /// Является текстовым литералом
    /// </summary>
    public bool IsText => Type == TokenType.Text;

    /// <summary>
    /// Является маркером форматирования
    /// </summary>
    public bool IsFormatting => (Type == TokenType.MultiEmphasis) ||
                                (Type == TokenType.Emphasis) ||
                                (Type == TokenType.LittleEmphasis);

    /// <summary>
    /// Является ли маркером начала элемента ссылки
    /// </summary>
    public bool IsLinkStart => (Type == TokenType.LinkStart) || 
                               (Type == TokenType.ExtLinkStart);

    /// <summary>
    /// Является ли маркером начала шаблона
    /// </summary>
    public bool IsTemplateStart => Type == TokenType.TemplateStart;

    /// <summary>
    /// Является ли маркером начала списка
    /// </summary>
    public bool IsList => (Type == TokenType.Star) || (Type == TokenType.Sharp);

    public TokenInfo(TokenType type, string text, bool atStart = false) : this(text, atStart)
    {
        Type = type;
    }
}