using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UploadSampleApp.Models;

namespace UploadSampleApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _logger;

        private readonly ScanServiceSettings _settings;

        public HomeController(IOptions<ScanServiceSettings> options, ILogger<HomeController> logger)
        {
            _logger = logger;
            _settings = options.Value;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(IList<IFormFile> files)
        {
            IFormFile file = files[0];

            using (var client = new HttpClient())
            {
                try
                {
                    client.BaseAddress = new Uri(_settings.ScanServiceURL);

                    byte[] data;
                    using (var br = new BinaryReader(file.OpenReadStream()))
                        data = br.ReadBytes((int)file.OpenReadStream().Length);

                    ByteArrayContent bytes = new ByteArrayContent(data);


                    MultipartFormDataContent multiContent = new MultipartFormDataContent();

                    multiContent.Headers.Add("servicetoken", _settings.ServiceToken);
                    multiContent.Add(bytes, "file", file.FileName);

                    var result = client.PostAsync(_settings.ScanServicePath, multiContent).Result;

                    string status = "Status: " + result.StatusCode.ToString() + Environment.NewLine;
                    string requestedUrl = "Requested Url: " + result.RequestMessage.RequestUri.ToString() + Environment.NewLine;
                    string requestHeaders = "Request Headers:" + Environment.NewLine + result.RequestMessage.Headers.ToString();
                    string responseHeaders = "Response Headers:" + Environment.NewLine + result.Headers.ToString();
                    string responseBody = "Response Body:" + Environment.NewLine + await result.Content.ReadAsStringAsync();

                    string virusOutput = status + requestedUrl + requestHeaders + Environment.NewLine + responseHeaders + Environment.NewLine + responseBody;

                    return Ok(virusOutput);

                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Failed to call the service: " + ex.Message); // 500 is generic server error
                }
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
