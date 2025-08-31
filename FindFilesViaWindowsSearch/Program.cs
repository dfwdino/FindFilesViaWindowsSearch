


using FindFilesViaWindowsSearch.Data.Models;
using FindFilesViaWindowsSearch.Infrastructure.Services;
using System.Text.Json;

Console.WriteLine("Initializing Windows Search Service...");
WindowsSearchService _WindowsSearchService = new WindowsSearchService();

// Configuration
Console.WriteLine("Loading configuration...");
FileProcessingConfigModel _fileProcessingConfig = new FileProcessingConfigModel(
    excludedExtensions: new[] { ".ini", ".exe", ".zip", ".pdf" },
    searchFolder: @"C:\Users\Shane\Desktop\Temp Photos\",
    reportsFolder: @"C:\Temp\myList.json"
);

_fileProcessingConfig.MoveIfFound = true;
Console.WriteLine($"Searching in: {_fileProcessingConfig.SearchFolder}");
Console.WriteLine($"Excluding files with extensions: {string.Join(", ", _fileProcessingConfig.ExcludedExtensions)}");



Console.WriteLine("\nScanning directory for files...");
var FileList = Directory.GetFiles(_fileProcessingConfig.SearchFolder, "*", SearchOption.AllDirectories)
                        .Select(Path.GetFileName)
                        .Where(file => !_fileProcessingConfig.ExcludedExtensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
Console.WriteLine($"Found {FileList.Count()} files to process.");


if (!FileList.Any())
{
    Console.WriteLine("No files found in the specified directory.");
    return;
}

List<AllSearchResults> allSearchResults = new();

int totalFiles = Math.Min(FileList.Count(), 5);
int processedFiles = 0;
Console.WriteLine($"\nProcessing {totalFiles} files...");

foreach (string file in FileList.Take(5))
{
    processedFiles++;
    Console.WriteLine($"\n[{processedFiles}/{totalFiles}] Processing file: {file}");
    Console.Write("Searching for matching files... ");
    var WinodwsSearchResults = await _WindowsSearchService.SearchFilesAsync(file, "*");
    Console.WriteLine($"Found {WinodwsSearchResults.Count} potential matches");
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
                string sourcePath = Path.Combine(_fileProcessingConfig.SearchFolder, file);
                string destPath = Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.MatchedFolder, file);

                Console.WriteLine($"  ✓ Match found! Moving to matched folder...");
                try
                {
                    File.Move(sourcePath, destPath);
                    Console.WriteLine($" Moved to: {_fileProcessingConfig.MatchedFolder}\\{file}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error moving file: {ex.Message}");
                }
            }

            if (_fileProcessingConfig.MoveIfFound)
            {
                File.Move(Path.Combine(_fileProcessingConfig.SearchFolder, file), Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.FoundFolder, file));
            }

        }

    }

    allSearchResults.Add(new AllSearchResults { searchTerm = file, results = WinodwsSearchResults, Size = SourceSize, SizeOnDisk = SourceSizeOnDisk });
}


// Generate and save results
Console.WriteLine("\nGenerating report...");
string jsonString = JsonSerializer.Serialize(allSearchResults, new JsonSerializerOptions { WriteIndented = true });


File.WriteAllText(_fileProcessingConfig.ReportsFullFile, jsonString);

// Summary
Console.WriteLine("\n=== Processing Complete ===");
Console.WriteLine($"Total files processed: {allSearchResults.Count}");
Console.WriteLine($"Results saved to: {_fileProcessingConfig.ReportsFullFile}");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

