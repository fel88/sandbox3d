using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class MaterialStuff
    {
        /// <summary>
        /// Loads all materials in a file, along with their required maps.
        /// Materials will not overwrite existing materials with the same name.
        /// </summary>
        /// <param name="filename">MTL file to load from</param>
        public void loadMaterials(String filename)
        {

            foreach (var mat in Material.LoadFromFile(filename))
            {
                if (!materials.ContainsKey(mat.Key))
                {
                    materials.Add(mat.Key, mat.Value);
                }
            }
            var d = new FileInfo(filename);
            var name = d.DirectoryName;
            Directory.SetCurrentDirectory(name);

            // Load textures
            foreach (Material mat in materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !textures.ContainsKey(mat.AmbientMap))
                {
                    var td = loadImage(mat.AmbientMap);
                    TDesc.Add(td);
                    textures.Add(mat.AmbientMap, td.Index);
                }

                if (File.Exists(mat.DiffuseMap) && !textures.ContainsKey(mat.DiffuseMap))
                {
                    var td = loadImage(mat.DiffuseMap);
                    TDesc.Add(td);
                    textures.Add(mat.DiffuseMap, td.Index);
                }

                if (File.Exists(mat.SpecularMap) && !textures.ContainsKey(mat.SpecularMap))
                {
                    var td = loadImage(mat.SpecularMap);
                    TDesc.Add(td);
                    textures.Add(mat.SpecularMap, td.Index);
                }

                if (File.Exists(mat.NormalMap) && !textures.ContainsKey(mat.NormalMap))
                {
                    var td = loadImage(mat.NormalMap);
                    TDesc.Add(td);
                    textures.Add(mat.NormalMap, td.Index);
                }

                if (File.Exists(mat.OpacityMap) && !textures.ContainsKey(mat.OpacityMap))
                {
                    var td = loadImage(mat.OpacityMap);
                    TDesc.Add(td);
                    textures.Add(mat.OpacityMap, td.Index);
                }
            }
        }
        static int loadImage(Bitmap image)
        {
            int texID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texID);
            BitmapData data = image.LockBits(new System.Drawing.Rectangle(0, 0, image.Width, image.Height),
                                             ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            int lin1 = (int)TextureMinFilter.Linear;
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, ref lin1);
            GL.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, ref lin1);
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.GenerateMipmap, 1);


            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                          OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            //GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texID;
        }

        static TextureDescriptor loadImage(string filename)
        {
            try
            {
                //using (
                Bitmap file = new Bitmap(filename);
                //)
                {
                    file.RotateFlip(RotateFlipType.RotateNoneFlipY);
                    return new TextureDescriptor() { Index = loadImage(file), Preview = file };
                }
            }
            catch (FileNotFoundException e)
            {
                return null;
            }
        }


        public Dictionary<String, Material> materials = new Dictionary<string, Material>();
        public Dictionary<string, int> textures = new Dictionary<string, int>();
        public List<TextureDescriptor> TDesc = new List<TextureDescriptor>();
    }
}

