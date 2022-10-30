namespace Sandbox.Lib
{
    public interface IIkConstraint
    {
        bool Check(IkSkeleton skeleton);
        void SetRegister(object bn, int slot);
    }

}
