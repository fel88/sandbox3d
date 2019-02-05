using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    public class VertexPositionNormalTexture
    {
        public VertexPositionNormalTexture(
            Vector3 position,
            Vector3 normal,
            Vector2 textureCoordinate
            )
        {
            Position = position;
            Normal = normal;
            TextureCoordinate = textureCoordinate;
        }

        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoordinate;
    }
}
