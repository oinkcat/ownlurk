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
            ParsedDocument.AppendContent(parsedElement);
        }
    }

    private WikiElement ParseGenericElement()
    {
        var token = tokenizer.Current;

        if(tokenizer.Current.IsText)
        {
            return new WikiTextElement(token.text);
        }
        else if(token.Type == TokenType.LinkStart)
        {
            return ParseLinkElement();
        }
        else if(token.Type == TokenType.NewLine)
        {
            return new WikiEolElement();
        }
        else
        {
            return null;
        }
    }

    private WikiLinkElement ParseLinkElement()
    {
        tokenizer.MoveNext();

        if(tokenizer.Current.IsText)
        {
            var linkElem = new WikiLinkElement
            {
                Uri = new WikiTextElement(tokenizer.Current.Text)
            };

            while(tokenizer.Current.Type != TokenType.LinkEnd)
            {
                tokenizer.MoveNext();

                var linkContentElem = ParseGenericElement();

                if(linkContentElem != null)
                {
                    linkElem.Content.Add(linkContentElem);
                }
            }

            return linkElem;
        }
        else
        {
            return null;
        }
    }
}