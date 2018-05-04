using System.Diagnostics;
using System.Windows.Forms;

namespace BakaTsukiExtractor
{
    public partial class About : Form
    {
        string version = "1.5";

        public About()
        {
            InitializeComponent();
            label1.Text += " " + version;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://github.com/HermesPasser/BakaTsuki-Extractor");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(@"https://hermespasser.github.io/pages/bakatsuki-extractor.html");
        }

        private void About_FormClosing(object sender, FormClosingEventArgs e)
        {
            MainForm.instance.Enabled = true;
        }
    }
}
