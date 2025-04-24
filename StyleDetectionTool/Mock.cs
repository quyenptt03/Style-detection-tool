using System.Text;

namespace StyleDetectionTool;

public static class Mock
{
    public static string GetInputJson()
    {
        return """
               {
                   "colors": [{
                           "name": "primary",
                           "contrast": "#f4f4f6",
                           "light": "#d2d6dc",
                           "default": "#6b6c7e",
                           "dark": "#272833"
                       }, {
                           "name": "secondary",
                           "contrast": "#2eb7ff",
                           "light": "#0b5fff",
                           "default": "#0011a8",
                           "dark": "#000742"
                       }, {
                           "name": "tertiary",
                           "contrast": "#d6e4ff",
                           "light": "#ffff33",
                           "default": "#c4a386",
                           "dark": "#996830"
                       }
                   ],
                   "paragraphs": [{
                           "id": "heading1",
                           "name": "Heading 1",
                           "fontWeight": "inherit",
                           "fontStyle": "inherit",
                           "lineHeight": "1.125",
                           "class": "text-4xl lg:text-5xl",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775"
                       }, {
                           "id": "heading2",
                           "name": "Heading 2",
                           "lineHeight": "1.25",
                           "fontWeight": "inherit",
                           "class": "text-3.5xl lg:text-4.5xl",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775",
                           "color": "var(--secondary)"
                       }, {
                           "id": "heading3",
                           "name": "Heading 3",
                           "fontWeight": "inherit",
                           "class": "text-2.5xl lg:text-3.5xl",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775"
                       }, {
                           "id": "heading4",
                           "name": "Heading 4",
                           "fontWeight": "inherit",
                           "class": "text-2xl lg:text-3.5xl",
                           "fontFamily": "SourceSansPro-Bold_638729439950922489"
                       }, {
                           "id": "heading5",
                           "name": "Heading 5",
                           "fontWeight": "inherit",
                           "class": ""
                       }, {
                           "id": "paragraph1",
                           "name": "Paragraph 1",
                           "lineHeight": "1.75",
                           "fontSize": "1.125em",
                           "class": ""
                       }, {
                           "id": "paragraph2",
                           "name": "Paragraph 2",
                           "fontSize": "1.25em",
                           "color": "var(--secondary)",
                           "class": ""
                       }, {
                           "id": "paragraph3",
                           "name": "Paragraph 3",
                           "fontWeight": "bold",
                           "color": "var(--primary)",
                           "class": ""
                       }, {
                           "id": "paragraph4",
                           "name": "Paragraph 4",
                           "color": "var(--primary)",
                           "class": ""
                       }, {
                           "id": "paragraph5",
                           "name": "Paragraph 5",
                           "fontFamily": "Material Symbols Outline",
                           "fontSize": "1em",
                           "lineHeight": "1",
                           "class": ""
                       }, {
                           "id": "tcma-sb",
                           "name": "tcma-sb",
                           "color": "var(--secondary)",
                           "fontSize": "16px",
                           "fontFamily": "SourceSansPro-Regular_638729439724684629",
                           "fontUrl": ["https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-Regular_638729439724684629.ttf", "https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-SemiBold_638729439784859775.ttf", "https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-Light_638729439834938264.ttf", "https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-ExtraLight_638729439896886076.ttf", "https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-Bold_638729439950922489.ttf", "https://tcmacore.prod.azw2k8-public.impartner.io/static/customer-resources/liferay-partner-program/assets/fonts/SourceSansPro-Black_638729440000950507.ttf"],
                           "class": ""
                       }
                   ],
                   "buttons": [{
                           "id": "button1",
                           "name": "Button 1",
                           "class": "py-2 px-4 font-medium focus:outline-none rounded-lg focus:z-10 focus:ring-4 focus:ring-gray-200 flex items-center gap-2 leading-6 text-sm lg:text-base justify-center bg-secondary-light text-white relative cursor-pointer",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775",
                           "fontSize": "1.125em"
                       }, {
                           "id": "button2",
                           "name": "Button 2",
                           "class": "focus:outline-none flex leading-6 text-center justify-center cursor-pointer",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775",
                           "fontSize": "1.125em"
                       }, {
                           "id": "button3",
                           "name": "Button 3",
                           "class": "py-2 px-4 font-medium focus:outline-none rounded-lg focus:z-10 focus:ring-4 focus:ring-gray-200 flex items-center gap-2 leading-6 text-sm lg:text-base justify-center bg-tertiary-contrast text-secondary-light relative cursor-pointer",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775",
                           "fontSize": "1.125em"
                       }, {
                           "id": "button4",
                           "name": "Button 4",
                           "color": "var(--secondary-light)",
                           "fontSize": "1.25em",
                           "class": "group button4 flex items-center gap-1",
                           "fontFamily": "SourceSansPro-SemiBold_638729439784859775"
                       }, {
                           "id": "button5",
                           "name": "Button 5",
                           "class": "text-2xl primary-dark p-1 hover:bg-primary-light rounded disabled:text-primary-light mat-symbol hover:text-primary-dark",
                           "color": "var(--primary-dark)"
                       }
                   ]
               }
               """;
    }

    public static List<string> GetUrls()
    {
        return [
  "https://tcma-api-public.dev.azw2k8-public.impartner.io/showcase/1r-zy1nhm-nl7zmhnl&bgjfgj/page/linh-test/languageid/38",
  "https://tcma-api-public.dev.azw2k8-public.impartner.io/showcase/1r-zy1nhm-nl7zmhnl&bgjfgj/page/kitchen/languageid/38",
  "https://tcma-api-public.dev.azw2k8-public.impartner.io/showcase/1r-zy1nhm-nl7zmhnl&bgjfgj/page/test-new-2/languageid/38",
  "https://tcma-api-public.dev.azw2k8-public.impartner.io/showcase/1r-zy1nhm-nl7zmhnl&bgjfgj/page/test-st-01/languageid/38",
  "https://tcma-api-public.dev.azw2k8-public.impartner.io/showcase/1r-zy1nhm-nl7zmhnl&bgjfgj/page/test-2/languageid/38"
];
    }

    public static readonly int MaxConcurrency = Environment.ProcessorCount;

    public static void WriteLog(this string input)
    {
        Console.WriteLine($"{DateTime.Now} {input}");
    }

    public static string EscapeCssSelector(this string input)
    {
        return input.Replace(":", "\\:");
    }
}
