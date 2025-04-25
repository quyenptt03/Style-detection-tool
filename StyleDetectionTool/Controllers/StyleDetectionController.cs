using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Xml.Linq;
using AngleSharp;
using AngleSharp.Dom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Playwright;
using StyleDetectionTool.Models;
using StyleDetectionTool.Services;
using Polly;

namespace StyleDetectionTool.Controllers;

public class StyleDetectionController(
    ApiService apiService,
    StyleCheckingService styleCheckingService)
    : Controller
{

    [HttpPost("api/style-detection")]
    public async Task<IActionResult> StyleDetection([FromForm] Input input)
    {
        var sw = Stopwatch.StartNew();
        if (String.IsNullOrWhiteSpace(input.Link))
        {
            return BadRequest("No url found");
        }

        if (input.ThemeFile == null || input.ThemeFile.Length == 0)
            return BadRequest("No theme file uploaded.");

        var error = "";
        var urlList = await apiService.GetDataFromApiAsync(input.Link);

        using var reader = new StreamReader(input.ThemeFile.OpenReadStream());
        string jsonContent = await reader.ReadToEndAsync();

        var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);

        var themeConfig = JsonSerializer.Deserialize<ThemeConfig>(jsonContent,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new JsonException("Failed to deserialize JSON to RootObject.");

        var allStyleIds = themeConfig.Buttons.Select(b => b.Id)
            .Concat(themeConfig.Paragraphs.Select(p => p.Id))
            .Concat(themeConfig.ColorNames.Select(c => c))
            .Distinct();

        var styleUsage = allStyleIds.Select(styleId => new StyleUsage
        {
            Name = styleId,
            IsUsed = false,
            InUsed = new List<PageUsage>()
        }).ToList();

        try
        {
            var crawledDocuments = await GetHtmlDocuments(urlList);
            var cancellationTokenSource = new CancellationTokenSource();
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Mock.MaxConcurrency,
                CancellationToken = cancellationTokenSource.Token
            };

            var updatedStyles = new ConcurrentBag<StyleUsage>();
            var firstValidPage = crawledDocuments.FirstOrDefault(doc => doc.Title != "Page Not Found");

            Parallel.ForEach(crawledDocuments, options, crawled =>
            {
                var document = crawled.Document;
                var url = crawled.Url;
                var title = crawled.Title;

                if (title == "Page Not Found")
                    return;


                if (crawled.Url == firstValidPage?.Url)
                {
                    styleCheckingService.ProcessColorsInTheme(themeConfig, crawled, updatedStyles);
                }

                styleCheckingService.ProcessButtons(themeConfig, crawled, updatedStyles);
                styleCheckingService.ProcessParagraphs(themeConfig, crawled, updatedStyles);
                styleCheckingService.ProcessColors(themeConfig, crawled, updatedStyles);

            });

            styleCheckingService.MergeStyleUsage(styleUsage, updatedStyles);
        }
        catch (Exception ex)
        {
            error = ex.Message;
        }

        sw.Stop();
        return Ok(new
        {
            Elapsed = $"{sw.ElapsedMilliseconds / 1000} s",
            error,
            result = styleUsage
        });
    }

    private async Task<List<CrawledDocument>> GetHtmlDocuments(List<string> urls)
    {
        var results = new ConcurrentBag<CrawledDocument>();
        var semaphore = new SemaphoreSlim(Mock.MaxConcurrency);
        using var playwright = await Playwright.CreateAsync();
        await using var browser =
            await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);

        var tasks = urls.Select(async url =>
        {
            await semaphore.WaitAsync();
            try
            {
                var page = await browser.NewPageAsync();
            
                await page.RouteAsync("**/*", async route =>
                {
                    var type = route.Request.ResourceType;
                    if (type is "image" or "font" or "media")
                        await route.AbortAsync();
                    else
                        await route.ContinueAsync();
                });

                var retryGotoPolicy = Policy
             .Handle<TimeoutException>()
             .Or<PlaywrightException>()
             .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(2),
                 (exception, timeSpan, retryCount, context) =>
                 {
                     Console.WriteLine($"[GotoAsync] Retry {retryCount} after {timeSpan.TotalSeconds}s due to: {exception.Message}");
                 });

                await retryGotoPolicy.ExecuteAsync(async () =>
                {
                    await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle,
                        Timeout = 300000
                    });
                });

                var html = await page.ContentAsync();
                var document = await context.OpenAsync(req => req.Content(html));
                var title = document.Title ?? "";

                results.Add(new CrawledDocument
                {
                    Document = document,
                    Url = url,
                    Title = title
                });
            }
            catch (Exception ex)
            {
                $"Error crawling {url}: {ex.Message}".WriteLog();
            }
            finally
            {
                semaphore.Release();
            }
        });

        await Task.WhenAll(tasks);
        return results.ToList();
    }

}