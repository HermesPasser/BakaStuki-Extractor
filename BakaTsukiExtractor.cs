using System;
using System.Text;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

//criar verificação para não baixar imagens replicadas

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
            if (match.Success) body = "<!-- "+ Form1.about + "\n-->\n<!DOCTYPE html>\n<html>\n<body" + match.Groups["theBody"].Value + "\n</body>\n</html>";
            return body;
        }

        // One day, refactor code with agility pack
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

        // Get image address in href of <a>  tag and download it
        private string DownloadImagesAndSwitchLinks(string fileText)
        {
            string sub = "", ext = "";
            string[] fileArr = fileText.Split('\n');

            for (int i = 0; i < fileArr.Length; i++)
            {
                // For rendered pages (like the saved pages)
                if (fileArr[i].Contains("jpg") || fileArr[i].Contains("png"))
                {
                    // Get the url of image to be downloaded and replaced by downloaded image
                    ext = fileArr[i].Contains("jpg") ? "jpg" : "png";
                    sub = fileArr[i].Substring(fileArr[i].IndexOf("<a href"), fileArr[i].Length - fileArr[i].IndexOf("<a href"));
                    sub = sub.Substring(0, sub.IndexOf(ext) + 3);
                    sub = sub.Replace("<a href=", "").Replace("\"", "");
                    sub = sub.Replace("www.", "");

                    if (!sub.Contains("baka-tsuki.org"))
                        sub = "https://www.baka-tsuki.org" + sub;

                    string name = GetTruePathOfImage(sub.Replace("<a href=", ""), ext);
                    fileArr[i] = "<img alt=\"" + name + "\" src=\"" + name + "\">";
                }
            }
            return string.Join("\n", fileArr); ;
        }

        private string GetTruePathOfImage(string url, string extension)
        {
            StreamReader reader = null;
            string[] result = new string[1] {"Error at search the image"};

            try
            {
                Console.WriteLine(url + " " + extension);
                WebRequest req = WebRequest.Create(url);
                reader = new StreamReader(req.GetResponse().GetResponseStream());
                result = reader.ReadToEnd().Split('\n');
                reader.Close();

            }
            catch (UriFormatException uf)
            {
                System.Windows.Forms.MessageBox.Show("url: " + url + "\nError: " + uf.ToString());
            }

            foreach (string line in result)
            {
                if (line.Contains("fullImageLink")) // div class name
                {
                    string sub = line.Substring(line.IndexOf("/project"), line.Length - line.IndexOf("/project"));
                    sub = sub.Substring(0, sub.IndexOf(extension) + 3);

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
            if (File.Exists(filename)) return;
            try
            {
                using (WebClient client = new WebClient()) client.DownloadFile(link, filename);
                
            } catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("link: " + link + "\nfilename: " + filename + "\nError: " + e.ToString());
            }
        }
    }
}
