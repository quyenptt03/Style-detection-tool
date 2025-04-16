using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using StyleDetectionTool.Utils;
using System.Text.Json;

namespace StyleDetectionTool.Controllers;

[ApiController]
[Route("api/style-detection")]
public class AnalyzeController : ControllerBase
{
    [HttpPost("convert-style")]
    public async Task<IActionResult> UploadStyleJson(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        using var reader = new StreamReader(file.OpenReadStream());
        string jsonContent = await reader.ReadToEndAsync();

        // Parse JSON nội dung với System.Text.Json
        var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        // Chuyển đổi dữ liệu
        var result = StyleConverter.ConvertInput(jsonData);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CheckStyles([FromBody] JsonElement request)
    {
        string url = request.GetProperty("url").GetString();

        var paragraphs = request.GetProperty("paragraphs").EnumerateArray().Select(p => p.GetString()).ToList();
        var buttons = request.GetProperty("buttons").EnumerateArray().Select(b => b.GetString()).ToList();

        var colorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(request.GetProperty("colors").ToString());

        var results = new List<object>();

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        try
        {
            await page.GotoAsync(url);

            var allSelectors = new Dictionary<string, List<string>>
            {
                { "paragraph", paragraphs },
                { "button", buttons }
            };

            foreach (var group in allSelectors)
            {
                foreach (var style in group.Value)
                {
                    var selector = $".{style}";
                    var elements = await page.QuerySelectorAllAsync(selector);
                    var usedIn = new List<string>();

                    foreach (var el in elements)
                    {
                        var section = await el.EvaluateAsync<string>(@"
                            (el) => {
                                let current = el;
                                while (current && current !== document.body) {
                                    if (current.className && current.className.toLowerCase().includes('section')) {
                                        return current.className;
                                    }
                                    current = current.parentElement;
                                }
                                return null;
                            }
                        ");

                        if (!string.IsNullOrEmpty(section))
                        {
                            usedIn.Add(section);
                        }
                    }

                    results.Add(new
                    {
                        style = style,
                        category = group.Key,
                        isUsed = usedIn.Any(),
                        usedInSections = usedIn.Distinct().ToList()
                    });
                }
            }

            var propertiesToCheck = new[] { "color", "background-color", "border-color", "outline-color", "fill", "stroke" };

            foreach (var color in colorDict)
            {
                var colorName = color.Key;
                var targetValue = color.Value;

                var jsResult = await page.EvaluateAsync<JsonElement>(@"({ targetValue, propertiesToCheck }) => {
                                const matches = new Set();
                                const elements = document.querySelectorAll('*');

                                elements.forEach(el => {
                                    const styles = getComputedStyle(el);
                                    for (const prop of propertiesToCheck) {
                                        if (styles[prop] && styles[prop].replace(/\s/g, '').toLowerCase() === targetValue.replace(/\s/g, '').toLowerCase()) {
                                            matches.add(el.className || el.tagName);
                                            break;
                                        }
                                    }
                                });

                                return Array.from(matches);
                            }", new { targetValue, propertiesToCheck });

                var usedIn = jsResult.EnumerateArray().Select(x => x.GetString()).ToArray();

                results.Add(new
                {
                    style = colorName,
                    value = targetValue,
                    category = "color",
                    isUsed = usedIn.Any(),
                    usedInSections = usedIn.ToList()
                });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }

        return Ok(results);
    }

}

public class UrlRequest
{
    public string Url { get; set; }
}
