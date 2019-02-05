using OpenTK;
using System;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    public class Camera
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 CamFrom { get; set; } = new Vector3(70, 70, 70);

        public Vector3 CamTo { get; set; } = new Vector3(0, 0, 0);
        public Vector3 CamUp { get; set; } = new Vector3(0, 0, 1);

        public bool IsOrtho { get; set; } = false;
        public float Fovy { get; set; } = 60;
        public float Aspect { get; private set; }
        public object Direction
        {
            get
            {
                return CamFrom - CamTo;
            }
        }

        public Matrix4 ProjectionMatrix { get; set; }
        public Matrix4 ViewMatrix { get; set; }
        public int[] viewport = new int[4];
        public void Setup(GLControl gl)
        {
            GL.Viewport(0, 0, gl.Width, gl.Height);
            viewport[0] = 0;
            viewport[1] = 0;
            viewport[2] = gl.Width;
            viewport[3] = gl.Height;
            var o = Matrix4.CreateOrthographic(gl.Width, gl.Height, -1000, 100000);
            Aspect = (float)gl.Width / gl.Height;
            Matrix4 mp = Matrix4.CreatePerspectiveFieldOfView((float)(Fovy * Math.PI / 180), Aspect, 0.01f, 50000);
            GL.MatrixMode(MatrixMode.Projection);
            if (IsOrtho)
            {
                ProjectionMatrix = o;
                GL.LoadMatrix(ref o);
            }
            else
            {
                ProjectionMatrix = mp;
                GL.LoadMatrix(ref mp);
            }

                        
            Vector3 v0 = new Vector3(1, 0, 1);
            var m = Matrix3.CreateRotationY((float)(0.5f * Math.PI));
            var v1 = m * v0;
            
            
            Matrix4 modelview = Matrix4.LookAt(CamFrom, CamTo, CamUp);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
            ViewMatrix = modelview;            
        }

        public float OrthoZoom = 1;

        public void SetupViewOnly()
        {
            Matrix4 modelview = Matrix4.LookAt(CamFrom, CamTo, CamUp);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview);
        }

        public void CopyFrom(Camera cam)
        {
            CamFrom = cam.CamFrom;
            CamTo = cam.CamTo;
            CamUp = cam.CamUp;
            IsOrtho = cam.IsOrtho;
        }

        public Matrix4 GetBillboardMatrix(Vector3 pos)
        {
            var pm = ViewMatrix;
            var m = new Matrix4(                
                pm.Row0[0], pm.Row1[0], pm.Row2[0], 0,
             pm.Row0[1], pm.Row1[1], pm.Row2[1], 0,
             pm.Row0[2], pm.Row1[2], pm.Row2[2], 0,
             pos.X, pos.Y, pos.Z, 1
                );
            return m;            
        }
    }
}
