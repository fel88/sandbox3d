using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using OpenTK;

namespace RenderTool
{
    [SceneStorableObject(XmlKey = "blankModel")]
    public class BlankModel : PBRModel, ICommandsContainer
    {
        public BlankModel(Scene sc) : base(sc)
        {
            InitResources();
        }
        

        public override void InitResources()
        {
            //shader = new Shader("1.2.pbr.vs", "1.2.pbr.fs");
            //MaskShader =new Shader ("")
            base.InitResources();
        }
        public override void Draw(IDrawingEnvironment denv)
        {            
            base.Draw(denv);
        }
        private double _rotateZ;
        public double RotateZ
        {
            get => _rotateZ;
            set
            {
                _rotateZ = value;
                Matrix = Matrix4.CreateRotationZ((float)(_rotateZ * Math.PI / 180f));

            }
        }
        public override void StoreXml(StringBuilder sb)
        {
            sb.AppendLine($"<blankModel id=\"{Id}\" dummyVisible=\"{DummyVisible}\" visible=\"{Visible}\" name=\"{Name}\"  pos=\"{Position.X};{Position.Y};{Position.Z}\" texture=\"{TexturePath}\" scale=\"{Scale}\" scaleY=\"{ScaleY}\" scaleZ=\"{ScaleZ}\" rotationZ=\"{RotateZ}\" texScaleX=\"{TextureScalerX}\" texScaleY=\"{TextureScalerY}\">");
            foreach (var item in Lights)
            {
                if (item == null) continue;
                item.StoreXml(sb);
            }
            sb.AppendLine("<blank>");
            sb.AppendLine("<region>");
            foreach (var item in Blank.Points)
            {
                sb.AppendLine($"<point x=\"{item.X}\" y=\"{item.Y}\"/>");
            }

            sb.AppendLine("</region>");
            foreach (var item in Blank.Childs)
            {
                sb.AppendLine("<region>");
                foreach (var item1 in item.Points)
                {
                    sb.AppendLine($"<point x=\"{item1.X}\" y=\"{item1.Y}\"/>");
                }

                sb.AppendLine("</region>");
            }
            sb.AppendLine("</blank>");

            sb.AppendLine("</blankModel>");
        }

        public override void RestoreXml(XElement sb)
        {
            InitResources();            

            if (sb.Attribute("visible") != null)
                Visible = bool.Parse(sb.Attribute("visible").Value);

            if (sb.Attribute("dummyVisible") != null)
                DummyVisible = bool.Parse(sb.Attribute("dummyVisible").Value);

            Id = int.Parse(sb.Attribute("id").Value);
            Name = (sb.Attribute("name").Value);

            Scale = float.Parse(sb.Attribute("scale").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            ScaleY = float.Parse(sb.Attribute("scaleY").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            ScaleZ = float.Parse(sb.Attribute("scaleZ").Value.Replace(",", "."), CultureInfo.InvariantCulture);

            TextureScalerX = float.Parse(sb.Attribute("texScaleX").Value.Replace(",", "."), CultureInfo.InvariantCulture);
            TextureScalerY = Helpers.ParseFloat(sb.Attribute("texScaleY").Value);

            RotateZ = Helpers.ParseFloat(sb.Attribute("rotationZ").Value);

            var texturePath = sb.Attribute("texture").Value;

            if (File.Exists(texturePath))
            {
                if (Parent.Pool.Textures.Any(z => z.OriginFilePath == texturePath))
                {
                    SetTexture(Parent.Pool.Textures.First(z => z.OriginFilePath == texturePath));
                }
                else
                {
                    var pbi = new PBRTextureItem() { OriginFilePath = texturePath };
                    pbi.StartAsyncLoad(texturePath);
                    Parent.Pool.Textures.Add(pbi);
                    SetTexture(Parent.Pool.Textures.First(z => z.OriginFilePath == texturePath));
                    //asyncPreLoadFromZipTexture(texturePath);
                }
            }
            else
            {
                asyncPreLoadDefaultTextures();
            }
            //LoadTexture(texturePath);

            var spl = sb.Attribute("pos").Value.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            var pos = spl.Select(z => float.Parse(z.Replace(",", "."), CultureInfo.InvariantCulture)).ToArray();
            Position = new Vector3(pos[0], pos[1], pos[2]);
            Lights.Clear();
            foreach (var item in sb.Elements("pbrLight"))
            {
                Lights.Add(new PBRLight(Parent));
                Lights.Last().RestoreXml(item);
            }

            ////blank

            Blank = new Blank();
            var bl = sb.Element("blank");
            PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();
            int cntr = 0;
            foreach (var item in bl.Elements("region"))
            {
                if (cntr == 0)
                {
                    foreach (var pp in item.Elements("point"))
                    {
                        var xx = float.Parse(pp.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                        var yy = float.Parse(pp.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                        Blank.Points.Add(new PointF(xx, yy));
                    }
                }
                else
                {
                    var b1 = new Blank();
                    Blank.Childs.Add(b1);
                    foreach (var pp in item.Elements("point"))
                    {
                        var xx = float.Parse(pp.Attribute("x").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                        var yy = float.Parse(pp.Attribute("y").Value.Replace(",", "."), CultureInfo.InvariantCulture);
                        b1.Points.Add(new PointF(xx, yy));
                    }
                }
                cntr++;

            }

            Model = RenderHelpers.GetBlankModel(Blank, new PointF(0, 0));
            base.RestoreXml(sb);
        }

        public Blank Blank;
        public ICommand[] Commands => new ICommand[] { new EditBlankCommand(), new SetPBRTextureToBlankCommand() };

        public override SceneObject Clone()
        {
            BlankModel obj = new BlankModel(Parent);

            obj.Model = Model.Clone();

            obj.Position = Position;
            obj.Scale = Scale;
            obj.ScaleY = ScaleY;
            obj.ScaleZ = ScaleZ;
            obj.RotateZ = RotateZ;
            obj.Name = Name;
            obj.TextureScalerX = TextureScalerX;
            obj.TextureScalerY = TextureScalerY;            
            obj.Blank = Blank.Clone();
            for (int i = 0; i < Lights.Count; i++)
            {
                PBRLight item = Lights[i];
                if (item != null)
                {
                    obj.Lights[i] = item.Clone() as PBRLight;
                }
            }
            return obj;
        }
    }
}