using System.Collections.Frozen;

namespace FindFilesViaWindowsSearch.Data.Models
{

    /// <summary>
    /// Immutable configuration class for file processing operations.
    /// Contains paths and file extension filters that remain constant throughout the application lifecycle.
    /// </summary>
    public sealed class FileProcessingConfigModel
    {
        /// <summary>
        /// Gets the collection of file extensions to exclude from processing.
        /// </summary>
        public FrozenSet<string> ExcludedExtensions { get; }

        /// <summary>
        /// Gets the root folder path where files will be searched.
        /// </summary>
        public string SearchFolder { get; }

        /// <summary>
        /// Gets the folder name for files that were successfully found/processed.
        /// </summary>
        public string FoundFolder { get; }

        public string MatchedFolder { get; }

        /// <summary>
        /// Gets the folder name for files that were not found or failed processing.
        /// </summary>
        public string NotFoundFolder { get; }

        public string ReportsFullFile { get; }

        public bool MoveIfFound { get; set; } = false;
        public bool MoveIfNotFound { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the FileProcessingConfig class.
        /// </summary>
        /// <param name="excludedExtensions">File extensions to exclude from processing</param>
        /// <param name="searchFolder">Root folder path for file searching</param>
        /// <param name="foundFolder">Folder name for successfully processed files</param>
        /// <param name="notFoundFolder">Folder name for failed or missing files</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
        /// <exception cref="ArgumentException">Thrown when string parameters are empty or whitespace</exception>
        public FileProcessingConfigModel(
            IEnumerable<string> excludedExtensions,
            string searchFolder,
            string reportsFolder,
            string foundFolder = "FoundFiles",
            string matchedFolder = "MatchFiles",
            string notFoundFolder = "NotFoundFiles")
        {
            ArgumentNullException.ThrowIfNull(excludedExtensions);
            ArgumentException.ThrowIfNullOrWhiteSpace(searchFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(foundFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(notFoundFolder);
            ArgumentException.ThrowIfNullOrWhiteSpace(matchedFolder);

            CheckFolderAndCreate(Path.Combine(searchFolder, foundFolder));
            CheckFolderAndCreate(Path.Combine(searchFolder, notFoundFolder));
            CheckFolderAndCreate(Path.Combine(searchFolder, matchedFolder));

            ExcludedExtensions = excludedExtensions.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
            SearchFolder = searchFolder;
            FoundFolder = foundFolder;
            NotFoundFolder = notFoundFolder;
            MatchedFolder = matchedFolder;
            ReportsFullFile = reportsFolder;
        }

        private void CheckFolderAndCreate(string folderlocation)
        {
            if (!Directory.Exists(folderlocation))
            {
                Directory.CreateDirectory(folderlocation);
            }
        }

        /// <summary>
        /// Creates a default configuration instance with commonly excluded file extensions.
        /// </summary>
        /// <param name="searchFolder">Root folder path for file searching</param>
        /// <returns>A new FileProcessingConfig instance with default settings</returns>
        public static FileProcessingConfigModel CreateDefault(string searchFolder)
        {
            var defaultExclusions = new[] { ".ini", ".exe", ".zip" };
            return new FileProcessingConfigModel(defaultExclusions, searchFolder);
        }

    }
}