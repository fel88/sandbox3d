using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using TriangleNet.Geometry;
using TriangleNet.Meshing;

namespace RenderTool
{
    public class RenderHelpers
    {
        public static Model GetBlankModel(Blank blank, PointF zo)
        {
            Model ret = new Model();
            //triangulate first..
            List<PointF[]> pnts = new List<PointF[]>();
            pnts.Add(blank.Points.ToArray());
            List<PointF[]> holes = new List<PointF[]>();
            foreach (var blankChild in blank.Childs)
            {
                holes.Add(blankChild.Points.ToArray());
            }


            float zoffset = 10f;
            var tt = Triangulate(pnts.ToArray(), holes.ToArray());

            foreach (var hole in holes)
            {
                for (int i = 0; i < hole.Length; i++)
                {
                    PointF p0;
                    if (i == 0)
                    {
                        p0 = hole.Last();
                    }
                    else
                    {
                        p0 = hole[i - 1];
                    }

                    var p1 = hole[i];
                    if (blank.IsRelative)
                    {
                        p1.X -= zo.X;
                        p1.Y -= zo.Y;

                        p0.X -= zo.X;
                        p0.Y -= zo.Y;
                    }

                    var crs = Vector3.Cross(new Vector3(p1.X - p0.X, p1.Y - p0.Y, 0), new Vector3(0, 0, -zoffset));
                    crs.Normalize();
                    DrawPolygon dp = new DrawPolygon();
                    ret.Polygons.Add(dp);
                    dp.Vertices = new DrawVertex[]
                    {
                        new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,zoffset)},
                        new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,zoffset)},
                        new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,0)},
                    };
                    dp = new DrawPolygon();
                    ret.Polygons.Add(dp);
                    dp.Vertices = new DrawVertex[]
                    {
                        new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,0)},
                        new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,0)},
                        new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,zoffset)},
                    };
                }
            }

            for (int i = 0; i < pnts[0].Length; i++)
            {
                PointF p0;
                if (i == 0)
                {
                    p0 = pnts[0].Last();
                }
                else
                {
                    p0 = pnts[0][i - 1];
                }

                var p1 = pnts[0][i];
                if (blank.IsRelative)
                {
                    p1.X -= zo.X;
                    p1.Y -= zo.Y;

                    p0.X -= zo.X;
                    p0.Y -= zo.Y;
                }

                var crs = Vector3d.Cross(new Vector3d(p1.X - p0.X, p1.Y - p0.Y, 0), new Vector3d(0, 0, -zoffset));
                crs.Normalize();

                var dp = new DrawPolygon();
                ret.Polygons.Add(dp);
                dp.Vertices = new DrawVertex[]
                {
                    new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,zoffset)},
                    new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,zoffset)},
                    new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,0)},
                };

                dp = new DrawPolygon();
                ret.Polygons.Add(dp);
                dp.Vertices = new DrawVertex[]
                {
                    new DrawVertex(){Position = new Vector3d(p0.X,p0.Y,0)},
                    new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,0)},
                    new DrawVertex(){Position = new Vector3d(p1.X,p1.Y,zoffset)},
                };
            }

            var maxx = tt.SelectMany(z => z).Max(z => z.X);
            var minx = tt.SelectMany(z => z).Min(z => z.X);
            var maxy = tt.SelectMany(z => z).Max(z => z.Y);
            var miny = tt.SelectMany(z => z).Min(z => z.Y);

            var dx = maxx - minx;
            var dy = maxy - miny;
            var maxd = Math.Max(dx, dy);
            dy = dx = maxd;

            foreach (var pointF in tt)
            {
                var v0 = pointF[0];
                var v1 = pointF[1];
                var v2 = pointF[2];
                if (blank.IsRelative)
                {
                    v0.X -= zo.X;
                    v1.X -= zo.X;
                    v2.X -= zo.X;

                    v0.Y -= zo.Y;
                    v1.Y -= zo.Y;
                    v2.Y -= zo.Y;
                }

                var dp = new DrawPolygon();
                ret.Polygons.Add(dp);
                dp.Vertices = new DrawVertex[]
                {
                    new DrawVertex(){Position = new Vector3d(v0.X,v0.Y,zoffset),Normal=Vector3d.UnitZ,Tex=new Vector2d ((v0.X-minx)/dx,(v0.Y-miny)/dy)},
                    new DrawVertex(){Position = new Vector3d(v1.X,v1.Y,zoffset),Normal=Vector3d.UnitZ,Tex=new Vector2d ((v1.X-minx)/dx,(v1.Y-miny)/dy)},
                    new DrawVertex(){Position = new Vector3d(v2.X,v2.Y,zoffset),Normal=Vector3d.UnitZ,Tex=new Vector2d ((v2.X-minx)/dx,(v2.Y-miny)/dy)},
                };

                dp = new DrawPolygon();
                ret.Polygons.Add(dp);
                dp.Vertices = new DrawVertex[]
                {
                    new DrawVertex(){Position = new Vector3d(v0.X,v0.Y,0),Normal=Vector3d.UnitZ},
                    new DrawVertex(){Position = new Vector3d(v1.X,v1.Y,0),Normal=Vector3d.UnitZ},
                    new DrawVertex(){Position = new Vector3d(v2.X,v2.Y,0),Normal=Vector3d.UnitZ},
                };



            }

            return ret;
        }
        public static PointF[][] TriangulateWithHoles(PointF[][] points, PointF[][] holes)
        {
            TriangleNet.Geometry.Polygon poly2 = new TriangleNet.Geometry.Polygon();

            foreach (var item in points)
            {
                var a = item.Select(z => new TriangleNet.Geometry.Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    poly2.Add(new Contour(a));
                }
            }

            foreach (var item in holes)
            {
                var a = item.Select(z => new TriangleNet.Geometry.Vertex(z.X, z.Y, 0)).ToArray();
                if (a.Count() > 2)
                {
                    PointF test = new PointF(0, 0);
                    int cnt = 0;
                    int cntlimit = 200;
                    while (true)
                    {
                        cnt++;
                        if (cnt > cntlimit) { throw new TriangleException("cntlimit reached"); }
                        var maxx = item.Max(z => z.X);
                        var minx = item.Min(z => z.X);
                        var maxy = item.Max(z => z.Y);
                        var miny = item.Min(z => z.Y);


                        if (Stuff.pnpoly(item.ToArray(), test.X, test.Y))
                        {
                            break;
                        }

                        var dx = maxx - minx;
                        var dy = maxy - miny;
                        test.X = (float)(Rand.NextDouble() * dx + minx);
                        test.Y = (float)(Rand.NextDouble() * dy + miny);
                    }

                    poly2.Add(new Contour(a), new TriangleNet.Geometry.Point(test.X, test.Y));

                }
            }

            ConstraintMesher.ScoutCounter = 0;

            var trng = (new GenericMesher()).Triangulate(poly2, new ConstraintOptions(), new QualityOptions());

            return trng.Triangles.Select(z => new PointF[] {
                    new PointF((float)z.GetVertex(0).X, (float)z.GetVertex(0).Y),
                    new PointF((float)z.GetVertex(1).X, (float)z.GetVertex(1).Y),
                    new PointF((float)z.GetVertex(2).X, (float)z.GetVertex(2).Y)
                }
            ).ToArray();

        }
        public static Random Rand = new Random();

        public static PointF[][] Triangulate(PointF[][] p, PointF[][] h)
        {
            return TriangulateWithHoles(p, h);
        }
        public static Bitmap ReadResourceBmp(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fr1 = assembly.GetManifestResourceNames().First(z => z.Contains(resourceName));

            using (Stream stream = assembly.GetManifestResourceStream(fr1))
            {
                return Bitmap.FromStream(stream) as Bitmap;
            }
        }

        static object locker = new object();
        public static int LoadTexture(Bitmap bitmap)
        {
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            int textureID = GL.GenTexture();
            GL.GenTextures(1, out textureID);
            //https://community.khronos.org/t/dma-via-pbo-asynchronous-loading-of-textures/58976

            GL.BindTexture(TextureTarget.Texture2D, textureID);

            BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, bitmap.PixelFormat);
            var format = PixelInternalFormat.Rgba;
            var format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgra;

            var ps = Image.GetPixelFormatSize(bitmap.PixelFormat);
            if (ps == 24)
            {
                format = PixelInternalFormat.Rgb;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Bgr;
            }
            if (ps == 8)
            {
                format = PixelInternalFormat.R8;
                format2 = OpenTK.Graphics.OpenGL.PixelFormat.Red;
            }
            GL.TexImage2D(TextureTarget.Texture2D, 0, format, data.Width, data.Height, 0,
                format2, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            bitmap.Dispose();
            return textureID;
        }

        public static void FillCubeCentric(double w, double h, double l)
        {
            GL.Begin(PrimitiveType.Triangles);
            GL.Normal3(0, 0, 1);
            GL.Vertex3(-w / 2, -h / 2, -l / 2);
            GL.Normal3(0, 0, 1);

            GL.Vertex3(-w / 2, h / 2, -l / 2);
            GL.Normal3(0, 0, 1);

            GL.Vertex3(w / 2, h / 2, -l / 2);

            GL.Normal3(0, 0, -1);
            GL.Vertex3(w / 2, h / 2, -l / 2);
            GL.Normal3(0, 0, -1);
            GL.Vertex3(w / 2, -h / 2, -l / 2);
            GL.Normal3(0, 0, -1);
            GL.Vertex3(-w / 2, -h / 2, -l / 2);
            //////////
            GL.Normal3(0, 0, -1);
            GL.Vertex3(-w / 2, -h / 2, l / 2);
            GL.Vertex3(-w / 2, h / 2, l / 2);
            GL.Vertex3(w / 2, h / 2, l / 2);


            GL.Vertex3(w / 2, h / 2, l / 2);
            GL.Vertex3(w / 2, -h / 2, l / 2);
            GL.Vertex3(-w / 2, -h / 2, l / 2);

            /////////
            GL.Vertex3(w / 2, h / 2, l / 2);
            GL.Vertex3(w / 2, -h / 2, l / 2);
            GL.Vertex3(-w / 2, -h / 2, l / 2);
            GL.End();
        }
        public static void FillMeshCentric(Vector3[,] pos)
        {
            GL.Begin(PrimitiveType.Triangles);
            int w = pos.GetLength(0);
            int h = pos.GetLength(1);
            for (int i = 1; i < w; i++)
            {
                for (int j = 1; j < h; j++)
                {
                    var v0 = pos[i - 1, j];
                    var v1 = pos[i, j - 1];
                    var v2 = pos[i - 1, j - 1];
                    var v3 = pos[i, j];

                    GL.Color3(Color.Brown);
                    GL.Normal3(Vector3.Cross(v0 - v2, v1 - v2));
                    GL.Vertex3(v0.X, v0.Y, v0.Z);
                    GL.Vertex3(v1.X, v1.Y, v1.Z);
                    GL.Vertex3(v2.X, v2.Y, v2.Z);

                    GL.Color3(Color.DarkOrange);
                    GL.Normal3(Vector3.Cross(v3 - v0, v3 - v1));
                    GL.Vertex3(v0.X, v0.Y, v0.Z);
                    GL.Vertex3(v1.X, v1.Y, v1.Z);
                    GL.Vertex3(v3.X, v3.Y, v3.Z);
                }
            }
            GL.End();
        }

        // renders (and builds at first invocation) a sphere
        // -------------------------------------------------
        uint sphereVAO = 0;
        int indexCount;

        public uint GenerateSphere(int X_SEGMENTS = 16, int Y_SEGMENTS = 16)
        {
            uint sphereVAO;

            GL.GenVertexArrays(1, out sphereVAO);


            uint vbo, ebo;
            GL.GenBuffers(1, out vbo);
            GL.GenBuffers(1, out ebo);

            List<Vector3> positions = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector3> normals = new List<Vector3>();
            List<int> indices = new List<int>();

            for (uint x = 0; x <= X_SEGMENTS; ++x)
            {
                for (uint y = 0; y <= Y_SEGMENTS; ++y)
                {
                    float xSegment = (float)x / (float)X_SEGMENTS;
                    float ySegment = (float)y / (float)Y_SEGMENTS;
                    var xPos = (float)(Math.Cos(xSegment * 2.0f * Math.PI) * Math.Sin(ySegment * Math.PI));
                    var yPos = (float)(Math.Cos(ySegment * Math.PI));
                    var zPos = (float)(Math.Sin(xSegment * 2.0f * Math.PI) * Math.Sin(ySegment * Math.PI));

                    positions.Add(new Vector3(xPos, yPos, zPos));
                    uv.Add(new Vector2(xSegment, ySegment));
                    normals.Add(new Vector3(xPos, yPos, zPos));
                }
            }

            bool oddRow = false;
            for (uint y = 0; y < Y_SEGMENTS; ++y)
            {
                if (!oddRow) // even rows: y == 0, y == 2; and so on
                {
                    for (int x = 0; x <= X_SEGMENTS; ++x)
                    {
                        indices.Add((int)(y * (X_SEGMENTS + 1) + x));
                        indices.Add((int)((y + 1) * (X_SEGMENTS + 1) + x));
                    }
                }
                else
                {
                    for (int x = X_SEGMENTS; x >= 0; --x)
                    {
                        indices.Add((int)((y + 1) * (X_SEGMENTS + 1) + x));
                        indices.Add((int)(y * (X_SEGMENTS + 1) + x));
                    }
                }
                oddRow = !oddRow;
            }
            indexCount = indices.Count;

            List<float> data = new List<float>(); ;
            for (int i = 0; i < positions.Count; ++i)
            {
                data.Add(positions[i].X);
                data.Add(positions[i].Y);
                data.Add(positions[i].Z);
                if (normals.Count > 0)
                {
                    data.Add(normals[i].X);
                    data.Add(normals[i].Y);
                    data.Add(normals[i].Z);
                }
                if (uv.Count > 0)
                {
                    data.Add(uv[i].X);
                    data.Add(uv[i].Y);
                }
            }
            GL.BindVertexArray(sphereVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            var arr1 = data.ToArray();
            GL.BufferData<float>(BufferTarget.ArrayBuffer, data.Count * sizeof(float), arr1, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            var arr2 = indices.ToArray();
            GL.BufferData<int>(BufferTarget.ElementArrayBuffer, indices.Count * sizeof(uint), arr2, BufferUsageHint.StaticDraw);
            int stride = (3 + 2 + 3) * sizeof(float);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride, (3 * sizeof(float)));
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, (6 * sizeof(float)));
            return sphereVAO;
        }

        public void RenderSphere()
        {
            if (sphereVAO == 0)
            {
                sphereVAO = GenerateSphere();
            }
            GL.BindVertexArray(sphereVAO);
            GL.DrawElements(BeginMode.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, 0);
        }
        public void RenderSphere(uint sphereVAO)
        {            
            GL.BindVertexArray(sphereVAO);
            GL.DrawElements(BeginMode.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, 0);
            //GL.DrawElementsInstanced(BeginMode.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, 0,)
        }
        public void RenderSpheres(uint sphereVAO, int count)
        {
            GL.BindVertexArray(sphereVAO);
            GL.DrawElementsInstanced(PrimitiveType.TriangleStrip, indexCount, DrawElementsType.UnsignedInt, IntPtr.Zero, count);
        }
    }
}


