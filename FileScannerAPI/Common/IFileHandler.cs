using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace FileScannerAPI.Common
{
    public interface IFileHandler
    {
        /// <summary>
        /// Write file in disk
        /// </summary>
        /// <param name="file">file data</param>
        /// <returns>boolean value if process completed or not</returns>
        Task<bool> CreateFileAsync(IFormFile file, string filename);

        /// <summary>
        /// Scan a file using windows defender
        /// </summary>
        /// <param name="filename">full file path</param>
        /// <param name="isClean">returns true if is clean false if not </param>
        /// <param name="output">return scan results from the antivirus</param>
        /// <returns></returns>
        bool ScanFile(string filename, out bool isClean, out string output);

        /// <summary>
        /// Create a unique filename using a guid
        /// </summary>
        /// <returns>returns the filename</returns>
        string GetTempFileFullPath();
    }
}
