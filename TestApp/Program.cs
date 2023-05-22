using System;
using System.IO;
using System.Text;
using WikiReader;
using WikiReader.Dom;

const string TestDataDir = @"../data";
const string TestDataPath = @$"{TestDataDir}/test_wiki.txt";
const string TestOutFile = @$"{TestDataDir}/test_text.txt";

using var outWriter = new StreamWriter(TestOutFile);

string testArticleText = File.ReadAllText(TestDataPath);

var wikiPageParser = new WikiParser(testArticleText);
wikiPageParser.Parse();

var textBuffer = new StringBuilder();

foreach(var element in wikiPageParser.ParsedDocument.Contents)
{
    Console.WriteLine(element);

    if(element is WikiTextElement textElem)
    {
        textBuffer.Append(textElem.Text);
    }
    else if(element is WikiEolElement)
    {
        outWriter.WriteLine(textBuffer.ToString());
        textBuffer.Clear();
    }
}