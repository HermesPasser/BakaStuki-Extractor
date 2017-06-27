using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace BakaTsukiFormater
{
    class BakaTsukiExtractor
    {
        private string htmlText;
        private string filepath;

        public BakaTsukiExtractor(string htmlText)
        {
            this.htmlText = htmlText;
        }

        public void InitAndSave(string filepath)
        {
            if (filepath.Contains("\\"))
                this.filepath = filepath.Substring(0, filepath.LastIndexOf("\\")) + "\\";

            string temp = htmlText;
            temp = GetBody(temp);
            temp = RemoveNonTextualElements(temp);
            temp = DownloadImagesAndSwitchLinks(temp);

            using (FileStream fs = new FileStream(filepath, FileMode.Create))
            {
                using (StreamWriter w = new StreamWriter(fs, Encoding.UTF8))
                {
                    w.WriteLine(temp);
                }
            }
        }
        
        private string GetBody(string fileText)
        {
            Regex regx = new Regex("<body(?<theBody>.*)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match match = regx.Match(fileText);

            string body = "";
            if (match.Success) body = "<!DOCTYPE html>\n<html>\n<body" + match.Groups["theBody"].Value + "\n</body>\n</html>";
            return body;
        }

        // One day, refactor code with agility pack
        // does work with toradora
        private string RemoveNonTextualElements(string fileText)
        {
            // Remove the content of script tags
            Regex regx = new Regex(@"<script[^>]*>[\s\S]*?</script>");
            fileText = regx.Replace(fileText, "");

            // Remove before content after the text
            regx = new Regex("<li class=\"toclevel[^>]*>[\\s\\S]*?</li>");
            fileText = regx.Replace(fileText, "");

            // Remove all content after the text
            regx = new Regex("<h2>Navigation menu</h2>[^>]*>[\\s\\S]*?</body>");
            fileText = regx.Replace(fileText, "");

            return fileText;
        }

        // Get image address in href of <a> tag and download it
        private string DownloadImagesAndSwitchLinks(string fileText)
        {
            string sub = "";
            string[] fileArr = fileText.Split('\n');

            for (int i = 0; i < fileArr.Length; i++)
            {
                // For rendered pages (like the saved pages)
                if (fileArr[i].Contains("jpg"))
                {
                    // Get the url of image to be downloaded and replaced by downloaded image
                    sub = fileArr[i].Substring(fileArr[i].IndexOf("<a href"), fileArr[i].Length - fileArr[i].IndexOf("<a href"));
                    sub = sub.Substring(0, sub.IndexOf("jpg") + 3);
                    sub = sub.Replace("<a href=", "").Replace("\"", "");

                    if (!sub.Contains("www.baka-tsuki.org"))
                        sub = "https://www.baka-tsuki.org" + sub;

                    string name = GetTruePathOfImage(sub.Replace("<a href=", ""));
                    fileArr[i] = "<img alt=\"" + name + "\" src=\"" + name + "\">";
                }
            }
            return string.Join("\n", fileArr); ;
        }

        private string GetTruePathOfImage(string url)
        {
            //https://msdn.microsoft.com/pt-br/library/system.net.networkinformation.networkinterface.getisnetworkavailable(v=vs.110).aspx
            //if (NetworkInterface.GetIsNetworkAvailable() == true)
            WebRequest req = WebRequest.Create(url);
            WebResponse res = req.GetResponse();
            StreamReader reader = new StreamReader(res.GetResponseStream());
            string[] result = reader.ReadToEnd().Split('\n');
            reader.Close();
            res.Close();

            // colocar aqui try cath para impedir que seja abortado caso não tenha net, nesse caso, devolve o url original
            foreach (string line in result)
            {
                if (line.Contains("fullImageLink")) // div class name
                {
                    string sub = line.Substring(line.IndexOf("/project"), line.Length - line.IndexOf("/project"));
                    sub = sub.Substring(0, sub.IndexOf("jpg") + 3);

                    string name = sub.Substring(sub.LastIndexOf("/") + 1, sub.Length - sub.LastIndexOf("/") - 1);
                    string link = @"https://www.baka-tsuki.org" + sub;
                    Console.WriteLine("Downloaded: " + link + " in " + filepath + name);
                    DownloadImage(link, filepath + name);
                    return name;
                }
            }
            return url;
        }

        private void DownloadImage(string link, string filename)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(link, filename);
            }
        }
    }
}
