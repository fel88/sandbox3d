using System;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Ode.Net.Collision;
using Ode.Net;
using Ode.Net.Joints;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using tkVector3 = OpenTK.Vector3;
using tkQuaternion = OpenTK.Quaternion;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FxEngine;
using System.IO;
using FxEngine.Shaders;
using FxEngine.Loaders.OBJ;
using Sandbox.Lib;
using IkBonePool = Sandbox.Lib.IkBonePool;

namespace Sandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            panel1.Controls.Add(drawer);
            drawer.Dock = DockStyle.Fill;
            drawer.Margin = new Padding(0);
            drawer.ContextMenuStrip = contextMenuStrip1;
            Stuff.CurrentEnvironment = new SandboxEnvironment();
            drawer.DrawObjects = DrawObjects;
            KeyUp += Form1_KeyUp;
            drawer.KeyUp += Drawer_KeyUp;
            checkBox5.Checked = drawer.EnableDrawGrid;
            InitPhysics();

            tabControl2.TabPages.RemoveAt(2);

            LoadModels();
        }
        Shader ModelDrawShader = new Model3DrawShader("model3.vs", "model3.fs");
        VaoModel vmod = null;

        public void LoadModels()
        {
            string objPath = "";
            string mtlPath = "";

            Stuff.models.Clear();
            try
            {
                Stuff.models.AddRange(ObjVolume.LoadFromFile(@"models\\ball.obj", Matrix4.Identity));
            }
            catch (Exception ex) { }
          //  var vol = Stuff.models.Last();
            vmod = new VaoModel();


        }

        private void Drawer_KeyUp(object sender, KeyEventArgs e)
        {
            keys[(int)e.KeyCode] = false;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            keys[(int)e.KeyCode] = false;
        }


        tkVector3 carDir = new tkVector3(1, 0, 0);
        bool[] keys = new bool[256];
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (msg.Msg != 0x100)
            {

            }
            if ((int)keyData < 256)
            {
                keys[(int)keyData] = true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
        public void DrawOdeBox(Box box)
        {
            GL.PushMatrix();

            var indices = new List<uint>();
            var vertices = new List<VertexPositionNormalTexture>();
            //(entity as Box).Lengths;

            var sz = new tkVector3((float)box.Lengths.X * 0.5f, (float)box.Lengths.Y * 0.5f, (float)box.Lengths.Z * 0.5f);
            var corners = BoundingBox.GetCorners(new tkVector3(-sz.X, -sz.Y, -sz.Z), new tkVector3(sz.X, sz.Y, sz.Z));

            var textureCoords = new Vector2[4];
            textureCoords[0] = new Vector2(0, 0);
            textureCoords[1] = new Vector2(1, 0);
            textureCoords[2] = new Vector2(1, 1);
            textureCoords[3] = new Vector2(0, 1);

            vertices.Add(new VertexPositionNormalTexture(corners[0], tkVector3.UnitZ, textureCoords[0]));
            vertices.Add(new VertexPositionNormalTexture(corners[1], tkVector3.UnitZ, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[2], tkVector3.UnitZ, textureCoords[2]));
            vertices.Add(new VertexPositionNormalTexture(corners[3], tkVector3.UnitZ, textureCoords[3]));
            indices.Add(0);
            indices.Add(1);
            indices.Add(2);
            indices.Add(0);
            indices.Add(2);
            indices.Add(3);

            vertices.Add(new VertexPositionNormalTexture(corners[1], tkVector3.UnitX, textureCoords[0]));
            vertices.Add(new VertexPositionNormalTexture(corners[2], tkVector3.UnitX, textureCoords[3]));
            vertices.Add(new VertexPositionNormalTexture(corners[5], tkVector3.UnitX, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[6], tkVector3.UnitX, textureCoords[2]));
            indices.Add(4);
            indices.Add(6);
            indices.Add(7);
            indices.Add(4);
            indices.Add(7);
            indices.Add(5);

            vertices.Add(new VertexPositionNormalTexture(corners[4], -tkVector3.UnitZ, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[5], -tkVector3.UnitZ, textureCoords[0]));
            vertices.Add(new VertexPositionNormalTexture(corners[6], -tkVector3.UnitZ, textureCoords[3]));
            vertices.Add(new VertexPositionNormalTexture(corners[7], -tkVector3.UnitZ, textureCoords[2]));
            indices.Add(9);
            indices.Add(8);
            indices.Add(11);
            indices.Add(9);
            indices.Add(11);
            indices.Add(10);

            vertices.Add(new VertexPositionNormalTexture(corners[0], -tkVector3.UnitX, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[3], -tkVector3.UnitX, textureCoords[2]));
            vertices.Add(new VertexPositionNormalTexture(corners[4], -tkVector3.UnitX, textureCoords[0]));
            vertices.Add(new VertexPositionNormalTexture(corners[7], -tkVector3.UnitX, textureCoords[3]));
            indices.Add(14);
            indices.Add(12);
            indices.Add(13);
            indices.Add(14);
            indices.Add(13);
            indices.Add(15);

            vertices.Add(new VertexPositionNormalTexture(corners[0], tkVector3.UnitY, textureCoords[2]));
            vertices.Add(new VertexPositionNormalTexture(corners[1], tkVector3.UnitY, textureCoords[3]));
            vertices.Add(new VertexPositionNormalTexture(corners[4], tkVector3.UnitY, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[5], tkVector3.UnitY, textureCoords[0]));
            indices.Add(16);
            indices.Add(19);
            indices.Add(17);
            indices.Add(16);
            indices.Add(18);
            indices.Add(19);

            vertices.Add(new VertexPositionNormalTexture(corners[2], -tkVector3.UnitY, textureCoords[1]));
            vertices.Add(new VertexPositionNormalTexture(corners[3], -tkVector3.UnitY, textureCoords[0]));
            vertices.Add(new VertexPositionNormalTexture(corners[6], -tkVector3.UnitY, textureCoords[2]));
            vertices.Add(new VertexPositionNormalTexture(corners[7], -tkVector3.UnitY, textureCoords[3]));
            indices.Add(21);
            indices.Add(20);
            indices.Add(22);
            indices.Add(21);
            indices.Add(22);
            indices.Add(23);
            DrawObject(ctx, indices.ToArray(), vertices.ToArray(), Stuff.ToTkVector3(box.Position), Stuff.ToTkQuternion(box.Quaternion), box.Tag, ctx.SelectedEntities.Contains(box));
            GL.PopMatrix();
        }
        public void DrawOdeSphere(Sphere box)
        {
            GL.PushMatrix();

            var indices = new List<uint>();
            var vertices = new List<VertexPositionNormalTexture>();

            int cnt = 0;
            for (int i = 0; i < 360; i += 15)
            {
                for (int j = 0; j < 360; j += 15)
                {
                    var xx = box.Radius * Math.Cos(i * Math.PI / 180) * Math.Sin(j * Math.PI / 180);
                    var yy = box.Radius * Math.Sin(i * Math.PI / 180) * Math.Cos(j * Math.PI / 180);
                    var z = box.Radius * Math.Cos(j * Math.PI / 180);

                    var j2 = j + 15;
                    var i2 = i + 15;
                    var xx1 = box.Radius * Math.Cos(i * Math.PI / 180) * Math.Sin(j2 * Math.PI / 180);
                    var yy1 = box.Radius * Math.Sin(i * Math.PI / 180) * Math.Cos(j2 * Math.PI / 180);
                    var z1 = box.Radius * Math.Cos(j2 * Math.PI / 180);

                    var xx2 = box.Radius * Math.Cos(i2 * Math.PI / 180) * Math.Sin(j2 * Math.PI / 180);
                    var yy2 = box.Radius * Math.Sin(i2 * Math.PI / 180) * Math.Cos(j2 * Math.PI / 180);
                    var z2 = box.Radius * Math.Cos(j2 * Math.PI / 180);

                    var xx3 = box.Radius * Math.Cos(i2 * Math.PI / 180) * Math.Sin(j * Math.PI / 180);
                    var yy3 = box.Radius * Math.Sin(i2 * Math.PI / 180) * Math.Cos(j * Math.PI / 180);
                    var z3 = box.Radius * Math.Cos(j * Math.PI / 180);

                    for (int jj = 0; jj < 6; jj++)
                    {
                        indices.Add((uint)indices.Count);
                    }

                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx, (float)yy, (float)z), tkVector3.UnitY, Vector2.One));
                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx1, (float)yy1, (float)z1), tkVector3.UnitY, Vector2.One));
                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx2, (float)yy2, (float)z2), tkVector3.UnitY, Vector2.One));
                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx2, (float)yy2, (float)z2), tkVector3.UnitY, Vector2.One));
                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx3, (float)yy3, (float)z3), tkVector3.UnitY, Vector2.One));
                    vertices.Add(new VertexPositionNormalTexture(new tkVector3((float)xx1, (float)yy1, (float)z1), tkVector3.UnitY, Vector2.One));

                }
            }


            DrawObject(ctx, indices.ToArray(), vertices.ToArray(), Stuff.ToTkVector3(box.Position), Stuff.ToTkQuternion(box.Quaternion), box.Tag, ctx.SelectedEntities.Contains(box));
            GL.PopMatrix();
        }
        public DrawingContext ctx = new DrawingContext();

        bool first = true;

        public void DrawObjects()
        {
            if (!checkBox4.Checked) return;
            Stopwatch sw = Stopwatch.StartNew();
            if (first)
            {
                first = false;
                ModelDrawShader.Init("model.vs", "model.fs");
                vmod.ModelInit(Stuff.models.ToArray());
            }

            lock (Stuff.World)
            {
                if (ball != null)
                {
                    if (cameraToRobot)
                    {
                        drawer.Camera.CamFrom = new tkVector3(-1500, (float)ball.Position.Y, 500);
                        drawer.Camera.CamTo = new tkVector3((float)ball.Position.X, (float)ball.Position.Y, 500);
                    }
                    if (ball.Position.X > 1500)
                    {
                        Ode.Net.Vector3 v3 = new Ode.Net.Vector3(-r.Next(80, 120), r.Next(-10, 10), r.Next(40, 100));
                        ball.Body.SetLinearVelocity(ref v3);
                    }
                    if (ball.Position.X < -1500)
                    {
                        Ode.Net.Vector3 v3 = new Ode.Net.Vector3(r.Next(80, 120), r.Next(-10, 10), r.Next(40, 100));
                        ball.Body.SetLinearVelocity(ref v3);
                    }
                    if (ball.Position.Z < -100)
                    {

                        Stuff.OdeSpace.Remove(ball);
                        ball = null;
                    }
                }
                
                foreach (var entity in Stuff.OdeSpace)
                {

                    if ((entity is Ode.Net.Collision.Box))
                    {
                        DrawOdeBox(entity as Box);
                    }
                    if ((entity is Ode.Net.Collision.Sphere))
                    {
                        var sh3 = ModelDrawShader as Model3DrawShader;

                        sh3.viewPos = drawer.Camera.CamFrom;
                        sh3.Model = Matrix4.CreateRotationZ((float)(0 * Math.PI / 180f));

                        GL.PushMatrix();
                        GL.Translate(entity.Position.X, entity.Position.Y, entity.Position.Z);
                        double k = 5 * (entity as Sphere).Radius;
                        GL.Scale(k, k, k);
                        vmod.DrawVao(ModelDrawShader);
                        GL.PopMatrix();
                        //DrawOdeSphere(entity as Sphere);
                    }
                }

            }
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            label1.Text = "ms: " + ms;
        }

        public void DrawBox()
        {

        }

        private void InitPhysics()
        {
            Ode.Net.Engine.Init();

            Stuff.World = new World();
            Stuff.World.QuickStepNumIterations = 50;

            Stuff.World.Erp = 0.2f;
            Stuff.World.Cfm = 1e-05f;

            Stuff.World.Gravity = new Ode.Net.Vector3(0, 0, -9.8f);

            Stuff.OdeSpace = new Ode.Net.Collision.HashSpace();
            Stuff.OdeSpace.Collide(new NearCallback(nearCallback));
        }

        public static Geom CreateOdeSphere(float rad = 1)
        {
            Ode.Net.Collision.Sphere b = new Ode.Net.Collision.Sphere(rad);
            var a = Mass.CreateSphere(1, rad);
            Ode.Net.Body bode = new Body(Stuff.World);
            b.Body = bode;
            bode.Mass = a;

            Stuff.OdeSpace.Add(b);

            return b;
        }

        public bool AllowOdeNearCallback = true;
        private void nearCallback(Geom geom1, Geom geom2)
        {
            if (!AllowOdeNearCallback) return;
            if (Stuff.SkipBroadPhase.Contains(geom1) || Stuff.SkipBroadPhase.Contains(geom2))
            {
                return;
            }
            Body body = geom1.Body;
            Body body2 = geom2.Body;
            if (body == null || body2 == null || (body.Kinematic && body2.Kinematic))
            {
                return;
            }

            int maxContacts = 10;
            ContactGeom[] array = new ContactGeom[maxContacts];
            ContactInfo contact = default(ContactInfo);
            for (int i = 0; i < Geom.Collide(geom1, geom2, array); i++)
            {
                contact.Geometry = array[i];
                contact.Surface.Mode = (ContactModes.Bounce | ContactModes.SoftCfm | ContactModes.SoftErp);
                contact.Surface.SoftCfm = 0.0001f;
                contact.Surface.SoftErp = 0.2f;
                contact.Surface.Mu = float.PositiveInfinity;
                contact.Surface.Bounce = 0.40f;
                contact.Surface.BounceVelocity = 0.0002f;
                Contact _contact = new Contact(Stuff.World, contact, Stuff.ContactGroup);
                _contact.Attach(body, body2);
            }
        }

        GlDrawer drawer = new GlDrawer();
        DateTime lastRenderTime = DateTime.Now;

        private void toSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            lock (Stuff.World)
            {
                CreateOdeGround();
            }
        }

        public static Geom CreateOdeGround()
        {
            Ode.Net.Collision.Box b = new Ode.Net.Collision.Box(2740, 1525, 1);
            var mass = Mass.CreateBoxTotal(1, 2740, 1525, 1);
            //var a = Mass.CreateBox(0, 25, 25, 1);


            Ode.Net.Body body = new Body(Stuff.World);
            body.Kinematic = true;

            body.Mass = mass;
            var tag = new EntityTagInfo();
            b.Tag = tag;

            tag.UseOneColor = true;
            tag.Color = GetRandomColor();

            Stuff.OdeSpace.Add(b);
            b.Body = body;
            body.Kinematic = true;
            //bode.Mass = mass;

            return b;
        }

        public static Color GetRandomColor()
        {
            var vls = Enum.GetValues(typeof(KnownColor));
            return Color.FromKnownColor((KnownColor)vls.GetValue(r.Next(vls.Length)));
        }

        static Random r = new Random();
        public static void DrawObject(DrawingContext ctx, uint[] indeices, VertexPositionNormalTexture[] array, tkVector3 position, tkQuaternion orientation, object tag, bool selected = false)
        {
            GL.Translate(position.X, position.Y, position.Z);

            OpenTK.Quaternion q = orientation;
            OpenTK.Vector3 r;
            float angle = 0;
            q.ToAxisAngle(out r, out angle);
            GL.Rotate((float)(angle * 180.0f / Math.PI), r);

            if (selected)
            {
                //DrawBoundingBox(array.Select(z => z.Position).Select(x => new BEPUutilities.Vector3(x.X, x.Y, x.Z)).ToArray());
            }
            else
            {
                GL.Color3(255, 255, 255);
            }
            //GL.Scale(10,10,10);
            List<tkVector3> vv = new List<tkVector3>();
            List<tkVector3> nv = new List<tkVector3>();

            for (int i = 0; i < indeices.Count(); i += 3)
            {
                for (int j = 0; j < 3; j++)
                {
                    vv.Add(array[indeices[i + j]].Position);
                    nv.Add(array[indeices[i + j]].Normal);
                }
            }

            if (ctx.Solid)
            {
                GL.Color3(Color.Gray);

                GL.Begin(PrimitiveType.Triangles);
                GL.Disable(EnableCap.ColorMaterial);
                int index = 0;
                if (tag != null && tag is EntityTagInfo)
                {
                    var info = tag as EntityTagInfo;
                    if (info.UseOneColor)
                    {
                        GL.Color3(info.Color);
                    }
                }
                for (int j = 0; j < vv.Count; j += 3)
                {

                    index++;

                    for (int i = 0; i < 3; i++)
                    {
                        var vector3 = vv[j + i];
                        var n3 = nv[j + i];

                        var v = vector3;
                        GL.Normal3(n3.X, n3.Y, n3.Z);

                        GL.Vertex3(v.X, v.Y, v.Z);
                    }

                }

                GL.End();
            }
            //triangl
            if (ctx.Wireframe)
            {

                GL.Disable(EnableCap.Lighting);
                GL.Enable(EnableCap.ColorMaterial);
                GL.LineWidth(ctx.WireframeLineWidth);

                for (int j = 0; j < vv.Count; j += 3)
                {
                    GL.Color3(255, 0, 0);

                    GL.Begin(PrimitiveType.LineLoop);

                    for (int i = 0; i < 3; i++)
                    {
                        var vector3 = vv[j + i];
                        var n3 = nv[j + i];
                        var v = vector3;
                        GL.Vertex3(v.X, v.Y, v.Z);
                    }
                    GL.End();

                }
            }

        }

        public Geom CreateOdeBox(Ode.Net.Vector3 pos, float density = 1, Ode.Net.Vector3? lengths = null)
        {
            int w = 3;

            Ode.Net.Collision.Box b;
            if (lengths != null)
            {
                b = new Ode.Net.Collision.Box(lengths.Value.X, lengths.Value.Y, lengths.Value.Z);
            }
            else
            {
                b = new Ode.Net.Collision.Box(w, w, w);
            }
            var a = Mass.CreateBox(density, w, w, w);

            Ode.Net.Body body = new Body(Stuff.World);
            b.Body = body;



            body.Mass = a;
            var tag = new EntityTagInfo();

            tag.UseOneColor = true;
            tag.Color = GetRandomColor();
            b.Tag = tag;

            Stuff.OdeSpace.Add(b);

            b.SetPosition(ref pos);
            b.Body.Kinematic = true;
            return b;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            CreateOdeBox(new Ode.Net.Vector3(0, 0, 15));
        }
        Sphere ball;
        private void button3_Click(object sender, EventArgs e)
        {
            lock (Stuff.World)
            {
                var b = CreateOdeSphere(20);
                ball = b as Sphere;
                var pos = new Ode.Net.Vector3(0, 0, 500);
                b.SetPosition(ref pos);
                propertyGrid1.SelectedObject = b.Body;
            }
        }

        private void contextMenuStrip1_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private Thread odeThread = null;
        public float timeScale = 1f;
        ManualResetEvent _event = new ManualResetEvent(true);

        public static SandboxEnvironment env
        {
            get { return Stuff.CurrentEnvironment; }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            env.IsSimulationEnable = checkBox1.Checked;
            if (odeThread == null)
            {
                odeThread = new Thread(new ThreadStart(this.Simulation));
                odeThread.IsBackground = true;
                odeThread.Start();
            }
            else
            {
                if (!env.IsSimulationEnable)
                {
                    _event.Reset();
                }
                else
                {
                    _event.Set();
                }
            }
        }

        public float odeStepTime = 1f / 750f;
        public int core = 2;
        public static int SimCount = 0;

        private void Simulation()
        {
            while (true)
            {
                _event.WaitOne();
                Interlocked.Increment(ref SimCount);
                lock (Stuff.World)
                {
                    Stuff.OdeSpace.Collide(new NearCallback(this.nearCallback));
                    Stuff.World.QuickStep(odeStepTime * this.timeScale);
                }
                Stuff.ContactGroup.Clear();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            lock (Stuff.World)
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        CreateOdeBox(new Ode.Net.Vector3(0, i * 3.1f, j * 3.1f + 2));
                    }
                }
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            lock (Stuff.World)
            {
                var arr = Stuff.OdeSpace.ToArray();
                for (int i = 0; i < arr.Count(); i++)
                {
                    Stuff.OdeSpace.Remove(arr[i]);

                }
            }

        }

        private void throwBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bx = CreateOdeBox(new Ode.Net.Vector3(0, 0, 15), 1.5f);
            var pos = drawer.PointToClient(contextMenuStrip1.Bounds.Location);
            var mr = new MouseRay(pos.X, pos.Y, drawer.Camera);
            var d = mr.Dir;
            d.Normalize();
            bx.Position = new Ode.Net.Vector3(mr.Start.X, mr.Start.Y, mr.Start.Z);
            d *= 150;

            bx.Body.LinearVelocity = new Ode.Net.Vector3(d.X, d.Y, d.Z);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            ctx.Wireframe = checkBox2.Checked;
        }
        public static List<IkChain> IkChains = new List<IkChain>();
        public bool AllowIkUpdate = false;

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (AllowIkUpdate)
            {
                foreach (var chain in IkChains)
                {
                    chain.Update();
                }
            }
            drawer.Draw();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            drawer.EnableDrawGrid = checkBox5.Checked;
        }

        private void Button6_Click(object sender, EventArgs e)
        {
            timeScale *= 10;
        }

        private void Button7_Click(object sender, EventArgs e)
        {
            timeScale /= 10;
        }

        private void Button8_Click(object sender, EventArgs e)
        {
            var bd = ball;
            Ode.Net.Vector3 v3 = new Ode.Net.Vector3(100, 0, 100);
            bd.Body.SetLinearVelocity(ref v3);

        }

        private void Button9_Click(object sender, EventArgs e)
        {
            CreateOdeBox(new Ode.Net.Vector3(0, 0, 155), 1, new Ode.Net.Vector3(20, 1525, 150));
        }

        private void Button10_Click(object sender, EventArgs e)
        {

            List<OpenTK.Vector3> pnts = new List<OpenTK.Vector3>();


            foreach (var vol in Stuff.models)
            {



                foreach (var item in vol.faces)
                {
                    for (int i = 0; i < item.Vertexes.Count(); i++)
                    {
                        var pos = item.Vertexes[i].Position;
                        pnts.Add(pos);
                    }
                }
            }

            //   FitToPoints(pnts.ToArray(), drawer.Camera);
        }
        public static OpenTK.Vector3? InstersectPlaneWithRay(Plane plane, MouseRay ray, bool checkT = true)
        {


            OpenTK.Vector3 l = ray.End - ray.Start;
            l.Normalize();

            //check point exists 
            var n = plane.Normal;
            var d = l.X * n.X + l.Y * n.Y + n.Z * l.Z;
            var r0n = ray.Start.X * n.X + ray.Start.Y * n.Y + ray.Start.Z * n.Z;
            if (Math.Abs(d) > 1e-4)
            {
                var t0 = -((r0n) + plane.W) / d;
                if (checkT)
                {
                    if (t0 >= 0)
                    {
                        return ray.Start + l * (float)t0;
                    }
                }
                else
                {
                    return ray.Start + l * (float)t0;
                }
            }
            return null;
        }

        private void Button11_Click(object sender, EventArgs e)
        {
            drawer.Camera.CamFrom = new tkVector3(-2000, 700, 1000);
            drawer.Camera.CamTo = new tkVector3(0, 0, 300);
        }
        bool cameraToRobot = false;
        private void Button12_Click(object sender, EventArgs e)
        {
            cameraToRobot = !cameraToRobot;
        }
        public IkChain CreateIkChain(EntityContainer contn, IkBonePool pool)
        {
            var ret = new IkChain() { Container = contn, Enable = true };
            contn.Chains.Add(ret);
            IkChains.Add(ret);


            var chain = IkChains.Last();

            var bn = pool.Bones.First() as IkLineBone;
            {
                chain.EndEffector = bn;
            }

            var prnts = bn.Parent.GetAllParents().ToList();
            var jj =
                bn.Parent.Parent.Joints.OfType<IkRevoluteJoint>()
                    .Where(z => prnts.Contains(z.ConnectionA) && prnts.Contains(z.ConnectionB))
                    .ToArray();
            prnts.Add(bn.Parent);
            jj =
                bn.Parent.Parent.Joints.OfType<IkRevoluteJoint>()
                    .Where(z => prnts.Contains(z.ConnectionA) && prnts.Contains(z.ConnectionB))
                    .ToArray();

            chain.Joints = jj;
            return ret;

        }
        private void button13_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Sandbox robot model (*.srm)|*.srm";
            if (ofd.ShowDialog() != DialogResult.OK) return;

            var robo = Robot.CreateWholeFromXml(ofd.FileName);
            //meraLib.Stuff.IkChains.Add(new IkChain() {});
            //fdr.Skeleton.Bones.First()
            robo.Skeleton.UpdateHierarchy();
            var ch1 = CreateIkChain(robo, robo.Skeleton.Bones.First(z => z.Name == "Right-Wrist").Parent);
            var ch2 = CreateIkChain(robo, robo.Skeleton.Bones.First(z => z.Name == "Left-Wrist").Parent);
            ch1.Settings.Type = IkTypeEnum.Position3D;
            ch2.Settings.Type = IkTypeEnum.Position3D;
            ch1.Settings.Delta = 0.1f;
            ch2.Settings.Delta = 0.1f;




            var ch3 = CreateIkChain(robo, robo.Skeleton.Bones.First(z => z.Name == "Right-AnkleS").Parent);
            var ch4 = CreateIkChain(robo, robo.Skeleton.Bones.First(z => z.Name == "Left-AnkleS").Parent);
            ch3.Settings.Type = IkTypeEnum.Full6D;
            ch4.Settings.Type = IkTypeEnum.Full6D;
            ch3.Settings.Delta = 0.1f;
            ch4.Settings.Delta = 0.1f;

            //UpdateTree();
            robo.Skeleton.ForwardCalc(OpenTK.Matrix3.Identity);
            foreach (var ikChain in robo.Chains)
            {
                ikChain.Settings.TargetPosition = ikChain.EndEffector.AbsolutePosition;
            }
        }

        //public void FitToPoints(OpenTK.Vector3[] pnts, Camera cam)
        //{
        //    var matr = Matrix4.CreateRotationZ((float)(0 * Math.PI / 180.0f));
        //    List<Vector2> vv = new List<Vector2>();
        //    foreach (var vertex in pnts)
        //    {
        //        var p = MouseRay.Project((new OpenTK.Vector4(vertex, 1)).Xyz, cam.ProjectionMatrix, cam.ViewMatrix, matr, drawer.Camera.viewport);
        //        vv.Add(p.Xy);
        //    }

        //    //prjs->xy coords
        //    var minx = vv.Min(z => z.X);
        //    var maxx = vv.Max(z => z.X);
        //    var miny = vv.Min(z => z.Y);
        //    var maxy = vv.Max(z => z.Y);


        //    var dx = (maxx - minx) * 1.2f;
        //    var dxadd = dx - (maxx - minx);

        //    var dy = (maxy - miny) * 1.2f;
        //    var dyadd = dy - (maxy - miny);

        //    var cx = dx / 2;
        //    var cy = dy / 2;
        //    var dir = cam.CamTo - cam.CamFrom;
        //    //center back to 3d

        //    var mr = new MouseRay(cx + minx - dxadd / 2, cy + miny - dyadd / 2, cam);
        //    var v0 = mr.Start;

        //    /*
        //     * 1. взять две плоскости верхнюю и нижнюю от модели и через них луч пересечь. и взять эти точки я думаю. тогда норм будет с far
        //     */

        //    var bottom = pnts.OrderBy(z => z.Z).First();
        //    var toppest = pnts.OrderBy(z => z.Z).Last();
        //    var pl1 = Plane.FromPoints(bottom, bottom + new tkVector3(10, 0, 0), bottom + new OpenTK.Vector3(0, 10, 0));
        //    var pl2 = Plane.FromPoints(toppest, toppest + new tkVector3(10, 0, 0), toppest + new OpenTK.Vector3(0, 10, 0));


        //    var inter1 = InstersectPlaneWithRay(pl1, mr, false);
        //    var inter2 = InstersectPlaneWithRay(pl2, mr, false);

        //    cam.CamFrom = v0;
        //    cam.CamTo = cam.CamFrom + dir;
        //    cam.CamFrom = inter1.Value;
        //    cam.CamTo = inter2.Value;
        //    //cam.OrthoWidth = 500;
        //    //cam.OrthoWidth = Math.Max(dx, dy);
        //    var aspect = drawer.Width / (float)(drawer.Height);

        //    dx /= drawer.Width;
        //    dx *= drawer.Camera.OrthoWidth;
        //    dy /= drawer.Height;
        //    dy *= drawer.Camera.OrthoWidth;

        //   drawer.Camera.OrthoWidth = Math.Max(dx, dy);

        //}
    }
}
