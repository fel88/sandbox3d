using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenTK;

namespace RenderTool
{
    public class ObjVolume : Volume
    {
        public ObjVolume()
        {
            ReverseVertexOrder = true;
            Scale = new Vector3(1, 1, 1);
        }


        public object Tag { get; set; }

        //ID of the vertex buffer we will use
        public int ID_VBO;
        //ID of the element buffer
        public int ID_EBO;
        public int ID_EBO_Wireframe;
        public int IndiciesWireframeLength;

        public uint[] Indices;//for vbo

        public ModelPathItem Parent;
        /// <summary>
        /// name in obj file
        /// </summary>
        public string Name;
        public Vector3[] vertices;
        public Vector3[] normals;
        public int[] indicies;
        public int[] origIndicies;
        public Vector3[] origVerts;
        public Vector3[] origNorms;
        Vector3[] colors;
        public Vector2[] texturecoords;
        
        public static bool ReverseVertexOrder { get; set; }
        public List<FaceItem2> faces = new List<FaceItem2>();

        public override int VertCount { get { return vertices.Length; } }
        public override int IndiceCount { get { return faces.Count * 3; } }
        public override int ColorDataCount { get { return colors.Length; } }
        public float? Volume { get; set; }

        /// <summary>
        /// Get vertice data for this object
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetVerts()
        {
            List<Vector3> verts = new List<Vector3>();

            foreach (var face in faces)
            {
                verts.Add(face.Item1.Position);
                verts.Add(face.Item2.Position);
                verts.Add(face.Item3.Position);
            }

            return verts.Distinct().ToArray();
        }

        /// <summary>
        /// Get indices
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public override int[] GetIndices(int offset = 0)
        {            
            List<int> ind = new List<int>();
            foreach (var faceItem2 in faces)
            {
                foreach (var tempVertex in faceItem2.ParentFace.V)
                {
                    ind.Add(tempVertex.Vertex);
                }                
            }
            return ind.ToArray();            
        }

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetColorData()
        {
            return new Vector3[ColorDataCount];
        }

        /// <summary>
        /// Get texture coordinates.
        /// </summary>
        /// <returns></returns>
        public override Vector2[] GetTextureCoords()
        {
            List<Vector2> coords = new List<Vector2>();

            foreach (var face in faces)
            {
                coords.Add(face.Item1.TextureCoord);
                coords.Add(face.Item2.TextureCoord);
                coords.Add(face.Item3.TextureCoord);
            }

            return coords.ToArray();
        }


        /// <summary>
        /// Calculates the model matrix from transforms
        /// </summary>
        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
        }

        /// <summary>
        /// Loads a model from a file.
        /// </summary>
        /// <param name="filename">File to load model from</param>
        /// <returns>ObjVolume of loaded model</returns>
        public static ObjVolume[] LoadFromFile(string filename, Matrix4 loadTransform)
        {
            ObjVolume[] obj;
            
            using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
            {
                obj = LoadFromString(reader.ReadToEnd(), filename, loadTransform);
            }

            return obj;
        }

        public static ObjVolume[] LoadFromString(string obj, string path, Matrix4 loadTransform)
        {
            // Seperate lines from the file
            List<String> lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            List<Vector3> verts = new List<Vector3>();
            List<Vector3> vertn = new List<Vector3>();
            List<Vector2> texs = new List<Vector2>();
            List<FaceItem> faces = new List<FaceItem>();

            // Base values
            //verts.Add(new Vector3());
            //texs.Add(new Vector2());
            //vertn.Add(new Vector3());
            ObjVolume vol = null;
            int currentindice = 0;
            string lastMtl = "";
            string mtllibPath = "";
            // Read file line by line
            List<ObjVolume> ret = new List<ObjVolume>();
            MaterialStuff mat = new MaterialStuff();
            int vstart = 0;
            int tstart = 0;
            int nstart = 0;

            bool lastFwas = false;
            foreach (String line in lines)
            {
                if (line.StartsWith("o ")) // object def
                {
                    if (vol != null)
                    {
                        //LoadVol(vol, faces, verts, vertn, texs, mat, vstart);
                        //vol.vertices = verts.Skip(vstart).ToArray();
                        //vol.normals = vertn.Skip(nstart).ToArray();
                        //vol.texturecoords = texs.Skip(tstart).ToArray();
                        List<int> ind = new List<int>();
                        List<Vector3> vvrts = new List<Vector3>();
                        List<Vector3> vnrts = new List<Vector3>();

                        foreach (var faceItem in faces)
                        {
                            foreach (var tempVertex in faceItem.V)
                            {
                                ind.Add(tempVertex.Vertex);
                                vvrts.Add(verts[tempVertex.Vertex]);
                                vnrts.Add(vertn[tempVertex.Normal]);
                            }
                        }

                        vol.indicies = ind.ToArray();
                        vol.vertices = vvrts.ToArray();
                        vol.normals = vnrts.ToArray();
                        foreach (var face in faces)
                        {

                            foreach (var item in face.V)
                            {
                                if (item.Vertex < 0)
                                {
                                    item.Vertex = verts.Count + item.Vertex;
                                }
                                if (item.Normal < 0)
                                {
                                    item.Normal = vertn.Count + item.Normal;
                                }
                                if (item.Texcoord < 0)
                                {
                                    item.Texcoord = texs.Count + item.Texcoord;
                                }
                            }

                            List<FaceVertex> fv = new List<FaceVertex>();
                            foreach (var item in face.V)
                            {
                                var t1 = new Vector2();
                                if (item.Texcoord >= 0 && texs.Any())
                                {
                                    t1 = texs[item.Texcoord];
                                }
                                FaceVertex v1 = new FaceVertex(verts[item.Vertex], vertn[item.Normal], t1);
                                fv.Add(v1);
                            }

                            vol.faces.Add(new FaceItem2() { ParentFace = face, Parent = vol, Vertexes = fv.ToArray(), Material = mat.materials[face.Material] });



                        }

                        //faces.Clear();
                        //verts.Clear();
                    }
                    vol = new ObjVolume();

                    var r = line.Split(new string[] { " ", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).Skip(1).ToArray();
                    vol.Name = r[0];
                    ret.Add(vol);

                }
                if (line.StartsWith("g ")) // group def
                {
                }
                if (line.StartsWith("v ")) // Vertex definition
                {
                    if (lastFwas)
                    {
                        lastFwas = false;
                        nstart = vertn.Count();
                        tstart = texs.Count();

                        List<int> ind = new List<int>();


                        foreach (var faceItem in faces)
                        {
                            foreach (var tempVertex in faceItem.V)
                            {
                                ind.Add(tempVertex.Vertex);

                            }
                        }

                        faces.Clear();
                        vol.origVerts = verts.Skip(vstart).ToArray();
                        vol.origNorms = vertn.Skip(vstart).ToArray();
                        vol.origIndicies = ind.ToArray();
                        var min = vol.origIndicies.Min();
                        vol.origIndicies = vol.origIndicies.Select(z => z - min).ToArray();
                        vstart = verts.Count();

                    }
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.X);
                        success |= float.TryParse(vertparts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.Y);
                        success |= float.TryParse(vertparts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    verts.Add((new Vector4(vec, 1) * loadTransform).Xyz);
                }
                else if (line.StartsWith("vn ")) // Vertex normal
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector3 vec = new Vector3();

                    if (temp.Trim().Count((char c) => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        String[] vertparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(vertparts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.X);
                        success |= float.TryParse(vertparts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.Y);
                        success |= float.TryParse(vertparts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    vertn.Add(vec);
                }
                else if (line.StartsWith("vt ")) // Texture coordinate
                {
                    // Cut off beginning of line
                    String temp = line.Substring(2);

                    Vector2 vec = new Vector2();

                    if (temp.Trim().Count((char c) => c == ' ') > 0) // Check if there's enough elements for a vertex
                    {
                        String[] texcoordparts = temp.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        bool success = float.TryParse(texcoordparts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.X);
                        success |= float.TryParse(texcoordparts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out vec.Y);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing texture coordinate: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing texture coordinate: {0}", line);
                    }

                    texs.Add(vec);
                }
                else if (line.StartsWith("usemtl "))
                {
                    string temp = line.Substring("usemtl ".Length);
                    lastMtl = temp.Replace("\r", "");


                }
                else if (line.StartsWith("mtllib "))
                {
                    string temp = line.Substring("mtllib ".Length);

                    mtllibPath = temp;
                    var d = new FileInfo(path);
                    var name = d.DirectoryName;
                    var curd = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(name);
                    mat.loadMaterials(mtllibPath.Replace("\r", ""));
                    Directory.SetCurrentDirectory(curd);

                    //mat.loadMaterials(mtllibPath.Replace("\r", ""));

                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    lastFwas = true;
                    // Cut off beginning of line
                    String temp = line.Substring(2);


                    //Tuple<TempVertex, TempVertex, TempVertex> face = new Tuple<TempVertex, TempVertex, TempVertex>(new TempVertex(), new TempVertex(), new TempVertex());
                    var cnt = temp.Trim().Count((char c) => c == ' ');
                    //if (cnt > 3) throw new Exception("too many vertex in face detected");

                    String[] faceparts = temp.Trim().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    List<TempVertex> tmp = new List<TempVertex>();
                    for (int i = 0; i < faceparts.Count(); i++)
                    {
                        int i1;
                        var spl = faceparts[i].Split('/').ToArray();
                        bool success = int.TryParse(spl[0], out i1);
                        i1--;


                        int t1 = 0;
                        success = int.TryParse(spl[1], out t1);
                        if (success)
                        {
                            t1--;

                        }

                        int n1 = 0;
                        if (spl.Count() > 2)
                        {
                            success = int.TryParse(spl[2], out n1);
                            n1--;
                        }

                        TempVertex v1 = new TempVertex(i1, n1, t1);
                        tmp.Add(v1);


                        if (texs.Count < v1.Texcoord)
                        {
                            texs.Add(new Vector2());
                        }
                    }

                    if (ReverseVertexOrder)
                    {
                        faces.Add(new FaceItem() { V = tmp.ToArray().Reverse().ToArray(), Material = lastMtl });
                    }
                    else
                    {
                        faces.Add(new FaceItem() { V = tmp.ToArray(), Material = lastMtl });
                    }

                }
            }

            // Create the ObjVolume

            texs.Add(new Vector2());
            texs.Add(new Vector2());
            texs.Add(new Vector2());
            if (vol != null)
            {
                LoadVol(vol, faces, verts, vertn, texs, mat, vstart);
            }

            foreach (var objVolume in ret)
            {
                objVolume.mat = mat;
            }
            return ret.ToArray();
        }


        public static void LoadVol(ObjVolume vol, List<FaceItem> faces, List<Vector3> verts, List<Vector3> vertn, List<Vector2> texs, MaterialStuff mat, int vstart)
        {

            foreach (var face in faces)
            {

                foreach (var item in face.V)
                {
                    if (item.Vertex < 0)
                    {
                        item.Vertex = verts.Count + item.Vertex;
                    }
                    if (item.Texcoord < 0)
                    {
                        item.Texcoord = texs.Count + item.Texcoord;
                    }
                    if (item.Normal < 0)
                    {
                        item.Normal = vertn.Count + item.Normal;
                    }
                }


                List<FaceVertex> fv = new List<FaceVertex>();

                Vector3 norm1 = new Vector3();
                foreach (var item in face.V)
                {
                    if (vertn.Count != 0)
                    {
                        norm1 = vertn[item.Normal];
                    }
                    FaceVertex fv1 = new FaceVertex(verts[item.Vertex], norm1, texs[item.Texcoord], item);
                    fv.Add(fv1);
                }

                Material matt = null;
                if (mat.materials.Any())
                {
                    matt = mat.materials[face.Material];
                }
                vol.faces.Add(new FaceItem2() { ParentFace = face, Parent = vol, Vertexes = fv.ToArray(), Material = matt });

            }

            if (vol != null)
            {
                List<int> ind = new List<int>();
                List<Vector3> vvrts = new List<Vector3>();
                List<Vector3> vnrts = new List<Vector3>();

                foreach (var faceItem in faces)
                {
                    foreach (var tempVertex in faceItem.V)
                    {
                        ind.Add(tempVertex.Vertex);
                        vvrts.Add(verts[tempVertex.Vertex]);
                        vnrts.Add(vertn[tempVertex.Normal]);
                    }
                }
                vol.indicies = ind.ToArray();
                vol.vertices = vvrts.ToArray();
                vol.normals = vnrts.ToArray();

                vol.origVerts = verts.Skip(vstart).ToArray();
                vol.origNorms = vertn.Skip(vstart).ToArray();
                vol.origIndicies = ind.Select(z => z - vstart).ToArray();
                var min = vol.origIndicies.Min();
                vol.origIndicies = vol.origIndicies.Select(z => z - min).ToArray();
                faces.Clear();


            }
        }

        public BoundingBox GetBoundingBox(Matrix3 preTransform)
        {
            Vector3 maxes = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Vector3 mins = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);

            BoundingBox ret = new BoundingBox();
            foreach (var item in faces)
            {
                foreach (var zitem in item.Vertexes)
                {
                    var pos = zitem.Position * preTransform;

                    for (int i = 0; i < 3; i++)
                    {
                        maxes[i] = Math.Max(pos[i], maxes[i]);
                        mins[i] = Math.Min(pos[i], mins[i]);
                    }
                }
            }
            ret.Position = mins;
            ret.Size = maxes - mins;
            return ret;
        }

        public MaterialStuff mat = new MaterialStuff();
    }
}

