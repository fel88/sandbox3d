using OpenTK;
using PolyBoolCS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RenderTool;

namespace RenderTool
{
    public partial class BlankEditorDialog : Form
    {
        public BlankEditorDialog()
        {
            InitializeComponent();
            Load += BlankEditorDialog_Load;
            pictureBox1.Paint += PictureBox1_Paint;
            pictureBox1.MouseWheel += PictureBox1_MouseWheel;
            pictureBox1.PreviewKeyDown += PictureBox1_PreviewKeyDown;
            KeyDown += BlankEditorDialog_KeyDown;
            KeyPreview = true;
        }

        private void PictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Render(e.Graphics);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return && this.CreatePointsMode)
            {
                ApplyBlank();
                Mode = BlankEditorMode.EditPoints;
                //this.OnModeChanged();
                return;
            }
            if (e.KeyCode == Keys.Back && this.CreatePointsMode)
            {
                if (this.EditedBlank.Points.Count > 0)
                {
                    this.EditedBlank.Points.RemoveAt(this.EditedBlank.Points.Count - 1);
                }

                return;
            }
            if (e.KeyCode == Keys.Escape)
            {
                if (this.Mode == BlankEditorMode.SplitEdge)
                {
                    this.Mode = BlankEditorMode.EditPoints;
                }
                else
                {
                    this.EditedBlank = null;
                    this.Mode = BlankEditorMode.CreatePoints;

                }

                return;
            }
            if (e.KeyCode == Keys.Delete)
            {
                if (this.dragIndex != -1)
                {
                    if (this.EditedBlank.Points.Count > 3)
                    {
                        this.EditedBlank.Points.RemoveAt(this.dragIndex);
                        this.dragIndex = -1;

                    }

                    return;
                }
                if (this.selectedBlank != null)
                {
                    if (this.selectedBlank.Parent != null)
                    {
                        this.selectedBlank.Parent.Childs.Remove(this.selectedBlank);
                        return;
                    }
                    Blanks.Remove(this.selectedBlank);
                }
            }
            base.OnKeyDown(e);
        }

        private void BlankEditorDialog_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void PictureBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {



        }

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
            {
                ZoomTo(e.Location, this.DrawingScale / 1.23f);
            }
            if (e.Delta > 0)
            {
                ZoomTo(e.Location, this.DrawingScale * 1.23f);
            }
        }

        public void ZoomTo(System.Drawing.Point location, float newDrawingScale)
        {
            if (!this.CheckScale(newDrawingScale))
            {
                return;
            }
            PointF realLocation = this.WorldFromScreen(location);
            this.Center.X = realLocation.X - (realLocation.X - this.Center.X) * this.DrawingScale / newDrawingScale;
            this.Center.Y = realLocation.Y - (realLocation.Y - this.Center.Y) * this.DrawingScale / newDrawingScale;
            this.DrawingScale = newDrawingScale;
        }

        private bool CheckScale(float newScale)
        {
            return (double)newScale >= 0.03 && newScale <= 1000f;
        }

        private void BlankEditorDialog_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();
            Application.AddMessageFilter(mf);

        }


        void updateList()
        {
            listView1.Items.Clear();
            foreach (var oo in model.Lights)
            {
                //if (oo == null) continue;
                listView1.Items.Add(new ListViewItem(oo == null ? "(null)" : "light") { Tag = oo });
            }
        }
        MessageFilter mf = null;

        internal void Init(BlankModel blankModel)
        {
            model = blankModel;
            updateList();

            if (blankModel is BlankModel bm)
            {
                Blanks.Add(bm.Blank);
                EditedBlank = bm.Blank;
            }
            Mode = BlankEditorMode.EditPoints;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
        public PointF Center;
        public void DrawBlankAnchors(Blank blank, Graphics gr)
        {

            List<PointF> points = blank.Points;
            if (selectedBlank == blank)
            {
                Pen bpen = new Pen(Color.White);
                bpen.DashPattern = new float[]
                {
                    10f,
                    10f
                };
                RectangleF bb = selectedBlank.BoundingBox();
                if (blank.IsRelative)
                {
                    bb.X -= ZeroOffset.X;
                    bb.Y -= ZeroOffset.Y;
                }
                System.Drawing.Point p = ScreenFromReal(new PointF(bb.X, bb.Y));
                float w = bb.Width * DrawingScale;
                float h = bb.Height * DrawingScale;
                gr.DrawRectangle(bpen, (float)p.X, (float)p.Y - h, w, h);
            }
            for (int index = 0; index < points.Count; index++)
            {
                PointF point = points[index];
                if (blank.IsRelative)
                {
                    point.X -= ZeroOffset.X;
                    point.Y -= ZeroOffset.Y;
                }
                System.Drawing.Point p2 = ScreenFromReal(point);
                if (hoveredIndex == index && blank == hoveredBlank)
                {
                    gr.DrawRectangle(Pens.Yellow, p2.X - 2, p2.Y - 2, 4, 4);
                }
                else
                {
                    gr.DrawRectangle(Pens.Red, p2.X - 2, p2.Y - 2, 4, 4);
                }
                int arg_1D7_0 = dragIndex;
            }
        }
        public void DrawBlank(Blank blank, Graphics gr)
        {
            List<PointF> points = blank.Points;

            gr.FillPolygon(new SolidBrush(Color.FromArgb(64, Color.Red)), points.Select(z => ScreenFromReal(z)).ToArray());
        }

        BlankModel model;
        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Invalidate();
        }

        void Render(Graphics gr)
        {
            gr.Clear(Color.Gray);

            gr.ResetTransform();

            PointF arg_57_0 = Center;
            float arg_68_0 = DrawingScale;
            PointF arg_79_0 = ZeroOffset;


            {
                ScreenFromReal(new PointF(0f, 0f));
                Pen pen = new Pen(CreatePointsMode ? Brushes.Blue : Brushes.White, 1f);
                pen.DashPattern = new float[]
                {
                    10f,
                    5f
                };
                if (EditedBlank != null)
                {
                    System.Drawing.Point[] pnts;
                    if (!EditedBlank.IsRelative)
                    {
                        pnts = (from z in EditedBlank.Points
                                select ScreenFromReal(z)).ToArray<System.Drawing.Point>();
                    }
                    else
                    {
                        pnts = (from z in EditedBlank.Points
                                select ScreenFromReal(new PointF(z.X - ZeroOffset.X, z.Y - ZeroOffset.Y))).ToArray<System.Drawing.Point>();
                    }
                    if (pnts.Length >= 3)
                    {
                        gr.DrawPolygon(pen, pnts);
                    }
                    else if (pnts.Length == 2)
                    {
                        gr.DrawLine(pen, pnts[0], pnts[1]);
                    }
                }
                foreach (Blank blank in Blanks)
                {
                    List<PointF> arg_1A7_0 = blank.Points;
                    GraphicsPath path = new GraphicsPath();
                    //DrawBlank(blank, gr);
                    path.AddPolygon(blank.Points.Select(z => ScreenFromReal(z)).ToArray());
                    this.DrawBlankAnchors(blank, gr);
                    foreach (Blank child in blank.Childs)
                    {
                        //DrawBlank(child, gr);
                        path.AddPolygon(child.Points.Select(z => ScreenFromReal(z)).Reverse().ToArray());
                        this.DrawBlankAnchors(child, gr);
                    }

                    gr.FillPath(new SolidBrush(Color.FromArgb(64, Color.Blue)), path);
                    //gr.DrawPath(new Pen(Color.White, 2f), path);
                }
                if (EditedBlank != null)
                {
                    for (int index = 0; index < EditedBlank.Points.Count; index++)
                    {
                        PointF point = EditedBlank.Points[index];
                        if (EditedBlank.IsRelative)
                        {
                            point.X -= ZeroOffset.X;
                            point.Y -= ZeroOffset.Y;
                        }
                        System.Drawing.Point p = ScreenFromReal(point);
                        if (hoveredIndex == index)
                        {
                            gr.DrawRectangle(Pens.Yellow, p.X - 2, p.Y - 2, 4, 4);
                        }
                        else
                        {
                            gr.DrawRectangle(Pens.Red, p.X - 2, p.Y - 2, 4, 4);
                        }
                        if (dragIndex == index)
                        {
                            gr.DrawRectangle(Pens.Blue, p.X - 5, p.Y - 5, 10, 10);
                        }
                    }
                    if (Mode == BlankEditorMode.CreateCut && EditedBlank.Points.Count > 0 && DrawCreateCutLine && CreateCutPosition.X != float.NaN && CreateCutPosition.Y != float.NaN)
                    {
                        System.Drawing.Point point2 = ScreenFromReal(EditedBlank.Points.Last<PointF>());
                        gr.DrawLine(Pens.LightGray, point2, ScreenFromReal(CreateCutPosition));
                    }
                }
                if (Mode == BlankEditorMode.SplitEdge && startSplitIndex != -1)
                {
                    System.Drawing.Point spl = ScreenFromReal(SplitTarget);
                    gr.DrawLine(Pens.White, spl.X - 10, spl.Y, spl.X + 10, spl.Y);
                    gr.DrawLine(Pens.White, spl.X, spl.Y - 10, spl.X, spl.Y + 10);
                }
            }
        }

        public int ScreenFromRealX(double x)
        {
            return (int)((x - (double)this.Center.X) * (double)this.DrawingScale + (double)((float)base.Width / 2f));
        }
        public int ScreenFromRealY(double y)
        {
            return (int)(((double)this.Center.Y - y) * (double)this.DrawingScale + (double)((float)base.Height / 2f));
        }


        private System.Drawing.Point ScreenFromReal(PointF createCutPosition)
        {
            return new System.Drawing.Point(this.ScreenFromRealX(createCutPosition.X), this.ScreenFromRealY(createCutPosition.Y));
        }

        public PointF CreateCutPosition = new PointF(float.NaN, float.NaN);

        public int dragIndex = -1;

        public bool DrawCreateCutLine;

        public Blank EditedBlank;

        public int endSplitIndex = -1;

        public Blank hoveredBlank;

        public int hoveredIndex = -1;

        private bool isDrag;

        public BlankEditorMode Mode;

        private void assignLightToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var light = listView1.SelectedItems[0].Tag as PBRLight;
            model.Lights.Add(light.Clone() as PBRLight);
            updateList();
        }


        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            propertyGrid1.SelectedObject = tag;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var pd = propertyGrid1.SelectedGridItem.PropertyDescriptor;
            var tp = propertyGrid1.SelectedObject.GetType();
            var prop = tp.GetProperty(pd.Name);
            if (prop.PropertyType == typeof(Vector3))
            {
                var val = (Vector3)prop.GetValue(propertyGrid1.SelectedObject);
                vec3EditorDialog d = new vec3EditorDialog();
                d.Init(val);
                d.ShowDialog();
                prop.SetValue(propertyGrid1.SelectedObject, new Vector3(d.X, d.Y, d.Z));
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            if (!Helpers.Question("Delete light?")) return;
            var pl = listView1.SelectedItems[0].Tag as PBRLight;
            model.Lights.Remove(pl);
            updateList();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Blank bb = EditedBlank;
            if (selectedBlank != null)
            {
                bb = selectedBlank;
            }
            if (bb == null)
            {
                Helpers.Error("edited blank empty");
                return;
            }
            var bm = RenderHelpers.GetBlankModel(bb, new PointF(0, 0));
            //PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();
            model.Model = bm;
            model.Blank = bb;
            model.DeleteVao();
        }
        public bool CreatePointsMode
        {
            get
            {
                return this.Mode == BlankEditorMode.CreatePoints || this.Mode == BlankEditorMode.CreateCut;
            }

        }
        public PointF ZeroOffset { get; internal set; }
        public PointF SplitTarget;

        public Blank SplitEdgeBlank;
        private void SplitEdge()
        {
            if (this.SplitEdgeBlank != null)
            {
                if (this.SplitEdgeBlank.IsRelative)
                {
                    this.SplitEdgeBlank.Points.Insert(this.endSplitIndex, new PointF(this.SplitTarget.X + ZeroOffset.X, this.SplitTarget.Y + ZeroOffset.Y));
                    return;
                }
                this.SplitEdgeBlank.Points.Insert(this.endSplitIndex, this.SplitTarget);
            }
        }
        public int startSplitIndex = -1;
        internal PointF WorldFromScreen(System.Drawing.Point location)
        {
            return new PointF(this.WorldFromScreenX(location.X), this.RealFromScreenY(location.Y));
        }

        public double WorldFromScreenX_d(int x)
        {
            return (double)(((float)x - (float)base.Width / 2f) / this.DrawingScale + this.Center.X);
        }

        public double WorldFromScreenY_d(int y)
        {
            return (double)(-(double)((float)y - (float)base.Height / 2f) / this.DrawingScale + this.Center.Y);
        }

        public float WorldFromScreenX(int x)
        {
            return (float)this.WorldFromScreenX_d(x);
        }

        public float RealFromScreenY(int y)
        {
            return (float)this.WorldFromScreenY_d(y);
        }

        List<Blank> Blanks { get; set; } = new List<Blank>();

        private PointF start = new PointF(float.NaN, float.NaN);
        public Blank selectedBlank;
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                DragStart = e.Location;
                DrawingCenterStart = Center;
            }
            PointF pos = WorldFromScreen(e.Location);
            this.start = e.Location;
            if (e.Button == MouseButtons.Left && this.CreatePointsMode)
            {
                PointF pp = WorldFromScreen(e.Location);
                pp.X += ZeroOffset.X;
                pp.Y += ZeroOffset.Y;
                if (Control.ModifierKeys == Keys.Shift && this.Mode == BlankEditorMode.CreateCut && this.EditedBlank.Points.Count > 0)
                {
                    this.EditedBlank.Points.Add(this.CreateCutPosition);
                }
                else
                {
                    this.EditedBlank.Points.Add(pp);
                }

            }
            if (e.Button == MouseButtons.Left && this.Mode == BlankEditorMode.EditPoints)
            {
                if (this.hoveredIndex != -1)
                {
                    this.isDrag = true;
                    this.dragIndex = this.hoveredIndex;
                    this.selectedBlank = null;
                    this.EditedBlank = this.hoveredBlank;

                }
                else
                {
                    this.selectedBlank = null;
                    foreach (Blank blank in Blanks)
                    {
                        PointF pos2 = pos;
                        if (blank.IsRelative)
                        {
                            pos2.X += ZeroOffset.X;
                            pos2.Y += ZeroOffset.Y;
                        }
                        if (Helpers.pnpoly(blank.Points.ToArray(), pos2.X, pos2.Y))
                        {
                            this.selectedBlank = blank;
                            this.isDrag = false;
                            this.dragIndex = -1;
                            this.hoveredIndex = -1;
                            this.EditedBlank = null;
                        }
                        foreach (Blank blankChild in blank.Childs)
                        {
                            if (Helpers.pnpoly(blankChild.Points.ToArray(), pos2.X, pos2.Y))
                            {
                                this.selectedBlank = blankChild;
                                this.isDrag = false;
                                this.dragIndex = -1;
                                this.hoveredIndex = -1;
                                this.EditedBlank = null;
                            }
                        }
                    }
                }
            }
            if (e.Button == MouseButtons.Left && this.Mode == BlankEditorMode.SplitEdge && this.startSplitIndex != -1 && this.endSplitIndex != -1)
            {
                this.SplitEdge();
                this.Mode = BlankEditorMode.EditPoints;

            }

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            this.isDrag = false;

        }
        public double dist2(PointF p1, PointF p2)
        {
            return Math.Pow((double)(p1.X - p2.X), 2.0) + Math.Pow((double)(p1.Y - p2.Y), 2.0);
        }

        public double gett(PointF v, PointF w, PointF p)
        {
            double l2 = this.dist2(v, w);
            if (l2 == 0.0)
            {
                return this.dist2(p, v);
            }
            double t = (double)((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
            return Math.Max(0.0, Math.Min(1.0, t));
        }

        public double distToSegmentSquared(PointF v, PointF w, PointF p)
        {
            double l2 = this.dist2(v, w);
            if (l2 == 0.0)
            {
                return this.dist2(p, v);
            }
            double t = (double)((p.X - v.X) * (w.X - v.X) + (p.Y - v.Y) * (w.Y - v.Y)) / l2;
            t = Math.Max(0.0, Math.Min(1.0, t));
            PointF p2 = default(PointF);
            p2.X = (float)((double)v.X + t * (double)(w.X - v.X));
            p2.Y = (float)((double)v.Y + t * (double)(w.Y - v.Y));
            return this.dist2(p, p2);
        }

        // drawingUnicont.DrawingTools
        public System.Drawing.Point DragStart = new System.Drawing.Point(-2147483648, -2147483648);
        public PointF DrawingCenterStart;
        public float DrawingScale { get; set; } = 1;

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {            
            PointF pos = WorldFromScreen(e.Location);

            if (e.Button == MouseButtons.Right)
            {
                var dx = DragStart.X - e.Location.X;
                var dy = DragStart.Y - e.Location.Y;

                Center = new PointF(this.DrawingCenterStart.X + (float)(dx) / this.DrawingScale, this.DrawingCenterStart.Y - (float)(dy) / this.DrawingScale);
            }

            if (this.Mode == BlankEditorMode.CreateCut && this.EditedBlank.Points.Count > 0)
            {
                if (Control.ModifierKeys == Keys.Shift)
                {
                    PointF start = this.EditedBlank.Points.Last<PointF>();
                    PointF end = WorldFromScreen(e.Location);
                    
                    double arg_CC_0 = (new PointF(end.X, start.Y).DistTo(end));
                    double yAxisDist = (new PointF(start.X, end.Y).DistTo(end));
                    if (arg_CC_0 > yAxisDist)
                    {
                        this.CreateCutPosition = new PointF(start.X, end.Y);
                    }
                    else
                    {
                        this.CreateCutPosition = new PointF(end.X, start.Y);
                    }
                    this.DrawCreateCutLine = true;
                }
                else
                {
                    this.CreateCutPosition = e.Location;
                    this.DrawCreateCutLine = false;
                }

            }
            if (this.Mode == BlankEditorMode.SplitEdge)
            {
                double mind = 1.7976931348623157E+308;
                int mindx = -1;
                this.startSplitIndex = -1;
                this.endSplitIndex = -1;
                double mint = -1.0;
                Blank cb = null;
                int treshold = 100;
                foreach (Blank blank in Blanks)
                {
                    for (int index = 0; index < blank.Points.Count; index++)
                    {
                        int i = index - 1;
                        if (i < 0)
                        {
                            i = blank.Points.Count + i;
                        }
                        PointF pos2 = pos;
                        if (blank.IsRelative)
                        {
                            pos2.X += ZeroOffset.X;
                            pos2.Y += ZeroOffset.Y;
                        }
                        double dist = this.distToSegmentSquared(blank.Points[index], blank.Points[i], pos2);
                        if (dist < mind && dist < Math.Pow((double)treshold, 2.0) / (double)DrawingScale)
                        {
                            cb = blank;
                            mind = dist;
                            mindx = index;
                            this.startSplitIndex = i;
                            this.endSplitIndex = index;
                            mint = this.gett(blank.Points[i], blank.Points[index], pos2);
                        }
                    }
                    foreach (Blank blankChild in blank.Childs)
                    {
                        for (int index2 = 0; index2 < blankChild.Points.Count; index2++)
                        {
                            int i2 = index2 - 1;
                            if (i2 < 0)
                            {
                                i2 = blankChild.Points.Count + i2;
                            }
                            PointF pos3 = pos;
                            if (blank.IsRelative)
                            {
                                pos3.X += ZeroOffset.X;
                                pos3.Y += ZeroOffset.Y;
                            }
                            double dist2 = this.distToSegmentSquared(blankChild.Points[index2], blankChild.Points[i2], pos3);
                            if (dist2 < mind && dist2 < Math.Pow((double)treshold, 2.0) / (double)DrawingScale)
                            {
                                cb = blankChild;
                                mind = dist2;
                                mindx = index2;
                                this.startSplitIndex = i2;
                                this.endSplitIndex = index2;
                                mint = this.gett(blankChild.Points[i2], blankChild.Points[index2], pos3);
                            }
                        }
                    }
                }
                if (mindx != -1)
                {
                    this.SplitEdgeBlank = cb;
                    float dx = cb.Points[this.endSplitIndex].X - cb.Points[this.startSplitIndex].X;
                    float dy = cb.Points[this.endSplitIndex].Y - cb.Points[this.startSplitIndex].Y;
                    double xx = (double)cb.Points[this.startSplitIndex].X + (double)dx * mint;
                    double yy = (double)cb.Points[this.startSplitIndex].Y + (double)dy * mint;
                    this.SplitTarget = new PointF((float)xx, (float)yy);
                    if (this.SplitEdgeBlank.IsRelative)
                    {
                        this.SplitTarget = new PointF((float)(xx - (double)ZeroOffset.X), (float)(yy - (double)ZeroOffset.Y));
                    }
                }

            }
            if (this.Mode == BlankEditorMode.EditPoints)
            {
                float mind2 = 3.40282347E+38f;
                int mindx2 = -1;
                this.hoveredBlank = null;
                foreach (Blank blank2 in Blanks)
                {
                    PointF[] pp = blank2.Points.ToArray();
                    for (int j = 0; j < pp.Length; j++)
                    {
                        PointF blankPoint = pp[j];
                        if (blank2.IsRelative)
                        {
                            blankPoint.X -= ZeroOffset.X;
                            blankPoint.Y -= ZeroOffset.Y;
                        }
                        float dist3 = (float)Math.Sqrt(Math.Pow((double)(blankPoint.X - pos.X), 2.0) + Math.Pow((double)(blankPoint.Y - pos.Y), 2.0));
                        if (dist3 < mind2 && dist3 < 20f / DrawingScale)
                        {
                            mind2 = dist3;
                            this.hoveredBlank = blank2;
                            mindx2 = j;
                        }
                    }
                    foreach (Blank blankChild2 in blank2.Childs)
                    {
                        pp = blankChild2.Points.ToArray();
                        for (int k = 0; k < pp.Length; k++)
                        {
                            PointF blankPoint2 = pp[k];
                            if (blank2.IsRelative)
                            {
                                blankPoint2.X -= ZeroOffset.X;
                                blankPoint2.Y -= ZeroOffset.Y;
                            }
                            float dist4 = (float)Math.Sqrt(Math.Pow((double)(blankPoint2.X - pos.X), 2.0) + Math.Pow((double)(blankPoint2.Y - pos.Y), 2.0));
                            if (dist4 < mind2 && dist4 < 20f / DrawingScale)
                            {
                                mind2 = dist4;
                                this.hoveredBlank = blankChild2;
                                mindx2 = k;
                            }
                        }
                    }
                }
                if (this.EditedBlank != null)
                {
                    for (int index3 = 0; index3 < this.EditedBlank.Points.Count; index3++)
                    {
                        PointF blankPoint3 = this.EditedBlank.Points[index3];
                        if (this.EditedBlank.IsRelative)
                        {
                            blankPoint3.X -= ZeroOffset.X;
                            blankPoint3.Y -= ZeroOffset.Y;
                        }
                        float dist5 = (float)Math.Sqrt(Math.Pow((double)(blankPoint3.X - pos.X), 2.0) + Math.Pow((double)(blankPoint3.Y - pos.Y), 2.0));
                        if (dist5 < mind2 && dist5 < 20f / DrawingScale)
                        {
                            mind2 = dist5;
                            mindx2 = index3;
                        }
                    }
                }
                this.hoveredIndex = mindx2;
            }
            if (this.isDrag && this.EditedBlank != null && this.EditedBlank.Points.Count > this.dragIndex)
            {
                if (this.EditedBlank.IsRelative)
                {
                    this.EditedBlank.Points[this.dragIndex] = new PointF(pos.X + ZeroOffset.X, pos.Y + ZeroOffset.Y);
                }
                else
                {
                    this.EditedBlank.Points[this.dragIndex] = pos;
                }
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Mode = BlankEditorMode.SplitEdge;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            EditedBlank = new Blank
            {
                IsRelative = true
            };
            Mode = BlankEditorMode.CreateCut;
        }


        public void ApplyBlank()
        {
            if (EditedBlank == null)
            {
                return;
            }
            if (this.Mode != BlankEditorMode.CreateCut)
            {
                if (!Blanks.Contains(this.EditedBlank) && this.EditedBlank.Parent == null)
                {
                    Blank i = new Blank(this.EditedBlank.Points.ToArray());
                    i.IsRelative = this.EditedBlank.IsRelative;
                    if (this.EditedBlank.Points.Count > 2)
                    {
                        Blanks.Add(i);
                    }
                    this.EditedBlank.Points.Clear();
                }

                return;
            }
            if (this.EditedBlank.Points.Count > 2)
            {
                this.ApplyCut();
                return;
            }
            this.EditedBlank = null;
        }


        public Polygon GetPolygon(Blank blank)
        {
            Polygon p = new Polygon();
            p.regions = new List<PointList>();
            foreach (Blank v in blank.Childs)
            {
                PointList pps = this.GetPointList(v);
                p.regions.Add(pps);
            }
            PointList plist = this.GetPointList(blank);
            p.regions.Add(plist);
            return p;
        }


        public PointList GetPointList(Blank blank)
        {
            PointList plist = new PointList();
            PointF zo = ZeroOffset;
            foreach (PointF blankPoint in blank.Points)
            {
                plist.Add(new PolyBoolCS.Point((double)(blankPoint.X - zo.X), (double)(blankPoint.Y - zo.Y)));
            }
            return plist;
        }

        public void ApplyCut()
        {
            PolyBool clipper = new PolyBool();
            Polygon cl = this.GetPolygon(this.EditedBlank);
            List<Blank> toAdd = new List<Blank>();
            PointF zo = ZeroOffset;
            foreach (Blank blank in Blanks)
            {
                Polygon v = this.GetPolygon(blank);
                foreach (var arg in clipper.difference(v, cl).regions)
                {
                    Blank b = new Blank
                    {
                        IsRelative = true
                    };
                    foreach (PolyBoolCS.Point pn in arg)
                    {
                        b.Points.Add(new PointF((float)pn.x + zo.X, (float)pn.y + zo.Y));
                    }
                    toAdd.Add(b);
                }
            }
            Blanks.Clear();
            Blanks.AddRange(toAdd);
            List<Blank> toDel = new List<Blank>();
            for (int i = 0; i < Blanks.Count; i++)
            {
                for (int j = 0; j < Blanks.Count; j++)
                {
                    if (i != j)
                    {
                        Blank d2 = Blanks[i];
                        Blank d3 = Blanks[j];
                        PointF f0 = d3.Points[0];
                        if (Helpers.pnpoly(d2.Points.ToArray(), f0.X, f0.Y))
                        {
                            d3.Parent = d2;
                            if (!d2.Childs.Contains(d3))
                            {
                                d2.Childs.Add(d3);
                                toDel.Add(d3);
                            }
                        }
                    }
                }
            }
            foreach (var tt in toDel)
            {
                Blanks.Remove(tt);
            }
            this.EditedBlank = null;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {

            if (Mode == BlankEditorMode.CreatePoints || Mode == BlankEditorMode.CreateCut)
            {
                ApplyBlank();
                Mode = BlankEditorMode.EditPoints;

                return;
            }
            EditedBlank = null;
            Mode = BlankEditorMode.EditPoints;

        }

        private void addLightToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (model.Lights.Count >= 4)
            {
                Helpers.Error("impossible to create more than 4 source of light");
                return;
            }
            model.Lights.Add(new PBRLight(model.Parent));
            updateList();
        }
    }
}
