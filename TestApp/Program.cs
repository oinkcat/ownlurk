using System;
using System.IO;
using WikiReader;

const string TestDataDir = $@"../../../../data";
const string TestDataPath = @$"{TestDataDir}/test_wiki.txt";
const string TestOutFile = @$"{TestDataDir}/test_text.html";

const string TemplatePath = $@"{TestDataDir}/template.html";

string testArticleText = File.ReadAllText(TestDataPath);

var wikiPageParser = new WikiParser(testArticleText);
wikiPageParser.Parse();

using var outWriter = new StreamWriter(TestOutFile);
var htmlGenerator = new HtmlGenerator(wikiPageParser.ParsedDocument, TemplatePath);
htmlGenerator.Generate(outWriter);

Console.WriteLine("Converted");