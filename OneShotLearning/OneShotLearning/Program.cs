using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using MyCaffe;
using MyCaffe.common;
using MyCaffe.param;
using MyCaffe.param.beta;
using MyCaffe.basecode;
using MyCaffe.db.image;
using MyCaffe.basecode.descriptors;
using System.ServiceProcess;

/// <summary>
/// OneShot Learning Sample
/// </summary>
/// <remarks>
/// IMPORTANT: This sample has the following required software.
///     a.) Install Microsoft SQL Express (or SQL)
///     b.) Install the MyCaffe Test Application located at https://github.com/MyCaffe/MyCaffe/releases
///     c.) Create the Database - select the 'Database | Create Database' menu.
///     d.) Load MNIST - select the 'Database | Load MNIST' menu.
///  
/// NOTE: MyCaffe is 64-bit, so make sure to uncheck 'Prefer 32-bit' in your projects that use MyCaffe.
/// </remarks>
namespace OneShotLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!sqlCheck())
                return;

            Log log = new Log("test");
            log.OnWriteLine += Log_OnWriteLine;
            CancelEvent cancel = new CancelEvent();
            SettingsCaffe settings = new SettingsCaffe();

            // Load all images into memory before training.
            settings.DbLoadMethod = DB_LOAD_METHOD.LOAD_ALL;
            // Use GPU ID = 0
            settings.GpuIds = "0";

            // Load the descriptors from their respective files
            string strSolver = load_file("C:\\ProgramData\\MyCaffe\\test_data\\models\\siamese\\mnist\\solver.prototxt");
            string strModel = load_file("C:\\ProgramData\\MyCaffe\\test_data\\models\\siamese\\mnist\\train_val.prototxt");

            RawProto proto = RawProto.Parse(strModel);
            NetParameter net_param = NetParameter.FromProto(proto);
            LayerParameter layer = net_param.FindLayer(LayerParameter.LayerType.DECODE);
            layer.decode_param.target = DecodeParameter.TARGET.CENTROID;
            proto = net_param.ToProto("root");
            strModel = proto.ToString();

            // Load the MNIST data descriptor.
            DatasetFactory factory = new DatasetFactory();
            DatasetDescriptor ds = factory.LoadDataset("MNIST");

            // Create a test project with the dataset and descriptors
            ProjectEx project = new ProjectEx("Test");
            project.SetDataset(ds);
            project.ModelDescription = strModel;
            project.SolverDescription = strSolver;

            // Create the MyCaffeControl (with the 'float' base type)
            string strCudaPath = "C:\\Program Files\\SignalPop\\MyCaffe\\cuda_11.8\\CudaDnnDll.11.8.dll";
            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, log, cancel, null, null, null, null, strCudaPath);

            // Load the project, using the TRAIN phase.
            mycaffe.Load(Phase.TRAIN, project);

            // Train the model for 4000 iterations
            // (which uses the internal solver and internal training net)
            int nIterations = 4000;
            mycaffe.Train(nIterations);

            // Test the model for 100 iterations
            // (which uses the internal testing net)
            nIterations = 100;
            double dfAccuracy = mycaffe.Test(nIterations);

            // Report the testing accuracy.
            log.WriteLine("Accuracy = " + dfAccuracy.ToString("P"));

            mycaffe.Dispose();

            Console.Write("Press any key...");
            Console.ReadKey();
        }

        private static bool sqlCheck()
        {
            List<string> rgSqlInst = DatabaseInstanceQuery.GetInstances();

            if (rgSqlInst == null || rgSqlInst.Count == 0)
            {
                string strMsg = "The ImageClassification sample requires Microsoft SQL or Microsoft SQL Express.  You must download and install 'Microsoft SQL' or 'Microsoft SQL Express' first!" + Environment.NewLine;
                strMsg += "see 'https://www.microsoft.com/en-us/sql-server/sql-server-editions-express'";
                Console.WriteLine("ERROR: " + strMsg);
                return false;
            }

            string strService = rgSqlInst[0].TrimStart('.', '\\');
            if (strService == "SQLEXPRESS")
                strService = "MSSQL$" + strService;

            ServiceController sc = new ServiceController(strService);
            if (sc.Status != ServiceControllerStatus.Running)
            {
                string strMsg = "Microsoft SQL instance '" + rgSqlInst[0] + "' found, but it is not running.  You must start the SQL service to continue with this sample.";
                Console.WriteLine("ERROR: " + strMsg);
                return false;
            }

            return true;
        }

        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Console.WriteLine(e.Message);
        }

        private static string load_file(string strFile)
        {
            using (StreamReader sr = new StreamReader(strFile))
            {
                return sr.ReadToEnd();
            }
        }
    }
}
