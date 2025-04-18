using HtmlAgilityPack;
using Microsoft.Playwright;
using StyleDetectionTool.Models;
using System.Collections.Concurrent;

namespace StyleDetectionTool.Services
{
    public class CrawHTMLService
    {
        public List<Element> elemennts = new List<Element>();
        public CrawHTMLService() { }
        public CrawHTMLService(List<Element> elemennts)
        {
            this.elemennts = elemennts;
        }

        public async Task<List<Element>> CrawlSinglePage(IBrowser browser, string url)
        {
            //using var playwright = await Playwright.CreateAsync();
            //await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();

            await page.RouteAsync("**/*", async route =>
            {
                var type = route.Request.ResourceType;
                if (type is "image" or "font" or "media")
                    await route.AbortAsync();
                else
                    await route.ContinueAsync();
            });

            await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle });
            var content = await page.ContentAsync();
            var elements = ParseHtmlToElements(content);
            return elements;
        }

        //public async Task<List<Page>> CrawlMultiplePage(List<string> urls)
        //{
        //    var pages = new List<Page>();

        //    using var playwright = await Playwright.CreateAsync();
        //    await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

        //    var tasks = urls.Select(async url =>
        //    {
        //        var elements = await CrawlSinglePage(browser, url);
        //        var page = new Page(url, elements);
        //        lock (pages)
        //        {
        //            pages.Add(page);
        //        }
        //    });

        //    await Task.WhenAll(tasks);
        //    return pages;
        //}
        public async Task<List<Page>> CrawlMultiplePage(List<string> urls, int maxConcurrency = 5)
        {
            var pages = new ConcurrentBag<Page>();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });

            var semaphore = new SemaphoreSlim(maxConcurrency); // Giới hạn số crawl song song

            var tasks = urls.Select(async url =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var elements = await CrawlSinglePage(browser, url);
                    pages.Add(new Page(url, elements));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error crawling {url}: {ex.Message}");
                    // Optionally: Add a Page with empty elements or log separately
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
            return pages.ToList();
        }

        private List<Element> ParseHtmlToElements(string html)
        {
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);

            var elements = new List<Element>();
            Traverse(doc.DocumentNode, 0, elements);

            return elements;
        }

        private void Traverse(HtmlNode node, int depth, List<Element> list)
        {
            if (node.NodeType == HtmlNodeType.Element)
            {
                var element = new Element
                {
                    Depth = depth,
                    Tag = node.Name,
                    Text = node.InnerText?.Trim(),
                    Class = node.GetAttributeValue("class", ""),
                    Style = node.GetAttributeValue("style", "")
                };

                list.Add(element);
            }

            foreach (var child in node.ChildNodes)
            {
                Traverse(child, depth + 1, list);
            }
        }
    }
}