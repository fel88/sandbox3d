using OpenTK;
using System;
using System.Drawing;

namespace RenderTool
{
    public static partial class Extensions
    {
        public static PointF ToPointF(this Vector2d p)
        {
            return new PointF((float)p.X, (float)p.Y);
        }

        public static float DistTo(this PointF p, PointF p2)
        {
            return (float)Math.Sqrt(Math.Pow(p.X - p2.X, 2) + Math.Pow(p.Y - p2.Y, 2));
        }
        public static PointF Normalize(this PointF p)
        {
            var l = (float)Math.Sqrt(Math.Pow(p.X, 2) + Math.Pow(p.Y, 2));
            return new PointF(p.X / l, p.Y / l);
        }
    }


}
