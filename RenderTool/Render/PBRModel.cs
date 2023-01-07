using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;


namespace RenderTool
{
    public class PBRModel : SimpleModel, ITextureLoad, ISetTexture, ILights
    {
        public PBRModel(Scene sc) : base(sc)
        {

        }
        //protected string lastTexturePath;

        public void LoadTexture(AbstractTexture tex)
        {
            SetTexture(tex as PBRTextureItem);
        }

        public bool RelativeLightsPosition { get; set; } = true;
        public double LightDummyScale { get; set; } = 10;
        public override void InitResources()
        {
            if (_inited) return;
            //base.InitResources();

            shader = new Shader("1.2.pbr.vs", "1.2.pbr.fs");            
            shader.use();
            shader.setInt("albedoMap", 0);
            shader.setInt("normalMap", 1);
            shader.setInt("metallicMap", 2);
            shader.setInt("roughnessMap", 3);
            shader.setInt("aoMap", 4);
            //asyncPreLoad();
            //loadTextures();
            Vector3[] lightPositions = {
        new Vector3(0.0f, 0.0f, 80.0f),
    };
            Vector3[] lightColors = {
        new Vector3(150.0f, 150.0f, 150.0f),
    };
            Lights.Clear();
            Lights.Add((new PBRLight(Parent) { Color = lightColors[0], Position = lightPositions[0] }));
        }

        public void asyncPreLoadDefaultTextures()
        {
            texture = Parent.Pool.DefaultTexture;
        }

        public List<PBRLight> Lights { get; set; } = new List<PBRLight>();
        protected Shader shader;
        
        

        RenderHelpers rh = new RenderHelpers();

        public void SetTexture(AbstractTexture t)
        {
            if (texture != null)
            {
                if (TexturePool.UnloadOnAssign)
                {
                    //Parent.Pool.Textures.Contains(texture);
                    texture.Unload();
                }
            }
            texture = t;
            t.Load();
        }

        public PBRTextureItem GetTexture()
        {
            return texture as PBRTextureItem;
        }

        protected AbstractTexture texture;

        int cubeVAO = 0;
        int cubeVBO = 0;

        
        //public Model Model=new Model ();
        int pcnt = 0;//top count
        int scnt = 0;//side count

        void renderVAO(bool side = false)
        {
            // initialize (if necessary)

            if (cubeVAO == 0)
            {
                List<double> _vertices = new List<double>();
                foreach (var item in Model.Polygons)
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

                //mask top face only
                var maxz = Model.Polygons.Max(z => z.Vertices.Max(u => u.Position.Z));
                var minz = Model.Polygons.Min(z => z.Vertices.Max(u => u.Position.Z));
                var midz = (maxz + minz) / 2f;
                _vertices = new List<double>();
                foreach (var item in Model.Polygons)
                {
                    if (!item.Vertices.All(z => z.Position.Z > midz)) continue;
                    pcnt++;
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

                vertices = _vertices.Select(z => (float)z).ToArray();
                

                /////////////////
                _vertices = new List<double>();
                foreach (var item in Model.Polygons)
                {
                    if (!(item.Vertices.Any(z => z.Position.Z > midz) && item.Vertices.Any(z => z.Position.Z < midz))) continue;
                    scnt++;
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

                vertices = _vertices.Select(z => (float)z).ToArray();
                
            }


            GL.BindVertexArray(cubeVAO);
            GL.DrawArrays(PrimitiveType.Triangles, 0, Model.Polygons.Count * 3);
            GL.BindVertexArray(0);

        }

        public void DeleteVao()
        {
            GL.DeleteVertexArray(cubeVAO);            
            GL.DeleteBuffer(cubeVBO);          

            cubeVBO = 0;
            cubeVAO = 0;            
        }

        public float ScaleZ { get; set; } = 1;
        public float ScaleY { get; set; } = 1;

        public float TextureScalerX { get; set; } = 1;
        public float TextureScalerY { get; set; } = 1;
        public float TextureShiftX { get; set; } = 0;
        public float TextureShiftY { get; set; } = 0;
        public bool DummyVisible { get; set; } = true;
        public string TexturePath { get => texture.OriginFilePath; }

        public override void Draw(IDrawingEnvironment denv)
        {
            var asyncPreLoadFinished = texture.PreLoadFinished;
            if (!asyncPreLoadFinished) return;

            texture.Load();
            if (asyncPreLoadFinished && !_inited)
            {

                //loadTextures();
                _inited = true;
            }

            if (!_inited) return;
            if (shader != null)
            {
                Shader curShader = shader;                

                curShader.use();

                var view = denv.Camera.GetViewMatrix();
                curShader.setMat4("view", view);
                curShader.setVec3("camPos", denv.Camera.CamFrom);



                texture.Bind();


                // render rows*column number of spheres with material properties defined by textures (they all have the same material properties)
                Matrix4 model = Matrix4.Identity;


                var tr = Matrix4.CreateTranslation(Position);

                model = Matrix4.CreateScale((float)Scale, (float)ScaleY, ScaleZ) * model;
                model = model * Matrix;
                model = model * tr;

                curShader.setMat4("model", model);
                curShader.setMat4("projection", denv.Camera.ProjectionMatrix);


                {
                    curShader.setVec2("texScaler", new Vector2(TextureScalerX, TextureScalerY));
                    curShader.setVec2("texShift", new Vector2(TextureShiftX, TextureShiftY));
                }
                //var projection = Matrix4.CreatePerspectiveFieldOfView((float)Camera.radians(camera.Zoom), (float)SCR_WIDTH / (float)SCR_HEIGHT, 0.1f, 100.0f);
                //   shader.use();
                //  shader.setMat4("projection", projection);

                //renderCube();

                ApplyLights(curShader);

                renderVAO();     

                DrawLightsDummies();

            }
            GL.UseProgram(0);
        }

        protected void DrawLightsDummies()
        {
            // render light source (simply re-render sphere at light positions)
            // this looks a bit off as we use the same shader, but it'll make their positions obvious and 
            // keeps the codeprint small.
            int cntr = 0;
            Matrix4 model = Matrix4.Identity;

            foreach (var light in Lights)
            {
                if (light == null) continue;
                if (!light.Enabled) continue;

                var newPos = light.Position;
                if (RelativeLightsPosition)
                {
                    newPos += Position;
                }


                model = Matrix4.Identity;
                model = Matrix4.CreateTranslation(newPos) * model;
                var sc = Matrix4.CreateScale((float)LightDummyScale);
                model = sc * model;
                shader.setMat4("model", model);
                if (DummyVisible && light.Visible)
                    rh.RenderSphere();
                cntr++;
            }
        }

        protected void ApplyLights(Shader shader)
        {

            int cntr = 0;
            for (int i = 0; i < 4; i++)
            {
                shader.setFloat("lightPow[" + i + "]", 0);
                shader.setFloat("lightAtten[" + i + "]", 0);
                shader.setVec3("lightColors[" + i + "]", Vector3.Zero);

            }
            foreach (var light in Lights)
            {
                if (light == null) continue;
                if (!light.Enabled) continue;

                var newPos = light.Position;
                if (RelativeLightsPosition)
                {
                    newPos += Position;
                }
                shader.setVec3("lightPositions[" + cntr + "]", newPos);
                float pow = light.Power;

                {
                    shader.setFloat("lightAtten[" + cntr + "]", light.Attenuation);
                    shader.setVec3("lightColors[" + cntr + "]", light.Color);
                }

                shader.setFloat("lightPow[" + cntr + "]", pow);
                cntr++;
            }
        }
    }
}