using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RenderTool
{
    public class Scene
    {
        public List<SceneObject> Objects = new List<SceneObject>();
        
        public event Action<SceneObject> ObjectRemoved;

        

        public TexturePool Pool = new TexturePool();

        internal string ToXml(int activeCameraId, Form1 parent)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<?xml version=\"1.0\"?>");
            sb.AppendLine($"<root activeCamera=\"{activeCameraId}\" outputW=\"{offscreenRender.maxW}\" outputH=\"{offscreenRender.maxH}\" windowW=\"{parent.Width}\" windowH=\"{parent.Height}\">");
            sb.AppendLine("<objects>");

            foreach (var item in Objects)
            {
                item.StoreXml(sb);
            }
            sb.AppendLine("</objects>");
            
            sb.AppendLine("<pool>");

            foreach (var item in Pool.Textures)
            {
                sb.AppendLine($"<texture id=\"{item.Id}\" type=\"{((item is PBRTextureItem)?"pbr":"flat")}\">");
                sb.AppendLine($"<![CDATA[{item.OriginFilePath}]]>");
                sb.AppendLine("</texture>");
            }
            
            sb.AppendLine("</pool>");
            sb.AppendLine("</root>");
            return sb.ToString();
        }

        internal void RemoveItem(SceneObject tag)
        {
            Objects.Remove(tag);
            ObjectRemoved?.Invoke(tag);
        }
    }
}


