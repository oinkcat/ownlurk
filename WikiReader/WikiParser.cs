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

    private WikiParagraphElement paragraph;

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
            bool startNewParagraph = parsedElement is WikiHeaderElement;
            
            if (startNewParagraph)
            {
                AppendCurrentParagraph();
            }

            IWikiContentElement container = (paragraph == null) ? ParsedDocument : paragraph;
            container.AppendContent(parsedElement);

            if(startNewParagraph)
            {
                paragraph = new WikiParagraphElement();
            }
        }

        AppendCurrentParagraph();
    }

    // Разобрать токен
    private WikiElement ParseGenericElement()
    {
        var token = tokenizer.Current;

        if(token == null)
        {
            return null;
        }
        else if(token.IsText)
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
            if(tokenizer.AtEmptyLine) { break; }

            // HACK
            if((tokenizer.Current.Type == TokenType.LinkEnd) && (endToken == TokenType.ExtLinkEnd))
            {
                tokenizer.ExplodeLinkToken();
                break;
            }

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
        (formattedElem as IWikiContentElement).AppendContent(nestedFormattedElem);
        formattedElem = nestedFormattedElem;

        tokenizer.MoveNext();

        while(true)
        {
            (formattedElem as IWikiContentElement).AppendContent(ParseGenericElement());
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
            else if (tokenizer.Current.Type == TokenType.NewLine)
            {
                break; // ?
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
            (formattedElem as IWikiContentElement).AppendContent(ParseGenericElement());
            tokenizer.MoveNext();

            if(tokenizer.Current.Type == TokenType.MultiEmphasis)
            {
                var replaceTokenType = isBold ? TokenType.LittleEmphasis : TokenType.Emphasis;
                string replaceTokenText = isBold ? "''" : "'''";
                tokenizer.PushBack(new TokenInfo(replaceTokenType, replaceTokenText));
                break;
            }
            else if(tokenizer.Current.Type == TokenType.NewLine)
            {
                break; // ?
            }
        }
    }

    // Разобрать элемент окончания строки
    private WikiEolElement ParseEndOfLineElement()
    {
        if(tokenizer.Current.AtStartOfLine)
        {
            AppendCurrentParagraph();
            paragraph = new WikiParagraphElement();
            return null;
        }
        else
        {
            return new WikiEolElement();
        }
    }

    private void AppendCurrentParagraph()
    {
        if ((paragraph != null) && !paragraph.IsEmpty)
        {
            (ParsedDocument as IWikiContentElement).AppendContent(paragraph);
        }

        paragraph = null;
    }

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
            bool hasTrailingTag = false; // ???

            while (tokenizer.Current.Type != TokenType.NewLine)
            {
                tokenizer.MoveNext();
                element.Add(ParseGenericElement());

                if(tokenizer.Current.Type == TokenType.TemplateEnd)
                {
                    hasTrailingTag = true;
                    break;
                }
            }

            if(hasTrailingTag) { break; }

            tokenizer.MoveNext();

            if(tokenizer.AtEmptyLine) { break; }
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
        templateElem.Name = tokenizer.Current.Text;

        tokenizer.MoveNext();

        // Подстановки шаблона
        while (tokenizer.Current.Type != TokenType.TemplateEnd)
        {
            if (tokenizer.Current.Type == TokenType.Bar)
            {
                templateElem.StartNewSubstitution();
            }
            else if(tokenizer.AtEmptyLine)
            {
                break;
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