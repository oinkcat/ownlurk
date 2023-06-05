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

    private const string EscapeEnd = "}}}";

    private readonly Dictionary<string, TokenType> tokensMap = new()
    {
        [EscapeStart] = TokenType.EscapeStart,
        ["{{"] = TokenType.ObjectStart,
        ["}}"] = TokenType.ObjectEnd,
        ["[["] = TokenType.LinkStart,
        ["]]"] = TokenType.LinkEnd,
        ["["] = TokenType.ExtLinkStart,
        ["]"] = TokenType.ExtLinkEnd,
        ["==="] = TokenType.ThreeEqual,
        ["=="] = TokenType.TwoEqual,
        ["|"] = TokenType.Bar,
        ["*"] = TokenType.Star,
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
    
    public WikiTokenizer(string inputText)
    {
        pushedBackTokens = new Queue<TokenInfo>();
        regex = new Regex(ConstructParseRegexTemplate(), Options);

        textToTokenize = inputText;
        prevAtEol = true;
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
        TokenType? matchedTokenType = null;
        
        if(newMatch.Success)
        {
            var (i, l) = (newMatch.Index, newMatch.Length);
            string tokenText = textToTokenize.Substring(i, l);
            matchedTokenType = tokensMap[tokenText];

            TokenInfo matchedTokenInfo;

            if(matchedTokenType == TokenType.EscapeStart)
            {
                int escapeEndIdx = textToTokenize.IndexOf(EscapeEnd, i);
                int escLen = EscapeStart.Length;

                string escapedText = textToTokenize[(i + escLen) .. escapeEndIdx];
                matchedTokenInfo = new TokenInfo(escapedText, prevAtEol);

                l = escapeEndIdx - i + 1;
            }
            else
            {
                matchedTokenInfo = new TokenInfo(tokensMap[tokenText], tokenText, prevAtEol);
            }

            if (TryExtractPossibleTextMatch(nextMatchIndex, i, out string text))
            {
                Current = new TokenInfo(text, prevAtEol);
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
                ? new TokenInfo(rest, prevAtEol)
                : null;
                
            nextMatchIndex = textToTokenize.Length;
        }

        prevAtEol = matchedTokenType == TokenType.NewLine;
        
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
        pushedBackTokens.Enqueue(token != null ? token : Current);
    }

    public void Reset() => throw new NotSupportedException();

    public void Dispose()
    {
        // ...
    }
    
    public IEnumerator<TokenInfo> GetEnumerator() => this;
}