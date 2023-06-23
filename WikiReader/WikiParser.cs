using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader;

/// <summary>
/// Производит синтаксический разбор токенов разметки Wiki-страницы
/// </summary>
public class WikiParser
{
    private readonly string textToParse;

    private readonly WikiTokenizer tokenizer;

    /// <summary>
    /// Элементы разобранного документа
    /// </summary>
    public WikiDocument ParsedDocument { get; private set; }

    public WikiParser(string inputText)
    {
        textToParse = inputText;
        tokenizer = new WikiTokenizer(inputText);
    }

    /// <summary>
    /// Разобрать документ
    /// </summary>
    public void Parse()
    {
        if(ParsedDocument != null) { return; }

        ParsedDocument = new WikiDocument();

        while(tokenizer.MoveNext())
        {
            var parsedElement = ParseGenericElement();
            (ParsedDocument as IWikiContentElement).AppendContent(parsedElement);
        }
    }

    // Разобрать токен
    private WikiElement ParseGenericElement()
    {
        var token = tokenizer.Current;

        if(token.IsText)
        {
            return new WikiTextElement(token.Text);
        }
        else if(token.IsLinkStart)
        {
            return ParseLinkElement(token.Type == TokenType.ExtLinkStart);
        }
        else if(token.IsFormatting)
        {
            return ParseFormattedElement(token.Type == TokenType.Emphasis);
        }
        else if(token.Type == TokenType.NewLine)
        {
            return new WikiEolElement();
        }
        else if(token.AtStartOfLine && (token.Type == TokenType.TwoEqual))
        {
            return ParseHeaderElement(token.Text.Length);
        }
        else if(token.AtStartOfLine && token.IsList)
        {
            return ParseListElement(token.Type == TokenType.Sharp);
        }
        else
        {
            return null;
        }
    }

    // Разобрать разметку ссылки
    private WikiLinkElement ParseLinkElement(bool isExternal)
    {
        tokenizer.MoveNext();

        if(tokenizer.Current.IsText)
        {
            var linkElem = new WikiLinkElement
            {
                Uri = new WikiTextElement(tokenizer.Current.Text),
                IsExternal = isExternal
            };

            tokenizer.MoveNext();
            var expectedEndToken = isExternal ? TokenType.ExtLinkEnd : TokenType.LinkEnd;
            AppendContentUntilEndToken(linkElem, expectedEndToken);

            return linkElem;
        }
        else
        {
            return null;
        }
    }

    private void AppendContentUntilEndToken(IWikiContentElement element, TokenType endToken)
    {       
        while(tokenizer.Current.Type != endToken)
        {
            element.AppendContent(ParseGenericElement());
            tokenizer.MoveNext();
        }
    }

    // Разобрать разметку списка
    private WikiListElement ParseListElement(bool isNumbered)
    {
        var listElem = new WikiListElement { IsNumbered = isNumbered };

        do
        {
            var element = listElem.AddElement();

            while (tokenizer.Current.Type != TokenType.NewLine)
            {
                tokenizer.MoveNext();
                element.Add(ParseGenericElement());
            }

            tokenizer.MoveNext();
        }
        while (tokenizer.Current.AtStartOfLine && tokenizer.Current.IsList);

        return listElem;
    }

    // Разобрать разметку форматированного текста
    private WikiFormattedElement ParseFormattedElement(bool isBold)
    {
        var formattedElem = new WikiFormattedElement
        {
            Type = isBold ? FormattingType.Bold : FormattingType.Italic
        };

        tokenizer.MoveNext();
        var expectedEndToken = isBold ? TokenType.Emphasis : TokenType.LittleEmphasis;
        AppendContentUntilEndToken(formattedElem, expectedEndToken);

        return formattedElem;
    }

    // Разобрать разметку элемента списка
    private WikiHeaderElement ParseHeaderElement(int level)
    {
        var headerElem = new WikiHeaderElement { Level = level };

        tokenizer.MoveNext();
        AppendContentUntilEndToken(headerElem, TokenType.NewLine);

        return headerElem;
    }
}