


using FindFilesViaWindowsSearch.Configuration;
using FindFilesViaWindowsSearch.Data.Models;
using FindFilesViaWindowsSearch.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

Console.WriteLine("Initializing Windows Search Service...");
WindowsSearchService _WindowsSearchService = new WindowsSearchService();

//Need to move this or create a function for tha the main file processing.
// Configuration
Console.WriteLine("Loading configuration...");
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var services = new ServiceCollection();
services.AddFileProcessingConfig(configuration);
var serviceProvider = services.BuildServiceProvider();

// Get the configuration
FileProcessingConfigModel _fileProcessingConfig = serviceProvider.GetRequiredService<FileProcessingConfigModel>();


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

Console.WriteLine($"\nProcessing {FileCounterModel.TotalFilesCount} files...");

foreach (string file in FileList.Take(50))
{
    FileCounterModel.ProcessedFilesCount++;
    Console.WriteLine($"\n[{FileCounterModel.ProcessedFilesCount}/{FileCounterModel.TotalFilesCount}] Processing file: {file}");
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
            bool HasMoved = false;
            result.SizeOnDisk = SizeOnDisk.GetSizeOnDisk(result.FullPath);

            //If the file has been moved and there are more then one file in the windows search. I still want to get the file size of the file in the next list just in case.
            //Therefore, i need to skip the rest of the process. 
            if (HasMoved)
            {
                continue;
            }

            if (SourceSizeOnDisk == result.SizeOnDisk) //If Source and search Size On Disk bytes match.  Very likly its the same item. 
            {
                result.IsSameSizeOnDisk = true;
                string sourcePath = Path.Combine(_fileProcessingConfig.SearchFolder, file);
                string destPath = Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.MatchedFolder, file);

                Console.WriteLine($"  ✓ Match found! Moving to matched folder...");
                try
                {
                    Console.WriteLine($"Found matching name and Size on Disk the same.  Assuming its the same file name. ");
                    FileCounterModel.MatchedFilesCount++;
                    File.Move(sourcePath, destPath);
                    Console.WriteLine($" Moved to: {_fileProcessingConfig.MatchedFolder}\\{file}");
                    HasMoved = true;

                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error moving file: {ex.Message}");
                }

            }
            else if (_fileProcessingConfig.MoveIfFound)//If the file name is found. It will get moved to the found folder but assume its the same file. 
            {
                Console.WriteLine($"Found matching file {file} but can't assume its the same file. Moving to match found. ");
                FileCounterModel.FoundFilesCount++;
                File.Move(Path.Combine(_fileProcessingConfig.SearchFolder, file), Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.FoundFolder, file));
                HasMoved = true;
            }



        }
    }
    else if (_fileProcessingConfig.MoveIfNotFound)
    {
        Console.WriteLine($"Could not find other {file} by that name. ");
        FileCounterModel.NotFoundFilesCount++;
        File.Move(Path.Combine(_fileProcessingConfig.SearchFolder, file), Path.Combine(_fileProcessingConfig.SearchFolder, _fileProcessingConfig.NotFoundFolder, file));
    }
    allSearchResults.Add(new AllSearchResults { searchTerm = file, results = WinodwsSearchResults, Size = SourceSize, SizeOnDisk = SourceSizeOnDisk });
}


// Generate and save results
Console.WriteLine("\nGenerating report...");
string jsonString = JsonSerializer.Serialize(allSearchResults, new JsonSerializerOptions { WriteIndented = true });


File.WriteAllText(_fileProcessingConfig.ReportsFullFile, jsonString);

// Summary
Console.WriteLine("\n=== Processing Complete ===");
Console.WriteLine($"Total files processed: {allSearchResults.Count}, found files {FileCounterModel.FoundFilesCount} and matched where {FileCounterModel.MatchedFilesCount} and not files linked {FileCounterModel.NotFoundFilesCount}");
Console.WriteLine($"Results saved to: {_fileProcessingConfig.ReportsFullFile}");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();

