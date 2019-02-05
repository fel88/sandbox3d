using Ode.Net;
using Ode.Net.Collision;
using Ode.Net.Joints;
using System.Collections.Generic;

namespace Sandbox
{
    public class Stuff
    {
        public static SandboxEnvironment CurrentEnvironment;
        
        public static List<Geom> SkipBroadPhase = new List<Geom>();
        public static JointGroup ContactGroup
        {
            get { return CurrentEnvironment.ContactGroup; }
            set { CurrentEnvironment.ContactGroup = value; }
        }
        public static World World
        {
            get { return CurrentEnvironment.World; }
            set { CurrentEnvironment.World = value; }

        }
        
        public static Ode.Net.Collision.Space OdeSpace
        {
            get { return CurrentEnvironment.OdeSpace; }
            set { CurrentEnvironment.OdeSpace = value; }
        }

        public static OpenTK.Vector3 ToTkVector3(Ode.Net.Vector3 position)
        {            
            return new OpenTK.Vector3((float)position.X, (float)position.Y, (float)position.Z);
        }

        public static OpenTK.Quaternion ToTkQuternion(Ode.Net.Quaternion quaternion)
        {
            return new OpenTK.Quaternion((float)quaternion.X, (float)quaternion.Y, (float)quaternion.Z, (float)quaternion.W);
        }
    }

  
}
