using Xunit;
using WikiReader.Dom;

namespace WikiReader.Tests;

/// <summary>
/// Тестирование разбора разметки
/// </summary>
public class ParserTests
{
    private const string SimpleString = "'''Hello''' ''World''!";

    private const string NestedString = "'''Hello, ''lol'', World!'''";

    private const string DoubleOpenString = "'''''Hello'' World!'''";

    private const string DoubleClosingString = "''Hello '''World!'''''";

    private const string ComplexString = "'''He'''''llo'' ''W'''''o'''''rld''!";

    /// <summary>
    /// Тестирование разбора простых тегов форматирования
    /// </summary>
    [Fact]
    public void TestParsingSimpleFormatTags()
    {
        var parsedSimple = ParseLayoutString(SimpleString);

        Assert.NotEmpty(parsedSimple.Content);
    }

    /// <summary>
    /// Тестирование разбора вложенных тегов
    /// </summary>
    [Fact]
    public void TestParsingNestedFormatTags()
    {
        var parsedNested = ParseLayoutString(NestedString);

        Assert.NotEmpty(parsedNested.Content);
    }

    /// <summary>
    /// Тестирование разбора двойных открывающих тегов
    /// </summary>
    [Fact]
    public void TestParsingDoubleOpenTags()
    {
        var parsedDoubleOpen = ParseLayoutString(DoubleOpenString);

        Assert.NotEmpty(parsedDoubleOpen.Content);
    }

    /// <summary>
    /// Тестирование разбора двойных закрывающих тегов
    /// </summary>
    [Fact]
    public void TestParsingDoubleClosingTags()
    {
        var parsedDoubleClosing = ParseLayoutString(DoubleClosingString);

        Assert.NotEmpty(parsedDoubleClosing.Content);
    }

    /// <summary>
    /// Тестирование разбора нескольких двойных тегов
    /// </summary>
    [Fact]
    public void TestParsingComplexTags()
    {
        var parsedComplex = ParseLayoutString(ComplexString);

        Assert.NotEmpty(parsedComplex.Content);
    }

    private WikiDocument ParseLayoutString(string layout)
    {
        var parser = new WikiParser(layout);
        parser.Parse();
        return parser.ParsedDocument;
    }
}