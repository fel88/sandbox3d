namespace RenderTool
{
    public class ModelPathItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// name in object file
        /// </summary>
        public string ObjName { get; set; }
        public string ObjPath { get; set; }
        public string MtlPath { get; set; }
        public ObjVolume Model { get; set; }
        public float Scale { get; set; }
    }
}

