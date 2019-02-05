using OpenTK;
using System.Drawing;
using OpenTK.Graphics.OpenGL;

namespace Sandbox
{
    public class MouseRay
    {
        public Vector3 _start;
        public Vector3 _end;

        public Vector3 Start { get { return _start; } }

        public Vector3 End { get { return _end; } }

        public MouseRay(Point mouse)
            : this(mouse.X, mouse.Y)
        {
        }

        public MouseRay(Vector3 start, Vector3 end)
        {
            _start = start;
            _end = end;

        }
        public static int[] viewport = new int[4];
        public static Matrix4 modelMatrix, projMatrix;

        public static void UpdateMatrices()
        {
            GL.GetFloat(GetPName.ModelviewMatrix, out modelMatrix);
            GL.GetFloat(GetPName.ProjectionMatrix, out projMatrix);
            GL.GetInteger(GetPName.Viewport, viewport);
        }

        public Vector3 Dir
        {
            get { return (End - Start).Normalized(); }
        }

        public MouseRay(int x, int y, Camera view)
        {
            int[] viewport = new int[4];
            Matrix4 modelMatrix, projMatrix;
            viewport = view.viewport;
            modelMatrix = view.ViewMatrix;
            projMatrix = view.ProjectionMatrix;


            _start = UnProject(new Vector3(x, y, 0.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
            _end = UnProject(new Vector3(x, y, 1.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
        }

        public MouseRay(int x, int y)
        {

            _start = UnProject(new Vector3(x, y, 0.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
            _end = UnProject(new Vector3(x, y, 1.0f), projMatrix, modelMatrix, new Size(viewport[2], viewport[3]));
        }

        public static Vector3 UnProject(Vector3 mouse, Matrix4 projection, Matrix4 view, Size viewport)
        {
            Vector4 vec;

            vec.X = 2.0f * mouse.X / (float)viewport.Width - 1;
            vec.Y = -(2.0f * mouse.Y / (float)viewport.Height - 1);
            vec.Z = mouse.Z;
            vec.W = 1.0f;

            Matrix4 viewInv = Matrix4.Invert(view);
            Matrix4 projInv = Matrix4.Invert(projection);

            Vector4.Transform(ref vec, ref projInv, out vec);
            Vector4.Transform(ref vec, ref viewInv, out vec);

            if (vec.W > 0.000001f || vec.W < -0.000001f)
            {
                vec.X /= vec.W;
                vec.Y /= vec.W;
                vec.Z /= vec.W;
            }

            return vec.Xyz;
        }

    }
}
