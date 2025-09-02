using System;
using System.Threading.Tasks;
using FileScannerAPI.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileScannerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScannerController : ControllerBase
    {
        private readonly ILogger _logger;

        private readonly IFileHandler _fileHandler;

        private readonly FileScanSettings _fileScanSettings;

        public ScannerController(IFileHandler fileHandler, ILogger<ScannerController> logger, IOptions<FileScanSettings> fileScanSettings)
        {
            _fileHandler = fileHandler;
            _logger = logger;
            _fileScanSettings = fileScanSettings.Value;
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            return Ok("Service is running.");
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            var scanResponse = new ScanResultModel();

            // Check Token
            if ((Request.Headers.Keys.Count > 0 && !Request.Headers.ContainsKey("servicetoken")) ||
                !Request.Headers["servicetoken"][0].Equals(_fileScanSettings.ServiceToken))
            {
                _logger.LogInformation(new Exception("Document Scan processing failed"), $"No Service Token: {file.FileName}");
                scanResponse.Output = "Document processing failed";
                return StatusCode(500, scanResponse);
            }

            //Validate file
            if (file == null)
            {
                _logger.LogInformation(new NullReferenceException("Document has no content"), $"Scan file {file.FileName} is null");
                scanResponse.Output = "Document has no content";
                return StatusCode(500, scanResponse);
            }

            //Genarate temp file name
            string tempFile = _fileHandler.GetTempFileFullPath();

            //Create file
            if (!await _fileHandler.CreateFileAsync(file, tempFile))
            {
                System.IO.File.Delete(tempFile);
                _logger.LogInformation(new Exception("Document Scan process failed"), string.Format("Failed to create temp file {0}", tempFile));
                scanResponse.Output = $"Document process failed";
                return StatusCode(500, scanResponse);
            }

            scanResponse.IsScanSuccessfull = _fileHandler.ScanFile(tempFile, out bool isClean, out string output);
            scanResponse.IsClean = isClean;
            scanResponse.Output = output;

            if (!scanResponse.IsScanSuccessfull)
            {
                System.IO.File.Delete(tempFile);
                _logger.LogInformation(new Exception("Document Scan process failed"), string.Format("Failed to scan file {0}: {1}", file.FileName, scanResponse.Output));
                return StatusCode(500, scanResponse);
            }
            else
            {
                System.IO.File.Delete(tempFile);
                if(!scanResponse.IsClean)
                {
                    _logger.LogInformation(new Exception("Document Scan process failed"), string.Format("Virus detected {0}: {1}", file.FileName, scanResponse.Output));
                }
                return Ok(scanResponse);
            }
        }
    }
}
