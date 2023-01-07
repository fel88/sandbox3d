using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RenderTool
{
    public partial class vec3EditorDialog : Form
    {
        public vec3EditorDialog()
        {
            InitializeComponent();
        }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                X = float.Parse(textBox1.Text);
            }
            catch
            {

            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Y = float.Parse(textBox2.Text);
            }
            catch
            {

            }
        }

        private void vec3EditorDialog_Load(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            try
            {
                Z = float.Parse(textBox3.Text);
            }
            catch
            {

            }
        }

        internal void Init(Vector3 val)
        {
            X = val.X;
            Y = val.Y;
            Z = val.Z;
            textBox1.Text = X.ToString();
            textBox2.Text = Y.ToString();
            textBox3.Text = Z.ToString();
        }
    }
}
