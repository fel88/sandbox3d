using System;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkJoint : IObjectClonable
    {
        public IkJoint()
        {
            Enable = true;
        }
        public bool Enable { get; set; }
        public string Name { get; set; }
        public bool Pinned { get; set; }
        public virtual void Random(Random r) { }
        public Vector3 LocalOffsetA { get; set; }
        public Vector3 LocalOffsetB { get; set; }

        public virtual void RandomDelta(Random r, float max) { }

        public virtual void Update()
        {
            if (!Enable) return;
            ConnectionB.CoordSystem = Matrix4.CreateTranslation(LocalOffsetB) * ConnectionA.CoordSystem * Matrix4.CreateTranslation(LocalOffsetA);

        }
        public object Tag { get; set; }
        
        public IkBonePool ConnectionA { get; set; }
        
        public IkBonePool ConnectionB { get; set; }

        public virtual object Clone(CloneContext context)
        {

            var r = new IkJoint()
            {
                ConnectionA = (IkBonePool)context.Get(ConnectionA),
                ConnectionB = (IkBonePool)context.Get(ConnectionB),
                Name = Name,
                LocalOffsetB = new Vector3(LocalOffsetB.X, LocalOffsetB.Y, LocalOffsetB.Z)
            };
            context.CloneList.Add(new CloneItem() { Clone = r, Original = this });
            return r;
        }
    }

}
