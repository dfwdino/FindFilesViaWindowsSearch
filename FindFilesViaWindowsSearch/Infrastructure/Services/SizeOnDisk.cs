using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FindFilesViaWindowsSearch.Infrastructure.Services
{

    public class SizeOnDisk
    {
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern bool GetDiskFreeSpace(string lpRootPathName,
         out uint lpSectorsPerCluster,
         out uint lpBytesPerSector,
         out uint lpNumberOfFreeClusters,
         out uint lpTotalNumberOfClusters);

        public static long GetSizeOnDisk(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
                throw new FileNotFoundException("File not found", filePath);

            // Get the drive's cluster size
            string drive = Path.GetPathRoot(filePath);
            uint sectorsPerCluster, bytesPerSector, freeClusters, totalClusters;

            if (!GetDiskFreeSpace(drive, out sectorsPerCluster, out bytesPerSector,
                                  out freeClusters, out totalClusters))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            uint clusterSize = sectorsPerCluster * bytesPerSector;
            long fileSize = fileInfo.Length;

            // Calculate how many clusters the file occupies
            long clustersNeeded = (fileSize + clusterSize - 1) / clusterSize;
            return clustersNeeded * clusterSize;
        }
    }
}
