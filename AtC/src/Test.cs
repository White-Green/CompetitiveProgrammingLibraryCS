using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using Extensions;
using Newtonsoft.Json;

public class Test
{
#if DEBUG
    public static void Main(string[] args)
    {
        Console.WriteLine("=====Algorithm test=====");
        var consoleInput = new StringBuilder();
        string buf;
        while (!string.IsNullOrEmpty(buf = Console.ReadLine()))
        {
            if (consoleInput.Length == 0 &&
                Regex.IsMatch(buf, "^https?://atcoder\\.jp/.+$"))
            {
                TestTestCases(SetUpTestCase(buf));
                return;
            }

            consoleInput.Append(buf);
            consoleInput.Append('\n');
        }

        var input = consoleInput.ToString();
        TestSingleTestCase(input);
    }

    private static void TestSingleTestCase(string input)
    {
        var time = Solve(new StringReader(input), out var output);
        Console.Write("/=>");
        Console.Write(output);
        Console.WriteLine("<=/");

        Console.WriteLine("===Algorithm test end===");
        Console.WriteLine("=========Result=========");
        Console.WriteLine($"Time : {time:N0}ms");
    }
#endif

    private static void TestTestCases(IList<TestCase> testCases)
    {
        Console.WriteLine("========AutoTest========");
        var result = new StringBuilder();
        for (var i = 0; i < testCases.Count; i++)
        {
            Console.WriteLine("# TestCase " + testCases[i].No);
            var time = Solve(new StringReader(testCases[i].In), out var output);
            result.Append($"testCase{testCases[i].No}:");
            if (testCases[i].Out != output.ToString())
            {
                result.Append("WA ");
                Console.WriteLine("WA");
                Console.Write("Expected:\n/=>");
                Console.Write(testCases[i].Out);
                Console.WriteLine("<=/");
                Console.Write("But output:\n/=>");
                Console.Write(output.ToString());
                Console.WriteLine("<=/");
            }
            else
            {
                result.Append("AC ");
                Console.Write("AC ");
            }

            result.Append($"{time:N0}ms\n");
            Console.WriteLine($"Time : {time:N0}ms");
            if (i != testCases.Count - 1)
                Console.WriteLine("========================");
        }

        Console.WriteLine("===Algorithm test end===");
        Console.WriteLine("=========Result=========");
        Console.WriteLine(result);
    }

    private static IList<TestCase> SetUpTestCase(string url)
    {
        var fileName = Regex.Replace(url, "[\\\\/:*?\"<>|]", "@") + ".json";
        Console.WriteLine("TestCase:" + fileName);

        IList<TestCase> testCases;
        while (!File.Exists(fileName) || (testCases = JsonConvert.DeserializeObject<IList<TestCase>>(
            new StreamReader(File.OpenRead(fileName)).ReadToEnd()
        )) == null)
            CreateTestCaseFile(url, fileName);
        return testCases;
    }

    private static void CreateTestCaseFile(string url, string fileName)
    {
        Console.WriteLine("Create TestCaseFile");

        using (var file = File.CreateText(fileName))
        {
            IHtmlDocument doc;
            using (var client = new HttpClient())
            using (var stream = client.GetStreamAsync(new Uri(url)).Result)
            {
                var parser = new HtmlParser();
                doc = parser.ParseDocument(stream);
            }


            var testCases = new List<TestCase>();
            var allH3 = doc.QuerySelectorAll("h3")
                .Where(val => Regex.IsMatch(val.InnerHtml, "[入出]力例 ?\\d+"))
                .Select(val => (val.InnerHtml[0] == '入' ? 0 : 1,
                    int.Parse(Regex.Match(val.InnerHtml, "\\d+").Value),
                    val.ParentElement.QuerySelector("pre").InnerHtml))
                .OrderBy(val => val.Item2)
                .ThenBy(val => val.Item1)
                .ToArray();

            for (int i = 0; i * 2 < allH3.Length; i++)
            {
                var input = allH3[2 * i].Item3;
                var output = allH3[2 * i + 1].Item3;
                testCases.Add(new TestCase {No = allH3[2 * i].Item2, In = input, Out = output});
            }

            file.Write(JsonConvert.SerializeObject(testCases));
        }
    }

    private static long Solve(TextReader cin, out TextWriter cout)
    {
        cout = new StringWriter();
        var inputStream = Console.In;
        var outputStream = Console.Out;
        Console.SetIn(cin);
        Console.SetOut(cout);

        var stopwatch = Stopwatch.StartNew();
        new Solver().Solve();
        stopwatch.Stop();
        ConsoleOutputExtension.Flush();

        Console.SetIn(inputStream);
        Console.SetOut(outputStream);
        return stopwatch.ElapsedMilliseconds;
    }
}

class TestCase
{
    public int No { get; set; }
    public string In { get; set; }
    public string Out { get; set; }
}