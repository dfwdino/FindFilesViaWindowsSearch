namespace FindFilesViaWindowsSearch.Data.Models
{
    public class FileCounterModel
    {
        public static int TotalFilesCount { get; set; }
        public static int ProcessedFilesCount { get; set; }
        public static int MatchedFilesCount { get; set; }
        public static int FoundFilesCount { get; set; }
        public static int NotFoundFilesCount { get; set; }

    }
}
