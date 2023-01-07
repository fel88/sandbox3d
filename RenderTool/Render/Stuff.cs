using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;

namespace RenderTool
{
    public class Stuff
    {
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
        public static OpenTK.Quaternion ToQuaternion(object f)
        {
            if (f is Quaternion)
            {
                var q = (Quaternion)f;
                return new OpenTK.Quaternion(q.X, q.Y, q.Z, q.W);
            }
            if (f is OpenTK.Quaternion)
            {
                return (OpenTK.Quaternion)f;
            }

            throw new ArgumentException("cant cast " + f.GetType().Name + " to opentk.quaternion");
        }

        public static Vector3 ToVector3(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public static List<string> LoadedObjs = new List<string>();


        public static List<ModelPathItem> ModelsPathes = new List<ModelPathItem>();

        public static List<TextureDescriptor> Textures { get; set; } = new List<TextureDescriptor>();
    }
}

