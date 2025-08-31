namespace FindFilesViaWindowsSearch.Data.Models
{
    public class AllSearchResults
    {
        public string searchTerm { get; set; } = string.Empty;
        public long Size { get; set; } = 0;
        public long SizeOnDisk { get; set; } = 0;

        public List<SearchResultModel> results { get; set; } = new List<SearchResultModel>();
    }
}
