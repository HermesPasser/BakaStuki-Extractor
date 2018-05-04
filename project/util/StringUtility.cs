using System.IO;

namespace BakaTsukiExtractor.util
{
    public static class StringUtility
    {
        public static readonly string HtmlFilter = " HTML file (*.html, *.htm) | *.html; *.htm";
        public static readonly string TemplatePath = @"res\template.html";

        public static string RemoveInvalidPathChars(this string text)
        {
            char[] InvalidFileNameChars = Path.GetInvalidFileNameChars(),
                   InvalidPathChars = Path.GetInvalidPathChars();

            string invalids = new string(InvalidFileNameChars) + new string(InvalidPathChars);

            foreach (char ch in invalids)
                text.Replace(ch.ToString(), "");

            return text;
        }

        public static void SaveText(string text, string filename)
        {
            using (StreamWriter sw = File.CreateText(filename))
            {
                sw.Write(text);
            }
        }
    }
}
