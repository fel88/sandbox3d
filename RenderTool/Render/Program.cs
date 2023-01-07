using System;
using System.Windows.Forms;

namespace RenderTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MainForm = new mdi();
            Application.Run(MainForm);
        }
        
        public static mdi MainForm;
    }
}
