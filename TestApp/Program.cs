using System;
using System.IO;
using System.Threading.Tasks;
using WikiReader;
using WikiReader.Toc;
using WikiReader.Bundle;

const string TestDataDir = $"../../../../data";
const string TestBundlePath = $"{TestDataDir}/lurk_data.zip";
const string TestArticleName = "Виталик";

const string TestOutDir = $"{TestDataDir}/out";
const string TestOutFile = $"{TestOutDir}/test_text.html";
const string TestOutLinksFile = $"{TestOutDir}/all_links.txt";

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

async static Task TestParseAllArticlesAndExtractLinks(ContentBundle bundle)
{
    Console.WriteLine("Parsing all articles and link extraction test");

    using var linksOutStream = File.CreateText(TestOutLinksFile);

    var toc = new TableOfContents();
    await toc.LoadFromStream(bundle.GetTocStream());

    foreach(int articleId in bundle.GetExistingArticleIds())
    {
        Console.Write($"{articleId} - ");

        try
        {
            using var reader = new StreamReader(bundle.GetArticleStream(articleId));
            string testArticleText = await reader.ReadToEndAsync();

            var wikiPageParser = new WikiParser(testArticleText);
            wikiPageParser.Parse();

            string articleName = toc.GetArticleNameById(articleId) ?? "-";
            linksOutStream.WriteLine("* {0}", articleName);

            foreach(var linkElem in wikiPageParser.Links)
            {
                if(linkElem is null) { continue; }

                linksOutStream.WriteLine(linkElem.Uri);
            }

            Console.WriteLine($"OK, # of links: {wikiPageParser.Links.Count}");
        }
        catch(Exception e)
        {
            Console.Write("FAIL - ");
            Console.WriteLine(e.Message);
        }
    }
}

if(!Directory.Exists(TestOutDir))
{
    Directory.CreateDirectory(TestOutDir);
}

using var lurkDataBundle = new ContentBundle(TestBundlePath);

await TestRenderPage(TestArticleName, lurkDataBundle);
await TestParseAllArticlesAndExtractLinks(lurkDataBundle);