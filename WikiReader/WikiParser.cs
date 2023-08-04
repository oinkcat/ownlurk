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
    private readonly WikiTokenizer tokenizer;

    private readonly Stack<(WikiFormattedElement, int)> formatTagsNesting;

    /// <summary>
    /// Элементы разобранного документа
    /// </summary>
    public WikiDocument ParsedDocument { get; private set; }

    public WikiParser(string inputText)
    {
        tokenizer = new WikiTokenizer(inputText);
        formatTagsNesting = new();
    }

    /// <summary>
    /// Разобрать документ
    /// </summary>
    public void Parse()
    {
        if(ParsedDocument != null) { return; }

        ParsedDocument = new WikiDocument();
        formatTagsNesting.Clear();

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
            return ParseFormattedElement();
        }
        else if(token.Type == TokenType.NewLine)
        {
            return ParseEndOfLineElement();
        }
        else if(token.AtStartOfLine && (token.Type == TokenType.TwoEqual))
        {
            return ParseHeaderElement(token.Text.Length);
        }
        else if(token.AtStartOfLine && token.IsList)
        {
            return ParseListElement(token.Type == TokenType.Sharp);
        }
        else if(token.IsTemplateStart)
        {
            return ParseTemplateElement();
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

            linkElem.FixExternalLinkContent();

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

    // Разобрать разметку форматированного текста
    private WikiFormattedElement ParseFormattedElement()
    {
        if((tokenizer.Current.Type == TokenType.MultiEmphasis) &&
           (tokenizer.Current.Text.Length % 2 == 0))
        {
            return null;
        }

        // Открывающий тэг
        var formattedElem = new WikiFormattedElement();
        formatTagsNesting.Push((formattedElem, tokenizer.CurrentRowNumber));

        // Содержимое
        if(tokenizer.Current.Type == TokenType.MultiEmphasis)
        {
            ParseMultiFormatting(formattedElem);
        }
        else
        {
            ParseSimpleFormatting(formattedElem, tokenizer.Current.Type == TokenType.Emphasis);
        }

        formatTagsNesting.Pop();

        return formattedElem;
    }

    private void ParseMultiFormatting(WikiFormattedElement formattedElem)
    {
        var nestedFormattedElem = new WikiFormattedElement();
        formatTagsNesting.Push((nestedFormattedElem, tokenizer.CurrentRowNumber));
        formattedElem.Content.Add(nestedFormattedElem);
        formattedElem = nestedFormattedElem;

        tokenizer.MoveNext();

        while(true)
        {
            formattedElem.Content.Add(ParseGenericElement());
            tokenizer.MoveNext();

            if(tokenizer.Current.Type == TokenType.MultiEmphasis)
            {
                break;
            }
            else if(tokenizer.Current.IsFormatting)
            {
                bool nestedIsBold = tokenizer.Current.Type == TokenType.Emphasis;
                formattedElem.Type = nestedIsBold ? FormattingType.Bold : FormattingType.Italic;

                _ = formatTagsNesting.Pop();
                (formattedElem, _) = formatTagsNesting.Peek();

                ParseSimpleFormatting(formattedElem, !nestedIsBold);
                break;
            }
        }
    }

    private void ParseSimpleFormatting(WikiFormattedElement formattedElem, bool isBold)
    {
        formattedElem.Type = isBold ? FormattingType.Bold : FormattingType.Italic;
        var closingTagType = isBold ? TokenType.Emphasis : TokenType.LittleEmphasis;
        tokenizer.MoveNext();

        while(tokenizer.Current.Type != closingTagType)
        {
            formattedElem.Content.Add(ParseGenericElement());
            tokenizer.MoveNext();

            if(tokenizer.Current.Type == TokenType.MultiEmphasis)
            {
                var replaceTokenType = isBold ? TokenType.LittleEmphasis : TokenType.Emphasis;
                string replaceTokenText = isBold ? "''" : "'''";
                tokenizer.PushBack(new TokenInfo(replaceTokenType, replaceTokenText));
                break;
            }
        }
    }

    // Разобрать элемент окончания строки
    private WikiEolElement ParseEndOfLineElement() => new()
    { 
        IsaHardBreak = tokenizer.Current.AtStartOfLine 
    };

    // Разобрать разметку элемента списка
    private WikiHeaderElement ParseHeaderElement(int level)
    {
        var headerElem = new WikiHeaderElement { Level = level };

        tokenizer.MoveNext();
        AppendContentUntilEndToken(headerElem, TokenType.NewLine);

        return headerElem;
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

        tokenizer.PushBack();

        return listElem;
    }

    // Разобрать разметку шаблона (ссылка на шаблон)
    private WikiTemplateElement ParseTemplateElement()
    {
        var templateElem = new WikiTemplateElement();

        tokenizer.MoveNext();

        // Название шаблона
        if(tokenizer.Current.IsText)
        {
            templateElem.Name = tokenizer.Current.Text;
        }
        else
        {
            throw new ApplicationException("Invalid template name!");
        }

        tokenizer.MoveNext();

        // Подстановки шаблона
        while (tokenizer.Current.Type != TokenType.TemplateEnd)
        {
            if (tokenizer.Current.Type == TokenType.Bar)
            {
                templateElem.StartNewSubstitution();
            }
            else
            {
                templateElem.AppendSubstitutionContent(ParseGenericElement());
            }

            tokenizer.MoveNext();
        }

        return templateElem;
    }
}