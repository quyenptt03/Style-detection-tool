namespace StyleDetectionTool.Models
{
    public class Element
    {
        public int Depth { get; set; }
        public string Tag { get; set; }
        public string Text { get; set; }
        public string Class { get; set; }
        public string Style { get; set; }
        public Element() { }
        public Element(int depth, string tag, string text, string classList, string style)
        {
            Depth = depth;
            Tag = tag;
            Text = text;
            Class = classList;
            Style = style;
        }
    }
}
