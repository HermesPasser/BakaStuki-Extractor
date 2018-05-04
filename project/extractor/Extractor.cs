using System.IO;
using System.Xml;
using System.Text;
using BakaTsukiExtractor.crawler;
using BakaTsukiExtractor.util;
using System.Linq;
using System.Collections.Generic;

namespace BakaTsukiExtractor.extractor
{
    public class Extractor
    {
        private string filepath;
        private bool extracted;

        private XmlDocument template;
        private XmlDocument page;
        private XmlNode pageContent;

        private ICrawable crawler;

        public Extractor(string htmlText, ICrawable crawler)
        {
            this.crawler = crawler;

            template = new XmlDocument();
            page = new XmlDocument();

            try
            {
                template.Load(StringUtility.TemplatePath);
                page.LoadXml(htmlText);
            }
            catch (XmlException)
            {
                throw new ExtractorException("This file is not a html document valid, did you download it using \"Save as\" function of some browser?\nNext time use \"View source code\" and copy all the text into the file or just get the link and use From URL option in this program.");
            }

            // Get elements to add in template
            XmlNode body     = page.SelectSingleNode("//body");
            XmlNode titleH1  = body.SelectSingleNode("//h1[@id='firstHeading']");
            XmlNode titleTag = page.SelectSingleNode("//head").SelectSingleNode("//title");
            pageContent  = body.SelectSingleNode("//div[@id='mw-content-text']");
       
            // Add elements to template
            template.SelectSingleNode("//head").AppendChild(template.ImportNode(titleTag, true));
            template.SelectSingleNode("//body").AppendChild(template.ImportNode(titleH1, true));
        }

        public void Extract()
        {
            if (extracted)
                return;

            extracted = true;
            RemoveStyle();
            RemoveEditAnchors();
            RemoveTables();
            RemoveComments();
            SetUpImages();

            // if true is set the anchors with no content will be closed in itself (<a />) and chrome not process this kind
            //of anchor, but if is false then nothing will be imported.
            template.SelectSingleNode("//body").AppendChild(template.ImportNode(pageContent, true));
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
            pageContent.RemoveNodesByXpath("//comment()");
        }

        private void RemoveEditAnchors()
        {
            pageContent.RemoveNodesByXpath("//span[contains(@class, 'mw-editsection')]"); // not working for some reason the spans with that class still remain
            pageContent.RemoveNodesByXpath("//span[contains(@class, 'mw-editsection-bracket')]");
            pageContent.RemoveNodesByXpath("//a[contains(@title, 'Edit section:')]");
        }

        private void RemoveTables()
        {
            pageContent.RemoveNodesByXpath("//table");
        }

        private void RemoveStyle()
        {
            XmlNodeList nodesWithStyle = page.SelectNodes("//*[@style]");

            foreach (XmlNode node in nodesWithStyle)
                node.Attributes.Remove(node.Attributes["style"]);
        }

        // Get image url, add image tag and download image, read about-images.rtf to undertand how iworkst 
        private void SetUpImages()
        {
            // Get all the <div> nodes with the <a> childs that have the image url.
            XmlNodeList thumbDivs = pageContent.SelectNodes("//div[@class='thumb']");
            XmlNodeList thumbDivs2 = pageContent.SelectNodes("//div[@class='thumb tright']");
            IEnumerable<XmlNode> thumbs = thumbDivs.Concat(thumbDivs2);

            string pageLink = "", imageName = "";

            foreach (XmlNode thumbDiv in thumbs)
            {
                XmlNode anchorTag = thumbDiv.SelectSingleNode(".//a[@class='image']");

                pageLink = anchorTag.Attributes["href"].Value;
                pageLink = AddBakatsukiUrlIfNotContain(pageLink);
                pageLink = pageLink.RemoveInvalidPathChars();

                // I'm using this line because in the novel illustrations the image name not appear in the alt of the img, 
                // so i cannot use this line: lis[i].SelectSingleNode("img").Attributes["alt"].Value;
                imageName = Path.GetFileName(pageLink);

                // Create a image node (img) with the source attribute (src="imageName)" with the same name 
                // as the image will be saved and append in the current div node (list[i]).
                XmlElement element = page.CreateElement("", "img", "");
                element.SetAttribute("src", imageName);
                element.SetAttribute("alt", imageName);
                thumbDiv.ParentNode.InsertBefore(element, thumbDiv);

                try
                {
                    // Get the image link in the image page
                    crawler.DownloadImage(GetImageLink(pageLink), filepath + imageName);
                } catch (ExtractorException)
                {
                    throw;
                }
            }

            pageContent.RemoveNodesByXpath("//div[@class='thumb']");
            pageContent.RemoveNodesByXpath("//div[@class='thumb tright']");
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
