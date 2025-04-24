using AngleSharp.Dom;
using StyleDetectionTool.Models;
using System.Collections.Concurrent;

namespace StyleDetectionTool.Services
{
    public class StyleCheckingService
    {
        public StyleCheckingService() { }

        public void ProcessButtons(ThemeConfig config, CrawledDocument page, ConcurrentBag<StyleUsage> result)
        {
            foreach (var btn in config.Buttons)
            {
                var query = $".{btn.Class.Replace(" ", ".")}.{btn.Id}".EscapeCssSelector();
                var elements = page.Document.QuerySelectorAll(query);

                foreach (var element in elements)
                {
                    var section = element.Ancestors<IElement>()
                        .FirstOrDefault(x => (x.ClassName ?? "").ToLower().Contains("section"));
                    if (section != null)
                    {
                        var sectionClass = (section.ClassName ?? "").ToLower()
.Split(' ', StringSplitOptions.RemoveEmptyEntries)
.FirstOrDefault(cls => cls.Contains("section"));

                        result.Add(new StyleUsage
                        {
                            Name = btn.Id,
                            IsUsed = true,
                            InUsed = new()
                    {
                        new PageUsage
                        {
                            Url = page.Url,
                            Title = page.Title,
                            Section = new() { sectionClass }
                        }
                    }
                        });
                    }
                }
            }
        }

        public void ProcessParagraphs(ThemeConfig config, CrawledDocument page, ConcurrentBag<StyleUsage> result)
        {
            foreach (var para in config.Paragraphs)
            {
                var elements = page.Document.All
                    .Where(el => (el.ClassName ?? "").Contains(para.Class) && (el.ClassName ?? "").Contains(para.Id))
                    .ToList();

                foreach (var element in elements)
                {
                    var section = element.Ancestors<IElement>()
                        .FirstOrDefault(x => (x.ClassName ?? "").ToLower().Contains("section"));
                    if (section != null)
                    {
                        var sectionClass = (section.ClassName ?? "").ToLower()
.Split(' ', StringSplitOptions.RemoveEmptyEntries)
.FirstOrDefault(cls => cls.Contains("section"));

                        result.Add(new StyleUsage
                        {
                            Name = para.Id,
                            IsUsed = true,
                            InUsed = new()
                    {
                        new PageUsage
                        {
                            Url = page.Url,
                            Title = page.Title,
                            Section = new() { sectionClass }
                        }
                    }
                        });
                    }
                }
            }
        }

        public void ProcessColors(ThemeConfig config, CrawledDocument page, ConcurrentBag<StyleUsage> result)
        {
            foreach (var color in config.ColorNames)
            {
                var colorElements = page.Document.All.Where(el =>
                    (!string.IsNullOrEmpty(el.ClassName) && el.ClassName.Contains(color)) ||
                    (!string.IsNullOrEmpty(el.GetAttribute("style")) && el.GetAttribute("style")!.Contains($"var(--{color})"))
                );

                foreach (var el in colorElements)
                {
                    var section = el.Ancestors<IElement>().FirstOrDefault(x => (x.ClassName ?? "").ToLower().Contains("section"));
                    if (section != null)
                    {
                        var sectionClass = (section.ClassName ?? "").ToLower()
.Split(' ', StringSplitOptions.RemoveEmptyEntries)
.FirstOrDefault(cls => cls.Contains("section"));

                        result.Add(new StyleUsage
                        {
                            Name = color,
                            IsUsed = true,
                            InUsed = new()
                    {
                        new PageUsage
                        {
                            Url = page.Url,
                            Title = page.Title,
                            Section = new() { sectionClass }
                        }
                    }
                        });
                    }
                }
            }
        }
        public void ProcessColorsInTheme(ThemeConfig config, CrawledDocument page, ConcurrentBag<StyleUsage> result)
        {
            foreach (var color in config.ColorNames)
            {
                var styleTags = page.Document.QuerySelectorAll("style");

                foreach (var tag in styleTags)
                {
                    var text = tag.TextContent ?? "";
                    var rules = text.Split('}');

                    foreach (var rule in rules)
                    {
                        var parts = rule.Split('{');
                        if (parts.Length != 2 || !parts[1].Contains($"var(--{color})")) continue;

                        var selector = parts[0].Trim();
                        var group = selector.Split('.').LastOrDefault(s =>
                            s.StartsWith("button") || s.StartsWith("paragraph") || s.StartsWith("heading"));

                        if (group != null)
                        {
                            result.Add(new StyleUsage
                            {
                                Name = color,
                                IsUsed = true,
                                InUsed = new()
                        {
                            new PageUsage
                            {
                                Url = "Theme config",
                                Title = "Theme config",
                                Section = new() { group }
                            }
                        }
                            });
                        }
                    }
                }
            }
        }
        public void MergeStyleUsage(List<StyleUsage> baseList, ConcurrentBag<StyleUsage> updates)
        {
            foreach (var updated in updates)
            {
                var existing = baseList.FirstOrDefault(s => s.Name == updated.Name);
                if (existing != null)
                {
                    existing.IsUsed |= updated.IsUsed;

                    foreach (var pageUsage in updated.InUsed)
                    {
                        var existingPage = existing.InUsed.FirstOrDefault(x => x.Url == pageUsage.Url);
                        if (existingPage != null)
                        {
                            foreach (var section in pageUsage.Section)
                            {
                                if (!existingPage.Section.Contains(section))
                                    existingPage.Section.Add(section);
                            }
                        }
                        else
                        {
                            existing.InUsed.Add(pageUsage);
                        }
                    }
                }
                else
                {
                    baseList.Add(updated);
                }
            }
        }

    }
}
