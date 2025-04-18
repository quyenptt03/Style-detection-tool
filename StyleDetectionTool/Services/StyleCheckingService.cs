using StyleDetectionTool.Models;

namespace StyleDetectionTool.Services
{
    public class StyleCheckingService
    {
        public StyleCheckingService() { }

        public Dictionary<string, UsageInfo> AnalyzeClassUsage(List<Element> elements, List<string> checkClass)
        {
            var result = checkClass.ToDictionary(c => c, c => new UsageInfo());

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                if (string.IsNullOrWhiteSpace(element.Class)) continue;

                var elementClassList = element.Class.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                foreach (var cls in elementClassList)
                {
                    if (!checkClass.Contains(cls)) continue;

                    result[cls].Used = true;

                    string sectionIdentifier = null;

                    for (int j = i - 1; j >= 0; j--)
                    {
                        var parent = elements[j];
                        if (parent.Depth < element.Depth)
                        {
                            if (parent.Class != null &&
                                !elements[j].Text.Contains("hidden by cat") &&
                                parent.Class.Contains("hide") &&
                                parent.Class.Contains("hidden"))
                            {
                                sectionIdentifier = !string.IsNullOrWhiteSpace(parent.Text)
                                    ? parent.Text.Trim()
                                    : parent.Tag;
                                break;
                            }
                        }
                    }

                    if (sectionIdentifier != null && !result[cls].InUsed.Contains(sectionIdentifier))
                    {
                        result[cls].InUsed.Add(sectionIdentifier);
                    }
                }
            }

            return result;
        }

        public Dictionary<string, UsageInfo> AnalyzeColorUsage(List<Element> elements, List<string> colors)
        {
            var result = colors.ToDictionary(c => c, c => new UsageInfo());

            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];
                var tag = element.Tag?.ToLower() ?? "";

                // CASE 1: TAG == STYLE
                if (tag == "style" && !string.IsNullOrEmpty(element.Text))
                {
                    string styleText = element.Text;

                    var rules = styleText.Split('}');
                    foreach (var rule in rules)
                    {
                        var parts = rule.Split('{');
                        if (parts.Length != 2) continue;

                        var selector = parts[0].Trim();   // .tcma-sb .heading2
                        var body = parts[1];              // ...color:var(--secondary)...

                        foreach (var color in colors)
                        {
                            if (body.Contains($"var(--{color})"))
                            {
                                result[color].Used = true;

                                string groupName = "unknown";
                                if (selector.Contains(".button"))
                                    groupName = selector.Split('.').Last(s => s.StartsWith("button"));
                                else if (selector.Contains(".paragraph"))
                                    groupName = selector.Split('.').Last(s => s.StartsWith("paragraph"));
                                else if (selector.Contains(".heading"))
                                    groupName = selector.Split('.').Last(s => s.StartsWith("heading"));
                                else
                                    groupName = "default-font";

                                if (!result[color].InUsed.Contains($"style: {groupName}"))
                                    result[color].InUsed.Add($"style: {groupName}");
                            }
                        }
                    }
                }

                // CASE 2: THẺ KHÁC STYLE
                else
                {
                    foreach (var color in colors)
                    {
                        bool matched = false;

                        if (!string.IsNullOrEmpty(element.Class) &&
                            element.Class.Contains(color))
                        {
                            matched = true;
                        }

                        if (!matched && !string.IsNullOrEmpty(element.Style) &&
                            element.Style.Contains($"var(--{color})"))
                        {
                            matched = true;
                        }

                        if (matched)
                        {
                            result[color].Used = true;

                            // tìm section có class hide hidden
                            string section = null;
                            for (int j = i; j >= 0; j--)
                            {
                                if (elements[j].Depth < element.Depth &&
                                    elements[j].Class != null &&
                                    elements[j].Tag != "small" &&
                                    elements[j].Class.Contains("hide") &&
                                    elements[j].Class.Contains("hidden"))
                                {
                                    section = !string.IsNullOrWhiteSpace(elements[j].Text)
                                        ? elements[j].Text.Trim()
                                        : elements[j].Tag;
                                    break;
                                }
                            }

                            if (section != null && !result[color].InUsed.Contains($"section: {section}"))
                            {
                                result[color].InUsed.Add($"section: {section}");
                            }
                        }
                    }
                }
            }

            return result;
        }
    }
}
