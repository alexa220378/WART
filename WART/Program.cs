using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WART
{
    static class Program
    {
        public static bool UseUI = true;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            UseUI = !IsConsole();

            if (UseUI)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
            }
            Context c = new Context();
            c.Run();
        }

        static bool IsConsole()
        {
            try
            {
                int foo = Console.WindowHeight;
                return true;
            }
            catch (Exception)
            { }
            return false;
        }
    }
}
