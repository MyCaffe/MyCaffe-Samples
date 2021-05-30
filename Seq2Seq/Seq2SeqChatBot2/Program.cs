using MyCaffe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Seq2SeqChatBot
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!checkMyCaffeVersion())
                return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }

        static bool checkMyCaffeVersion()
        {
            string strMinVer = "0.11.3.32"; // Requires MyCaffe version 0.11.3.32 or greater.

            try
            {
                string strVersion = MyCaffeControl<float>.Version.FileVersion;

                if (string.Compare(strVersion, strMinVer) < 0)
                    throw new Exception("Incompatible version!");

                return true;
            }
            catch (Exception)
            {
                Console.Write("You need to install a later version of MyCaffe. Minimum version = " + strMinVer);
                return false;
            }
        }
    }
}
