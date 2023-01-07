using System;
using System.Windows.Forms;

namespace pbr
{
    public class UiMouseEvent : UiEvent
    {
        public UiMouseEvent(UiMouseEventType tp, EventArgs args)
        {
            Args = args;
            Type = tp;
        }
        public MouseEventArgs MouseEventArgs
        {
            get { return Args as MouseEventArgs; }
        }
        public MouseButtons Button;

        public UiMouseEventType Type;

    }
}
