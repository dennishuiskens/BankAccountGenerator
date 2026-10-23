using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// Downloads the Norwegian BIC/IBAN table from bits.no and rewrites the web project's data/banks.json.
// Optional argument: path to a local xlsx file to use instead of downloading.

const string SourceUrl = "https://bits.no/document/iban/";
XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

var outputPath = Path.Combine(FindSolutionDirectory(), "src", "BankAccountGenerator", "wwwroot", "data", "banks.json");

byte[] xlsx;
if (args.Length > 0)
{
    xlsx = await File.ReadAllBytesAsync(args[0]);
}
else
{
    Console.WriteLine($"Downloading {SourceUrl}");
    using var http = new HttpClient();
    xlsx = await http.GetByteArrayAsync(SourceUrl);
}

using var zip = new ZipArchive(new MemoryStream(xlsx));
var sharedStrings = XDocument.Load(zip.GetEntry("xl/sharedStrings.xml")!.Open())
    .Descendants(ns + "si")
    .Select(si => string.Concat(si.Descendants(ns + "t").Select(t => t.Value)))
    .ToList();
var rows = XDocument.Load(zip.GetEntry("xl/worksheets/sheet1.xml")!.Open())
    .Descendants(ns + "row")
    .Skip(1);

var banks = new List<BankEntry>();
foreach (var row in rows)
{
    var cells = new Dictionary<string, string>();
    foreach (var cell in row.Elements(ns + "c"))
    {
        var value = cell.Element(ns + "v")?.Value;
        if (value is null)
        {
            continue;
        }

        var column = Regex.Match((string)cell.Attribute("r")!, "^[A-Z]+").Value;
        cells[column] = (string?)cell.Attribute("t") == "s" ? sharedStrings[int.Parse(value)] : value;
    }

    var code = cells.GetValueOrDefault("A", "").Trim();
    if (Regex.IsMatch(code, @"^\d{4}$"))
    {
        banks.Add(new BankEntry(code, cells.GetValueOrDefault("B", "").Trim(), cells.GetValueOrDefault("C", "").Trim()));
    }
}

// Rows without BIC and name belong to the same bank as the row above.
for (var i = 1; i < banks.Count; i++)
{
    if (banks[i].Name.Length == 0 && banks[i].Bic.Length == 0)
    {
        banks[i] = banks[i] with { Bic = banks[i - 1].Bic, Name = banks[i - 1].Name };
    }
}

if (banks.Count == 0)
{
    Console.Error.WriteLine("No bank identifiers found; the file layout may have changed. banks.json was not modified.");
    return 1;
}

Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
var options = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};
await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(banks, options));
Console.WriteLine($"Wrote {banks.Count} bank identifiers ({banks.Select(b => b.Name).Distinct().Count()} banks) to {outputPath}");
return 0;

static string FindSolutionDirectory()
{
    var directory = new DirectoryInfo(AppContext.BaseDirectory);
    while (directory is not null && !directory.GetFiles("*.sln").Any())
    {
        directory = directory.Parent;
    }

    return directory?.FullName ?? throw new InvalidOperationException("Could not find the solution directory.");
}

internal sealed record BankEntry(string Code, string Bic, string Name);
