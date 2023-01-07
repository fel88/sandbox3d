using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class Model
    {
        public object Tag { get; set; }
        public void CalcNormals()
        {
            foreach (var v in Polygons)
            {
                //calc normal
                var v1 = v.Vertices[1].Position - v.Vertices[0].Position;
                var v2 = v.Vertices[2].Position - v.Vertices[0].Position;
                var nrm = Vector3d.Cross(v1, v2);
                nrm.Normalize();

                foreach (var vector3 in v.Vertices)
                {
                    vector3.Normal = nrm;
                }
            }
        }

        public Color Color { get; set; } = Color.White;
        public Color Wirecolor { get; set; } = Color.Black;

        public List<DrawPolygon> Polygons = new List<DrawPolygon>();

        public void Triangulate()
        {

            List<DrawPolygon> pp = new List<DrawPolygon>();
            foreach (var drawPolygon in Polygons)
            {
                if (drawPolygon.Vertices.Length > 3)
                {
                    for (int i = 2; i < drawPolygon.Vertices.Length; i++)
                    {
                        var v0 = drawPolygon.Vertices[0];
                        var v1 = drawPolygon.Vertices[i - 1];
                        var v2 = drawPolygon.Vertices[i];
                        pp.Add(new DrawPolygon() { Vertices = new[] { v0, v1, v2 } });
                    }
                }
                else
                {
                    pp.Add(drawPolygon);
                }
            }

            Polygons = pp;
        }

        public DrawPolygon SelectedPolygon;

        public bool Enable { get; set; } = true;

        public void DrawWireframe()
        {
            if (!Enable) return;

            var plg = Polygons;

            //GL.Disable(EnableCap.Lighting);
            //GL.Color3(Color.White);
            //GL.LineWidth(2);

            foreach (var item in plg)
            {
                if (item.Vertices.Count() > 2)
                {
                    if (item.IsWireframe)
                    {
                        GL.Begin(PrimitiveType.LineLoop);
                        foreach (var vitem in item.Vertices)
                        {
                            GL.Normal3(vitem.Normal);
                            GL.Vertex3(vitem.Position);
                        }

                        GL.End();
                    }
                }
            }
        }

        public void Draw()
        {
            if (!Enable) return;
            var plg = Polygons;
            foreach (var item in plg)
            {
                if (item.Vertices.Count() > 2)
                {

                    GL.Begin(item.DrawType);
                    foreach (var vitem in item.Vertices)
                    {
                        GL.Normal3(vitem.Normal);
                        GL.Vertex3(vitem.Position);
                    }
                    GL.End();
                }
            }
        }

        public static Model FromArray(Vector3d[][] toArray)
        {
            var m = new Model();
            m.Polygons = toArray.Select(z => new DrawPolygon()
            { Vertices = z.Select(u => new DrawVertex() { Position = u }).ToArray() }).ToList();

            m.CalcNormals();

            return m;
        }

        public Model Clone()
        {
            Model ret = new Model();
            foreach (var polygon in Polygons)
            {
                ret.Polygons.Add(new DrawPolygon());
                ret.Polygons.Last().IsWireframe = polygon.IsWireframe;
                List<DrawVertex> vv = new List<DrawVertex>();
                foreach (var polygonVertex in polygon.Vertices)
                {
                    vv.Add(new DrawVertex() { Position = new Vector3d(polygonVertex.Position.X, polygonVertex.Position.Y, polygonVertex.Position.Z) });
                }

                ret.Polygons.Last().Vertices = vv.ToArray();
            }
            ret.CalcNormals();
            return ret;
        }
    }

    
}