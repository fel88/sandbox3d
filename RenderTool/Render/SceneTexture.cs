using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace RenderTool
{
    [SceneStorableObject(XmlKey = "sceneTexture")]
    public class SceneTexture : SceneObject, ICommandsContainer, ITextureLoad
    {
        public SceneTexture(Scene scene) : base(scene)
        {
            InitResources();
            Scale = 100;
        }
        protected Shader shader;

        public override SceneObject Clone()
        {
            return base.Clone();
        }

        public double ScaleY { get; set; } = 100;
        public double ScaleZ { get; set; } = 100;
        public void asyncPreLoadDefaultTextures()
        {
            texture = Parent.Pool.DefaultFlatTexture;
        }
        int cubeVAO = 0;
        int cubeVBO = 0;

        int pcnt = 0;
        public ICommand[] Commands => new ICommand[] { new AddSceneTextureCommand(), new SetFlatTextureToBlankCommand(), new GenerateChessboardTextureCommand() };
        void renderVAO()
        {


            if (cubeVAO == 0)
            {
                Model model = new Model();
                model.Polygons.Add(new DrawPolygon()
                {
                    Vertices = new DrawVertex[] {
                    new DrawVertex (){ Position=new Vector3d (0,0,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (0,0)},
                    new DrawVertex (){ Position=new Vector3d (1,0,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (1,0)},
                    new DrawVertex (){ Position=new Vector3d (0,1,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (0,1)}
            }
                });
                model.Polygons.Add(new DrawPolygon()
                {
                    Vertices = new DrawVertex[] {
                    new DrawVertex (){ Position=new Vector3d (1,1,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (1,1)},
                    new DrawVertex (){ Position=new Vector3d (1,0,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (1,0)},
                    new DrawVertex (){ Position=new Vector3d (0,1,0),Normal=Vector3d.UnitZ,Tex=new Vector2d (0,1)}
            }
                });
                //generate flat quad

                List<double> _vertices = new List<double>();
                foreach (var item in model.Polygons)
                {
                    foreach (var vv in item.Vertices)
                    {
                        _vertices.Add(vv.Position.X);
                        _vertices.Add(vv.Position.Y);
                        _vertices.Add(vv.Position.Z);
                        _vertices.Add(vv.Normal.X);
                        _vertices.Add(vv.Normal.Y);
                        _vertices.Add(vv.Normal.Z);
                        _vertices.Add(vv.Tex.X);
                        _vertices.Add(vv.Tex.Y);
                    }
                }

                var vertices = _vertices.Select(z => (float)z).ToArray();
                GL.GenVertexArrays(1, out cubeVAO);
                GL.GenBuffers(1, out cubeVBO);
                // fill buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, cubeVBO);
                GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * vertices.Length, vertices, BufferUsageHint.StaticDraw);
                // link vertex attributes
                GL.BindVertexArray(cubeVAO);
                GL.EnableVertexAttribArray(0);
                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
                GL.EnableVertexAttribArray(1);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), (3 * sizeof(float)));
                GL.EnableVertexAttribArray(2);
                GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), (6 * sizeof(float)));
                GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
                GL.BindVertexArray(0);
                pcnt = model.Polygons.Count * 3;
            }
            // render Cube
            GL.BindVertexArray(cubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, pcnt);
            GL.BindVertexArray(0);
        }
        private double _rotateZ;
        public double RotateZ
        {
            get => _rotateZ;
            set
            {
                _rotateZ = value;
                Matrix = Matrix4.CreateRotationZ((float)(_rotateZ * Math.PI / 180f));

            }
        }

        public Vector3 _normal;
        public Vector3 Normal
        {
            get => _normal;
            set
            {
                _normal = value.Normalized();
                var axis = Vector3.Cross(Vector3.UnitZ, _normal);
                var ang = (float)Math.Acos(Vector3.Dot(Vector3.UnitZ, _normal) / 1f);
                //Matrix = new Matrix4(_normal.X, 0, 0, 0, 0, _normal.Y, 0, 0, 0, 0, _normal.Z, 0, 0, 0, 0, 1);
                Matrix = Matrix4.CreateFromAxisAngle(axis, ang);

            }
        }

        public override void Draw(IDrawingEnvironment denv)
        {
            var asyncPreLoadFinished = texture.PreLoadFinished;
            if (!asyncPreLoadFinished) return;

            texture.Load();

            /*if (!texture.Loaded)
            {
            }*/

            if (asyncPreLoadFinished && !_inited)
            {
                //texture.Load();
                _inited = true;
            }

            if (!_inited) 
                return;

            if (shader != null)
            {
                shader.use();

                var view = denv.Camera.GetViewMatrix();
                shader.setMat4("view", view);
                shader.setVec3("camPos", denv.Camera.CamFrom);
            }

            Matrix4 model = Matrix4.Identity;

            model = Matrix4.Identity;

            var tr = Matrix4.CreateTranslation(Position);

            model = Matrix4.CreateScale((float)Scale, (float)ScaleY, (float)ScaleZ) * model;
            model = model * Matrix;
            model = model * tr;

            shader.setMat4("model", model);
            shader.setMat4("projection", denv.Camera.ProjectionMatrix);

            texture.Bind();
            renderVAO();
        }

        internal AbstractTexture GetTexture()
        {
            return texture;
        }

        public override void InitResources()
        {
            if (_inited) 
                return;

            shader = new Shader("flat.vs", "flat.fs");

            shader.use();
            shader.setInt("albedoMap", 0);
            shader.setInt("useAlpha", 0);

            base.InitResources();
        }

        bool _useAlpha = false;
        public bool UseAlpha
        {
            get => _useAlpha;
            set
            {
                _useAlpha = value;
                shader.use();
                shader.setInt("useAlpha", value ? 1 : 0);
            }
        }

        AbstractTexture texture;

        public override void RestoreXml(XElement sb)
        {
            InitResources();

            if (sb.Attribute("visible") != null)
                Visible = bool.Parse(sb.Attribute("visible").Value);

            Id = int.Parse(sb.Attribute("id").Value);
            Name = (sb.Attribute("name").Value);

            Scale = float.Parse(sb.Attribute("scale").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            ScaleY = float.Parse(sb.Attribute("scaleY").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            ScaleZ = float.Parse(sb.Attribute("scaleZ").Value.Replace(",", "."), CultureInfo.InvariantCulture);

            RotateZ = Helpers.ParseFloat(sb.Attribute("rotationZ").Value);

            var spl = sb.Attribute("pos").Value.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var pos = spl.Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3(pos[0], pos[1], pos[2]);

            var texturePath = sb.Attribute("texture").Value;

            if (File.Exists(texturePath))
            {
                /*_parent.Pool.FlatTextures.Add(new FlatTextureItem());
                _parent.Pool.FlatTextures.Last().StartAsyncLoad(texturePath); 
                SetTexture(_parent.Pool.FlatTextures.Last());*/
                if (Parent.Pool.Textures.Any(z => z.OriginFilePath == texturePath))
                {
                    SetTexture(Parent.Pool.Textures.First(z => z.OriginFilePath == texturePath));
                }
                else
                {
                    var pbi = new FlatTextureItem() { OriginFilePath = texturePath };
                    pbi.StartAsyncLoad(texturePath);
                    Parent.Pool.Textures.Add(pbi);
                    SetTexture(Parent.Pool.Textures.First(z => z.OriginFilePath == texturePath));
                    //asyncPreLoadFromZipTexture(texturePath);
                }
            }
            else
            {
                asyncPreLoadDefaultTextures();
            }

        }

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<sceneTexture id=\"{Id}\"  visible=\"{Visible}\" name=\"{Name}\"  pos=\"{Position.X};{Position.Y};{Position.Z}\" texture=\"{texture.OriginFilePath}\" scale=\"{Scale}\" scaleY=\"{ScaleY}\" scaleZ=\"{ScaleZ}\" rotationZ=\"{RotateZ}\" >");

            sb.AppendLine("</sceneTexture>");
        }

        internal void SetTexture(AbstractTexture t)
        {
            if (texture != null)
            {
                if (TexturePool.UnloadOnAssign)
                {
                    texture.Unload();
                }
            }
            texture = t;
            t.Load();
        }

        public void LoadTexture(AbstractTexture tex)
        {
            SetTexture(tex);
        }
        public class SetFlatTextureToBlankCommand : ICommand
        {
            public void Execute(CommandEnvironment sender)
            {
                TexturesManager tm = new TexturesManager();
                tm.Init((sender.Sender as ITextureLoad), sender.Form.Scene.Pool, true);
                tm.TopMost = true;
                tm.Show();
            }
        }
        public class GenerateChessboardTextureCommand : ICommand
        {
            public void Execute(CommandEnvironment sender)
            {
                var ft = new FlatTextureItem();
                sender.Form.Scene.Pool.Textures.Add(ft);
                var bmp = Helpers.CreateChessboardBitmap(10, 7);
                bmp.SetPixel(0, 0, Color.Green);
                bmp.Save("chessboard.temp.jpg");
                ft.StartAsyncLoad("chessboard.temp.jpg");

                (sender.Sender as SceneTexture).SetTexture(ft);
            }
        }
        public class AddSceneTextureCommand : ICommand
        {
            public void Execute(CommandEnvironment sender)
            {
                OpenFileDialog ofd = new OpenFileDialog();
                if (ofd.ShowDialog() != DialogResult.OK) return;
                var fi = new FileInfo(ofd.FileName);

                var ft = new FlatTextureItem();
                sender.Form.Scene.Pool.Textures.Add(ft);
                ft.StartAsyncLoad(fi.FullName);

                (sender.Sender as SceneTexture).SetTexture(ft);

                /*TexturesManager tm = new TexturesManager();

                tm.Init((sender.Sender as ITextureLoad), sender.Form.Scene.Pool);
                tm.TopMost = true;
                tm.Show();*/

                //(sender.Sender as BlankModel).LoadTexture(fi.FullName);
            }
        }
    }

}

