using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace RenderTool
{
    public class PBRTextureItem : AbstractTexture
    {
        public PBRTextureItem()
        {
            Id = TextureObjId++;
        }

        public DateTime lastBindTimestamp;
        public override void Bind()
        {
            if (!_loaded)
                return;

            lastBindTimestamp = DateTime.Now;
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, albedo);
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture2D, normal);
            GL.ActiveTexture(TextureUnit.Texture2);
            GL.BindTexture(TextureTarget.Texture2D, metallic);
            GL.ActiveTexture(TextureUnit.Texture3);
            GL.BindTexture(TextureTarget.Texture2D, roughness);
            GL.ActiveTexture(TextureUnit.Texture4);
            GL.BindTexture(TextureTarget.Texture2D, ao);
        }


        bool _loaded;
        public override bool Loaded => _loaded;

        public int albedo;
        public int normal;
        public int ao;
        public int roughness;
        public int metallic;

        public PBRTextureItem(int albedo, int normal, int metallic, int roughness, int ao)
        {
            this.albedo = albedo;
            this.normal = normal;
            this.metallic = metallic;
            this.roughness = roughness;
            this.ao = ao;
            Id = TextureObjId++;
        }

        internal void LoadFromDirectory(string fullName)
        {
            var di = new DirectoryInfo(fullName);
            foreach (var item in di.GetFiles())
            {
                if (item.FullName.ToLower().Contains("albedo"))
                    albedo = RenderHelpers.LoadTexture(Bitmap.FromFile(item.FullName) as Bitmap);

                if (item.FullName.ToLower().Contains("normal"))
                    normal = RenderHelpers.LoadTexture(Bitmap.FromFile(item.FullName) as Bitmap);

                if (item.FullName.ToLower().Contains("metallic"))
                    metallic = RenderHelpers.LoadTexture(Bitmap.FromFile(item.FullName) as Bitmap);

                if (item.FullName.ToLower().Contains("roughness"))
                    roughness = RenderHelpers.LoadTexture(Bitmap.FromFile(item.FullName) as Bitmap);

                if (item.FullName.ToLower().Contains("ao"))
                    ao = RenderHelpers.LoadTexture(Bitmap.FromFile(item.FullName) as Bitmap);
            }
        }


        Bitmap[] bmps = new Bitmap[5];

        bool preLoadFinished = false;
        bool preLoadStarted = false;
        public override bool PreLoadFinished => preLoadFinished;

        public override void Unload()
        {
            _loaded = false;
            GL.DeleteTexture(albedo);
            GL.DeleteTexture(normal);
            GL.DeleteTexture(metallic);
            GL.DeleteTexture(roughness);
            GL.DeleteTexture(ao);

            albedo = 0;
            normal = 0;
            metallic = 0;
            roughness = 0;
            ao = 0;
        }

        public override void Load()
        {
            if (Loaded) return;
            if (!preLoadFinished) return;
            if (TexturePool.SingleStageTextureLoadMode)
                Load1Stage();
            else
            {
                albedo = RenderHelpers.LoadTexture(bmps[0]);
                normal = RenderHelpers.LoadTexture(bmps[1]);
                metallic = RenderHelpers.LoadTexture(bmps[2]);
                roughness = RenderHelpers.LoadTexture(bmps[3]);
                ao = 0;
                if (bmps[4] != null)
                    ao = RenderHelpers.LoadTexture(bmps[4]);

                foreach (var item in bmps)
                {
                    if (item == null) continue;
                    item.Dispose();
                }
                bmps = null;
            }
            _loaded = true;
        }

        private void Load1Stage()
        {

            bool metalFounded = false;

            string[] exts = new[] { ".png", ".jpg", ".jpeg" };
            using (ZipArchive zip = ZipFile.Open(OriginFilePath, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (!exts.Any(z => entry.Name.ToLower().EndsWith(z))) continue;

                    var cnt1 = Regex.Matches(entry.Name.ToLower(), "metal").Count;
                    if (cnt1 > 1)
                    {
                        metalFounded = true;
                        using (var st = entry.Open())
                        {
                            metallic = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                        break;
                    }
                }

                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (!exts.Any(z => entry.Name.ToLower().EndsWith(z))) continue;
                    if (entry.Name.ToLower().Contains("albedo") || entry.Name.ToLower().Contains("basecolor"))
                    {
                        using (var st = entry.Open())
                        {
                            albedo = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("normal"))
                    {
                        using (var st = entry.Open())
                        {
                            normal = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("metal") && !metalFounded)
                    {
                        metalFounded = true;
                        using (var st = entry.Open())
                        {
                            metallic = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("rough"))
                    {
                        using (var st = entry.Open())
                        {
                            roughness = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("ao") || entry.Name.ToLower().Contains("ambientOcclusion"))
                    {
                        using (var st = entry.Open())
                        {
                            ao = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                }
            }
        }

        public override void StartAsyncLoad(string anyFilePath = null)
        {
            if (preLoadStarted) return;
            if (anyFilePath == null)
            {
                anyFilePath = OriginFilePath;
            }
            Thread th = new Thread(() =>
            {
                try
                {
                    StartLoad(anyFilePath);
                }
                catch (Exception ex)
                {

                }
            });

            th.IsBackground = true;
            th.Start();
        }



        public void StartLoad(string anyFilePath)
        {
            if (preLoadStarted) return;
            preLoadStarted = true;
            OriginFilePath = anyFilePath;
            if (!TexturePool.SingleStageTextureLoadMode)
            {

                using (ZipArchive zip = ZipFile.Open(anyFilePath, ZipArchiveMode.Read))
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.Name.ToLower().Contains("albedo") || entry.Name.ToLower().Contains("basecolor"))
                        {
                            using (var st = entry.Open())
                            {
                                bmps[0] = (Bitmap.FromStream(st) as Bitmap);
                            }
                        }
                        if (entry.Name.ToLower().Contains("normal"))
                        {
                            using (var st = entry.Open())
                            {
                                bmps[1] = (Bitmap.FromStream(st) as Bitmap);
                            }
                        }
                        if (entry.Name.ToLower().Contains("metal"))
                        {
                            using (var st = entry.Open())
                            {
                                bmps[2] = (Bitmap.FromStream(st) as Bitmap);
                            }
                        }
                        if (entry.Name.ToLower().Contains("rough"))
                        {
                            using (var st = entry.Open())
                            {
                                bmps[3] = (Bitmap.FromStream(st) as Bitmap);
                            }
                        }
                        if (entry.Name.ToLower().Contains("ao"))
                        {
                            using (var st = entry.Open())
                            {
                                bmps[4] = (Bitmap.FromStream(st) as Bitmap);
                            }
                        }
                    }
            }
            preLoadFinished = true;
        }

        internal void LoadFromZip(string anyFilePath)
        {
            OriginFilePath = anyFilePath;
            using (ZipArchive zip = ZipFile.Open(anyFilePath, ZipArchiveMode.Read))
                foreach (ZipArchiveEntry entry in zip.Entries)
                {
                    if (entry.Name.ToLower().Contains("albedo") || entry.Name.ToLower().Contains("basecolor"))
                    {
                        using (var st = entry.Open())
                        {
                            albedo = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("normal"))
                    {
                        using (var st = entry.Open())
                        {
                            normal = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("metal"))
                    {
                        using (var st = entry.Open())
                        {
                            metallic = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("rough"))
                    {
                        using (var st = entry.Open())
                        {
                            roughness = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                    if (entry.Name.ToLower().Contains("ao"))
                    {
                        using (var st = entry.Open())
                        {
                            ao = RenderHelpers.LoadTexture(Bitmap.FromStream(st) as Bitmap);
                        }
                    }
                }
        }
    }
}


