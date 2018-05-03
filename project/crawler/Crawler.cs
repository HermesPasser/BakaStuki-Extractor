using System;
using System.IO;
using System.Net;
using BakaTsukiExtractor.extractor;

namespace BakaTsukiExtractor.crawler
{
    public class Crawler : ICrawable
    {

        public void DownloadImage(string url, string filename)
        {
            if (File.Exists(filename)) return; // To not dowload a file twice
            try
            {
                using (WebClient client = new WebClient()) client.DownloadFile(url, filename);

            }
            catch (Exception e)
            {
                throw new ExtractorException("link: " + url + "\nfilename: " + filename + "\n\nError: " + e.ToString());
            }
        }

        public string GetHtml(string url)
        {
            WebRequest req = null;
            WebResponse res = null;
            StreamReader reader = null;
            string result = null;

            try
            {
                req = WebRequest.Create(url);
                res = req.GetResponse();
                reader = new StreamReader(res.GetResponseStream());
                result = reader.ReadToEnd();
            }
            catch (NotSupportedException e)
            {
                throw new ExtractorException(e.ToString());
            }
            catch (UriFormatException)
            {
                throw new ExtractorException(url + " is not a uri valid. Do you add the protocol?");
            }
            finally
            {
                if (res != null) res.Close();
                if (reader != null) reader.Close();
            }
            return result;
        }
    }
}
