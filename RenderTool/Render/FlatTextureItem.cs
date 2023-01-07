using OpenTK.Graphics.OpenGL;
using System;
using System.Drawing;
using System.IO.Compression;
using System.Threading;

namespace RenderTool
{
    public class FlatTextureItem : AbstractTexture
    {
        public FlatTextureItem()
        {
            Id = TextureObjId++;
        }

        bool _loaded;
        public override bool Loaded => _loaded;

        public int albedo;
        public override void Bind()
        {
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, albedo);
        }


        Bitmap bmp;

        bool preLoadFinished = false;
        bool preLoadStarted = false;
        public override bool PreLoadFinished => preLoadFinished;

        public override void Unload()
        {
            _loaded = false;
            GL.DeleteTexture(albedo);
        }
        public override void Load()
        {
            if (Loaded) return;
            if (!preLoadFinished) return;
            if (TexturePool.SingleStageTextureLoadMode)
            {
                albedo = RenderHelpers.LoadTexture((Bitmap.FromFile(OriginFilePath) as Bitmap));
            }
            else
            {
                albedo = RenderHelpers.LoadTexture(bmp);
                bmp.Dispose();
                bmp = null;
            }
            _loaded = true;
        }

        public override void StartAsyncLoad(string anyFilePath = null)
        {
            if (preLoadStarted) return;
            if (anyFilePath == null)
            {
                anyFilePath = OriginFilePath;
            }
            Thread th = new Thread(() =>
            {
                try
                {
                    StartLoad(anyFilePath);
                }
                catch (Exception ex)
                {

                }
            });

            th.IsBackground = true;
            th.Start();
        }
        public void StartLoad(string anyFilePath)
        {
            if (preLoadStarted) return;
            preLoadStarted = true;
            OriginFilePath = anyFilePath;
            bmp = (Bitmap.FromFile(anyFilePath) as Bitmap);

            preLoadFinished = true;
        }
    }
}


