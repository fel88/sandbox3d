using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class DrawPolygon
    {
        public object Tag { get; set; }
        public DrawVertex[] Vertices;
        public PrimitiveType DrawType = PrimitiveType.TriangleFan;
        public bool IsWireframe { get; set; }

        public void CalcNormals()
        {
            var norm = Vector3d.Cross(Vertices[1].Position-Vertices[0].Position, Vertices[2].Position - Vertices[0].Position);
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = norm;
            }

        }
        internal void Scale(int cellW, int cellH, int cellZ)
        {
            foreach (var item in Vertices)
            {
                item.Position.X *= cellW;
                item.Position.Y *= cellH;
                item.Position.Z *= cellZ;
            }
        }

        internal void Translate(int v1, int v2, int v3)
        {
            foreach (var item in Vertices)
            {
                item.Position.X += v1;
                item.Position.Y += v2;
                item.Position.Z += v3;
            }
        }

        internal void ScaleTextureCoords(float texScaler1, float texScaler2)
        {
            foreach (var item in Vertices)
            {
                item.Tex.X *= texScaler1;
                item.Tex.Y *= texScaler2;
            }
        }

        internal void TranslateTexCoords(float v1, float v2)
        {
            foreach (var item in Vertices)
            {
                item.Tex.X += v1;
                item.Tex.Y += v2;
            }
        }
    }

}



