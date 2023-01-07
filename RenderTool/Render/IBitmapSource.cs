using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace RenderTool
{
    public interface IBitmapSource
    {
        Bitmap GetBitmap();
    }

}
