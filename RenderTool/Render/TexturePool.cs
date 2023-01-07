using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace RenderTool
{
    public class TexturePool
    {
        public List<AbstractTexture> Textures = new List<AbstractTexture>();

        public static bool UnloadOnAssign = true;
        public static bool SingleStageTextureLoadMode = true;

        public PBRTextureItem DefaultTexture;
        public FlatTextureItem DefaultFlatTexture;

        public static bool CleanupByTime = false;

        public TexturePool()
        {
            
            /*if (!File.Exists("defaultTexture.zip"))
                File.WriteAllBytes("defaultTexture.zip", bb);*/

            DefaultTexture = new PBRTextureItem() { OriginFilePath = "defaultTexture.zip" };
            DefaultTexture.StartAsyncLoad("defaultTexture.zip");

            if (!File.Exists("defaultFlatTexture.jpg"))
            {
                Bitmap bmp = new Bitmap(256, 256);
                var gr = Graphics.FromImage(bmp);
                gr.Clear(Color.Blue);
                var w = bmp.Width / 8;
                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        gr.FillRectangle((i + j) % 2 == 0 ? Brushes.White : Brushes.DarkBlue, i * w, j * w, w, w);
                    }
                }

                bmp.SetPixel(0, 0, Color.Green);
                bmp.Save("defaultFlatTexture.jpg");
            }
            DefaultFlatTexture = new FlatTextureItem() { Id = 99999 };
            AbstractTexture.TextureObjId--;
            DefaultFlatTexture.StartAsyncLoad("defaultFlatTexture.jpg");
        }

        internal void DeleteTexture(AbstractTexture pbr)
        {
            Textures.Remove(pbr);
            pbr.Unload();
        }
    }
}