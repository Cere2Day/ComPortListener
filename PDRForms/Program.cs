using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace PDRForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()                                              
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
            
        }
    }
}
