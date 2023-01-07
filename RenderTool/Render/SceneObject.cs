using OpenTK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RenderTool
{
    public class SceneObject
    {
        public SceneObject(Scene scene)
        {
            Id = GetNewObjectId();
            _parent = scene;
        }
        protected Scene _parent;
        public Scene Parent { get => _parent; }
        public int Id { get; protected set; }
        public static int NewObjectId = 0;
        public int GetNewObjectId()
        {
            return NewObjectId++;
        }
        public bool Visible { get; set; } = true;
        public double Scale { get; set; } = 1;
        Vector3 _position;
        public Vector3 Position { get => _position; set => _position = value; }
        public float Z { get => Position.Z; set => _position.Z = value; }
        public Matrix4 Matrix { get; set; } = Matrix4.Identity;
        public string Name { get; set; }

        public virtual void Draw(IDrawingEnvironment denv)
        {

        }

        internal string[][] GetXmlAttrs()
        {
            List<string[]> ret = new List<string[]>
            {
                new string[] { "name", Name },
                new[] { "pos", $"{Position.X};{Position.Y};{Position.Z}" }
            };
            return ret.ToArray();
        }

        internal string[] GetVec3Attr(string name, Vector3 v)
        {
            return (new string[] { name, $"{v.X};{v.Y};{v.Z}" });
        }        

        protected bool _inited = false;
        public virtual void InitResources()
        {
            if (_inited) return;
            _inited = true;
        }

        public virtual void StoreXml(StringBuilder sb)
        {

        }

        public virtual void RestoreXml(XElement sb)
        {

        }

        public virtual SceneObject Clone()
        {
            SceneObject obj = new SceneObject(Parent);
            return obj;
        }
    }
}