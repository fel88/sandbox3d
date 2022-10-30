using System.Collections.Generic;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkBonePool : IObjectClonable
    {

        public IkBonePool()
        {
            Masses = new List<IkMassItem>();
        }
        public bool Pinned { get; set; }
        public IkBonePool[] GetAllChilds()
        {
            List<IkBonePool> ret = new List<IkBonePool>();
            ret.AddRange(DownPools);
            foreach (var ikBonePool in DownPools)
            {
                ret.AddRange(ikBonePool.GetAllChilds());
            }
            return ret.ToArray();
        }

        public IkBonePool[] GetAllParents()
        {
            List<IkBonePool> ret = new List<IkBonePool>();
            IkBonePool up = UpPool;
            while (up != null)
            {
                ret.Add(up);
                up = up.UpPool;
            }
            return ret.ToArray();
        }
        
        public IkBonePool UpPool { get; set; }
        
        public List<IkBonePool> DownPools = new List<IkBonePool>();
        public IkSkeleton Parent;
        public string Name { get; set; }

        public List<IkMassItem> Masses { get; set; }
        
        public Vector3 GetCom()
        {
            Vector3 com = new Vector3(0, 0, 0);
            float sum = 0;
            foreach (var ikMassItem in Masses)
            {
                com += ikMassItem.Offset * ikMassItem.Mass;
                sum += ikMassItem.Mass;
            }
            com /= sum;
            return com;
        }

        public List<IkBone> Bones = new List<IkBone>();
        
        public Quaternion Orientation
        {
            get { return CoordSystem.ExtractRotation(); }
        }
        public object Tag { get; set; }

        public Matrix4 CoordSystem = Matrix4.Identity;
        public void AddChild(IkBone prearm)
        {
            Bones.Add(prearm);
            prearm.Parent = this;
        }

        public object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var r = new IkBonePool() { Name = Name };
            context.CloneList.Add(new CloneItem() { Original = this, Clone = r });

            foreach (var ikBone in Bones)
            {
                r.Bones.Add((IkBone)ikBone.Clone(context));
            }
            return r;
        }
    }

}
