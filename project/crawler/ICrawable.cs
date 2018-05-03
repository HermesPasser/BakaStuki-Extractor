namespace BakaTsukiExtractor.crawler
{
    public interface ICrawable
    {
        void DownloadImage(string url, string filename);
        string GetHtml(string url);
    }
}
