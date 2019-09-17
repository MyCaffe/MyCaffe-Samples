﻿using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.param;
using MyCaffe.solvers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageClassificationBareBones
{
    public partial class MainForm : Form
    {
        CancelEvent m_evtCancel = new CancelEvent();
        Log m_log = new Log("Test");
        string m_strImageDirTraining;
        string m_strImageDirTesting;
        string[] m_rgstrTrainingFiles;
        string[] m_rgstrTestingFiles;
        CryptoRandom m_random;


        public MainForm()
        {
            InitializeComponent();

            m_strImageDirTraining = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\mnist\\training";
            m_strImageDirTesting = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\mnist\\testing";
            m_log.EnableTrace = true; // Write to debug output window.
            m_random = new CryptoRandom();
        }

        /// <summary>
        /// Load the descriptor files for the solver and model.
        /// </summary>
        /// <param name="strSolver">Specifies the solver descriptor used to build the solver.</param>
        /// <param name="strModel">Specifies the model descriptor used to build the model.</param>
        private void load_descriptors(string strName, out string strSolver, out string strModel)
        {
            string strDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\models\\" + strName + "\\";

            using (StreamReader sr = new StreamReader(strDir + "lenet_solver.prototxt"))
            {
                strSolver = sr.ReadToEnd();
            }

            using (StreamReader sr = new StreamReader(strDir + "lenet_train_test.prototxt"))
            {
                strModel = sr.ReadToEnd();
            }
        }

        /// <summary>
        /// Replace the Data input layer with the MemoryData input layer.
        /// </summary>
        /// <param name="strModel">Specifies the model descriptor to change.</param>
        /// <param name="nBatchSize">Specifies the batch size.</param>
        /// <returns>The new model descriptor with the MemoryData layer is returned.</returns>
        private string fixup_model(string strModel, int nBatchSize)
        {
            RawProto proto = RawProto.Parse(strModel);
            NetParameter net_param = NetParameter.FromProto(proto);

            for (int i = 0; i < net_param.layer.Count; i++)
            {
                if (net_param.layer[i].type == LayerParameter.LayerType.DATA)
                {
                    LayerParameter layer = new LayerParameter(LayerParameter.LayerType.INPUT);
                    layer.name = net_param.layer[i].name;
                    layer.top = net_param.layer[i].top;
                    layer.bottom = net_param.layer[i].bottom;
                    layer.include = net_param.layer[i].include;

                    layer.input_param.shape.Add(new BlobShape(nBatchSize, 1, 28, 28));
                    layer.input_param.shape.Add(new BlobShape(nBatchSize, 1, 1, 1));
                    net_param.layer[i] = layer;
                }
            }

            return net_param.ToProto("root").ToString();
        }

        /// <summary>
        /// Set the solver testing interval.
        /// </summary>
        /// <param name="strSolver">Specifies the solver parameter.</param>
        /// <returns>The solver description is returned.</returns>
        private string fixup_solver(string strSolver, int nInterval)
        {
            RawProto proto = RawProto.Parse(strSolver);
            SolverParameter solver_param = SolverParameter.FromProto(proto);

            // Set the testining interval during training.
            solver_param.test_interval = nInterval;
            solver_param.test_initialization = false;

            return solver_param.ToProto("root").ToString();
        }

        /// <summary>
        /// Retrieve the label name from an image file name.
        /// </summary>
        /// <param name="strFile">Specifies the file name.</param>
        /// <returns>The label value is returned.</returns>
        private int getLabelFromFileName(string strFile)
        {
            int nPos = strFile.IndexOf('-');
            if (nPos < 0)
                throw new Exception("Could not find the label.");

            strFile = strFile.Substring(nPos + 1);
            nPos = strFile.IndexOf('.');
            if (nPos < 0)
                throw new Exception("Could not find the '.'");

            string strLabel = strFile.Substring(0, nPos);

            return int.Parse(strLabel);
        }

        /// <summary>
        /// Load a batch of data by randomly selecting from the files and loading the data into the data and label blobs.
        /// </summary>
        /// <param name="rgstrFiles">Specifies the image files to select from.</param>
        /// <param name="nBatchSize">Specifies the batch size.</param>
        /// <param name="dataBlob">Specifies the data blob to load with image data.</param>
        /// <param name="labelBlob">Specifies the label blob to load with label data.</param>
        private void loadData(string[] rgstrFiles, int nBatchSize, Blob<float> dataBlob, Blob<float> labelBlob)
        {
            List<float> rgData = new List<float>();
            List<float> rgLabel = new List<float>();
            float fScale = 0.00390625f;

            // Load a batch of data.
            for (int j = 0; j < nBatchSize; j++)
            {
                int nIdx = m_random.Next(rgstrFiles.Length);
                string strFile = rgstrFiles[nIdx];
                Bitmap bmp = new Bitmap(strFile);
                int nLabel = getLabelFromFileName(strFile);

                Datum d = ImageData.GetImageData(bmp, 1, false, nLabel);
                rgData.AddRange(d.ByteData.Select(p => (float)p * fScale));
                rgLabel.Add(nLabel);
                bmp.Dispose();
            }

            // Load the data and label blobs (of the INPUT layer)
            dataBlob.mutable_cpu_data = rgData.ToArray();
            labelBlob.mutable_cpu_data = rgLabel.ToArray();
        }


        //-----------------------------------------------------------------------------------------
        //  Simple Classification (using direct net surgery)
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// The SimpleClassification sample is designed to show how to manually train the MNIST dataset using raw image data stored 
        /// in the \ProgramData\MyCaffe\test_data\images\mnist\training directory (previously loaded with the 'Export Images'
        /// sample above).
        /// </summary>
        /// <remarks>
        /// This sample requires that you have already loaded the MNIST dataset into SQL (or SQLEXPRESS) using the MyCaffe
        /// Test Application by selecting its 'Database | Load MNIST...' menu item.
        /// </remarks>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event argument.</param>
        private void btnSimpleClassification_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            int nBatchSize = 32;
            SettingsCaffe settings = new SettingsCaffe();
            settings.GpuIds = "0";

            m_rgstrTrainingFiles = Directory.GetFiles(m_strImageDirTraining);
            m_rgstrTestingFiles = Directory.GetFiles(m_strImageDirTesting);

            string strSolver;
            string strModel;

            load_descriptors("mnist", out strSolver, out strModel); // Load the descriptors from their respective files (installed by MyCaffe Test Application install)
            strModel = fixup_model(strModel, nBatchSize);

            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

            mycaffe.LoadLite(Phase.TRAIN,   // using the training phase. 
                         strSolver,     // solver descriptor, that specifies to use the SGD solver.
                         strModel,      // simple LENET model descriptor.
                         null);         // no weights are loaded.

            // Perform your own training
            Solver<float> solver = mycaffe.GetInternalSolver();
            Net<float> net = mycaffe.GetInternalNet(Phase.TRAIN);
            Blob<float> dataBlob = net.blob_by_name("data");
            Blob<float> labelBlob = net.blob_by_name("label");

            sw.Start();

            int nIterations = 5000;
            for (int i = 0; i < nIterations; i++)
            {
                // Load the data into the data and label blobs.
                loadData(m_rgstrTrainingFiles, nBatchSize, dataBlob, labelBlob);

                // Run the forward and backward passes.
                double dfLoss;
                net.Forward(out dfLoss);
                net.ClearParamDiffs();
                net.Backward();

                // Apply the gradients calculated during Backward.
                solver.ApplyUpdate(i);

                // Output the loss.
                if (sw.Elapsed.TotalMilliseconds > 1000)
                {
                    m_log.Progress = (double)i / (double)nIterations;
                    m_log.WriteLine("Loss = " + dfLoss.ToString());
                    sw.Restart();
                }
            }

            // Run testing using the MyCaffe control (who's internal Run net is already updated 
            // for it shares its weight memory with the training net.
            net = mycaffe.GetInternalNet(Phase.TEST);
            dataBlob = net.blob_by_name("data");
            labelBlob = net.blob_by_name("label");

            float fTotalAccuracy = 0;

            nIterations = 100;
            for (int i = 0; i < nIterations; i++)
            {
                // Load the data into the data and label blobs.
                loadData(m_rgstrTestingFiles, nBatchSize, dataBlob, labelBlob);

                // Run the forward pass.
                double dfLoss;
                BlobCollection<float> res = net.Forward(out dfLoss);
                fTotalAccuracy += res[0].GetData(0);

                // Output the training progress.
                if (sw.Elapsed.TotalMilliseconds > 1000)
                {
                    m_log.Progress = (double)i / (double)nIterations;
                    m_log.WriteLine("training...");
                    sw.Restart();
                }
            }

            double dfAccuracy = (double)fTotalAccuracy / (double)nIterations;
            m_log.WriteLine("Accuracy = " + dfAccuracy.ToString("P"));

            MessageBox.Show("Average Accuracy = " + dfAccuracy.ToString("P"), "Traing/Test on MNIST Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        //-----------------------------------------------------------------------------------------
        //  Simpler Classification (using solver)
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// The SimplerClassification shows how to use the solver directly and load data via its OnStart (for training) and
        /// OnTestStart (for testing) events.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void btnSimplerClassification_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            int nBatchSize = 32;
            SettingsCaffe settings = new SettingsCaffe();
            settings.GpuIds = "0";

            m_rgstrTrainingFiles = Directory.GetFiles(m_strImageDirTraining);
            m_rgstrTestingFiles = Directory.GetFiles(m_strImageDirTesting);

            string strSolver;
            string strModel;

            load_descriptors("mnist", out strSolver, out strModel); // Load the descriptors from their respective files (installed by MyCaffe Test Application install)
            strModel = fixup_model(strModel, nBatchSize);
            strSolver = fixup_solver(strSolver, 10000); // set the interval beyond the iterations to skip testing during solving.

            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);
            mycaffe.LoadLite(Phase.TRAIN,   // using the training phase. 
                         strSolver,     // solver descriptor, that specifies to use the SGD solver.
                         strModel,      // simple LENET model descriptor.
                         null);         // no weights are loaded.

            // Perform your own training
            Solver<float> solver = mycaffe.GetInternalSolver();
            solver.OnStart += Solver_OnStart;
            solver.OnTestStart += Solver_OnTestStart;

            // Run the solver to train the net.
            int nIterations = 5000;
            solver.Solve(nIterations);

            // Run the solver to test the net (using its internal test net)
            nIterations = 100;
            double dfAccuracy = solver.Test(nIterations);

            m_log.WriteLine("Accuracy = " + dfAccuracy.ToString("P"));

            MessageBox.Show("Average Accuracy = " + dfAccuracy.ToString("P"), "Traing/Test on MNIST Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Event called at the start of each testing pass.
        /// </summary>
        /// <param name="sender">Specifies the solver firing the event.</param>
        /// <param name="e">Specifies the event args.</param>
        private void Solver_OnTestStart(object sender, EventArgs e)
        {
            Solver<float> solver = sender as Solver<float>;

            Net<float> net = solver.TrainingNet;
            Blob<float> dataBlob = net.blob_by_name("data");
            Blob<float> labelBlob = net.blob_by_name("label");

            loadData(m_rgstrTrainingFiles, 32, dataBlob, labelBlob);
        }

        /// <summary>
        /// Event called at the start of each training pass.
        /// </summary>
        /// <param name="sender">Specifies the solver firing the event.</param>
        /// <param name="e">Specifies the event args.</param>
        private void Solver_OnStart(object sender, EventArgs e)
        {
            Solver<float> solver = sender as Solver<float>;

            Net<float> net = solver.TestingNet;
            Blob<float> dataBlob = net.blob_by_name("data");
            Blob<float> labelBlob = net.blob_by_name("label");

            loadData(m_rgstrTestingFiles, 32, dataBlob, labelBlob);
        }
    }
}
