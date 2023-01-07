using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace RenderTool
{
    public static class Helpers
    {        
        public static double NextDouble(this Random r, double min, double max)
        {
            var diap = max - min;
            return r.NextDouble() * diap + min;
        }

        public static float NextFloat(this Random r, float min, float max)
        {
            var diap = max - min;
            return (float)(r.NextDouble() * diap + min);
        }

        public static double ParseDouble(string str)
        {
            return double.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }
        public static float ParseFloat(string str)
        {
            return float.Parse(str.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public static void AppendPairsToXml(string[][] pairs, StringBuilder sb)
        {
            foreach (var item in pairs)
            {
                sb.Append($"{item[0]}=\"{item[1]}\" ");
            }
        }

        public static void Error(string str)
        {
            MessageBox.Show(str, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public static void Warning(string str)
        {
            MessageBox.Show(str, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        public static void Info(string str)
        {
            MessageBox.Show(str, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        public static bool Question(string str)
        {
            return MessageBox.Show(str, "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
        public static bool pnpoly(PointF[] verts, float testx, float testy)
        {
            int nvert = verts.Length;
            int i, j;
            bool c = false;
            for (i = 0, j = nvert - 1; i < nvert; j = i++)
            {
                if (((verts[i].Y > testy) != (verts[j].Y > testy)) &&
                    (testx < (verts[j].X - verts[i].X) * (testy - verts[i].Y) / (verts[j].Y - verts[i].Y) + verts[i].X))
                    c = !c;
            }
            return c;
        }
    }
}
