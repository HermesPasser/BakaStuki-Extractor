using System.IO;
using System.Xml;
using System.Text;
using BakaTsukiExtractor.crawler;
using BakaTsukiExtractor.util;

namespace BakaTsukiExtractor.extractor
{
    public class Extractor
    {
        private string filepath;
        private bool extracted;

        private XmlDocument template;
        private XmlDocument htmlPage;
        private XmlNode htmlPageContent;

        private ICrawable crawler;

        public Extractor(string htmlText, ICrawable crawler)
        {
            this.crawler = crawler;

            // Load the template document
            template = new XmlDocument();
            template.Load(@"res\template.html");

            try
            {
                // Load the html (from file or url)
                htmlPage = new XmlDocument();
                htmlPage.LoadXml(htmlText);
            }
            catch (XmlException)
            {
                throw new ExtractorException("This file is not a html document valid, did you download it using \"Save as\" function of some browser?\nNext time use \"View source code\" and copy all the text into the file or just get the link and use From URL option in this program.");
            }

            // Get elements to add in template
            XmlNode body     = htmlPage.SelectSingleNode("//body");
            XmlNode titleH1  = body.SelectSingleNode("//h1[@id='firstHeading']");
            XmlNode titleTag = htmlPage.SelectSingleNode("//head").SelectSingleNode("//title");
            htmlPageContent  = body.SelectSingleNode("//div[@id='mw-content-text']");
       
            // Add elements to template
            template.SelectSingleNode("//head").AppendChild(template.ImportNode(titleTag, true));
            template.SelectSingleNode("//body").AppendChild(template.ImportNode(titleH1, true));
        }

        public void Extract()
        {
            if (extracted)
                return;

            extracted = true;
            SetUpImages();
            RemoveStyle();
            RemoveEditAnchors();
            RemoveTables();
            RemoveComments();
            
            // if true is set the anchors with no content will be closed in itself (<a />) and chrome not process this kind
            //of anchor, but if is false then nothing will be imported.
            template.SelectSingleNode("//body").AppendChild(template.ImportNode(htmlPageContent, true));
        }

        public void Save(string filepath)
        {
            if (filepath.Contains("\\"))
                this.filepath = filepath.Substring(0, filepath.LastIndexOf("\\")) + "\\";

            Extract();

            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(template.InnerXml);
                }
            }
        }

        private void RemoveComments()
        {
            XmlNodeList comments = htmlPageContent.SelectNodes("//comment()");

            foreach (XmlNode child in comments)
                child.ParentNode.RemoveChild(child);
        }

        private void RemoveEditAnchors()
        {
            XmlNodeList spansWithBrackets = htmlPageContent.SelectNodes("//span[contains(@class, 'mw-editsection-bracket')]");
            XmlNodeList anchorsWithEditInTitle = htmlPageContent.SelectNodes("//a[contains(@title, 'Edit section:')]");

            foreach (XmlNode node in spansWithBrackets)
                node.ParentNode.RemoveChild(node);

            foreach (XmlNode node in anchorsWithEditInTitle)
                    node.ParentNode.RemoveChild(node);
        }

        private void RemoveTables()
        {
            XmlNodeList tables = htmlPageContent.SelectNodes("//table");

            foreach (XmlNode child in tables)
                child.ParentNode.RemoveChild(child);
        }

        private void RemoveStyle()
        {
            XmlNodeList nodesWithStyle = htmlPage.SelectNodes("//*[@style]");

            foreach (XmlNode node in nodesWithStyle)
                node.Attributes.Remove(node.Attributes["style"]);
        }

        // Get image url, add image tag and download image, read about-images.rtf to undertand how iworkst 
        private void SetUpImages()
        {
            // Get all the <div> nodes with the <a> childs that have the image url
            XmlNodeList lis = htmlPageContent.SelectNodes("//div[@class='thumb tright']");
            string pageLink = "", imageName = "";

            for (int i = 0; i < lis.Count; i++)
            {
                XmlNode anchorTag = lis[i].SelectSingleNode("//a[@class='image']");

                pageLink = anchorTag.Attributes["href"].Value;
                pageLink = AddBakatsukiUrlIfNotContain(pageLink);
                pageLink = pageLink.RemoveInvalidPathChars();

                // I'm using this line because in the novel illustrations the image name not appear in the alt of the img, 
                // so i cannot use this line: lis[i].SelectSingleNode("img").Attributes["alt"].Value;
                imageName = Path.GetFileName(pageLink);

                // Remove all childs of the current div node.
                foreach (XmlNode child in lis[i].ChildNodes)
                    child.ParentNode.RemoveChild(child);

                // Create a image node (img) with the source attribute (src="imageName)" with the same name 
                // as the image will be saved and append in the current div node (list[i]).
                XmlElement element = htmlPage.CreateElement("", "img", "");
                element.SetAttribute("src", imageName);
                element.SetAttribute("alt", imageName);
                lis[i].AppendChild(element);

                try
                {
                    // Get the image link in the image page
                    crawler.DownloadImage(GetImageLink(pageLink), filepath + imageName);
                } catch (ExtractorException)
                {
                    throw;
                }
            }
        }

        // Get the original version of the image in the file page
        private string GetImageLink(string url)
        {
            XmlDocument filePage = new XmlDocument();
            filePage.LoadXml(crawler.GetHtml(url));

            XmlNode originalImage = filePage.SelectSingleNode("//a[@class='internal']");
            string originalImageLink = originalImage.Attributes["href"].Value;
            return AddBakatsukiUrlIfNotContain(originalImageLink);
        }

        private string AddBakatsukiUrlIfNotContain(string text)
        {
            if (!text.Contains("baka-tsuki.org"))
                text = "https://www.baka-tsuki.org" + text;
            return text;
        }
    }
}
