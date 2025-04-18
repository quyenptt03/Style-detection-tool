using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using StyleDetectionTool.Models;
using StyleDetectionTool.Services;
using System.Diagnostics;
using System.Text.Json;

namespace StyleDetectionTool.Controllers
{
    public class StyleDetectionController : Controller
    {
        private readonly CrawHTMLService _crawlHTMLService;
        private readonly StyleStructure _style;
        private readonly ApiService _apiService;
        private readonly StyleCheckingService _styleCheckingService;

        public StyleDetectionController(CrawHTMLService crawlHTMLService, StyleStructure styleStructure, ApiService apiService, StyleCheckingService styleCheckingService)
        {
            _crawlHTMLService = crawlHTMLService;
            _style = styleStructure;
            _apiService = apiService;
            _styleCheckingService = styleCheckingService;
        }

        [HttpGet("call-external")]
        public async Task<IActionResult> CallExternalApi(string url)
        {
            List<string> result = await _apiService.GetDataFromApiAsync(url);
            return Ok(result);
        }

        [HttpPost("upload-json")]
        public async Task<IActionResult> UploadStyleJson(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var reader = new StreamReader(file.OpenReadStream());
            string jsonContent = await reader.ReadToEndAsync();

            var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

            var result = _style.Handle(jsonData);

            return Ok(result);
        }
        [HttpGet("crawl-html")]
        public async Task<IActionResult> CrawlHtml(string url)
        {
           
            var stopwatch = Stopwatch.StartNew();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            try
            {
                List<Element> elements = await _crawlHTMLService.CrawlSinglePage(browser, url);
                stopwatch.Stop();
                return Ok(new
                {
                    durationInMilliseconds = stopwatch.ElapsedMilliseconds,
                    elements
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("crawl-multi-page")]
        public async Task<IActionResult> CrawlHtmlMultiPage(string url)
        {

            var stopwatch = Stopwatch.StartNew();
            try
            {
                List<string> urlList = await _apiService.GetDataFromApiAsync(url);
             

                List<Page> elements = await _crawlHTMLService.CrawlMultiplePage(urlList);
                stopwatch.Stop();
                return Ok(new
                {
                    durationInMilliseconds = stopwatch.ElapsedMilliseconds,
                    elements
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("style-detection")]
        public async Task<IActionResult> StyleDetection(string url, IFormFile file)
        {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var stopwatch = Stopwatch.StartNew();
            if (file == null || file.Length == 0)
                return BadRequest("No theme file uploaded.");

            using var reader = new StreamReader(file.OpenReadStream());
            string jsonContent = await reader.ReadToEndAsync();

            var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

            var themeData = _style.Handle(jsonData);

            try
            {
                List<Element> elements = await _crawlHTMLService.CrawlSinglePage(browser, url);
                var stylesResult = _styleCheckingService.AnalyzeClassUsage(elements, themeData.Styles);
                var colorResult = _styleCheckingService.AnalyzeColorUsage(elements, themeData.Colors);

                stopwatch.Stop();
                return Ok(new
                {
                    durationInMilliseconds = stopwatch.ElapsedMilliseconds,
                    styles = stylesResult,
                    color = colorResult
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
