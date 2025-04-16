namespace StyleDetectionTool.Utils
{
    public class StyleConverter
    {
        public static string HexToRgb(string hex)
        {
            if (hex.StartsWith("#"))
                hex = hex[1..];

            if (hex.Length != 6)
                throw new ArgumentException("Invalid hex color");

            int r = Convert.ToInt32(hex.Substring(0, 2), 16);
            int g = Convert.ToInt32(hex.Substring(2, 2), 16);
            int b = Convert.ToInt32(hex.Substring(4, 2), 16);

            return $"rgb({r}, {g}, {b})";
        }

        public static Dictionary<string, object> ConvertInput(dynamic json)
        {
            var result = new Dictionary<string, object>();

            var flatColors = new Dictionary<string, string>();
            foreach (var color in json.GetProperty("colors").EnumerateArray())
            {
                string name = color.GetProperty("name").GetString();
                foreach (var prop in color.EnumerateObject())
                {
                    if (prop.Name != "name")
                    {
                        string key = prop.Name;
                        string value = prop.Value.GetString();
                        string styleKey = key == "default" ? name : $"{name}-{key}";
                        flatColors[styleKey] = value.StartsWith("#") ? HexToRgb(value) : value;
                    }
                }
            }

            var paragraphIds = new List<string>();
            foreach (var p in json.GetProperty("paragraphs").EnumerateArray())
            {
                paragraphIds.Add(p.GetProperty("id").GetString());
            }

            var buttonIds = new List<string>();
            foreach (var b in json.GetProperty("buttons").EnumerateArray())
            {
                buttonIds.Add(b.GetProperty("id").GetString());
            }

            result["colors"] = flatColors;
            result["paragraphs"] = paragraphIds;
            result["buttons"] = buttonIds;

            return result;
        }
    }
}
