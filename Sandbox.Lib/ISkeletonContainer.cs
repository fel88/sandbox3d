namespace Sandbox.Lib
{
    public interface ISkeletonContainer
    {
        void CreateSkeleton();
        void UpdateModel();
        IkSkeleton Skeleton { get; set; }
    }

}
