#define DEBUG


using System.Text;
using System.Text.RegularExpressions;

using System.Net;



const string OutputDirectory = "D:/MiYouShe Emotion";
const string DataDirectory = "Data.txt";


var Sets = new List<string>();
{
    var loaded = File.ReadAllText(DataDirectory);
    var splits = loaded.Split('\n');

    foreach (var split in splits)
    {
        var value = split.Trim();

        if (value.Length == 0)
            continue;

        Sets.Add(value);
    }

#if DEBUG
    Console.WriteLine($"Count: {Sets.Count}");
#endif
}


var GetFileExtension = (string path) =>
{
    var begin = path.LastIndexOf('.');
    begin = Math.Max(begin, 0);

    return path.Substring(begin);
};


int totalEmotions = 0;
var Extract = (string path, string data) =>
{
    var client = new WebClient();

    var divContainerRE = new Regex("<div title.+?</div>");
    foreach (Match match in divContainerRE.Matches(data))
    {
        if (!match.Success)
            continue;

        var value = match.Value;

        var titleRE = new Regex("title=.+?\"");
        var urlRE = new Regex("https:.+?png");
        var urlREFallback = new Regex("https:.+?PNG");

        var title = "";
        {
            var result = titleRE.Match(value);

            var begin = result.Value.IndexOf('\"') + 1;
            var end = result.Value.LastIndexOf('\"');

            title = result.Value.Substring(begin, end - begin);
        }

        var url = "";
        {
            var result = urlRE.Match(value);
            if(!result.Success)
                result = urlREFallback.Match(value);

            url = result.Value;
        }

#if DEBUG
        Console.WriteLine($"title: {title} | url: {url}");
#endif

        {
            totalEmotions++;

            var filename = Path.Combine(path, title + GetFileExtension(url));
            if (File.Exists(filename))
                continue;

            client.DownloadFile(url, filename);

#if DEBUG
            Console.WriteLine(filename);
#endif
        }
    }
};


int iteration = 0;
foreach(var set in Sets)
{
    var target = Path.Combine(OutputDirectory, $"Set{++iteration}");
    if(!Directory.Exists(target)) Directory.CreateDirectory(target);

    Extract(target, set);
}

#if DEBUG
Console.WriteLine($"Total Emotions: {totalEmotions}");
#endif