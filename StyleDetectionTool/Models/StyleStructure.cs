using System.Text.Json;

namespace StyleDetectionTool.Models

{
    public class StyleStructure
    {
        public List<string>? Colors { get; set; }
        public List<string>? Styles { get; set; }

        public StyleStructure() { }

        public StyleStructure(List<string> colors, List<string> styles)
        {
            Colors = colors;
            Styles = styles;
        }

        public StyleStructure Handle(JsonElement json)
        {
            var colors = ExtractColors(json.GetProperty("colors"));
            var paragraphIds = ExtractIds(json.GetProperty("paragraphs"));
            var buttonIds = ExtractIds(json.GetProperty("buttons")); ;
            List<string> styles = paragraphIds.Concat(buttonIds).ToList();
            return new StyleStructure(colors, styles);
        }

        private List<string> ExtractColors(JsonElement colorsJson)
        {
            var colors = new List<string>();

            foreach (var color in colorsJson.EnumerateArray())
            {
                string name = color.GetProperty("name").GetString();

                foreach (var prop in color.EnumerateObject())
                {
                    if (prop.Name == "name") continue;

                    string key = prop.Name;
                    string value = prop.Value.GetString();
                    string styleKey = key == "default" ? name : $"{name}-{key}";

                    colors.Add(styleKey);
                }
            }

            return colors;
        }

        private List<string> ExtractIds(JsonElement items)
        {
            var ids = new List<string>();

            foreach (var item in items.EnumerateArray())
            {
                if (item.GetProperty("id").GetString() == "tcma-sb")
                {
                    continue;
                }
                ids.Add(item.GetProperty("id").GetString());
            }

            return ids;
        } 
    }
}
