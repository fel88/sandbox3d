using OpenTK;
using System.Drawing;
using System.Collections.Generic;

namespace Sandbox
{
    public class ModelInstance22
    {
        public static int NewId = 0;
        public ModelInstance22()
        {
            Id = NewId;
            NewId++;
            _scale = new Vector3(1, 1, 1);
            Name = "model";
            orientation = Quaternion.Identity;
            Color = Color.White;
        }

        

        public bool UseBLending { get; set; }

        

        public float Density { get; set; }
        public string Name { get; set; }
        //public ModelPathItem Model;
        
        /// <summary>
        /// offset of pivot
        /// </summary>
        public Vector3 Offset { get; set; }
        private Vector3 _position;
        private Vector3 _scale;

        public Vector3 Position
        {
            get { return _position; }
            set { _position = value; }
        }


        public Vector3 Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }


        public Quaternion orientation;
        public Quaternion Orientation
        {
            get { return orientation; }
            set
            {
                Quaternion.Normalize(ref value, out orientation);
            }
        }
        public float Rotation { get; set; }

        public int Id { get; set; }

        public List<TextureReplacer> Replacers = new List<TextureReplacer>();
        public bool UseVBO { get; set; }

        public Color Color { get; set; }
    }
}
