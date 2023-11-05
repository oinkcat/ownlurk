namespace WikiReader;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/// <summary>
/// Разбивает текст Wiki-разметки на токены
/// </summary>
public class WikiTokenizer : IEnumerator<TokenInfo>
{
    private const string EscapeStart = "{{{";
    private const string HtmlEscapeStart = "<!--";

    private readonly Dictionary<string, string> escapeEndsMap = new ()
    {
        [EscapeStart] = "}}}",
        [HtmlEscapeStart] = "-->"
    };

    private readonly Dictionary<string, TokenType> tokensMap = new()
    {
        [EscapeStart] = TokenType.EscapeStart,
        [HtmlEscapeStart] = TokenType.EscapeStart,
        ["{{"] = TokenType.TemplateStart,
        ["}}"] = TokenType.TemplateEnd,
        ["[["] = TokenType.LinkStart,
        ["]]"] = TokenType.LinkEnd,
        ["["] = TokenType.ExtLinkStart,
        ["]"] = TokenType.ExtLinkEnd,
        ["==="] = TokenType.ThreeEqual,
        ["=="] = TokenType.TwoEqual,
        ["|"] = TokenType.Bar,
        ["*"] = TokenType.Star,
        ["#"] = TokenType.Sharp,
        ["'{4,}"] = TokenType.MultiEmphasis,
        ["'''"] = TokenType.Emphasis,
        ["''"] = TokenType.LittleEmphasis,
        ["\n"] = TokenType.NewLine,
        [@"\\"] = TokenType.NewLine
    };

    const RegexOptions Options = RegexOptions.Compiled |
                                 RegexOptions.Multiline |
                                 RegexOptions.IgnorePatternWhitespace;
                                 
    public TokenInfo Current { get; private set; }

    object IEnumerator.Current => Current;

    private readonly Regex regex;
    
    private readonly string textToTokenize;
    
    private readonly Queue<TokenInfo> pushedBackTokens;
    
    private TokenInfo next;
    
    private int nextMatchIndex;

    private bool prevAtEol;

    /// <summary>
    /// Номер текущей строки
    /// </summary>
    public int CurrentRowNumber { get; private set; }

    /// <summary>
    /// Текущая строка - пустая
    /// </summary>
    public bool AtEmptyLine => (Current == null) ||
                               (Current.AtStartOfLine && (Current.Type == TokenType.NewLine));

    public WikiTokenizer(string inputText)
    {
        pushedBackTokens = new Queue<TokenInfo>();
        regex = new Regex(ConstructParseRegexTemplate(), Options);

        textToTokenize = inputText;
        prevAtEol = true;
        CurrentRowNumber = 1;
    }
    
    private string ConstructParseRegexTemplate()
    {
        var alternatives = new List<string>();
        
        foreach((string templatePart, TokenType type) in tokensMap)
        {
            string altText = (type == TokenType.MultiEmphasis)
                ? templatePart
                : Regex.Escape(templatePart);
            alternatives.Add(altText);
        }
    
        return String.Join('|', alternatives);
    }

    /// <summary>
    /// Перейти к следующему токену
    /// </summary>
    /// <returns>Есть ли токен</returns>
    public bool MoveNext()
    {
        if(TryGetPendingToken())
        {
            return true;
        }
    
        var newMatch = regex.Match(textToTokenize, nextMatchIndex);
        TokenType? matchedTokenType = null;
        
        if(newMatch.Success)
        {
            var (i, l) = (newMatch.Index, newMatch.Length);
            string tokenText = textToTokenize.Substring(i, l);

            matchedTokenType = tokensMap.TryGetValue(tokenText, out var type)
                ? type
                : TokenType.MultiEmphasis;

            TokenInfo matchedTokenInfo;

            if (matchedTokenType == TokenType.EscapeStart)
            {
                // Отдельная обработка неразбираемого текста
                string escapeEnding = escapeEndsMap[tokenText];
                int escLen = tokenText.Length;
                int escapeEndIdx = textToTokenize.IndexOf(escapeEnding, i + escLen);

                string escapedText = textToTokenize[(i + escLen)..escapeEndIdx];
                matchedTokenInfo = new TokenInfo(escapedText, prevAtEol);

                l = escapeEndIdx - i + 1;
            }
            else
            {
                matchedTokenInfo = new TokenInfo(matchedTokenType.Value, tokenText, prevAtEol);
            }

            if (TryExtractPossibleTextMatch(nextMatchIndex, i, out string text))
            {
                Current = new TokenInfo(text, prevAtEol);

                // Токен за текстом не может быть в начале строки
                next = matchedTokenInfo with { AtStartOfLine = false };
            }
            else
            {
                Current = matchedTokenInfo;
            }
    
            nextMatchIndex = i + l;
        }
        else
        {
            int fullLength = textToTokenize.Length;
            Current = (TryExtractPossibleTextMatch(nextMatchIndex, fullLength, out string rest))
                ? new TokenInfo(rest, prevAtEol)
                : null;
                
            nextMatchIndex = textToTokenize.Length;
        }

        prevAtEol = matchedTokenType == TokenType.NewLine;

        if(prevAtEol)
        {
            CurrentRowNumber++;
        }
        
        return Current != null;
    }
    
    private bool TryGetPendingToken()
    {
        if(next != null)
        {
            Current = next;
            next = null;
            return true;
        }
        else if(pushedBackTokens.TryDequeue(out TokenInfo pushedBackItem))
        {
            Current = pushedBackItem;
            return true;
        }
        else
        {
            return false;
        }
    }
    
    private bool TryExtractPossibleTextMatch(int prevMatchIdx, int matchIdx, out string text)
    {
        if (matchIdx > prevMatchIdx)
        {
            int textTokenLength = matchIdx - prevMatchIdx;

            text = textToTokenize.Substring(prevMatchIdx, textTokenLength);

            if (text.Trim().Length > 0)
            {
                return true;
            }
        }

        text = null;
        return false;
    }
    
    /// <summary>
    /// Поместить токен обратно в очередь
    /// </summary>
    /// <param name="token">Токен для помещения в очередь</param>
    public void PushBack(TokenInfo token = null)
    {
        pushedBackTokens.Enqueue(token ?? Current);
    }

    /// <summary>
    /// Заменить двойной токен ссылки на одинарный
    /// </summary>
    public void ExplodeLinkToken()
    {
        if(Current.Type == TokenType.LinkEnd)
        {
            pushedBackTokens.Clear();
            Current = new TokenInfo(TokenType.ExtLinkEnd, "]");
            nextMatchIndex--;
        }
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
        // ...
    }
    
    public IEnumerator<TokenInfo> GetEnumerator() => this;
}