using System;
using System.IO;
using System.Text;
using WikiReader;
using WikiReader.Dom;

const string TestDataDir = @"../data";
const string TestDataPath = @$"{TestDataDir}/test_wiki.txt";
const string TestOutFile = @$"{TestDataDir}/test_text.txt";

string testArticleText = File.ReadAllText(TestDataPath);

var wikiPageParser = new WikiParser(testArticleText);
wikiPageParser.Parse();

using var outWriter = new StreamWriter(TestOutFile);
var htmlGenerator = new HtmlGenerationVisitor(outWriter);

foreach(var contentElement in wikiPageParser.ParsedDocument.Content)
{
    contentElement.AcceptHtmlGenerationVisitor(htmlGenerator);
}

Console.WriteLine("Converted");