namespace StyleDetectionTool.Models

{

    public class ThemeConfig
    {
        public List<Color> Colors { get; set; }
        public List<string> ColorNames =>
               Colors?.SelectMany(c => c.NameVariants).ToList() ?? new List<string>();
        public List<Paragraph> Paragraphs { get; set; }
        public List<Button> Buttons { get; set; }
    }

    public class Color
    {
        public string Name { get; set; }
        public string Contrast { get; set; }
        public string Light { get; set; }
        public string Default { get; set; }
        public string Dark { get; set; }
        public List<string> NameVariants => new List<string>
        {
            Name,
            $"{Name}-light",
            $"{Name}-contrast",
            $"{Name}-dark"
        };
    }

    public class Paragraph
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FontWeight { get; set; }
        public string FontStyle { get; set; }
        public string LineHeight { get; set; }
        public string Class { get; set; }
        public string FontFamily { get; set; }
        public string Color { get; set; }
        public string FontSize { get; set; }
        public List<string> FontUrl { get; set; }
    }

    public class Button
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Class { get; set; }
        public string FontFamily { get; set; }
        public string FontSize { get; set; }
        public string Color { get; set; }
    }

}
