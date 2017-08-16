using System;
using System.Threading;
using System.Windows.Forms;

namespace BakaTsukiFormater
{
    public partial class Form1 : Form
    {
        private string fileText = null;
        public static string about = "BakaStuki Extractor 0.3.2\nGitHub: HermesPasser/BakaStuki-Extractor\nBy Hermes Passer (gladiocitrico.blogspot.com)";

        public Form1()
        {
            InitializeComponent();
            openFileDialog1.Filter = " HTML file (*.html, *.htm) | *.html; *.htm";
            saveFileDialog1.Filter = " HTML file (*.html, *.htm) | *.html; *.htm";
            btnBrowser.Focus();
        }

        private void Browse()
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBrowse.Text = openFileDialog1.FileName;
                fileText = System.IO.File.ReadAllText(openFileDialog1.FileName);
            }
        }

        private string GetURL()
        {
            System.Net.WebRequest req = System.Net.WebRequest.Create(textURL.Text);
            System.Net.WebResponse res = req.GetResponse();
            System.IO.StreamReader reader = new System.IO.StreamReader(res.GetResponseStream());
            string result = reader.ReadToEnd();
            reader.Close();
            res.Close();
            return result;
        }
		
        private void btnBrowser_Click(object sender, EventArgs e)
        {
            Browse();
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if (fileText != null) Save();
                else Browse();
            }
            else
            {
                if (textURL.Text != null)
                {
                    fileText = GetURL();
                    Save();
                }
                else MessageBox.Show("URL cannot be empty.");
            }            
        }

        private void Save()
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            var thread = new Thread(() =>
            {
                string temp = saveFileDialog1.FileName;
                BakaTsukiExtractor bt = new BakaTsukiExtractor(fileText);
                bt.InitAndSave(temp);

                if (temp.Contains("\\"))
                    temp = temp.Remove(0, temp.LastIndexOf("\\") + 1);
                MessageBox.Show(temp + " completed.");
            });
            thread.Start();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(about, "About");
        }
    }
}
