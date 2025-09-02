using FindFilesViaWindowsSearch.Data.Models;
using System.Data.OleDb;

namespace FindFilesViaWindowsSearch.Infrastructure.Services
{
    public class WindowsSearchService
    {
        public async Task<List<SearchResultModel>> SearchFilesAsync(string searchTerm, string fileType = "*")
        {
            var results = new List<SearchResultModel>();
            var connectionString = "Provider=Search.CollatorDSO.1;Extended?Properties='Application=Windows';";
            var driveLetter = @"F:\";

            var query = $@"
               SELECT System.ItemName, System.ItemPathDisplay, System.Size, System.DateModified
                FROM SystemIndex 
                WHERE System.FileName like '{searchTerm}%' 
                AND scope='file:'";

            if (fileType != "*")
            {
                query += $" AND System.FileExtension = '.{fileType}'";
            }

            //query += $" AND System.ItemPathDisplay LIKE '{driveLetter}%'"; //Why did this stop working....

            try
            {
                using var connection = new OleDbConnection(connectionString);
                connection.Open();

                using var command = new OleDbCommand(query, connection);
                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    if (reader["System.ItemPathDisplay"].ToString().StartsWith(driveLetter, StringComparison.OrdinalIgnoreCase)) //Not sure what happened but I had to add this logic in to fix the SQL not puling Drive letter only. 
                    {
                        results.Add(new SearchResultModel
                        {
                            FileName = reader["System.ItemName"]?.ToString() ?? "",
                            FullPath = reader["System.ItemPathDisplay"]?.ToString() ?? "",
                            Size = Convert.ToInt64(reader["System.Size"] ?? 0),
                            Modified = Convert.ToDateTime(reader["System.DateModified"])
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle search service unavailable or other errors
                throw new InvalidOperationException("Windows Search service unavailable", ex);
            }

            return results;
        }
    }
}
