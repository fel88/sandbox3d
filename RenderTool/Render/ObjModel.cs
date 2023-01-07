using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class ObjModel : SceneObject
    {
        public ObjModel(Scene sc):base(sc)
        {

        }
        
        public static FxShader ModelDrawShader = new Model3DrawShader("model3.vs", "model3.fs");
        VaoModel vmod;
        public override void Draw(IDrawingEnvironment denv)
        {
            GL.PushMatrix();
            GL.Scale(new Vector3d(Scale, Scale, Scale));
            var sh3 = ModelDrawShader as Model3DrawShader;

            sh3.viewPos = denv.Camera.CamFrom;
            var rotation = 0;
            sh3.Model = Matrix4.CreateRotationZ((float)(rotation * Math.PI / 180f));
            if (vmod != null)
                vmod.DrawVao(ModelDrawShader);
            GL.PopMatrix();

        }
        List<ObjVolume> models = new List<ObjVolume>();
        internal void LoadFromFile(string fileName)
        {
            models.Clear();

            models.AddRange(ObjVolume.LoadFromFile(fileName, Matrix4.Identity));

            var vol = models.Last();
            vmod = new VaoModel();
            ModelDrawShader.Init("model.vs", "model.fs");
            vmod.ModelInit(models.ToArray());
        }
    }
}

