using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class offscreenRender : Form
    {
        public offscreenRender()
        {
            InitializeComponent();
            ctx1.Init(pictureBox4);

            textBox7.Text = maxW.ToString();
            textBox8.Text = maxH.ToString();

            pictureBox1.MouseDown += PictureBox1_MouseDown;
            pictureBox4.MouseDown += PictureBox4_MouseDown;
            pictureBox4.MouseWheel += PictureBox4_MouseWheel;
            
            Load += Form1_Load;
            Shown += OffscreenRender_Shown;       
        }

        private void OffscreenRender_Shown(object sender, EventArgs e)
        {
            updateCamerasList();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);
        }

        
        MessageFilter mf = null;
        private void PictureBox4_MouseWheel(object sender, MouseEventArgs e)
        {
            ctx1.PictureBox1_MouseWheel(sender, e);
        }

        
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            
        }
        private void PictureBox4_MouseDown(object sender, MouseEventArgs e)
        {
           
        }

        public static int maxW = 3000;
        public static int maxH = 1500;

                
        public DrawingContext ctx1 = new DrawingContext();

        public void skipRender(Form1 f1)
        {
            if (!f1.HighlightSelected) return;
            lock (f1.lock1)
            {
                
                f1.renderMode = true;
                f1.grabRequired = true;
            }
            f1.Render();
        }

        void renderItem(bool showInPictureBox = true)
        {
            if (!Program.MainForm.MdiChildren.Where(z => z is Form1).Any()) return;
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            f1.TimerEnabled = false;

            skipRender(f1);//to skip RED selection

            
            f1.renderMode = true;
            f1.grabRequired = true;
            f1.Render();
            if (showInPictureBox)
                pictureBox1.Invoke((Action)(() =>
                {
                    pictureBox1.Image = f1.lastGrab.Clone() as Bitmap;
                    drawBmp1 = f1.lastGrab.Clone() as Bitmap;

                }));

            
            
            f1.renderMode = false;
            f1.grabRequired = true;
            f1.Render();

            if (showInPictureBox)
                pictureBox2.Invoke((Action)(() =>
                {
                    pictureBox2.Image = f1.lastGrab.Clone() as Bitmap;                    
                }));

            
            f1.TimerEnabled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            renderItem();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void as8Bit10MaskToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            (_sourceControl as PictureBox).Image.Save(sfd.FileName);
        }

        Control _sourceControl = null;
        private void contextMenuStrip1_Opened(object sender, EventArgs e)
        {
            _sourceControl = contextMenuStrip1.SourceControl;
        }

        private void asRGB24bitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            if (sfd.ShowDialog() != DialogResult.OK) return;
            (_sourceControl as PictureBox).Image.Save(sfd.FileName);
        }
        private static float doffset = 0;
        
        public static bool pnpoly(PointF[] verts, PointF[] tests)
        {
            return tests.Any(z => Helpers.pnpoly(verts, z.X, z.Y));
        }
        Bitmap drawBmp1;
        
        public void Draw1()
        {
            //lastCenter1 = ctx1.BackTransform(new PointF(pictureBox1.Width / 2, pictureBox1.Height / 2));

            ctx1.gr.ResetTransform();
            ctx1.gr.SmoothingMode = SmoothingMode.AntiAlias;
            ctx1.gr.Clear(Color.White);
            var pos = pictureBox4.PointToClient(Cursor.Position);
            var posx = (pos.X / ctx1.zoom - ctx1.sx);
            var posy = (-pos.Y / ctx1.zoom - ctx1.sy);
            if (ctx1.isDrag)
            {
                var p = pictureBox4.PointToClient(Cursor.Position);

                ctx1.sx = ctx1.origsx + ((p.X - ctx1.startx) / ctx1.zoom);
                ctx1.sy = ctx1.origsy + (-(p.Y - ctx1.starty) / ctx1.zoom);
            }

            /*ctx1.gr.DrawString("X:" + posx.ToString("0.00") + " Y: " + posy.ToString("0.00"), new Font("Arial", 12),
                Brushes.Blue, 0, 0);*/
            ctx1.gr.DrawLine(Pens.Red, ctx1.Transform(new PointF(0, 0)), ctx1.Transform(new PointF
                (100, 0)));
            ctx1.gr.DrawLine(Pens.Blue, ctx1.Transform(new PointF(0, 0)), ctx1.Transform(new PointF(0, 100)));

            var p0 = ctx1.Transform(new PointF(0, 0));

            if (drawBmp1 != null)
                ctx1.gr.DrawImage(drawBmp1,
                    new RectangleF(p0.X, p0.Y, drawBmp1.Width * ctx1.zoom, drawBmp1.Height * ctx1.zoom),
                    new Rectangle(0, 0, drawBmp1.Width, drawBmp1.Height), GraphicsUnit.Pixel);

          

            ctx1.DrawUi();
            pictureBox4.Image = ctx1.Bmp;
        }
                

        public static GraphicsPath RoundedRect(RectangleF bounds, int radius)
        {
            int diameter = radius * 2;
            System.Drawing.Size size = new System.Drawing.Size(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public float[] GetKoefs(PointF a, PointF b)
        {
            float[] ret = new float[3];
            ret[0] = a.Y - b.Y;
            ret[1] = b.X - a.X;
            ret[2] = -(a.X * b.Y - b.X * a.Y);
            return ret;
        }

        public PointF? LinesIntersect(PointF a1, PointF a2, PointF b1, PointF b2)
        {
            var k1 = GetKoefs(a1, a2);
            var k2 = GetKoefs(b1, b2);
            return LinesIntersect(k1[0], k2[0], k1[1], k2[1], k1[2], k2[2]);
        }

        public PointF? LinesIntersect(float A1, float A2, float B1, float B2, float C1, float C2)
        {
            float delta = A1 * B2 - A2 * B1;

            if (delta == 0)
                return null;

            float x = (B2 * C1 - B1 * C2) / delta;
            float y = (A1 * C2 - A2 * C1) / delta;
            return new PointF(x, y);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Draw1();
        }

        
        public Scene Scene;
        
        
        private void updateCamerasList()
        {
            listView1.Items.Clear();
            foreach (var item in Scene.Objects.OfType<Camera>())
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.GetType().Name, item.Name }) { Tag = item });
            }
        }
        
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            try
            {
                f1.glControl.Width = int.Parse(textBox4.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            label5.Text = f1.glControl.Width + "x" + f1.glControl.Height;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            f1.glControl.Parent = null;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            try
            {
                f1.glControl.Height = int.Parse(textBox5.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            f1.ReattachGlControl();            
        }

        private void button11_Click(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            try
            {                
                f1.glControl.Width *= 2;
                f1.glControl.Height *= 2;
                /*for (int i = 0; i < editZone.Points.Length; i++)
                {
                    editZone.Points[i].X *= 2;
                    editZone.Points[i].Y *= 2;
                }*/
            }
            catch (Exception ex)
            {

            }
        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;

            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is Camera cc)
            {
                f1.SwitchCamera(cc);
            }
            
        }

                

        bool noiseEnabled = false;

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            noiseEnabled = checkBox2.Checked;
        }
        
        bool AddGaussianNoise(Mat mSrc, Mat mDst, double Mean = 0.0, double StdDev = 10.0)
        {

            Mat mSrc_16SC = new Mat();
            Mat mGaussian_noise = new Mat(mSrc.Size(), MatType.CV_16SC3);

            Cv2.Randn(mGaussian_noise, Scalar.All(Mean), Scalar.All(StdDev));

            mSrc.ConvertTo(mSrc_16SC, MatType.CV_16SC3);
            Cv2.AddWeighted(mSrc_16SC, 1.0, mGaussian_noise, 1.0, 0.0, mSrc_16SC);
            mSrc_16SC.ConvertTo(mDst, mSrc.Type());

            return true;
        }



        int stdDev = 10;
        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            try
            {
                stdDev = int.Parse(textBox6.Text);
            }
            catch (Exception ex)
            {

            }
        }

        

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            try
            {
                maxW = int.Parse(textBox7.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            try
            {
                maxH = int.Parse(textBox8.Text);
            }
            catch (Exception ex)
            {

            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
               
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            pictureBox2.Image.Save("temp.jpg");
            Process.Start("temp.jpg");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            f1.glControl.Dock = DockStyle.None;
        }
               
        
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            renderItem();
        }
    }
}