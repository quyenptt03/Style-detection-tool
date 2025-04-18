namespace StyleDetectionTool.Models
{
    public class Page
    {
        public string Url { get; set; }
        public List<Element> Elements { get; set; }

        public Page(string url, List<Element> elements)
        {
            Url = url;
            Elements = elements;
        }
    }
}
