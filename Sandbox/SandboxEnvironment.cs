using Ode.Net.Joints;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    public class SandboxEnvironment
    {
        public Ode.Net.World World { get; set; }
        public Ode.Net.Collision.Space OdeSpace { get; set; }
        public bool IsSimulationEnable { get; set; }
        public JointGroup ContactGroup = new JointGroup();

    }
}
