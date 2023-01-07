using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace RenderTool
{
    public class Blank
    {
        public List<PointF> Points = new List<PointF>();
        public List<Blank> Childs = new List<Blank>();
        public Blank Parent;
        public bool IsRelative;

        public Blank() { }
        public Blank(PointF[] pointFs)
        {
            Points = new List<PointF>(pointFs);
        }

        internal RectangleF BoundingBox()
        {
            float minX = 3.40282347E+38f;
            float minY = 3.40282347E+38f;
            float maxX = -3.40282347E+38f;
            float maxY = -3.40282347E+38f;
            for (int i = 0; i < this.Points.Count; i++)
            {
                minX = Math.Min(minX, this.Points[i].X);
                minY = Math.Min(minY, this.Points[i].Y);
                maxX = Math.Max(maxX, this.Points[i].X);
                maxY = Math.Max(maxY, this.Points[i].Y);
            }
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }

        public Blank Clone()
        {
            Blank ret = new Blank();
            ret.Points = Points.ToList();
            return ret;
        }
    }
}