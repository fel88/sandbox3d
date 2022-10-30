using OpenTK;
using Sandbox.Lib;
using System.Collections.Generic;

namespace Sandbox
{
    public class SkeletonContainer : EntityContainer, ISkeletonContainer
    {


        public int HingeNewId { get; set; }
        public SkeletonContainer()
        {
            Skeleton = new IkSkeleton();
        }

        public virtual void CreateSkeleton()
        {
            Skeleton = new IkSkeleton();
        }

        

        public virtual void UpdateModel()
        {

        }

        public bool IsComHistoryEnable { get; set; }

        public List<Vector3> ComHistory = new List<Vector3>();

        public IkSkeleton Skeleton { get; set; }
        

        public void ResetAllVelocities()
        {

            

        }
    }
}
