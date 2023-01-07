namespace RenderTool
{
    public interface IShader
    {
        int GetProgramId();
        void SetUniformsData();
        void Init();
        void Use();
    }
}

