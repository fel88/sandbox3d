using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class TexturesPool : Form
    {
        public TexturesPool()
        {
            InitializeComponent();
            checkBox1.Checked = TexturePool.UnloadOnAssign;
            checkBox2.Checked = !TexturePool.SingleStageTextureLoadMode;
            checkBox3.Checked = TexturePool.CleanupByTime;
        }

        Scene _scene;
        public void Init(Scene scene)
        {
            _scene = scene;
            updateList();
        }

        void updateList()
        {
            listView1.Items.Clear();
            foreach (var s in _scene.Pool.Textures)
            {
                listView1.Items.Add(new ListViewItem(new string[] { Path.GetFileName(s.OriginFilePath), (s is PBRTextureItem) ? "PBR" : "Flat", s.OriginFilePath }) { Tag = s });
            }
        }
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            ofd.Filter = "zip PBR texture (*.zip)|*.zip|flat texture (*.jpg,*.png)|*.jpg;*.png";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            for (int i = 0; i < ofd.FileNames.Length; i++)
            {
                var fn = ofd.FileNames[i];
                if (fn.EndsWith(".zip"))
                {
                    var p = new PBRTextureItem();
                    p.StartLoad(fn);
                    _scene.Pool.Textures.Add(p);
                    var lvi = new ListViewItem(new string[] { Path.GetFileName(p.OriginFilePath), "PBR", p.OriginFilePath }) { Tag = p };
                    checkListViewItem(lvi, p.OriginFilePath);
                    listView1.Items.Add(lvi);
                }
                else
                {
                    var p = new FlatTextureItem();
                    p.StartLoad(fn);
                    _scene.Pool.Textures.Add(p);
                    var lvi = new ListViewItem(new string[] { Path.GetFileName(p.OriginFilePath), "Flat", p.OriginFilePath }) { Tag = p };                    
                    listView1.Items.Add(lvi);
                }
            }
        }

        bool checkListViewItem(ListViewItem lvi, string zipPath)
        {
            string[] exts = new[] { ".png", ".jpg", ".jpeg" };

            bool normalFound = false;
            bool roughFound = false;
            bool aoFound = false;
            bool metalFound = false;
            bool baseColorFound = false;
            using (ZipArchive zip = ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (!exts.Any(z => entry.Name.ToLower().EndsWith(z))) continue;

                    listView2.Items.Add(new ListViewItem(entry.Name) { Tag = entry.Name });
                    if (entry.Name.ToLower().Contains("rough"))
                    {
                        roughFound = true;
                    }
                    if (entry.Name.ToLower().Contains("normal"))
                    {
                        normalFound = true;
                    }
                    if (entry.Name.ToLower().Contains("ao") || entry.Name.ToLower().Contains("ambientOcclusion"))
                    {
                        aoFound = true;
                    }
                    if (entry.Name.ToLower().Contains("metal"))
                    {
                        metalFound = true;
                    }
                    if (entry.Name.ToLower().Contains("albedo") || entry.Name.ToLower().Contains("basecolor"))
                    {
                        baseColorFound = true;
                        using (var st = entry.Open())
                        {
                            pictureBox1.Image = (Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                }
            }
            bool found = metalFound & roughFound & aoFound & normalFound & baseColorFound;
            lvi.BackColor = Color.White;
            lvi.ForeColor = Color.Black;

            if (!found)
            {
                lvi.BackColor = Color.Red;
                lvi.ForeColor = Color.White;
            }
            return found;
        }
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;


            listView2.Items.Clear();

            if (listView1.SelectedItems[0].Tag is PBRTextureItem pbi)
            {
                if (!checkListViewItem(listView1.SelectedItems[0], pbi.OriginFilePath))
                {
                    pictureBox1.Image = null;
                }
            }
            else if (listView1.SelectedItems[0].Tag is FlatTextureItem ft)
            {
                pictureBox1.Image = Bitmap.FromFile(ft.OriginFilePath);
            }
        }

        bool isTexureUsedAnywhere(AbstractTexture tag)
        {            
            foreach (var item in _scene.Objects.OfType<PBRModel>())
            {
                if (item.GetTexture() == tag)
                    return true;
            }
            foreach (var item in _scene.Objects.OfType<SceneTexture>())
            {
                if (item.GetTexture() == tag)
                    return true;

            }
            return false;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is AbstractTexture abt)
            {

                
                foreach (var item in _scene.Objects.OfType<PBRModel>())
                {
                    if (item.GetTexture() == tag)
                    {
                        Helpers.Error($"texure assign to model {(item.Name == null ? "(null)" : item.Name)}. delete it first from there");
                        return;
                    }
                }
                foreach (var item in _scene.Objects.OfType<SceneTexture>())
                {
                    if (item.GetTexture() == tag)
                    {
                        Helpers.Error($"texure assign to model {(item.Name == null ? "(null)" : item.Name)}. delete it first from there");
                        return;
                    }
                }
                if (Helpers.Question("Sure?"))
                {
                    _scene.Pool.DeleteTexture(abt);
                    updateList();
                }
            }
        }

        private void showInExplorerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems[0].Tag == null) return;
            if (listView1.SelectedItems[0].Tag is AbstractTexture p)
            {

                string args = string.Format("/e, /select, \"{0}\"", p.OriginFilePath);

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "explorer";
                info.Arguments = args;
                Process.Start(info);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            TexturePool.UnloadOnAssign = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            TexturePool.SingleStageTextureLoadMode = !checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            TexturePool.CleanupByTime = checkBox3.Checked;
        }

        private void deleteAllUnsuedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Helpers.Question("sure?")) return;

            List<AbstractTexture> toDel = new List<AbstractTexture>();
            foreach (var item in _scene.Pool.Textures)
            {
                if (!isTexureUsedAnywhere(item))
                {
                    toDel.Add(item);
                }
            }

            if (toDel.Count == 0)
            {
                Helpers.Info("nothing to delete");
                return;
            }
            foreach (var item in toDel)
            {
                _scene.Pool.Textures.Remove(item);
            }

            updateList();
            Helpers.Info($"{toDel.Count} deleted");
        }
    }
}
