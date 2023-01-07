using System;

namespace RenderTool
{
    public class ToolEvent
    {
        public AbstractDrawer Drawer { get; internal set; }
        public EventArgs Args { get; internal set; }
        public bool Handled { get; internal set; }
    }
}
