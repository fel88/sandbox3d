using OpenTK;

namespace Sandbox.Lib
{
    public class IkMassItem
    {
        public Vector3 Offset;
        public float Mass;
        public Vector3 AbsoluteOffset;

        public void CalcAbsolute(IkBonePool parent)
        {
            var q = parent.CoordSystem.ExtractRotation();
            AbsoluteOffset = q * Offset;

            var tr = parent.CoordSystem.ExtractTranslation();
            AbsoluteOffset += tr;

        }
    }

}
