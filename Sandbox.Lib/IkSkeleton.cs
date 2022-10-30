using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Linq;
using OpenTK;

namespace Sandbox.Lib
{
    public class IkSkeleton : IObjectClonable
    {
        public int Id { get; set; }
        public Quaternion Orientation;//start orientation
        public List<IIkConstraint> Constraints = new List<IIkConstraint>();

        EditModeEnum _editMode = EditModeEnum.NotEdited;

        public EditModeEnum EditMode
        {
            get
            {
                lock (this)
                {
                    return _editMode;
                }
            }
            set
            {
                lock (this)
                {
                    _editMode = value;
                }
            }
        }


        public Vector3 GetCenterMass()
        {
            Vector3 mc = new Vector3(0, 0, 0);
            float mass = 0;

            foreach (var ikBonePool in Pools)
            {
                foreach (var ikMassItem in ikBonePool.Masses)
                {
                    mc += ikMassItem.AbsoluteOffset * ikMassItem.Mass;
                    mass += ikMassItem.Mass;
                }
            }
            mc /= mass;
            return mc;
        }
        public void ExtractPoolsFromBones()
        {
            Pools.Clear();
            List<IkBonePool> pools = new List<IkBonePool>();
            foreach (var ikBone in Bones)
            {
                pools.Add(ikBone.Parent);
            }
            Pools.AddRange(pools.Distinct());
        }

        public void SetPose(Pose pose)
        {
            var jnts = Joints.OfType<IkRevoluteJoint>().ToArray();
            for (int i = 0; i < jnts.Count(); i++)
            {
                jnts[i].Rotate = pose.Values[i];
            }
            ForwardCalc(Matrix3.Identity);
        }

        public List<IkBone> Bones = new List<IkBone>();
        public List<IkBonePool> Pools = new List<IkBonePool>();
      

        public Pose GetPose()
        {
            return new Pose() { Values = Joints.OfType<IkRevoluteJoint>().Select(z => z.Rotate).ToArray() };
        }


        public List<IkJoint> Joints = new List<IkJoint>();
        public Vector3 Shift;

        public void Forward(IkBonePool pool)
        {
            var jnts = Joints.Where(z => z.ConnectionA == pool);
            foreach (var ikJoint in jnts)
            {
                ikJoint.Update();
            }
            foreach (var ikBonePool in pool.DownPools)
            {
                Forward(ikBonePool);
            }
        }
        public void ForwardCalc(Matrix3 orient)
        {
            
            var tops = Pools.Where(z => z.UpPool == null).ToArray();
            foreach (var ikBonePool in tops)
            {
                Forward(ikBonePool);
            }
            
            if (PivotBone != null)
            {                
                var m2 = DesiredPivotFrame.ExtractRotation() * PivotBone.CoordSystem.ExtractRotation().Inverted();

                var tr1 = PivotBone.CoordSystem.ExtractTranslation();
                var tr2 = DesiredPivotFrame.ExtractTranslation();

                var trrm1 = Matrix4.CreateTranslation(-tr1);
                var trrm2 = Matrix4.CreateTranslation(tr2);
                var rt = Matrix4.CreateFromQuaternion(m2);
                Matrix4 domno = trrm1 * rt * trrm2;
                
                foreach (var ikBonePool in Pools)
                {
                    ikBonePool.CoordSystem = ikBonePool.CoordSystem * domno;                    
                }
            }
            RecalcAbsolute();
        }

        public void RecalcAbsolute()
        {
            List<IkBonePool> pools = new List<IkBonePool>();
            foreach (var ikBone in Bones)
            {
                pools.Add(ikBone.Parent);
                //ikBone.CalcAbsolute();
            }
            pools = pools.Distinct().Where(z => z != null).ToList();

            foreach (var ikBonePool in pools)
            {
                foreach (var ikBone in ikBonePool.Bones)
                {
                    ikBone.CalcAbsolute();
                }
                foreach (var ikMassItem in ikBonePool.Masses)
                {
                    ikMassItem.CalcAbsolute(ikBonePool);
                }
            }


        }

        public bool AllowUpdateModelAutomatic { get; set; }
        
        public IkBonePool PivotBone { get; set; }
        
        public Matrix4 DesiredPivotFrame { get; set; }
        public void UpdateHierarchy()
        {
            //create hirerarcgy of joints
            foreach (var ikBonePool in Pools)
            {
                ikBonePool.DownPools.Clear();
                ikBonePool.UpPool = null;
            }
            foreach (var ikJoint in Joints)
            {
                //A->B
                ikJoint.ConnectionB.UpPool = ikJoint.ConnectionA;
                ikJoint.ConnectionA.DownPools.Add(ikJoint.ConnectionB);
            }

        }

        public object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var r = new IkSkeleton() { };
            context.CloneList.Add(new CloneItem() { Original = this, Clone = r });

            foreach (var ikBone in Bones)
            {
                r.Bones.Add((IkBone)ikBone.Clone(context));
            }

            foreach (var ikBone in Pools)
            {
                r.Pools.Add((IkBonePool)ikBone.Clone(context));
            }
            foreach (var ikBone in Joints)
            {
                r.Joints.Add((IkJoint)ikBone.Clone(context));
            }
            return r;

        }
        
        public void RestoreFromXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            int index = 0;


            var rjoints = Joints.OfType<IkRevoluteJoint>().ToArray();
            foreach (var descendant in doc.Descendants("ikJoint"))
            {

                var type = descendant.Attribute("type").Value;
                switch (type)
                {
                    case "r":
                        var jnt = rjoints[index];
                        var val = float.Parse(descendant.Attribute("goal").Value);
                        rjoints[index].Rotate = val;
                        index++;

                        break;

                }
            }

        }

        public void RestoreFromXml(SceneXmlState state)
        {
            string xml = "";
            foreach (var s in state.State)
            {
                xml += s + Environment.NewLine;
            }

            RestoreFromXml(xml);

        }
    }

}
