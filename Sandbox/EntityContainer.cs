using FxEngine;
using OpenTK;
using Sandbox.Lib;
using System.Collections.Generic;

namespace Sandbox
{
    public class EntityContainer : IEntityContainer
    {
        public EntityContainer()
        {

        }

        public List<IkChain> Chains = new List<IkChain>();
        public int Id { get; set; }
        public string Name { get; set; }

        public List<Ode.Net.Joints.Joint> OdeJoints { get; set; }
        public EntityContainer Parent;
        public List<EntityContainer> Childs = new List<EntityContainer>();
        public List<ModelInstance> Models = new List<ModelInstance>();

        public virtual Vector3 GetCenterMass()
        {
            return (new Vector3());
        }


        public bool IsUseModelCenterMass = true;


        public EntityContainer[] GetContainers()
        {
            List<EntityContainer> ret = new List<EntityContainer>();

            ret.AddRange(Childs);
            foreach (var entityContainer in Childs)
            {
                ret.AddRange(entityContainer.GetContainers());
            }
            return ret.ToArray();

        }



        public object[] StateStructure;
        public float[] TargetState;




        MgrNode last = null;


    }
}
