using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sandbox.Lib
{
    public class IkCalculator
    {
        public static float[,] CalcJacobian2PositionTarget(IkBone[] ees, IkRevoluteJoint[] joints, OpenTK.Vector3[] targets)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {

                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();
                List<float> ff = new List<float>();
                for (int index = 0; index < ees.Length; index++)
                {
                    var ikBone = ees[index];
                    var v1 = new OpenTK.Vector3(-lb.AbsolutePosition.X + targets[index].X, -lb.AbsolutePosition.Y + targets[index].Y, -lb.AbsolutePosition.Z + targets[index].Z);


                    var v2 = joint.WorldFreeAxisA;
                    var crs = OpenTK.Vector3.Cross(v2, v1).Normalized();

                    ff.AddRange(new[] { crs.X, crs.Y, crs.Z });
                }
                jacobs.Add(ff.ToArray());

            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }


        public static float[,] CalcJacobianExternalFrame(IkRevoluteJoint[] joints, OpenTK.Vector3 target, OpenTK.Vector3 pivot)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {

                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();
                var v1 = new OpenTK.Vector3(-lb.AbsolutePosition.X + target.X, -lb.AbsolutePosition.Y + target.Y, -lb.AbsolutePosition.Z + target.Z);
                v1 += pivot;



                var v2 = joint.WorldFreeAxisA;
                var crs = OpenTK.Vector3.Cross(v2, v1).Normalized();
                jacobs.Add(new float[] { crs.X, crs.Y, crs.Z });
            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }

        public static float[,] CalcJacobian(IkRevoluteJoint[] joints, OpenTK.Vector3 target)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {

                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();
                var v1 = new OpenTK.Vector3(-lb.AbsolutePosition.X + target.X, -lb.AbsolutePosition.Y + target.Y, -lb.AbsolutePosition.Z + target.Z);



                var v2 = joint.WorldFreeAxisA;
                var crs = OpenTK.Vector3.Cross(v2, v1).Normalized();
                jacobs.Add(new float[] { crs.X, crs.Y, crs.Z });
            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }

        public static float[,] CalcJacobianOrientation(IkRevoluteJoint[] joints, OpenTK.Vector3 target)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {

                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();
                var v1 = new OpenTK.Vector3(-lb.AbsolutePosition.X + target.X, -lb.AbsolutePosition.Y + target.Y, -lb.AbsolutePosition.Z + target.Z);

                var v2 = joint.WorldFreeAxisA;
                var crs = OpenTK.Vector3.Cross(v2, v1).Normalized();
                var or1 = joint.ConnectionA.Orientation.Normalized();
                jacobs.Add(new float[] { crs.X, crs.Y, crs.Z, v2.X, v2.Y, v2.Z });
            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }

        public static float[,] CalcJacobianOrientationExternalFrame(IkRevoluteJoint[] joints, OpenTK.Vector3 target, OpenTK.Vector3 pivot)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {

                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();
                var v1 = new OpenTK.Vector3(-lb.AbsolutePosition.X + target.X, -lb.AbsolutePosition.Y + target.Y, -lb.AbsolutePosition.Z + target.Z);

                v1 += pivot;

                var v2 = joint.WorldFreeAxisA;
                var crs = OpenTK.Vector3.Cross(v2, v1).Normalized();
                var or1 = joint.ConnectionA.Orientation.Normalized();
                jacobs.Add(new float[] { crs.X, crs.Y, crs.Z, v2.X, v2.Y, v2.Z });
            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }

        public static float[,] CalcJacobianOrientationOnly(IkRevoluteJoint[] joints)
        {
            List<float[]> jacobs = new List<float[]>();
            foreach (var joint in joints.Where(z => z.Pinned == false))
            {
                var lb = joint.ConnectionB.Bones.OfType<IkLineBone>().First();

                var v2 = joint.WorldFreeAxisA;
                var v3 = joint.ConnectionA.Orientation.Normalized();
                jacobs.Add(new float[] { v2.X, v2.Y, v2.Z });
            }

            float[,] ret = new float[jacobs[0].Length, jacobs.Count];
            for (int i = 0; i < jacobs.Count; i++)
            {
                for (int j = 0; j < jacobs[i].Length; j++)
                {
                    ret[j, i] = jacobs[i][j];

                }
            }
            return ret;
        }
    }
}
