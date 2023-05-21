namespace WikiReader;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using TokenType = System.Nullable<System.ValueTuple<WikiToken, System.String>>;

/// <summary>
/// Токен Wiki-разметки
/// </summary>
public enum WikiToken
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
    LineBreak,
    EndOfFile // ?
}

/// <summary>
/// Разбивает текст Wiki-разметки на токены
/// </summary>
public class WikiTokenizer : IEnumerator<TokenType>
{
    private const string EscapeStart = "{{{";

    private const string EscapeEnd = "}}}";

    private readonly Dictionary<string, WikiToken> tokensMap = new()
    {
        [EscapeStart] = WikiToken.EscapeStart,
        ["{{"] = WikiToken.ObjectStart,
        ["}}"] = WikiToken.ObjectEnd,
        ["[["] = WikiToken.LinkStart,
        ["]]"] = WikiToken.LinkEnd,
        ["["] = WikiToken.ExtLinkStart,
        ["]"] = WikiToken.ExtLinkEnd,
        ["==="] = WikiToken.ThreeEqual,
        ["=="] = WikiToken.TwoEqual,
        ["|"] = WikiToken.Bar,
        ["*"] = WikiToken.Star,
        ["'"] = WikiToken.Quote,
        ["\n"] = WikiToken.NewLine,
        [@"\\"] = WikiToken.LineBreak
    };

    const RegexOptions Options = RegexOptions.Compiled |
                                 RegexOptions.Multiline |
                                 RegexOptions.IgnorePatternWhitespace;
                                 
    public TokenType Current { get; private set; }

    object IEnumerator.Current => Current;

    private readonly Regex regex;
    
    private readonly string textToTokenize;
    
    private readonly Queue<TokenType> pushedBackTokens;
    
    private TokenType next;
    
    private int nextMatchIndex;

    private bool isAtStartOfLine;
    
    public WikiTokenizer(string inputText)
    {
        pushedBackTokens = new Queue<TokenType>();
        regex = new Regex(ConstructParseRegexTemplate(), Options);

        textToTokenize = inputText;
        isAtStartOfLine = true;
    }
    
    private string ConstructParseRegexTemplate()
    {
        var alternatives = new List<string>();
        
        foreach(string templatePart in tokensMap.Keys)
        {
            alternatives.Add(Regex.Escape(templatePart));
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
        
        if(newMatch.Success)
        {
            var (i, l) = (newMatch.Index, newMatch.Length);
            string tokenText = textToTokenize.Substring(i, l);
            var tokenType = tokensMap[tokenText];

            TokenType matchedTokenInfo;

            if(tokenType == WikiToken.EscapeStart)
            {
                int escapeEndIdx = textToTokenize.IndexOf(EscapeEnd, i);
                int escLen = EscapeStart.Length;
                int tokenEndIdx = escapeEndIdx + escLen;
                matchedTokenInfo = (WikiToken.Text, textToTokenize[(i + escLen) .. tokenEndIdx]);

                l = escapeEndIdx - i + 1;
            }
            else
            {
                matchedTokenInfo = (tokensMap[tokenText], tokenText);
            }

            if (TryExtractPossibleTextMatch(nextMatchIndex, i, out string text))
            {
                Current = (WikiToken.Text, text);
                next = matchedTokenInfo;
            }
            else
            {
                Current = matchedTokenInfo;
            }
    
            nextMatchIndex = i + l;
        }
        else
        {    
            Current = (TryExtractPossibleTextMatch(nextMatchIndex, textToTokenize.Length, out string rest))
                ? (WikiToken.Text, rest)
                : null;
                
            nextMatchIndex = textToTokenize.Length;
        }

        isAtStartOfLine = Current?.Item1 == WikiToken.NewLine;
        
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
        else if(pushedBackTokens.TryDequeue(out TokenType pushedBackItem))
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
    public void PushBack(TokenType token = null)
    {
        pushedBackTokens.Enqueue(token != null ? token : Current);
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
        // ...
    }
    
    public IEnumerator<TokenType> GetEnumerator() => this;
}