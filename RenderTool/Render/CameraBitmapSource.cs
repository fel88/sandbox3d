using System;
using System.Data;
using System.Drawing;
using System.Linq;

namespace RenderTool
{
    public class CameraBitmapSource : IBitmapSource
    {
        public CameraBitmapSource(Camera cam)
        {
            _camera = cam;
        }

        private readonly Camera _camera;
        public Action BitmapFetched;
        public Bitmap GetBitmap()
        {
            if (!Program.MainForm.MdiChildren.Where(z => z is Form1).Any()) return null;
            var f1 = Program.MainForm.MdiChildren.Where(z => z is Form1).First() as Form1;
            f1.TimerEnabled = false;
            f1.SwitchCamera(_camera);            
            f1.renderMode = true;
            f1.grabRequired = true;
            f1.Render();
            Bitmap drawBmp1 = null;
            f1.Invoke((Action)(() =>
            {
                drawBmp1 = f1.lastGrab.Clone() as Bitmap;
            }));
            BitmapFetched?.Invoke();            
            return drawBmp1;
        }
    }
}