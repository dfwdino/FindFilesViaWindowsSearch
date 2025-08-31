


using FindFilesViaWindowsSearch.Data.Models;
using FindFilesViaWindowsSearch.Infrastructure.Services;
using System.Text.Json;

WindowsSearchService _WindowsSearchService = new WindowsSearchService();


//Really should be setting this in appsettings.json file.
FileProcessingConfigModel _fileProcessingConfig = new FileProcessingConfigModel(
    excludedExtensions: new[] { ".ini", ".exe", ".zip", ".pdf" },
    searchFolder: @"C:\Users\Shane\Desktop\Temp Photos\"
    );

_fileProcessingConfig.MoveIfFound = true;



var FileList = Directory.GetFiles(_fileProcessingConfig.SearchFolder, "*", SearchOption.AllDirectories)
                            .Select(Path.GetFileName)
                            .Where(file => !_fileProcessingConfig.ExcludedExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));


if (!FileList.Any())
{
    Console.WriteLine("No files found in the specified directory.");
    return;
}

List<AllSearchResults> allSearchResults = new();

foreach (string file in FileList.Take(5))
{
    var WinodwsSearchResults = await _WindowsSearchService.SearchFilesAsync(file, "*");
    long SourceSize = 0;
    long SourceSizeOnDisk = 0;

    if (WinodwsSearchResults.Any())
    {

        SourceSizeOnDisk = SizeOnDisk.GetSizeOnDisk(Path.Combine(_fileProcessingConfig.SearchFolder, file));

        SourceSize = new FileInfo(Path.Combine(_fileProcessingConfig.SearchFolder, file)).Length;

        foreach (var result in WinodwsSearchResults)
        {

            result.SizeOnDisk = SizeOnDisk.GetSizeOnDisk(result.FullPath);

            if (SourceSizeOnDisk == result.SizeOnDisk)
            {
                result.IsSameSizeOnDisk = true;

                File.Move(Path.Combine(_fileProcessingConfig.SearchFolder, file), Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.MatchedFolder, file));
            }

            if (_fileProcessingConfig.MoveIfFound)
            {
                File.Move(Path.Combine(_fileProcessingConfig.SearchFolder, file), Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.FoundFolder, file));
            }

        }

    }

    allSearchResults.Add(new AllSearchResults { searchTerm = file, results = WinodwsSearchResults, Size = SourceSize, SizeOnDisk = SourceSizeOnDisk });
}


string jsonString = JsonSerializer.Serialize(allSearchResults, new JsonSerializerOptions { WriteIndented = true });

File.WriteAllText(@"c:\temp\myList.json", jsonString);

Console.WriteLine("List exported to myList.json successfully!");

Console.WriteLine(allSearchResults.Count);

