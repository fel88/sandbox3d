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
        public DrawingContext ctx = new DrawingContext();
        public void DrawObjects()
        {
            if (!checkBox4.Checked) return;
            Stopwatch sw = Stopwatch.StartNew();

            lock (Stuff.World)
            {
                foreach (var entity in Stuff.OdeSpace)
                {

                    if ((entity is Ode.Net.Collision.Box))
                    {
                        DrawOdeBox(entity as Box);
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

        public static Geom CreateOdeSphere()
        {
            Ode.Net.Collision.Sphere b = new Ode.Net.Collision.Sphere(1);
            var a = Mass.CreateSphere(1, 1);
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
                contact.Surface.Mode = (ContactModes.Bounce  | ContactModes.SoftCfm | ContactModes.SoftErp);
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
            Ode.Net.Collision.Box b = new Ode.Net.Collision.Box(500, 500, 1);
            var mass = Mass.CreateBoxTotal(1, 500, 500, 1);
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
            return b;
        }
        private void button2_Click(object sender, EventArgs e)
        {
            CreateOdeBox(new Ode.Net.Vector3(0, 0, 15));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var b = CreateOdeSphere();
            var pos = new Ode.Net.Vector3(0, 0, 15);
            b.SetPosition(ref pos);
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            drawer.Draw();
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            drawer.EnableDrawGrid = checkBox5.Checked;
        }
    }
}
