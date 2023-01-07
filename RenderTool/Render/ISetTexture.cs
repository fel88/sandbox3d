namespace RenderTool
{
    public interface ISetTexture
    {
        string Name { get; }
        int Id { get; }        
        void SetTexture(AbstractTexture t);
        float TextureScalerX { get; set; }
        float TextureScalerY { get; set; }
        float TextureShiftX { get; set; }
        float TextureShiftY { get; set; }
    }
}