using System;
using System.Drawing;

namespace RenderTool
{
    public class AbstractDrawer
    {
        public PointF ZeroOffset { get; internal set; }
        public float DrawingScale { get; internal set; }
        public IAppContainer AppContainer { get; internal set; }
        public PointF DrawingCenter { get; internal set; }

        internal PointF RealFromScreen(System.Drawing.Point location)
        {
            throw new NotImplementedException();
        }

        internal void RequestRebuild()
        {
            throw new NotImplementedException();
        }

        internal void RequestRepaint()
        {
            throw new NotImplementedException();
        }

        internal void ResetTool()
        {
            throw new NotImplementedException();
        }

        internal System.Drawing.Point ScreenFromReal(PointF pointF)
        {
            throw new NotImplementedException();
        }
    }
}
