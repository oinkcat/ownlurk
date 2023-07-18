using System;
using System.IO;
using WikiReader;
using WikiReader.Toc;
using WikiReader.Bundle;

const string TestDataDir = $"../../../../data";
const string TestBundlePath = $"{TestDataDir}/lurk_data.zip";
const string TestOutFile = $"{TestDataDir}/test_text.html";
const string TestArticleName = "Медвед";

const string TemplatePath = $"{TestDataDir}/template.html";

using var lurkDataBundle = new ContentBundle(TestBundlePath);

var table = new TableOfContents();
await table.LoadFromStream(lurkDataBundle.GetTocStream());

Console.WriteLine("Table of contents loaded");
Console.WriteLine($"Total articles #: {table.TotalArticlesCount}");

int testArticleId = table.GetArticleIdByName(TestArticleName).Value;

using var reader = new StreamReader(lurkDataBundle.GetArticleStream(testArticleId));
string testArticleText = await reader.ReadToEndAsync();

var wikiPageParser = new WikiParser(testArticleText);
wikiPageParser.Parse();

using var outWriter = new StreamWriter(TestOutFile);
var htmlGenerator = new HtmlGenerator(wikiPageParser.ParsedDocument, TemplatePath);
htmlGenerator.Generate(outWriter);

Console.WriteLine("Converted");