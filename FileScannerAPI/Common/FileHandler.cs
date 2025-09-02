using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace FileScannerAPI.Common
{
    public class FileHandler : IFileHandler
    {
        private readonly ILogger _logger;

        private readonly FileScanSettings _settings;

        public FileHandler(IOptions<FileScanSettings> options, ILogger<FileHandler> logger)
        {
            _logger = logger;
            _settings = options.Value;
        }

        /// <summary>
        /// Write file in disk
        /// </summary>
        /// <param name="file">file data</param>
        /// <returns>boolean value if process completed or not</returns>
        public async Task<bool> CreateFileAsync(IFormFile file, string filename)
        {
            if (file.Length > 0)
            {
                using (var fileStream = new FileStream(filename, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Scan a file using windows defender
        /// </summary>
        /// <param name="filename">full file path</param>
        /// <param name="isClean">returns true if is clean false if not </param>
        /// <param name="output">return scan results from the antivirus</param>
        /// <returns></returns>
        public bool ScanFile(string filename, out bool isClean, out string output)
        {
            Process process = new Process();
            process.StartInfo.FileName = _settings.DefenderFullPath;
            process.StartInfo.Arguments = string.Format(_settings.Arguments, filename);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();

            //* Read the output
            output = process.StandardOutput.ReadToEnd();
            isClean = false;
            if (!string.IsNullOrEmpty(output))
            {
                if(output.ToLower().Contains(_settings.CleanScannerMessage))
                {
                    isClean = true;
                }
            }
            
            //Read if there an error;
            string err = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (string.IsNullOrEmpty(err))
            {
                return true;
            }

            _logger.LogError("Error Scanning file: " + err);
            output = err;
            return false;
        }

        /// <summary>
        /// Create a unique filename using a guid
        /// </summary>
        /// <returns>returns the filename</returns>
        public string GetTempFileFullPath()
        {
            string filename = Guid.NewGuid().ToString() + ".tmp";
            return Path.Combine(_settings.UploadPath, filename);
        }
    }
}
