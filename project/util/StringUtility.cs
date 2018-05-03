using System.IO;

namespace BakaTsukiExtractor.util
{
    public static class StringUtility
    {
        public static string RemoveInvalidPathChars(this string text)
        {
            char[] InvalidFileNameChars = Path.GetInvalidFileNameChars(),
                   InvalidPathChars = Path.GetInvalidPathChars();

            string invalids = new string(InvalidFileNameChars) + new string(InvalidPathChars);

            foreach (char ch in invalids)
                text.Replace(ch.ToString(), "");

            return text;
        }
    }
}
