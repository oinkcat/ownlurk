using System;
using System.IO;
using System.Text;
using WikiReader;

const string TestDataDir = @"../data";
const string TestDataPath = @$"{TestDataDir}/test_wiki.txt";
const string TestOutFile = @$"{TestDataDir}/test_text.txt";

string testArticleText = File.ReadAllText(TestDataPath);

using var outWriter = new StreamWriter(TestOutFile);

var textBuffer = new StringBuilder();

foreach(var tokenInfo in new WikiTokenizer(testArticleText))
{
    var (token, value) = tokenInfo.Value;
    Console.WriteLine($"{token}: {value}");

    if(token == WikiToken.Text)
    {
        textBuffer.Append(value);
    }
    else if(token == WikiToken.NewLine)
    {
        outWriter.WriteLine(textBuffer.ToString());
        textBuffer.Clear();
    }
}

outWriter.Write(textBuffer.ToString());