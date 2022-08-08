using MyCaffe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageToSin
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
            string strMinVer = "1.11.7.7"; // Requires MyCaffe version 1.11.7.7 or greater.

            try
            {
                string strVersion = MyCaffeControl<float>.Version.FileVersion;

                if (string.Compare(strVersion, strMinVer) < 0)
                    throw new Exception("Incompatible version!");

                return true;
            }
            catch (Exception)
            {
                MessageBox.Show("MyCaffe Sample: You need to install a later version of MyCaffe. Minimum version = " + strMinVer, "MyCaffe Sample Version Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }
        }
    }
}
