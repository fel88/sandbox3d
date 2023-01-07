using OpenTK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RenderTool
{
    [SceneStorableObject(XmlKey = "pbrLight")]
    public class PBRLight : SceneObject
    {
        public PBRLight(Scene sc)
            : base(sc)
        {

        }
        public bool Enabled { get; set; } = true;
        public Vector3 Color { get; set; } = Vector3.One;
        public float Power { get; set; } = 0.5f;
        public float Attenuation { get; set; } = 1;
        

        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<pbrLight id=\"{Id}\" color=\"{Color.X};{Color.Y};{Color.Z}\" name=\"{Name}\" power=\"{Power}\" attenuation=\"{Attenuation}\" enabled=\"{Enabled}\" visible=\"{Visible}\" pos=\"{Position.X};{Position.Y};{Position.Z}\"/>");
        }

        public override void RestoreXml(XElement elem)
        {
            if (elem.Attribute("id") != null)
            {
                Id = int.Parse(elem.Attribute("id").Value);
            }
            var spl = elem.Attribute("pos").Value.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var pos = spl.Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3(pos[0], pos[1], pos[2]);

            var spl2 = elem.Attribute("color").Value.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var clr = spl2.Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Color = new Vector3(clr[0], clr[1], clr[2]);

            if (elem.Attribute("name") != null)
            {
                Name = elem.Attribute("name").Value;
            }

            Visible = bool.Parse(elem.Attribute("visible").Value);
            Enabled = bool.Parse(elem.Attribute("enabled").Value);
            Power = float.Parse(elem.Attribute("power").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            Attenuation = float.Parse(elem.Attribute("attenuation").Value.Replace(",", "."), CultureInfo.InvariantCulture);
        }

        public override SceneObject Clone()
        {
            PBRLight ret = new PBRLight(Parent);
            ret.Position = Position;
            ret.Power = Power;
            ret.Attenuation = Attenuation;
            ret.Color = Color;
            ret.Visible = Visible;
            ret.Name = Name;
            ret.Matrix = Matrix;
            ret.Scale = Scale;
            return ret;
        }
    }

    public interface ILights
    {
        List<PBRLight> Lights { get; set; }
    }
}

