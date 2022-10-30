using System;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkRevoluteJoint : IkJoint
    {
        public IkRevoluteJoint()
        {
            Limit = new IkLimit() { Max = 90, Min = -90 };
        }

        public override object Clone(CloneContext context)
        {
            var r = new IkRevoluteJoint()
            {
                ConnectionA = (IkBonePool)context.Get(ConnectionA),
                ConnectionB = (IkBonePool)context.Get(ConnectionB),
                Rotate = Rotate,
                Axis = new Vector3(Axis.X, Axis.Y, Axis.Z),
                Name = Name,
                LocalOffsetB = new Vector3(LocalOffsetB.X, LocalOffsetB.Y, LocalOffsetB.Z),
                Limit = (IkLimit)Limit.Clone(context)
            };
            context.CloneList.Add(new CloneItem() { Clone = r, Original = this });
            return r;
        }

        private Vector3 localFreeAxisA;
        
        public Vector3 LocalFreeAxisA
        {
            get { return localFreeAxisA; }
            set
            {
                localFreeAxisA = value;

            }
        }

        private Vector3 localFreeAxisB;
        
        public Vector3 LocalFreeAxisB
        {
            get { return localFreeAxisB; }
            set
            {
                localFreeAxisB = value;

            }
        }
        
        public Vector3 WorldFreeAxisA
        {
            get
            {
                return (ConnectionA.Orientation * localFreeAxisA);
            }
            set
            {
                var q = Quaternion.Conjugate(ConnectionA.Orientation);
                localFreeAxisA = q * value;

            }
        }

        
        public Vector3 WorldFreeAxisB
        {
            get { return (ConnectionB.Orientation * localFreeAxisB); }
            set
            {
                LocalFreeAxisB = (Quaternion.Conjugate(ConnectionB.Orientation) * value);
            }
        }

        public Vector3 axis;
        public OpenTK.Vector3 Axis
        {
            get { return axis; }
            set
            {
                axis = value;
                if (ConnectionA != null)
                {
                    WorldFreeAxisA = axis;
                }
                if (ConnectionB != null)
                {
                    WorldFreeAxisB = axis;
                }
            }
        }
        public float Rotate { get; set; }
        public IkBone Bone { get; set; }
        public IkLimit Limit { get; set; }

        public override void Update()
        {
            if (!Enable) return;
            //bo2.CoordSystem = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(45)) * Matrix4.CreateTranslation(5, 0, 0) * bo.CoordSystem;

            ConnectionB.CoordSystem = Matrix4.CreateFromAxisAngle(LocalFreeAxisB, MathHelper.DegreesToRadians(Rotate)) * Matrix4.CreateTranslation(LocalOffsetB) * ConnectionA.CoordSystem * Matrix4.CreateTranslation(LocalOffsetA);
            //var axis = Bone.Orientation * Axis;
            //var q = Quaternion.FromAxisAngle(axis, MathHelper.DegreesToRadians(Rotate));
            //Bone.Orientation = q;
            //Bone.End = (q * (new Vector3(Bone.Length, 0, 0)));
        }
        public override void RandomDelta(Random r, float max)
        {
            Rotate = Rotate + (float)(r.NextDouble() * (max) - max / 2);
            if (Rotate > Limit.Max)
            {
                Rotate = Limit.Max;
            }
            if (Rotate < Limit.Min)
            {
                Rotate = Limit.Min;
            }
        }
        public override void Random(Random r)
        {
            var diap = Limit.Max - Limit.Min;
            Rotate = (float)(r.NextDouble() * diap + Limit.Min);
        }

        public void FixLimit()
        {
            if (Limit != null)
            {
                if (Rotate > Limit.Max)
                {
                    Rotate = Limit.Max;
                }
                if (Rotate < Limit.Min)
                {
                    Rotate = Limit.Min;
                }

            }
        }
    }
}
