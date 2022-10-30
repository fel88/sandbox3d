using FxEngine;

namespace Sandbox
{
    public class RobotContainer : SkeletonContainer, IBipedRobot
    {
        public ModelInstance[] Feet { get; set; }
    }
}
