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
                var bmp = Helpers.CreateChessboardBitmap(8, 8);
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