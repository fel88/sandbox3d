using System.Collections.Generic;
using System.Drawing;

namespace Sandbox
{
    public class DrawingContext
    {
        public DrawingContext()
        {
            Multicolor = true;
            LineColor = Color.Blue;
            PolygonColor = Color.White;
            LineWidth = 3;
            WireframeLineWidth = 1;
            IsModelsDraw = true;
            PolygonOpacity = 0.5f;
        }
        /// <summary>
        /// helpers draw enable
        /// </summary>
        public bool IsEnableHelpersDraw = true;
        /// <summary>
        /// raw entity draw enable
        /// </summary>
        public bool RawDraw;

        public bool Wireframe = true;

        public List<object> SelectedEntities = new List<object>();

        public bool Solid = true;

        public bool Multicolor { get; set; }

        public float LineWidth { get; set; }
        public float WireframeLineWidth { get; set; }
        public bool IsVoxelsDraw { get; set; }

        public Color LineColor;
        public Color PolygonColor;
        public float PolygonOpacity { get; set; }

        public bool IsModelsDraw { get; set; }
    }

}
