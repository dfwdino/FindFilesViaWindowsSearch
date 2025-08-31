
namespace FindFilesViaWindowsSearch.Data.Models
{
    public record SearchResultModel
    {
        public string FileName { get; init; } = "";
        public string FullPath { get; init; } = "";
        public long Size { get; init; } = 0;

        public long SizeOnDisk { get; set; } = 0;

        public bool IsSameSizeOnDisk { get; set; } = false;

        public DateTime Modified { get; init; }
    }
}
