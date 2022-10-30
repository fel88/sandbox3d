using System;
using OpenTK;
using System.Linq;
using Sandbox.Lib;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;

namespace Sandbox
{
    /// <summary>
    /// xml версия загрузки робота
    /// </summary>
    public class RobotSkeletonContainerXmlVersion : RobotContainer
    {
        public RobotSkeletonContainerXmlVersion()
        {
            Skeleton = new IkSkeleton();
        }

        public IkBone[] LoadBonesFromNode(XElement el)
        {
            List<IkBone> ret = new List<IkBone>();
            foreach (var descendant in el.Elements())
            {
                if (descendant.Name == "pbone")
                {
                    var v = descendant.Attribute("name").Value;
                    var id = int.Parse(descendant.Attribute("id").Value);
                    var vrtx = descendant.Elements("vertex");
                    var pool = new IkBonePool() { Parent = Skeleton };
                    List<Vector3> vv = new List<Vector3>();
                    foreach (var element in vrtx)
                    {
                        var fls =
                            element.Attribute("pos")
                                .Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => float.Parse(x.Replace(".", ",")))
                                .ToArray();
                        vv.Add(new Vector3(fls[0], fls[1], fls[2]));
                    }
                    var bn = new IkPolygonBone(vrtx.Count())
                    {
                        Name = v,
                        Parent = pool,
                        Id = id,
                    };

                    bn.Vertices = vv.ToArray();
                    ret.Add(bn);
                    //Skeleton.Bones.Add(bn);
                    pool.AddChild(bn);

                }
                if (descendant.Name == "pool")
                {
                    var bns = LoadBonesFromNode(descendant);
                    var pool = new IkBonePool() { Parent = Skeleton };
                    foreach (var ikBone in bns)
                    {
                        ikBone.Parent = pool;
                        pool.AddChild(ikBone);
                    }
                    ret.AddRange(bns);
                    //Skeleton.Bones.AddRange(bns);
                }
                if (descendant.Name == "bone")
                {
                    var v = descendant.Attribute("name").Value;
                    var id = int.Parse(descendant.Attribute("id").Value);
                    var pos =
                        descendant.Attribute("position")
                            .Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(z => float.Parse(z.Replace(".", ",")))
                            .ToArray();
                    var end =
                        descendant.Attribute("end")
                            .Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(z => float.Parse(z.Replace(".", ",")))
                            .ToArray();
                    ;
                    float mass = 0;
                    Vector3? com = null;
                    if (descendant.Attribute("mass") != null)
                    {
                        mass = float.Parse(descendant.Attribute("mass").Value.Replace(".", ","));
                    }
                    if (descendant.Attribute("com") != null)
                    {
                        var arr =
                            (descendant.Attribute("com")
                                .Value.Split(new char[] { ';' })
                                .Select(z => float.Parse(z.Replace(".", ",")))).ToArray();
                        com = new Vector3(arr[0], arr[1], arr[2]);
                    }

                    var pool = new IkBonePool() { Parent = Skeleton };

                    var bn = new IkLineBone()
                    {
                        Name = v,
                        Parent = pool,
                        Id = id,
                        Position = new Vector3(pos[0], pos[1], pos[2]),
                        End = new Vector3(end[0], end[1], end[2])
                    };
                    if (com != null)
                    {
                        pool.Masses.Add(new IkMassItem() { Mass = mass, Offset = com.Value });
                    }
                    ret.Add(bn);
                    //Skeleton.Bones.Add(bn);
                    pool.AddChild(bn);
                }

            }
            return ret.ToArray();
        }

        public void CreateBones()
        {
            var dir = Directory.GetCurrentDirectory();
            var xx = XDocument.Load("models/robot.xml");
            var objnd = xx.Element("root").Element("object");
            var bones = objnd.Element("bones");
            var bns = LoadBonesFromNode(bones);
            Skeleton.Bones.AddRange(bns);

        }

        public void CreateJoints()
        {
            var dir = Directory.GetCurrentDirectory();
            var xx = XDocument.Load("models/robot.xml");
            var jnode = xx.Descendants("joints");
            foreach (var descendant in jnode.Elements())
            {
                var idA = int.Parse(descendant.Attribute("idA").Value);
                var idB = int.Parse(descendant.Attribute("idB").Value);
                var boneA = Skeleton.Bones.First(z => z.Id == idA);
                var boneB = Skeleton.Bones.First(z => z.Id == idB);
                if (descendant.Name == "revolute")
                {


                    var axis = descendant.Attribute("axis").Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(".", ","))).ToArray();
                    var offset = descendant.Attribute("offsetB").Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(".", ","))).ToArray();


                    var bn = new IkRevoluteJoint() { Name = boneA.Name + "->" + boneB.Name, ConnectionA = boneA.Parent, ConnectionB = boneB.Parent, Axis = new Vector3(axis[0], axis[1], axis[2]), LocalOffsetB = new Vector3(offset[0], offset[1], offset[2]) };
                    if (descendant.Attribute("limit") != null)
                    {
                        var limit =
                            descendant.Attribute("limit")
                                      .Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                                      .Select(z => float.Parse(z.Replace(".", ",")))
                                      .ToArray();
                        var lmax = limit[1];
                        var lmin = limit[0];
                        bn.Limit.Max = lmax;
                        bn.Limit.Min = lmin;
                    }

                    Skeleton.Joints.Add(bn);
                }
                if (descendant.Name == "weld")
                {
                    var offset = descendant.Attribute("offsetB").Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(".", ","))).ToArray();

                    var bn = new IkJoint() { Name = boneA.Name + "(w)" + boneB.Name, ConnectionA = boneA.Parent, ConnectionB = boneB.Parent, LocalOffsetB = new Vector3(offset[0], offset[1], offset[2]) };
                    Skeleton.Joints.Add(bn);
                }

            }
        }


        public List<ModelXmlInfoItem> ModelsItems = new List<ModelXmlInfoItem>();
        public void LoadModelDictionary()
        {
            var dir = Directory.GetCurrentDirectory();
            var xx = XDocument.Load("models/robot.xml");
            var jnode = xx.Descendants("models");
            foreach (var descendant in jnode.Elements())
            {
                var id = int.Parse(descendant.Attribute("id").Value);
                var name = (descendant.Attribute("name").Value);

                ModelsItems.Add(new ModelXmlInfoItem() { Id = id, Name = name });

            }
        }

        public BindXmlInfoItem[] LoadBindsItems()
        {
            List<BindXmlInfoItem> ret = new List<BindXmlInfoItem>();
            var dir = Directory.GetCurrentDirectory();
            var xx = XDocument.Load("models/robot.xml");
            var jnode = xx.Descendants("binds");
            foreach (var descendant in jnode.Elements())
            {
                var idA = int.Parse(descendant.Attribute("boneId").Value);
                var idB = int.Parse(descendant.Attribute("modelId").Value);

                var bone = Skeleton.Bones.FirstOrDefault(z => z.Id == idA);
                if (bone == null) continue;



                ret.Add(new BindXmlInfoItem()
                {
                    BonePool = bone.Parent,
                    ModelId = idB

                });
            }
            return ret.ToArray();
        }

        public override void CreateSkeleton()
        {
            CreateBones();
            CreateJoints();
            LoadModelDictionary();
            //BindModel();
            //updatePools?

        }

        public void BindModel()
        {
            var dir = Directory.GetCurrentDirectory();
            var xx = XDocument.Load("models/robot.xml");
            var jnode = xx.Descendants("binds");
            foreach (var descendant in jnode.Elements())
            {
                var idA = int.Parse(descendant.Attribute("boneId").Value);
                var idB = int.Parse(descendant.Attribute("modelId").Value);

                var bone = Skeleton.Bones.FirstOrDefault(z => z.Id == idA);
                if (bone == null) continue;
                var model = ModelsItems.First(z => z.Id == idB);
                var mm = Models.FirstOrDefault(z => z.Name == model.Name);
                if (mm == null)
                {
                    mm = Models.FirstOrDefault(z => z.Name.Contains(model.Name));
                }
                if (mm == null) continue;
                var offset = descendant.Attribute("offset").Value.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries).Select(z => float.Parse(z.Replace(".", ","))).ToArray();
                
            }
        }


        public override void UpdateModel()
        {

        }


    }
}
