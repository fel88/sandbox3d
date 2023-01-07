using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using RenderTool;

namespace RenderTool
{
    public partial class Form1 : Form
    {
        private void Form1_Load(object sender, EventArgs e)
        {
            mf = new MessageFilter();



            Application.AddMessageFilter(mf);
            propertyGrid1.DoubleClick += PropertyGrid1_DoubleClick;
            FormClosing += Form1_FormClosing;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            
        }



        private void PropertyGrid1_DoubleClick(object sender, EventArgs e)
        {

        }

        MessageFilter mf = null;

        Label label1;
        bool dynamicLight = true;
        public bool frameRendered = false;
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var deltaTime = 0.010f;
            if (tableLayoutPanel2.ContainsFocus) return false;
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }
            if (keyData == Keys.W)
            {
                //  camera.ProcessKeyboard(Camera_Movement.FORWARD, deltaTime);
                return true;
            }
            if (keyData == Keys.O)
            {
                camera1.IsOrtho = !camera1.IsOrtho;
                return true;
            }            
            if (keyData == Keys.L)
            {
                dynamicLight = !dynamicLight;
                return true;
            }
            if (keyData == Keys.S)
            {
                // camera.ProcessKeyboard(Camera_Movement.BACKWARD, deltaTime);
                return true;
            }
            if (keyData == Keys.A)
            {
                //  camera.ProcessKeyboard(Camera_Movement.LEFT, deltaTime);
                return true;
            }
            if (keyData == Keys.D)
            {
                //camera.ProcessKeyboard(Camera_Movement.RIGHT, deltaTime);
                return true;
            }

            return false;
        }

        public void ReattachGlControl()
        {
            glControl.Parent = null;
            tableLayoutPanel1.Controls.Add(glControl, 1, 0);
        }

        public Form1()
        {
            InitializeComponent();
            

            //glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 4), 3, 3, OpenTK.Graphics.GraphicsContextFlags.Default);
            glControl = new OpenTK.GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8), 3, 3, OpenTK.Graphics.GraphicsContextFlags.Debug);
            label1 = new Label() { AutoSize = true };
            glControl.Controls.Add(label1);
            label1.BackColor = Color.FromArgb(25, 25, 25);
            label1.ForeColor = Color.White;
            glControl.MouseMove += GlControl_MouseMove;
            glControl.Paint += Gl_Paint;

            tableLayoutPanel1.Controls.Add(glControl, 1, 0);
            glControl.Dock = DockStyle.Fill;
            //glControl.Width = 3000;
            // glControl.Height = 3000;
            Width = SCR_WIDTH;
            Height = SCR_HEIGHT;
            evwrapper = new EventWrapperGlControl(glControl);

            AddCamera(new Camera(Scene) { IsOrtho = true });
            camera1 = cameras.Last();
            //objects.Add(new PBRSample());
            //objects.Add(new SupportSceneObject());

            updateObjectList();
            ViewManager = new DefaultCameraViewManager();
            ViewManager.Attach(evwrapper, camera1);
            Load += Form1_Load;
        }

        private void GlControl_MouseMove(object sender, MouseEventArgs e)
        {
            var cur = PointToClient(Cursor.Position);

            var xpos = cur.X;
            var ypos = cur.Y;
            //var xOffset = Width / 2 - cur.X;
            //var yOffset = Height / 2 - cur.Y;

            if (firstMouse)
            {
                lastX = xpos;
                lastY = ypos;
                firstMouse = false;
            }

            float xoffset = xpos - lastX;
            float yoffset = lastY - ypos; // reversed since y-coordinates go from bottom to top

            lastX = xpos;
            lastY = ypos;
            //camera.ProcessMouseMovement(xoffset, yoffset);
        }

        float lastX = 800.0f / 2.0f;
        float lastY = 600.0f / 2.0f;
        bool firstMouse = true;

        public GLControl glControl;
        //Shader shader;
        bool first = true;
        const int SCR_WIDTH = 1280;
        const int SCR_HEIGHT = 720;
        public Scene Scene = new Scene();
        List<SceneObject> objects => Scene.Objects;

        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                GL.Viewport(0, 0, SCR_WIDTH, SCR_HEIGHT);
                //objects.Add(new PBRLight() { Color = lightColors[0], Position = lightPositions[0] });
                //glControl.Parent = null;
                updateObjectList();
                foreach (var item in objects)
                {
                    item.InitResources();
                }
                first = false;
            }

            Render();
        }

        public void Render()
        {
            lock (lock1)
                if (grabRequired)
                {
                    glControl.Invoke((Action)(() =>
                    {
                        Redraw();
                        if (lastGrab != null)
                        {
                            lastGrab.Dispose();
                        }
                        lastGrab = GrabScreenshot();
                        grabRequired = false;
                        glControl.SwapBuffers();
                    }));
                }
                else
                {
                    glControl.Invoke((Action)(() =>
                    {
                        Redraw();
                        //label1.Text = "Dynamic light: " + (dynamicLight ? "On" : "Off");
                        glControl.SwapBuffers();
                    }));
                }
        }

        public Bitmap lastGrab;

        public bool grabRequired = false;
        public object lock1 = new object();

        Camera camera1;
        void AddCamera(Camera c)
        {
            objects.Add(c);
        }
        Camera[] cameras { get => objects.OfType<Camera>().ToArray(); }

        private EventWrapperGlControl evwrapper;
        public CameraViewManager ViewManager;
        RenderHelpers rh = new RenderHelpers();

        public static Color BackColor1 = Color.LightBlue;
        public static Color BackColor2 = Color.AliceBlue;
        void oldStyleRedraw()
        {
            GL.UseProgram(0);
            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);

            GL.Disable(EnableCap.DepthTest);
            float zz = -500;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(BackColor1);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(BackColor2);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();

            //GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Enable(EnableCap.DepthTest);
            /////////////////


            ////////////
            GL.UseProgram(0);


            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();
            camera1.Setup(glControl);
            //shader.use();
            var model = Matrix4.Identity;
            //  shader.setMat4("model", model);
            //shader.setMat4("projection", camera1.ProjectionMatrix);

            if (!renderMode)
            {
                GL.PushMatrix();

                GL.LineWidth(2);
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(100, 0, 0);
                GL.End();

                GL.Color3(Color.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 100, 0);
                GL.End();

                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 100);
                GL.End();
                GL.PopMatrix();
            }

            /*   GL.Color3(Color.Brown);
               //GL.Enable(EnableCap.Lighting);
               GL.Enable(EnableCap.DepthTest);

               GL.Enable(EnableCap.Light0);
               GL.Light(LightName.Light0, LightParameter.Position, new Vector4(0, 0, 100, 1));
               GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(100, 100, 100, 1));
               GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(100, 100, 100, 1));
               GL.Light(LightName.Light0, LightParameter.SpotDirection, new Vector4(0, 0, -1, 1));*/
            //GL.ShadeModel(ShadingModel.Smooth);


            GL.Enable(EnableCap.DepthTest);

            DrawingEnvironment denv = new DrawingEnvironment() { Camera = camera1 };
            foreach (var item in objects)
            {
                if (!item.Visible) continue;
                if (renderMode)
                {
                    if (!(item is BlankModel || item is SceneTexture || item is ObjModel)) continue;
                }

                item.Draw(denv);
            }
            if (!renderMode && HighlightSelected)
            {
                if (selectedObject != null)
                {
                    if (selectedObject is PBRModel pbm)
                    {
                        var temp = pbm.Lights[0].Color;
                        pbm.Lights[0].Color = new Vector3(255, 0, 0);
                        (selectedObject as SceneObject).Draw(denv);
                        pbm.Lights[0].Color = temp;
                    }
                }
            }
            GL.Disable(EnableCap.Lighting);
        }

        public Bitmap GrabScreenshot()
        {
            Stopwatch sw = Stopwatch.StartNew();
            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            Bitmap bmp = new Bitmap(this.glControl.Width, this.glControl.Height);
            System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, glControl.Width, glControl.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            GL.ReadPixels(0, 0, glControl.Width, glControl.Height, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, data.Scan0);
            GL.Finish();
            bmp.UnlockBits(data);
            bmp.RotateFlip(RotateFlipType.RotateNoneFlipY);
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            return bmp;
        }


        TimeSpan unloadTimeSpan = new TimeSpan(0, 0, 30);
        void Redraw()
        {
            toolStripStatusLabel3.Text = $"Textures load: {Scene.Pool.Textures.Count(z => z.Loaded)} / {Scene.Pool.Textures.Count}";

            if (TexturePool.SingleStageTextureLoadMode)
            {
                foreach (var item in Scene.Pool.Textures)
                {
                    if (item.Loaded) continue;
                    if (!item.PreLoadFinished)
                    {
                        item.StartAsyncLoad();
                    }
                }
            }
            else
            {
                foreach (var item in Scene.Pool.Textures)
                {
                    if (item.Loaded) continue;
                    if (!item.PreLoadFinished)
                    {
                        item.StartAsyncLoad();
                    }
                    else
                    {
                        item.Load();
                    }
                    break;
                }
            }


            oldStyleRedraw();


            //cleanup unused texture
            if (TexturePool.CleanupByTime)
            {
                foreach (var item in Scene.Pool.Textures.OfType<PBRTextureItem>())
                {
                    if (DateTime.Now.Subtract(item.lastBindTimestamp) > unloadTimeSpan)
                    {
                        item.Unload();
                    }
                }
            }
        }



        public bool TimerEnabled { get => timer1.Enabled; set => timer1.Enabled = value; }
        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }


        public bool renderMode = false;
        
        private void cameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddCamera(new Camera(Scene));

            updateObjectList();
        }

        void updateObjectList()
        {
            listView1.Items.Clear();
            foreach (var item in objects)
            {
                listView1.Items.Add(new ListViewItem(new string[] { item.GetType().Name, item.Name }) { Tag = item });
            }
        }

        object selectedObject = null;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                selectedObject = null;
                return;
            }
            propertyGrid1.SelectedObject = listView1.SelectedItems[0].Tag;
            selectedObject = listView1.SelectedItems[0].Tag;
        }

        private void propertyGrid1_Click(object sender, EventArgs e)
        {

        }

        private void switchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag;
            if (tag is Camera cc)
            {
                camera1 = cc;
                camera1.UpdateMatricies(glControl);
                (ViewManager as DefaultCameraViewManager).Camera = cc;

                timer1.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            camera1.CamTo = camera1.CamFrom + new Vector3(camera1.DirLen, 0, 0);
            camera1.CamUp = Vector3.UnitZ;
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelected();
        }
        public void DeleteSelected()
        {
            if (MessageBox.Show("Are you sure?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as SceneObject;
            if (selectedObject == tag)
            {
                selectedObject = null;
            }
            Scene.RemoveItem(tag);
            updateObjectList();
        }

        private void propertyGrid1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            var pd = propertyGrid1.SelectedGridItem.PropertyDescriptor;
            var tp = propertyGrid1.SelectedObject.GetType();
            var prop = tp.GetProperty(pd.Name);
            if (prop.PropertyType == typeof(Vector3))
            {
                var val = (Vector3)prop.GetValue(propertyGrid1.SelectedObject);
                vec3EditorDialog d = new vec3EditorDialog();
                d.Init(val);
                d.ShowDialog();
                prop.SetValue(propertyGrid1.SelectedObject, new Vector3(d.X, d.Y, d.Z));
            }

        }

        private void propertyGrid1_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {

        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "XML scenes (*.xml)|*.xml";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var sb = Scene.ToXml(camera1.Id, this);
            File.WriteAllText(sfd.FileName, sb);
        }

        
        
        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            commandsToolStripMenuItem.DropDownItems.Clear();
            if (listView1.SelectedItems[0].Tag is ICommandsContainer cc)
            {

                foreach (var item in cc.Commands)
                {
                    var ccc = new ToolStripMenuItem(item.GetType().Name) { };
                    ccc.Click += (s, ee) =>
                    {
                        item.Execute(new CommandEnvironment() { Sender = cc, Form = this });
                    };
                    commandsToolStripMenuItem.DropDownItems.Add(ccc);
                }
            }
        }


        private void blankToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Blank bl = new Blank();

            bl.Points.Add(new PointF(0, 0));
            bl.Points.Add(new PointF(100, 0));
            bl.Points.Add(new PointF(100, 100));
            bl.Points.Add(new PointF(0, 100));
            var bm = RenderHelpers.GetBlankModel(bl, new PointF(0, 0));

            PolyBoolCS.PolyBool pb = new PolyBoolCS.PolyBool();

            var bm1 = new BlankModel(Scene) { Blank = bl, Model = bm };
            objects.Add(bm1);
            bm1.asyncPreLoadDefaultTextures();

            updateObjectList();
        }

        internal void SwitchCamera(Camera cc)
        {
            camera1 = cc;
            camera1.UpdateMatricies(glControl);
            (ViewManager as DefaultCameraViewManager).Camera = cc;
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "XML scene files (*.xml)|*.xml";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            var doc = XDocument.Load(ofd.FileName);
            loadScene(doc);
        }

        private void updateListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            updateObjectList();
        }

        private void oBJModelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Obj files(*.obj)|*.obj";
            string objPath = "";
            string mtlPath = "";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ObjModel mod = new ObjModel(Scene);

                mod.LoadFromFile(ofd.FileName);
                objects.Add(mod);
                updateObjectList();
            }
        }

        private void toolStripButton1_Click_1(object sender, EventArgs e)
        {
            camera1.Reset();
        }

        private void toolStripButton2_Click_1(object sender, EventArgs e)
        {
            offscreenRender u = new offscreenRender();
            u.Scene = Scene;
            u.MdiParent = MdiParent;
            u.Show();
        }


        private void button3_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                BackColor1 = colorDialog1.Color;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
                BackColor2 = colorDialog1.Color;
        }

        private void listView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteSelected();
            }
        }

        private void cloneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            if (listView1.SelectedItems.Count == 0) return;
            var tag = listView1.SelectedItems[0].Tag as SceneObject;
            var obj = tag.Clone();
            obj.Name += " (clone)";
            objects.Add(obj);
            updateObjectList();
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "ZIP scenes (*.zip)|*.zip";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var sb = Scene.ToXml(camera1.Id, this);

            HashSet<string> ts = new HashSet<string>();

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    archive.CreateEntry("resources\\");

                    foreach (var tex in Scene.Pool.Textures)
                    {
                        var tpath = tex.OriginFilePath;
                        var pp = "resources\\" + Path.GetFileName(tpath);
                        if (!ts.Contains(tpath))
                        {
                            archive.CreateEntryFromFile(tpath, "resources\\" + Path.GetFileName(tpath));
                            ts.Add(tpath);
                        }
                    }

                    foreach (var item in ts)
                    {
                        sb = sb.Replace(item, "resources\\" + Path.GetFileName(item));
                    }
                    var demoFile = archive.CreateEntry("scene.xml");

                    using (var entryStream = demoFile.Open())
                    using (var streamWriter = new StreamWriter(entryStream))
                    {
                        streamWriter.Write(sb.ToString());
                    }
                }

                using (var fileStream = new FileStream(sfd.FileName, FileMode.Create))
                {
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    memoryStream.CopyTo(fileStream);
                }
            }
        }

        

        void loadScene(XDocument doc)
        {
            try
            {
                objects.Clear();

                var root = doc.Element("root");
                if (root.Attribute("outputW") != null)
                {
                    offscreenRender.maxW = int.Parse(root.Attribute("outputW").Value);
                    offscreenRender.maxH = int.Parse(root.Attribute("outputH").Value);
                }
                if (root.Attribute("windowW") != null)
                {
                    Width = int.Parse(root.Attribute("windowW").Value);
                    Height = int.Parse(root.Attribute("windowH").Value);
                }
                var types = Assembly.GetExecutingAssembly().GetTypes();
                var sc = types.Where(z => z.CustomAttributes.Any(u => u.AttributeType == typeof(SceneStorableObjectAttribute))).ToArray();

                var objs = root.Element("objects");


                var pool = root.Element("pool");
                if (pool != null)
                {
                    foreach (var titem in pool.Elements("texture"))
                    {
                        var v = titem.Value;
                        var tId = int.Parse(titem.Attribute("id").Value);
                        AbstractTexture add = null;
                        bool isFlat = false;
                        if (titem.Attribute("type") != null)
                        {
                            var tp = titem.Attribute("type").Value.ToLower();
                            if (tp == "flat")
                            {
                                isFlat = true;
                            }
                        }
                        if (isFlat)
                            add = new FlatTextureItem();
                        else
                            add = new PBRTextureItem();

                        var fr = Scene.Pool.Textures.FirstOrDefault(z => z.Id == tId);
                        if (fr != null)
                        {
                            throw new Exception("texture ID already exists");
                            //fr.Id = AbstractTexture.TextureObjId++;
                        }
                        add.Id = tId;
                        add.OriginFilePath = v;
                        Scene.Pool.Textures.Add(add);
                        //Scene.Pool.Textures.Last().StartAsyncLoad(v);
                    }
                }
                foreach (var item in objs.Elements())
                {
                    var fr = sc.FirstOrDefault(z =>
                    {
                        var attr = z.GetCustomAttribute(typeof(SceneStorableObjectAttribute)) as SceneStorableObjectAttribute;
                        return attr.XmlKey == item.Name;
                    });
                    if (fr == null) continue;
                    var inst = Activator.CreateInstance(fr, Scene) as SceneObject;
                    inst.RestoreXml(item);
                    objects.Add(inst);
                }

                

                var camIdx = int.Parse(root.Attribute("activeCamera").Value);
                camera1 = objects.OfType<Camera>().First(z => z.Id == camIdx);
                (ViewManager as DefaultCameraViewManager).Camera = camera1;
                SceneObject.NewObjectId = objects.Max(z => z.Id) + 1;
                updateObjectList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "ZIP scene files (*.zip)|*.zip";
            if (ofd.ShowDialog() != DialogResult.OK) return;
            XDocument doc = null;
            using (ZipArchive zip = ZipFile.OpenRead(ofd.FileName))
            {
                var s = zip.GetEntry("scene.xml");
                using (var stream = s.Open())
                {
                    doc = XDocument.Load(stream);
                }
                //ZipEntry e = zip["MyReport.doc"];
                //e.Extract(OutputStream);
                //extract temp resources
                Directory.CreateDirectory("resources");
                foreach (var item in zip.Entries)
                {
                    if (item.Length == 0) continue;
                    if (!item.FullName.Contains("resources\\")) continue;

                    item.ExtractToFile("resources\\" + item.Name, true);
                }
            }
            loadScene(doc);
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            TexturesPool pool = new TexturesPool();
            pool.Init(Scene);
            pool.MdiParent = MdiParent;
            pool.Show();
        }

        public bool HighlightSelected = false;
        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            HighlightSelected = checkBox2.Checked;
        }

        bool keepWindowAspect = false;
        float aspect = 1;
        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!keepWindowAspect)
            {
                aspect = glControl.Width / (float)glControl.Height;
            }
            keepWindowAspect = checkBox3.Checked;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (keepWindowAspect)
            {
                Height = (int)(Width / aspect);
            }
        }


        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            MaximizeBox = !checkBox4.Checked;
            FormBorderStyle = checkBox4.Checked ? FormBorderStyle.FixedSingle : FormBorderStyle.Sizable;
        }

        private void sceneTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var bm1 = new SceneTexture(Scene) { };
            objects.Add(bm1);
            bm1.asyncPreLoadDefaultTextures();

            updateObjectList();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            toolStripStatusLabel4.Text = "timer: " + timer1.Enabled;
            toolStripStatusLabel4.ForeColor = timer1.Enabled ? Color.Green : Color.Red;
        }

        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {
            timer1.Enabled = true;
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            foreach (var texture in Scene.Pool.Textures)
                texture.Load();
        }
    }

    public class BoundingBox
    {
        public Vector3 Position;
        public Vector3 Size;
    }

    

    public interface ICommand
    {
        void Execute(CommandEnvironment sender);
    }

    public class EditBlankCommand : ICommand
    {
        public void Execute(CommandEnvironment sender)
        {
            BlankEditorDialog dd = new BlankEditorDialog();
            dd.MdiParent = Program.MainForm;
            dd.Init(sender.Sender as BlankModel);
            dd.Show();
        }
    }


    public class SetPBRTextureToBlankCommand : ICommand
    {
        public void Execute(CommandEnvironment sender)
        {
            //  OpenFileDialog ofd = new OpenFileDialog();
            //if (ofd.ShowDialog() != DialogResult.OK) return;
            // var fi = new FileInfo(ofd.FileName);
            TexturesManager tm = new TexturesManager();

            tm.Init((sender.Sender as ITextureLoad), sender.Form.Scene.Pool);
            tm.TopMost = true;
            tm.Show();

            //(sender.Sender as BlankModel).LoadTexture(fi.FullName);
        }
    }

    public class CommandEnvironment
    {
        public object Sender;
        public Form1 Form;
    }
    
    
    public class DrawingEnvironment : IDrawingEnvironment
    {
        public Camera Camera { get; set; }
    }

    
}