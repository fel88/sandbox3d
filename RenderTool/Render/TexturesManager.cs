using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class TexturesManager : Form
    {
        public TexturesManager()
        {
            InitializeComponent();
        }

        TexturePool _pool;
        ITextureLoad texLoad;
        public void Init(ITextureLoad t, TexturePool pool, bool flatMode = false)
        {
            _pool = pool;
            texLoad = t;
            listView1.Items.Clear();

            if (flatMode)
            {
                foreach (var item in pool.Textures.OfType<FlatTextureItem>())
                {
                    listView1.Items.Add(new ListViewItem(new string[] { Path.GetFileName(item.OriginFilePath), item.OriginFilePath }) { Tag = item });
                }
            }
            else
            {
                foreach (var item in pool.Textures.OfType<PBRTextureItem>())
                {
                    listView1.Items.Add(new ListViewItem(new string[] { Path.GetFileName(item.OriginFilePath), item.OriginFilePath }) { Tag = item });
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var fi = new FileInfo(ofd.FileName);
            updateList(fi.DirectoryName);
            lastPath = fi.DirectoryName;
        }
        void updateList(string path)
        {
            listView1.Items.Clear();
            string[] names = new string[] { "albedo",
            "normal",
            "ao",
            "metal",
            "rough","basecolor"};
            var di = new DirectoryInfo(path);
            foreach (var item in di.GetFiles("*.zip"))
            {
                if (!item.Name.ToLower().Contains(textBox1.Text.ToLower())) continue;

                bool good = false;
                string[] result;
                using (ZipArchive zip = ZipFile.Open(item.FullName, ZipArchiveMode.Read))
                {
                    result = names.Where(z => zip.Entries.Any(u => u.Name.ToLower().Contains(z))).ToArray();
                    if (result.Length >= 5) good = true;

                    if (result.Length < 3) continue;

                }
                listView1.Items.Add(new ListViewItem(new string[] { item.Name, string.Join(",", result) }) { Tag = item, ForeColor = good ? Color.Blue : Color.Red });
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            /*if (tag is FileInfo fi)
            {
                LoadTexturePreview(fi.FullName);
                texLoad.LoadTexture(fi.FullName);
            }*/
            if (tag is PBRTextureItem p)
            {
                if (texLoad is ISetTexture st)
                {
                    st.SetTexture(p);
                }
                else
                {
                    texLoad.LoadTexture(p);
                }
                LoadTexturePreview(p.OriginFilePath);
            }
            if (tag is FlatTextureItem ft)
            {
                if (texLoad is SceneTexture st)
                {
                    st.SetTexture(ft);
                }

                LoadFlatTexturePreview(ft.OriginFilePath);
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void openInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var fi = listView1.SelectedItems[0].Tag as FileInfo;
            Process.Start(fi.DirectoryName);
        }

        private void LoadTexturePreview(string fullName)
        {
            using (ZipArchive zip = ZipFile.Open(fullName, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.ToLower().Contains("albedo") || entry.Name.ToLower().Contains("basecolor"))
                    {
                        using (var st = entry.Open())
                        {
                            pictureBox1.Image = (Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                }

        }
        private void LoadFlatTexturePreview(string fullName)
        {
            pictureBox1.Image = Bitmap.FromFile(fullName);
        }

        string lastPath;
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updateList(lastPath);
        }
    }
}
