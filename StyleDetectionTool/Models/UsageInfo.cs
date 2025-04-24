using AngleSharp.Dom;

namespace StyleDetectionTool.Models
{

    public class StyleUsage
    {
        public string Name { get; set; }
        public bool IsUsed { get; set; }
        public List<PageUsage> InUsed { get; set; } = new List<PageUsage>();

        public StyleUsage() { }

        public StyleUsage(string name, bool isUsed, List<PageUsage> pageUsages)
        {
            Name = name;
            IsUsed = isUsed;
            InUsed = pageUsages;
        }
    }

    public class PageUsage
    {
        public string? Url { get; set; } = null;
        public string? Title { get; set; } = null;
        public List<string> Section { get; set; } = new List<string> { };

    }


    public class CrawledDocument
    {
        public IDocument Document { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
    }
}
