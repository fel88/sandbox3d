using OpenTK;

namespace Sandbox
{
    public class BoundingBox
    {
        public static Vector3[] GetCorners(Vector3 min, Vector3 max)
        {
            var ret = new Vector3[8];
            ret[0] = new Vector3(min.X, max.Y, max.Z);
            ret[1] = max;
            ret[2] = new Vector3(max.X, min.Y, max.Z);
            ret[3] = new Vector3(min.X, min.Y, max.Z);
            ret[4] = new Vector3(min.X, max.Y, min.Z);
            ret[5] = new Vector3(max.X, max.Y, min.Z);
            ret[6] = new Vector3(max.X, min.Y, min.Z);
            ret[7] = min;
            return ret;
        }
    }
}
