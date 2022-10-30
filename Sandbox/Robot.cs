using FxEngine;
using System.Collections.Generic;

namespace Sandbox
{
    public class Robot
    {
        public static SkeletonContainer CreateWholeFromXml(string path)
        {
            RobotSkeletonContainerXmlVersion sk = new RobotSkeletonContainerXmlVersion();
            //var f = Stuff.ComplexModels.First(z => z.Name == "robot.full");
            //var objs = f.Items.GroupBy(z => z.ObjPath).ToArray();
            /*gl.Invoke((Action)(() =>
            {
                foreach (var modelPathItem in objs)
                {
                    //Stuff.LoadObjFile(modelPathItem.Key);
                }

            }));*/




            //foreach (var modelPathItem in f.Items)
            {

                /*   var md = new ModelInstance()
                   {
                       Model = modelPathItem.Model,
                       Name = modelPathItem.Model.Name,
                       Position = modelPathItem.Model.Position,
                       UseVBO = true
                   };*/
                //sk.Models.Add(md);
                //Stuff.Environment.Models.Add(md);
            }


            sk.CreateSkeleton();

            //   Stuff.Environment.Containers.Add(sk);
            sk.Skeleton.ExtractPoolsFromBones();
            sk.BindModel();



            sk.Skeleton.AllowUpdateModelAutomatic = true;
            foreach (var modelInstance in sk.Models)
            {
                //modelInstance.UseBLending = true;
            }

            //foreach (var bindMesh in sk.Binds)
            {
                //Stuff.bmesh.Add(bindMesh);
            }
            sk.Name = "robot";
            return sk;
        }

    }
    public interface IBipedRobot
    {
        ModelInstance[] Feet { get; set; }
    }
    public class BindXmlInfoItem
    {
        public Lib.IkBonePool BonePool;
        public int ModelId;

    }
    public class ModelXmlInfoItem
    {
        public string Name { get; set; }
        public int Id { get; set; }
    }
    public class TextureReplacer
    {
        public string Original { get; set; }
        public string Replaced { get; set; }
    }
    public class MgrNode
    {
        public MgrState State;
        public List<MgrNode> Childs = new List<MgrNode>();
    }


    public class MgrState
    {
        public float[] Values;
    }
}
