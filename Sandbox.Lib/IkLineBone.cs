using System.Collections.Generic;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkLineBone : IkBone
    {
        
        public float Length { get { return (AbsoluteEnd - AbsolutePosition).Length; } }

        
        public Vector3 End { get; set; }//local
        public List<IkJoint> joints = new List<IkJoint>();

        public override object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var ret = new IkLineBone();
            context.CloneList.Add(new CloneItem() { Clone = ret, Original = this });
            ret.Position = new Vector3(Position.X, Position.Y, Position.Z);
            ret.End = new Vector3(End.X, End.Y, End.Z);
            ret.Name = Name;
            ret.Parent = (IkBonePool)Parent.Clone(context);

            return ret;
        }
        public override void SetScale(float scale)
        {
            Position *= scale;
            End *= scale;
        }

        
        public Vector3 Position { get; set; }


        public Vector3 AbsolutePosition { get; set; }
        public Vector3 AbsoluteEnd { get; set; }

        
        public override void CalcAbsolute()
        {
            var q = Parent.CoordSystem.ExtractRotation();
            AbsolutePosition = q * Position;
            AbsoluteEnd = q * End;
            var tr = Parent.CoordSystem.ExtractTranslation();
            AbsolutePosition += tr;
            AbsoluteEnd += tr;         
        }
    }
}
