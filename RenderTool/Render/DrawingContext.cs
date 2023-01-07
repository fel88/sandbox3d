using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RenderTool
{
    public class DrawingContext
    {
        public float startx, starty;
        public float origsx, origsy;
        public bool isDrag = false;

        public void UpdateDrag()
        {
            if (isDrag)
            {
                var p = PictureBox.Control.PointToClient(Cursor.Position);

                sx = origsx + ((p.X - startx) / zoom);
                sy = origsy + (-(p.Y - starty) / zoom);
            }
        }

        public virtual void Draw()
        {

        }
        public PointF GetCursor()
        {
            var p = PictureBox.Control.PointToClient(Cursor.Position);
            var pn = BackTransform(p);
            return pn;
        }

        public virtual void PictureBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        public virtual void PictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            //PictureBox.Focus();
        }

        //public List<UiElement> Elements = new List<UiElement>();

        public void Init(PictureBox pb)
        {
            Init(new EventWrapperPictureBox(pb) { });
        }

        public void Init(EventWrapperPictureBox pb)
        {
            PictureBox = pb;
            pb.MouseWheelAction = PictureBox1_MouseWheel;
            pb.MouseUpAction = PictureBox1_MouseUp;
            pb.MouseDownAction = PictureBox1_MouseDown;
            pb.MouseMoveAction = PictureBox1_MouseMove;
            pb.SizeChangedAction = Pb_SizeChanged;

            Bmp = new Bitmap(pb.Control.Width, pb.Control.Height);
            gr = Graphics.FromImage(Bmp);
        }

        
        public float ZoomFactor = 1.5f;

        public virtual void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            //zoom *= Math.Sign(e.Delta) * 1.3f;
            //zoom += Math.Sign(e.Delta) * 0.31f;

            var pos = PictureBox.Control.PointToClient(Cursor.Position);
            if (!PictureBox.Control.ClientRectangle.IntersectsWith(new Rectangle(pos.X, pos.Y, 1, 1)))
            {
                return;
            }

            float zold = zoom;

            if (e.Delta > 0) { zoom *= ZoomFactor; } else { zoom /= ZoomFactor; }

            if (zoom < 0.08) { zoom = 0.08f; }
            if (zoom > 100) { zoom = 100f; }

            sx = -(pos.X / zold - sx - pos.X / zoom);
            sy = (pos.Y / zold + sy - pos.Y / zoom);
        }

        public virtual void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = PictureBox.Control.PointToClient(Cursor.Position);

            if (e.Button == MouseButtons.Right)
            {
                isDrag = true;

                startx = pos.X;
                starty = pos.Y;
                origsx = sx;
                origsy = sy;
            }
        }

        public virtual void Pb_SizeChanged(object sender, EventArgs e)
        {
            Bmp = new Bitmap(PictureBox.Control.Width, PictureBox.Control.Height);
            gr = Graphics.FromImage(Bmp);
        }

        public EventWrapperPictureBox PictureBox;

        public float sx, sy;
        public float zoom = 1;
        public Graphics gr;
        public Bitmap Bmp;

        public virtual PointF BackTransform(PointF p1)
        {
            var posx = (p1.X / zoom - sx);
            var posy = (-p1.Y / zoom - sy);
            return new PointF(posx, posy);
        }
                

        public virtual void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            isDrag = false;            
        }

        public void DrawUi()
        {
            
        }

        public virtual PointF Transform(PointF p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }

        public virtual PointF Transform(OpenCvSharp.Point p1)
        {
            return new PointF((p1.X + sx) * zoom, -(p1.Y + sy) * zoom);
        }

        public void FitToPoints(PictureBox pb, PointF[] points, int gap = 0)
        {
            var maxx = points.Max(z => z.X) + gap;
            var minx = points.Min(z => z.X) - gap;
            var maxy = points.Max(z => z.Y) + gap;
            var miny = points.Min(z => z.Y) - gap;

            var w = pb.Width;
            var h = pb.Height;

            var dx = maxx - minx;
            var kx = w / dx;
            var dy = maxy - miny;
            var ky = h / dy;

            var oz = zoom;
            var sz1 = new Size((int)(dx * kx), (int)(dy * kx));
            var sz2 = new Size((int)(dx * ky), (int)(dy * ky));
            zoom = kx;
            if (sz1.Width > w || sz1.Height > h) zoom = ky;
            //dx = Math.Max(dx, dy);

            var x = dx / 2 + minx;
            var y = dy / 2 + miny;


            sx = (w / 2f) / zoom - x;
            sy = -((h / 2f) / zoom + y);

            var test = Transform(new PointF(x, y));

        }
    }
}
