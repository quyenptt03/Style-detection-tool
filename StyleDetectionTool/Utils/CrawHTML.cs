using Microsoft.Playwright;
using System.ComponentModel;
using System.Security.Principal;
using System.Text.Json;

namespace StyleDetectionTool.Services
{
    public class CrawHTML
    {
        static HashSet<string> visited = new();
        static Dictionary<string, int> tagCount = new();
        static Dictionary<string, int> colorUsage = new();
        static async Task CrawlPageAsync(IPage page, string url, string baseUrl)
        {
            if (visited.Contains(url)) return;
            visited.Add(url);

            try
            {
                await page.GotoAsync(url, new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 20000 });
            }
            catch
            {
                Console.WriteLine($"⚠️ Không thể truy cập: {url}");
                return;
            }

            Console.WriteLine($"✅ Đã crawl: {url}");

            await AnalyzePageAsync(page);

            var links = await page.EvalOnSelectorAllAsync<string[]>("a[href]", "els => els.map(e => e.href)");
            foreach (var link in links.Distinct())
            {
                if (link.StartsWith(baseUrl))
                    await CrawlPageAsync(page, link, baseUrl);
            }
        }

        static async Task AnalyzePageAsync(IPage page)
        {
            var tags = new[] { "button", "h1", "h2", "h3", "h4", "h5", "h6", "p", "a", "div", "span" };

            foreach (var tag in tags)
            {
                var elements = await page.QuerySelectorAllAsync(tag);
                if (!tagCount.ContainsKey(tag)) tagCount[tag] = 0;
                tagCount[tag] += elements.Count;

                foreach (var el in elements)
                {
                    var color = await el.EvaluateAsync<string>("el => getComputedStyle(el).color");
                    if (!string.IsNullOrWhiteSpace(color))
                    {
                        if (!colorUsage.ContainsKey(color)) colorUsage[color] = 0;
                        colorUsage[color]++;
                    }
                }
            }
        }
    }
}