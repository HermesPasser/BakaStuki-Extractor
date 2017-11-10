using System;
using System.IO;
using System.Xml;
using System.Net;
using System.Text;

namespace BakaTsukiExtractor
{
    class BakaTsukiExtractorException : Exception
    {
        public BakaTsukiExtractorException(string message) : base(message) { }
    }

    class BakaTsukiExtractor
    {
        private string filepath;

        private XmlDocument xml;
        private XmlNode content;

        public BakaTsukiExtractor(string htmlText)
        {
            //HtmlDocument document = new HtmlDocument();
            try
            {
                xml = new XmlDocument();
                xml.LoadXml(htmlText);
            }
            catch (XmlException)
            {
                throw new BakaTsukiExtractorException("This file is not a html document valid, did you download it using \"Save as\" function of some browser?\nNext time use \"View source code\" and copy all the text into the file or just get the link and use From URL option in this program.");
            }

            XmlNode head    = xml.SelectSingleNode("//head");
            XmlNode title   = head.SelectSingleNode("//title");
            XmlNode body    = xml.SelectSingleNode("//body");
            XmlNode h1Title = body.SelectSingleNode("//h1[@id='firstHeading']");
            content         = body.SelectSingleNode("//div[@id='mw-content-text']");

            head.RemoveAll();              // Clear the <head>
            head.AppendChild(title);       // Add the <title> in the head

            body.RemoveAll();               // Clear the <body>
            body.AppendChild(title);        // Add the <title> in the body
            body.AppendChild(content);      // Add the <div id='mw-content-text'> in the body
        }

        public static string GetHtml(string url)
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
                throw new BakaTsukiExtractorException(e.ToString());
            }
            catch (UriFormatException)
            {
                throw new BakaTsukiExtractorException(url + "is not a uri valid.");
            }
            finally
            {
                if (res != null) res.Close();
                if (reader != null) reader.Close();
            }
            return result;
        }

        public void Save(string filepath)
        {
            if (filepath.Contains("\\"))
                this.filepath = filepath.Substring(0, filepath.LastIndexOf("\\")) + "\\";

            SetUpHtml();

            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(xml.InnerXml);
                }
            }
        }
		
        // Read the about-images.rtf to undertand how it works
		private void SetUpHtml()
        {
            // Get all the <a> nodes with the link to the page with the image
            XmlNodeList lis = content.SelectNodes("//a[@class='image']");

            // Get the link of page that contain the image
            for (int i = 0; i < lis.Count; i++)
            {
                string pageLink = lis[i].Attributes["href"].Value;
                if (!pageLink.Contains("baka-tsuki.org"))
                    pageLink = "https://www.baka-tsuki.org" + pageLink;

                string invalids = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
                foreach (char ch in invalids)
                    pageLink.Replace(ch.ToString(), "");
                
                //I'm using this line because in the novel illustrations the image name not appear in the alt of the img, 
                //so i cannot use this line: lis[i].SelectSingleNode("img").Attributes["alt"].Value;
                string imageName = Path.GetFileName(pageLink); 

                // Replace the <a> node with a <img src="imageName" /> for the document search the images in the local folder
                XmlElement element = xml.CreateElement("", "img", "");
                element.SetAttribute("src", imageName);
                element.SetAttribute("alt", imageName);
                lis[i].ParentNode.ReplaceChild(element, lis[i]);

                // Get the image link in the image page
                DownloadImage(GetImageLink(pageLink), filepath + imageName);
            }
        }

        // Get the original version of the image in the file page
        private string GetImageLink(string url)
        {
            XmlDocument filePage = new XmlDocument();
            filePage.LoadXml(GetHtml(url));

            XmlNode originalImage = filePage.SelectSingleNode("//a[@class='internal']");
            string originalImageLink = originalImage.Attributes["href"].Value;

            if (!originalImageLink.Contains("baka-tsuki.org"))
                originalImageLink = "https://www.baka-tsuki.org" + originalImageLink;

            return originalImageLink;
        }

        private void DownloadImage(string url, string filename)
        {
            if (File.Exists(filename)) return; // To not dowload a file twice
            try
            {
                using (WebClient client = new WebClient()) client.DownloadFile(url, filename);
                
            } catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("link: " + url + "\nfilename: " + filename + "\n\nError: " + e.ToString());
            }
        }
    }
}
