using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BakaTsukiExtractor.util;
using BakaTsukiExtractor.crawler;
using BakaTsukiExtractor.extractor;

namespace BakaTsukiExtractor
{
    public partial class MainForm : Form
    {
        private string fileText = null;

        private Extractor ex;
        private Crawler cr = new Crawler();

        public static MainForm instance;

        public MainForm()
        {
            InitializeComponent();

            openFileDialog1.Filter = StringUtility.HtmlFilter;
            saveFileDialog1.Filter = StringUtility.HtmlFilter;
            tabControl1.SelectedIndex = 1;
            instance = this;

            if (!File.Exists(StringUtility.TemplatePath))
            {
                MessageBox.Show(StringUtility.TemplatePath + " does not exists, try download the program again.");
                Application.Exit();
            }
        }
        
        private void Browse()
        {
            openFileDialog1.FileName = "";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                textBrowse.Text = openFileDialog1.FileName;
                fileText = File.ReadAllText(openFileDialog1.FileName);
            }
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
                if (textURL.Text.Contains(@"baka-tsuki.org/project/index.php?title="))
                {
                    try
                    {
                        fileText = cr.GetHtml(textURL.Text);
                        Save();
                    } catch (ExtractorException ee)
                    {
                        MessageBox.Show(ee.Message);
                    }
                }
                else MessageBox.Show("URL is not from baka-tsuki.org.");
            }            
        }
        
        private void Save()
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            this.Enabled = false;
            var thread = new Thread(() =>
            {
                string temp = saveFileDialog1.FileName;
                ex = new Extractor(fileText, cr);

                try
                {
                    ex.Save(temp);
                }
                catch (ExtractorException e)
                {
                    MessageBox.Show(e.Message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred, see error_log.txt for more details");
                    StringUtility.SaveText(ex.Message + " \n " + ex.StackTrace, "error_log.txt");
                }


                UIThread(delegate { this.Enabled = true; });
                MessageBox.Show(Path.GetFileName(temp) + " completed.");
            });
            thread.Start();
        }

        private void exitToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new About().Show();
            this.Enabled = false;
        }

        private void UIThread(MethodInvoker code)
        {
            if (this.InvokeRequired) this.Invoke(code);
            else code.Invoke();
        }
    }
}
