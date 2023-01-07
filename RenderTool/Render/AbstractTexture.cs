namespace RenderTool
{
    public abstract class AbstractTexture
    {
        public int Id { get; set; }
        public static int TextureObjId = 1;
        public abstract void Bind();
        public abstract void StartAsyncLoad(string anyFilePath = null);
        public abstract void Load();
        public abstract void Unload();

        public string OriginFilePath { get; set; }
        public abstract bool PreLoadFinished { get; }
        public abstract bool Loaded { get; }
    }
}


