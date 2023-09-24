using System;
using System.IO;
using System.Threading.Tasks;
using WikiReader;
using WikiReader.Toc;
using WikiReader.Bundle;

const string TestDataDir = $"../../../../data";
const string TestBundlePath = $"{TestDataDir}/lurk_data.zip";
const string TestOutFile = $"{TestDataDir}/test_text.html";
const string TestArticleName = "Виталик";

const string TemplatePath = $"{TestDataDir}/template.html";

async static Task TestRenderPage(string pageName, ContentBundle bundle)
{
    var table = new TableOfContents();
    await table.LoadFromStream(bundle.GetTocStream());

    Console.WriteLine("Table of contents loaded");
    Console.WriteLine($"Total articles #: {table.TotalArticlesCount}");

    int testArticleId = table.GetArticleIdByName(pageName).Value;

    using var reader = new StreamReader(bundle.GetArticleStream(testArticleId));
    string testArticleText = await reader.ReadToEndAsync();

    var wikiPageParser = new WikiParser(testArticleText);
    wikiPageParser.Parse();

    using var outWriter = new StreamWriter(TestOutFile);
    var htmlGenerator = new HtmlGenerator(wikiPageParser.ParsedDocument, TemplatePath);
    htmlGenerator.Generate(outWriter);

    Console.WriteLine("Converted");
}

async static Task TestParseAllArticles(ContentBundle bundle)
{
    Console.WriteLine("Parsing all articles test");

    foreach(int articleId in bundle.GetExistingArticleIds())
    {
        Console.Write($"{articleId} - ");

        try
        {
            using var reader = new StreamReader(bundle.GetArticleStream(articleId));
            string testArticleText = await reader.ReadToEndAsync();

            var wikiPageParser = new WikiParser(testArticleText);
            wikiPageParser.Parse();

            Console.WriteLine("OK");
        }
        catch(Exception e)
        {
            Console.Write("FAIL - ");
            Console.WriteLine(e.Message);
        }
    }
}

using var lurkDataBundle = new ContentBundle(TestBundlePath);

await TestRenderPage(TestArticleName, lurkDataBundle);
await TestParseAllArticles(lurkDataBundle);