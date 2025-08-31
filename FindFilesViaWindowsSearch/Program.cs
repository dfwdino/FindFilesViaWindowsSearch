


using FindFilesViaWindowsSearch.Data.Models;
using FindFilesViaWindowsSearch.Infrastructure.Services;
using System.Runtime.InteropServices;
using System.Text.Json;

WindowsSearchService _WindowsSearchService = new WindowsSearchService();

var excludedExtensions = new[] { ".ini", ".exe", ".zip" };
string SearchFolder = @"C:\Users\Shane\Desktop\Temp Photos";

var FileList = Directory.GetFiles(SearchFolder, "*", SearchOption.AllDirectories)
                            .Select(Path.GetFileName)
                            .Where(file => !excludedExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));

[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
static extern bool GetCompressedFileSizeEx(string lpFileName, out long lpFileSize);


if (!FileList.Any())
{
    Console.WriteLine("No files found in the specified directory.");
    return;
}

List<AllSearchResults> allSearchResults = new();

foreach (string file in FileList.Take(2))
{
    var results = await _WindowsSearchService.SearchFilesAsync(file, "*");
    long size = 0;
    long sourceSizeOnDisk = 0;

    if (results.Any())
    {
        if (GetCompressedFileSizeEx(Path.Combine(SearchFolder, file), out long sizeOnDisk))
        {
            sourceSizeOnDisk = sizeOnDisk;
        }

        size = new FileInfo(Path.Combine(SearchFolder, file)).Length;
    }

    allSearchResults.Add(new AllSearchResults { searchTerm = file, results = results, Size = size, SizeOnDisk = sourceSizeOnDisk });
}


string jsonString = JsonSerializer.Serialize(allSearchResults, new JsonSerializerOptions { WriteIndented = true });

//File.Create(@"c:\tmep\myList.json");
// Write JSON string to a file
File.WriteAllText(@"c:\temp\myList.json", jsonString);

Console.WriteLine("List exported to myList.json successfully!");

Console.WriteLine(allSearchResults.Count);



