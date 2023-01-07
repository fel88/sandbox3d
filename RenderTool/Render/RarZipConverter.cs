using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class RarZipConverter : Form
    {
        public RarZipConverter()
        {
            InitializeComponent();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            textBox2.Text = Path.GetDirectoryName(ofd.FileName);
            updateList();
        }

        void updateList()
        {
            listView1.Items.Clear();
            var d = new DirectoryInfo(textBox2.Text);
            foreach (var item in d.GetFiles("*.rar"))
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, item.Extension }) { Tag = item });
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!Directory.Exists(textBox2.Text)) return;
            updateList();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var pp = Path.Combine(textBox2.Text, "tempDir");
            Directory.CreateDirectory(pp);

            for (int i = 0; i < listView1.Items.Count; i++)
            {

                var lvi = listView1.Items[i];
                var fi = (lvi.Tag as FileInfo);
                ProcessStartInfo pci = new ProcessStartInfo(textBox1.Text)
                {
                    Arguments = " e \"" + fi.FullName + "\" " + " -otempDir",
                    WorkingDirectory = textBox2.Text,
                    UseShellExecute=!noWindow,
                    CreateNoWindow=noWindow
                    
                };
                var p = new Process();
                p.StartInfo = pci;
                Directory.Delete(pp, true);
                p.Start();
                p.WaitForExit();

                pci = new ProcessStartInfo(textBox1.Text)
                {
                    Arguments = " a -tzip \"" + fi.Name.Replace(".rar","") + ".zip\" tempDir",
                    WorkingDirectory = textBox2.Text,
                    UseShellExecute = !noWindow,
                    CreateNoWindow = noWindow
                };
                p = new Process();
                p.StartInfo = pci;
                p.Start();
                p.WaitForExit();


            }
            Directory.Delete(pp, true);

        }

        bool noWindow = true;
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            noWindow = checkBox1.Checked;
        }
    }
}
