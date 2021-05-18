using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.layers;
using MyCaffe.param;
using SimpleGraphing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// This MyCaffe sample is a complete rewrite of the Caffe-LSTM-Mini-Tutorial which demonstrates how to use an LSTM model to learn a generated Sin curve.
/// </summary>
/// <remarks>
/// @see [Corvus/Caffe-LSTM-Mini-Tutorial](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial) open-source project distributed under
/// the [GNU License](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial/blob/master/LICENSE).
/// </remarks>
namespace SinCurve
{
    class Program
    {
        /// <summary>
        /// Main program for the sample.
        /// </summary>
        /// <param name="args">See Parameters for available arguments.</param>
        static void Main(string[] args)
        {
            try
            {
                Parameters param = new Parameters(args);
                if (!param.IsHelp)
                {
                    if (!checkMyCaffeVersion())
                        return;

                    Console.WriteLine("Running " + param.ToString());

                    Trainer trainer = new Trainer(param);

                    // Train the model specified within the trainer using the arguments
                    // from the parameters specified.
                    if (param.Mode == Parameters.MODE.TRAIN)
                        trainer.Train(param.NewWeights);

                    // Run the trained model.
                    trainer.Run();

                    // Cleanup.
                    trainer.Dispose();
                }
            }
            catch (Exception err)
            {
                Console.WriteLine("ERROR: " + err.Message);
            }

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static bool checkMyCaffeVersion()
        {
            string strMinVer = "0.11.3.24"; // Requires MyCaffe version 0.11.3.24 or greater.

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
