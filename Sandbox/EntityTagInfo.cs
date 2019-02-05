using System.Drawing;

namespace Sandbox
{
    public class EntityTagInfo
    {
        public int VertsCount;
        public bool UseOneColor;
        public Color Color;
        public RayCastType RayCastType;

        
        public int ID_VBO = -1;        
        public int ID_EBO = -1;

        public uint[] Indices;//for vbo

        public bool UseVboBuffer { get; set; }
    }
}
