using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenTK;
using System;

namespace Sandbox.Lib
{
    public class Stuff
    {
        public static float MeasureTime(Action act)
        {
            var dt = DateTime.Now;
            act();
            return (float)DateTime.Now.Subtract(dt).TotalMilliseconds;
        }
        public static float[,] FromMathNetMatrix(Matrix<double> m)
        {
            float[,] ret = new float[m.RowCount, m.ColumnCount];

            for (int i = 0; i < m.RowCount; i++)
            {
                for (int j = 0; j < m.ColumnCount; j++)
                {
                    ret[i, j] = (float)m[i, j];
                }
            }
            return ret;
        }

        public static OpenTK.Vector3 ToVector3(object f)
        {
            if (f is OpenTK.Vector3)
            {
                return (OpenTK.Vector3)f;
            }
            
            
            if (f is Vector3)
            {
                var ff = (Vector3)f;
                return new OpenTK.Vector3((float)ff.X, (float)ff.Y, (float)ff.Z);
            }
            throw new ArgumentException("cant cast " + f.GetType().Name + " to vector3");
        }

        public static Matrix<double> ToMathNetMatrix(float[,] m)
        {
            Matrix<double> n = new DenseMatrix(m.GetLength(0), m.GetLength(1));
            for (int i = 0; i < m.GetLength(0); i++)
            {
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    n[i, j] = m[i, j];
                }
            }
            return n;
        }
        public static Vector3d ToEulerianAngle(OpenTK.Quaternion q)
        {
            double ysqr = q.Y * q.Y;
            double roll;
            double pitch;
            double yaw;
            // roll (x-axis rotation)
            double t0 = +2.0 * (q.W * q.X + q.Y * q.Z);
            double t1 = +1.0 - 2.0 * (q.X * q.X + ysqr);
            roll = Math.Atan2(t0, t1);

            // pitch (y-axis rotation)
            double t2 = +2.0 * (q.W * q.Y - q.Z * q.X);
            t2 = ((t2 > 1.0) ? 1.0 : t2);
            t2 = ((t2 < -1.0) ? -1.0 : t2);
            pitch = Math.Asin(t2);

            // yaw (z-axis rotation)
            double t3 = +2.0 * (q.W * q.Z + q.X * q.Y);
            double t4 = +1.0 - 2.0 * (ysqr + q.Z * q.Z);
            yaw = Math.Atan2(t3, t4);

            //return new[] { roll, pitch, yaw };
            return new Vector3d(roll, pitch, yaw);
        }
        
    }
}