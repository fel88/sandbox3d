using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace Sandbox
{
    public class GlDrawer : OpenTK.GLControl
    {
        public void Draw()
        {
            Invalidate();
        }

        public GlDrawer() : base(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8))
        {
            Dock = DockStyle.Fill;
            Paint += Gl_Paint;
            KeyDown += GlDrawer_KeyDown;
            MouseWheel += GlDrawer_MouseWheel;
            MouseDown += GlDrawer_MouseDown;
            MouseUp += GlDrawer_MouseUp;
            KeyUp += GlDrawer_KeyUp;

            Controls.Add(infoLabel);
            infoLabel.BackColor = Color.White;
            infoLabel.AutoSize = true;
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        }

        Label infoLabel = new Label();

        private void GlDrawer_KeyUp(object sender, KeyEventArgs e)
        {
            lshift = false;
        }

        bool lshift = false;
        private void GlDrawer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Shift)
            {
                lshift = true;
            }
        }

        private void GlDrawer_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            ldrag = false;

        }

        float startShiftX;
        float startShiftY;
        float startPosX;
        float startPosY;
        Vector3 cameraFromStart;
        Vector3 cameraToStart;
        public PointF CursorPosition
        {
            get
            {
                return PointToClient(Cursor.Position);
            }
        }
        bool drag = false;
        bool ldrag = false;
        private void GlDrawer_MouseDown(object sender, MouseEventArgs e)
        {
            var pos = CursorPosition;
            startPosX = pos.X;
            startPosY = pos.Y;
            cameraFromStart = Camera.CamFrom;
            cameraToStart = Camera.CamTo;

            if (e.Button == MouseButtons.Left)
            {
                ldrag = true;
            }
            if (e.Button == MouseButtons.Middle)
            {
                drag = true;
                lshiftcmd = lshift;
            }
        }

        private void GlDrawer_MouseWheel(object sender, MouseEventArgs e)
        {
            float zoomK = 10;
            var cur = PointToClient(Cursor.Position);
            MouseRay mr = new MouseRay(cur.X, cur.Y, Camera);


            var camera = Camera;
            if (ClientRectangle.IntersectsWith(new Rectangle(PointToClient(Cursor.Position), new Size(1, 1))))
            {
                if (e.Delta > 0)
                {
                    var dir = camera.CamTo - camera.CamFrom;
                    dir = mr.Dir;
                    dir.Normalize();
                    camera.CamFrom += dir * zoomK;
                    camera.CamTo += dir * zoomK;
                }
                else
                {
                    var dir = camera.CamTo - camera.CamFrom;
                    dir = mr.Dir;
                    dir.Normalize();
                    camera.CamFrom -= dir * zoomK;
                    camera.CamTo -= dir * zoomK;
                }
            }
        }

        public GlDrawer PerspectiveDrawer;

        bool lshiftcmd = false;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {

           
                var proc = Process.GetCurrentProcess();

            infoLabel.Text = "Memory: " + proc.PeakPagedMemorySize / 1024 / 1024 + "MB; sim count: " + Form1.SimCount;
            var dir = Camera.CamFrom - Camera.CamTo;
            var cv = dir;
            var moveVec = new Vector3(cv.X, cv.Y, cv.Z).Normalized();
            var a1 = Vector3.Cross(Camera.CamUp, cv.Normalized()); ;
            //var moveVecTan = new Vector3(-moveVec.Y, moveVec.X, );
            var moveVecTan = a1.Normalized();
            moveVec = Vector3.Cross(a1.Normalized(), cv.Normalized()).Normalized();


            var pos = CursorPosition;
            float zoom = 240f / (Camera.CamFrom - Camera.CamTo).Length;
            if (ldrag)
            {
                var dx = moveVecTan * ((startPosX - pos.X) / zoom) + moveVec * ((startPosY - pos.Y) / zoom);
                Camera.CamFrom = cameraFromStart + dx;
                Camera.CamTo = cameraToStart + dx;
            }

            if (drag)
            {
                //rotate here
                float kk = 3;
                Vector3 v1 = cameraFromStart - cameraToStart;
                v1 *= Matrix3.CreateFromAxisAngle(Vector3.Cross(v1, Camera.CamUp), -(startPosY - pos.Y) / 180f / kk * (float)Math.PI);
                v1 *= Matrix3.CreateFromAxisAngle(Camera.CamUp, -(startPosX - pos.X) / 180f / kk * (float)Math.PI);
                Camera.CamFrom = cameraToStart + v1;


            }


            GL.Enable(EnableCap.Multisample);

            GL.ClearColor(Color.FromArgb(157, 163, 170));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            Camera.Setup(this);
            GL.Disable(EnableCap.DepthTest);

            if (EnableDrawGrid)
            {
                DrawGrid();
            }
            DrawAxis();
            GL.Enable(EnableCap.DepthTest);
            if (DrawObjects != null)
            {
                DrawObjects();
            }



            //objects
            SwapBuffers();
        }

        public bool EnableDrawGrid { get; set; } = true;
        PerformanceCounter cpuCounter;
        PerformanceCounter ramCounter;
        public Action DrawObjects;

        public void DrawAxis()
        {
            GL.LineWidth(2);
            GL.Begin(PrimitiveType.Lines);

            GL.Color3(Color.Red);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 500, 0);
            GL.Color3(Color.Green);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(500, 0, 0);
            GL.End();
            GL.LineWidth(1);
        }

        public void SetFrontCamera()
        {
            Camera.CamTo = new Vector3(0, 0, 0);
            Camera.CamFrom = new Vector3(0, 100, 0);
            Camera.CamUp = Vector3.UnitZ;
            Camera.IsOrtho = true;

        }
        public void SetTopCamera()
        {
            Camera.CamTo = new Vector3(0, 0, 0);
            Camera.CamFrom = new Vector3(0, 0, 100);
            Camera.CamUp = Vector3.UnitY;
            Camera.IsOrtho = true;
        }



        public void DrawGrid()
        {

            int step = 25;

            for (int i = -20; i < 20; i++)
            {
                for (int j = -20; j < 20; j++)
                {
                    GL.Color3(Color.FromArgb(147, 153, 160));

                    GL.Begin(PrimitiveType.LineLoop);
                    GL.Vertex3(step * i, step * j, 0);
                    GL.Vertex3(step * (i + 1), step * j, 0);
                    GL.Vertex3(step * (i + 1), step * (j + 1), 0);
                    GL.Vertex3(step * i, step * (j + 1), 0);
                    GL.End();
                }
            }
            for (int i = -20; i < 20; i += 5)
            {
                GL.Color3(Color.FromArgb(129, 134, 140));
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(step * i, -500, 0);
                GL.Vertex3(step * i, 500, 0);
                GL.Vertex3(500, step * i, 0);
                GL.Vertex3(-500, step * i, 0);
                GL.End();
            }
        }

        public Camera Camera = new Camera();


        public void Init()
        {

        }
    }
}
