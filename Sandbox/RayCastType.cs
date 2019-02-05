using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    public enum RayCastType
    {
        /// <summary>
        /// check each face
        /// </summary>
        Face,
        /// <summary>
        /// check only bounding box
        /// </summary>
        BoundingBox,
        /// <summary>
        /// invisible for raycast
        /// </summary>
        Invisible,
        /// <summary>
        /// only part of polygons checked
        /// </summary>
        Tesselate,
        /// <summary>
        /// bounding sphere
        /// </summary>
        BoundingSphere,
        /// <summary>
        /// one face
        /// </summary>
        OneFace
    }
}
