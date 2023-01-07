using System;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class mdi : Form
    {
        public mdi()
        {
            InitializeComponent();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Form1 f = new Form1();
            f.MdiParent = this;
            f.Show();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            TexturesManager u = new TexturesManager();
            u.MdiParent = this;
            u.Show();
        }

        private void rarzipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RarZipConverter r = new RarZipConverter();
            r.MdiParent = this;
            r.Show();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            AboutBox1 ab = new AboutBox1();
            ab.ShowDialog();
        }
    }
}
