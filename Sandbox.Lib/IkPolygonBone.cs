using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace Sandbox.Lib
{
    public class IkPolygonBone : IkBone
    {
        public IkPolygonBone(int cnt)
        {
            Vertices = new Vector3[cnt];
            AbsoluteVertices = new Vector3[cnt];
        }
        public Vector3[] Vertices;
        public Vector3[] AbsoluteVertices;
        //public Polygon Polygon;
        public override void SetScale(float scale)
        {
            for (int index = 0; index < Vertices.Length; index++)
            {
                Vertices[index] *= scale;
            }
        }

        public override void CalcAbsolute()
        {
            var q = Parent.CoordSystem.ExtractRotation();
            var tr = Parent.CoordSystem.ExtractTranslation();

            for (int i = 0; i < Vertices.Length; i++)
            {
                AbsoluteVertices[i] = q * Vertices[i];
                AbsoluteVertices[i] += tr;
            }

        }

        public override object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var r = new IkPolygonBone(Vertices.Count());
            context.CloneList.Add(new CloneItem() { Clone = r, Original = this });
            List<Vector3> vv = new List<Vector3>();
            foreach (var vector3 in Vertices)
            {
                vv.Add(new Vector3(vector3.X, vector3.Y, vector3.Z));

            }
            r.Vertices = vv.ToArray();
            r.Parent = (IkBonePool)Parent.Clone(context);

            return r;
        }
    }

}
