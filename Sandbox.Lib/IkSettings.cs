using OpenTK;

namespace Sandbox.Lib
{
    public class IkSettings
    {
        public IkTypeEnum Type { get; set; }
        public IkSettings()
        {
            Delta = 0.01f;
            OrientationDelta = 1f;
        }
        public bool IsRelative { get; set; }
        public bool IsComRelative { get; set; }
        public IkLineBone PivotBone { get; set; }
        public float Delta { get; set; }
        
        public float OrientationDelta { get; set; }
        public Vector3 TargetPosition { get; set; }

        public Vector3 Euler { get; set; }
        private Quaternion orientation = Quaternion.Identity;
        public Quaternion TargetOrientation
        {
            get
            {
                return orientation;
            }
            set
            {
                Quaternion.Normalize(ref value, out orientation);
            }
        }
        public IkMethodEnum Method { get; set; }
    }

}
