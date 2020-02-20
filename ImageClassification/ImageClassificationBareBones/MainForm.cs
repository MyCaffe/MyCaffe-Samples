using MyCaffe;
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

                Datum d = ImageData.GetImageDataD(bmp, 1, false, nLabel);
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

            if (!Directory.Exists(m_strImageDirTraining) || !Directory.Exists(m_strImageDirTesting))
            {
                string strMsg = "You must first expand the MNIST dataset into the following directories:" + Environment.NewLine;
                strMsg += "Training Images: '" + m_strImageDirTraining + "'" + Environment.NewLine;
                strMsg += "Testing Images: '" + m_strImageDirTesting + "'" + Environment.NewLine + Environment.NewLine;

                strMsg += "If you have Microsoft SQL or SQL Express installed, selecting the 'Export' button from the 'ImageClassification' project will export these images for you." + Environment.NewLine + Environment.NewLine;

                strMsg += "If you DO NOT have Microsoft SQL or SQL Express, running the MyCaffe Test Application and selecting the 'Database | Load MNIST...' menu item with the 'Export to file only' check box checked, will export the images for you without SQL." + Environment.NewLine + Environment.NewLine;

                strMsg += "To get the MNIST *.gz data files, please see http://yann.lecun.com/exdb/mnist/";

                MessageBox.Show(strMsg, "Images Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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

            // Save the trained weights for use later.
            saveWeights(mycaffe, "my_weights");

#if VER_10_2_160
            Bitmap bmp = new Bitmap(m_rgstrTestingFiles[0]);
            ResultCollection results = mycaffe.Run(bmp);

            MyCaffeControl<float> mycaffe2 = mycaffe.Clone(0);
            ResultCollection results2 = mycaffe2.Run(bmp);
#endif

            // Release resources used.
            mycaffe.Dispose();

#if VER_10_2_160
            mycaffe2.Dispose();
#endif
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

            if (!Directory.Exists(m_strImageDirTraining) || !Directory.Exists(m_strImageDirTesting))
            {
                MessageBox.Show("You must first 'export' the images by running the 'ImageClassification' sample and pressing the 'Export Images' button.", "Images Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            m_rgstrTrainingFiles = Directory.GetFiles(m_strImageDirTraining);
            m_rgstrTestingFiles = Directory.GetFiles(m_strImageDirTesting);

            string strSolver;
            string strModel;

            // Load the descriptors from their respective files 
            // (installed by MyCaffe Test Application install)
            load_descriptors("mnist", out strSolver, out strModel); 
            strModel = fixup_model(strModel, nBatchSize);
            // set the interval beyond the iterations to skip testing during solving.
            strSolver = fixup_solver(strSolver, 10000); 

            // Create the MyCaffeControl.
            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

            // Load the solver and model descriptors without the Image Database.
            mycaffe.LoadLite(Phase.TRAIN,   // using the training phase. 
                         strSolver,         // solver descriptor, that specifies to use the SGD solver.
                         strModel,          // simple LENET model descriptor.
                         null);             // no weights are loaded.

            // Load your own data via the OnStart and OnTestStart
            // events each called at the start of the Training
            // and Testing iterations respectively.
            Solver<float> solver = mycaffe.GetInternalSolver();
            solver.OnStart += Solver_OnStart;
            solver.OnTestStart += Solver_OnTestStart;

            // Run the solver to train the net.
            int nIterations = 5000;
            mycaffe.Train(nIterations);

            // Run the solver to test the net (using its internal test net)
            nIterations = 100;
            double dfAccuracy = mycaffe.Test(nIterations);

            // Report the testing accuracy.
            m_log.WriteLine("Accuracy = " + dfAccuracy.ToString("P"));

            MessageBox.Show("Average Accuracy = " + dfAccuracy.ToString("P"), "Traing/Test on MNIST Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Save the trained weights for use later.
            saveWeights(mycaffe, "my_weights");

#if VER_10_2_160
            Bitmap bmp = new Bitmap(m_rgstrTestingFiles[0]);
            ResultCollection results = mycaffe.Run(bmp);
#endif

            // Release any resources used.
            mycaffe.Dispose();
        }


        //-----------------------------------------------------------------------------------------
        //  Simpler Classification with Programmable Models (using solver)
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// The SimplerClassification with Programmable Models shows how programmatically build a model (instead of using
        /// a text descriptor) which is then used with the solver which loads data via its OnStart (for training) and
        /// OnTestStart (for testing) events.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void btnSimplerClassificationWithProgrammableModels_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            int nBatchSize = 32;
            SettingsCaffe settings = new SettingsCaffe();
            settings.GpuIds = "0";

            if (!Directory.Exists(m_strImageDirTraining) || !Directory.Exists(m_strImageDirTesting))
            {
                MessageBox.Show("You must first 'export' the images by running the 'ImageClassification' sample and pressing the 'Export Images' button.", "Images Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            m_rgstrTrainingFiles = Directory.GetFiles(m_strImageDirTraining);
            m_rgstrTestingFiles = Directory.GetFiles(m_strImageDirTesting);

            string strSolver;
            string strModel;

            // Create the model programmatically.
            strModel = create_model_descriptor_programmatically("mnist", nBatchSize);

            // Create the solver programmatically.
            strSolver = create_solver_descriptor_programmatically(10000);

            // Create the MyCaffeControl.
            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

            // Load the solver and model descriptors without the Image Database.
            mycaffe.LoadLite(Phase.TRAIN,   // using the training phase. 
                         strSolver,         // solver descriptor, that specifies to use the SGD solver.
                         strModel,          // simple LENET model descriptor.
                         null);             // no weights are loaded.

            // Load your own data via the OnStart and OnTestStart
            // events each called at the start of the Training
            // and Testing iterations respectively.
            Solver<float> solver = mycaffe.GetInternalSolver();
            solver.OnStart += Solver_OnStart;
            solver.OnTestStart += Solver_OnTestStart;

            // Run the solver to train the net.
            int nIterations = 5000;
            mycaffe.Train(nIterations);

            // Run the solver to test the net (using its internal test net)
            nIterations = 100;
            double dfAccuracy = mycaffe.Test(nIterations);

            // Report the testing accuracy.
            m_log.WriteLine("Accuracy = " + dfAccuracy.ToString("P"));

            MessageBox.Show("Average Accuracy = " + dfAccuracy.ToString("P"), "Traing/Test on MNIST Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Save the trained weights for use later.
            saveWeights(mycaffe, "my_weights");

#if VER_10_2_160
            Bitmap bmp = new Bitmap(m_rgstrTestingFiles[0]);
            ResultCollection results = mycaffe.Run(bmp);
#endif

            // Release any resources used.
            mycaffe.Dispose();
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

        //-----------------------------------------------------------------------------------------
        //  Create descriptors programically
        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Create the LeNet_train_test prototxt programmatically.
        /// </summary>
        /// <param name="strDataName">Specifies the dataset name.</param>
        /// <param name="nBatchSize">Specifies the batch size.</param>
        /// <returns>The model descriptor is returned as text.</returns>
        private string create_model_descriptor_programmatically(string strDataName, int nBatchSize)
        {
            NetParameter net_param = new NetParameter();
            net_param.name = "LeNet";

            LayerParameter input_param_train = new LayerParameter(LayerParameter.LayerType.INPUT);
            input_param_train.name = strDataName;
            input_param_train.top.Add("data");
            input_param_train.top.Add("label");
            input_param_train.include.Add(new NetStateRule(Phase.TRAIN));
            input_param_train.transform_param = new TransformationParameter();
            input_param_train.transform_param.scale = 1.0 / 256.0;
            input_param_train.input_param.shape = new List<BlobShape>()
                { new BlobShape(nBatchSize, 1, 28, 28), // data (the images)
                  new BlobShape(nBatchSize, 1, 1, 1) }; // label
            net_param.layer.Add(input_param_train);

            LayerParameter input_param_test = new LayerParameter(LayerParameter.LayerType.INPUT);
            input_param_test.name = strDataName;
            input_param_test.top.Add("data");
            input_param_test.top.Add("label");
            input_param_test.include.Add(new NetStateRule(Phase.TEST));
            input_param_test.transform_param = new TransformationParameter();
            input_param_test.transform_param.scale = 1.0 / 256.0;
            input_param_train.input_param.shape = new List<BlobShape>()
                { new BlobShape(nBatchSize, 1, 28, 28), // data (the images)
                  new BlobShape(nBatchSize, 1, 1, 1) }; // label
            net_param.layer.Add(input_param_test);

            LayerParameter conv1 = new LayerParameter(LayerParameter.LayerType.CONVOLUTION);
            conv1.name = "conv1";
            conv1.bottom.Add("data");
            conv1.top.Add("conv1");
            conv1.parameters.Add(new ParamSpec(1, 2));
            conv1.convolution_param.num_output = 20;
            conv1.convolution_param.kernel_size.Add(5);
            conv1.convolution_param.stride.Add(1);
            conv1.convolution_param.weight_filler = new FillerParameter("xavier");
            conv1.convolution_param.bias_filler = new FillerParameter("constant");
            net_param.layer.Add(conv1);

            LayerParameter pool1 = new LayerParameter(LayerParameter.LayerType.POOLING);
            pool1.name = "pool1";
            pool1.bottom.Add("conv1");
            pool1.top.Add("pool1");
            pool1.pooling_param.pool = PoolingParameter.PoolingMethod.MAX;
            pool1.pooling_param.kernel_size.Add(2);
            pool1.pooling_param.stride.Add(2);
            net_param.layer.Add(pool1);

            LayerParameter conv2 = new LayerParameter(LayerParameter.LayerType.CONVOLUTION);
            conv2.name = "conv2";
            conv2.bottom.Add("pool1");
            conv2.top.Add("conv2");
            conv2.parameters.Add(new ParamSpec(1, 2));
            conv2.convolution_param.num_output = 50;
            conv2.convolution_param.kernel_size.Add(5);
            conv2.convolution_param.stride.Add(1);
            conv2.convolution_param.weight_filler = new FillerParameter("xavier");
            conv2.convolution_param.bias_filler = new FillerParameter("constant");
            net_param.layer.Add(conv2);

            LayerParameter pool2 = new LayerParameter(LayerParameter.LayerType.POOLING);
            pool2.name = "pool2";
            pool2.bottom.Add("conv2");
            pool2.top.Add("pool2");
            pool2.pooling_param.pool = PoolingParameter.PoolingMethod.MAX;
            pool2.pooling_param.kernel_size.Add(2);
            pool2.pooling_param.stride.Add(2);
            net_param.layer.Add(pool2);

            LayerParameter ip1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1.name = "ip1";
            ip1.bottom.Add("pool2");
            ip1.top.Add("ip1");
            ip1.parameters.Add(new ParamSpec(1, 2));
            ip1.inner_product_param.num_output = 500;
            ip1.inner_product_param.weight_filler = new FillerParameter("xavier");
            ip1.inner_product_param.bias_filler = new FillerParameter("constant");
            net_param.layer.Add(ip1);

            LayerParameter relu1 = new LayerParameter(LayerParameter.LayerType.RELU);
            relu1.name = "relu1";
            relu1.bottom.Add("ip1");
            relu1.top.Add("ip1"); // inline.
            net_param.layer.Add(relu1);

            LayerParameter ip2 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip2.name = "ip2";
            ip2.bottom.Add("ip1");
            ip2.top.Add("ip2");
            ip2.parameters.Add(new ParamSpec(1, 2));
            ip2.inner_product_param.num_output = 10;
            ip2.inner_product_param.weight_filler = new FillerParameter("xavier");
            ip2.inner_product_param.bias_filler = new FillerParameter("constant");
            net_param.layer.Add(ip2);

            LayerParameter accuracy = new LayerParameter(LayerParameter.LayerType.ACCURACY);
            accuracy.name = "accuracy";
            accuracy.bottom.Add("ip2");
            accuracy.bottom.Add("label");
            accuracy.top.Add("accuracy");
            accuracy.include.Add(new NetStateRule(Phase.TEST));
            net_param.layer.Add(accuracy);

            LayerParameter loss = new LayerParameter(LayerParameter.LayerType.SOFTMAXWITH_LOSS);
            loss.name = "loss";
            loss.bottom.Add("ip2");
            loss.bottom.Add("label");
            loss.top.Add("loss");
            net_param.layer.Add(loss);

            // Convert model to text descriptor.
            RawProto proto = net_param.ToProto("root");
            return proto.ToString();
        }

        /// <summary>
        /// Create the LeNet solver prototxt programmatically.
        /// </summary>
        /// <param name="nIterations">Specifies the number of iterations to train.</param>
        /// <returns>The solver descriptor is returned as text.</returns>
        private string create_solver_descriptor_programmatically(int nIterations)
        {
            SolverParameter solver_param = new SolverParameter();
            solver_param.max_iter = nIterations;
            solver_param.test_iter = new List<int>();
            solver_param.test_iter.Add(100);
            solver_param.test_initialization = false;
            solver_param.test_interval = 500;
            solver_param.base_lr = 0.01;
            solver_param.momentum = 0.9;
            solver_param.weight_decay = 0.0005;
            solver_param.LearningRatePolicy = SolverParameter.LearningRatePolicyType.INV;
            solver_param.gamma = 0.0001;
            solver_param.power = 0.75;
            solver_param.display = 100;
            solver_param.snapshot = 5000;

            // Convert solver to text descriptor.
            RawProto proto = solver_param.ToProto("root");
            return proto.ToString();
        }


        //-----------------------------------------------------------------------------------------
        //  Using trained weights.
        //-----------------------------------------------------------------------------------------

        private void btnTestTrainedWeights_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            byte[] rgWeights = loadWeights("my_weights");
            if (rgWeights == null)
            {
                MessageBox.Show("You must first run the 'Simple Classification' or 'Simpler Classification' first to save the weights.", "No Weights Found", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            if (!Directory.Exists(m_strImageDirTraining) || !Directory.Exists(m_strImageDirTesting))
            {
                MessageBox.Show("You must first 'export' the images by running the 'ImageClassification' sample and pressing the 'Export Images' button.", "Images Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (m_rgstrTestingFiles == null || m_rgstrTestingFiles.Length == 0)
                m_rgstrTestingFiles = Directory.GetFiles(m_strImageDirTesting);

            SettingsCaffe settings = new SettingsCaffe();
            settings.GpuIds = "0";

            string strSolver; // not used
            string strModel;

            // Load the descriptors from their respective files 
            // (installed by MyCaffe Test Application install)
            load_descriptors("mnist", out strSolver, out strModel);
            strModel = fixup_model(strModel, 1);

            // Create the MyCaffeControl.
            MyCaffeControl<float> mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);
            mycaffe.LoadToRun(strModel, rgWeights, new BlobShape(1, 1, 28, 28));

            int nCorrectCount = 0;
            int nTotalCount = 0;

            sw.Start();

            for (int i=0; i<m_rgstrTestingFiles.Length; i++)
            {
                string strFile = m_rgstrTestingFiles[i];
                string strExt = Path.GetExtension(strFile).ToLower();

                if (strExt == ".png")
                {
                    // Get the label from the image file name.
                    int nLabel = getLabelFromFileName(strFile);

                    // Load the image file into a datum.
                    Bitmap bmp = new Bitmap(strFile);
                    Datum d = ImageData.GetImageDataF(bmp, 1, false, nLabel);

                    // Run the trained model on the datum.
                    // Note, could also use mycaffe.Run(bmp) for same result.
                    ResultCollection result = mycaffe.Run(d);
                    if (result.DetectedLabel == d.Label)
                        nCorrectCount++;

                    nTotalCount++;
                    bmp.Dispose();

                    // Output progress
                    if (sw.Elapsed.TotalMilliseconds >= 1000)
                    {
                        double dfProgressPct = ((double)i / m_rgstrTestingFiles.Length);
                        double dfAccuracy = ((double)nCorrectCount / (double)nTotalCount);
                        Trace.WriteLine("(" + dfProgressPct.ToString("P") + ") accuracy = " + dfAccuracy.ToString("P"));
                        sw.Restart();
                    }
                }
            }

            double dfTotalAccuracy = ((double)nCorrectCount / (double)nTotalCount);
            MessageBox.Show("Total accuracy = " + dfTotalAccuracy.ToString("P"), "Testing Weights", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Release all resources used.
            mycaffe.Dispose();
        }

        private void saveWeights(MyCaffeControl<float> mycaffe, string strName)
        {
            string strDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\models\\mnist\\";
            string strFile = strDir + strName + ".mycaffemodel";

#if VER_10_2_160
            byte[] rgWeights = mycaffe.GetWeights();
#else
            Net<float> net = mycaffe.GetInternalNet(Phase.TRAIN);
            byte[] rgWeights = net.SaveWeights(mycaffe.Persist, false);
#endif

            if (File.Exists(strFile))
                File.Delete(strFile);

            using (FileStream fs = File.OpenWrite(strFile))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(rgWeights);
            }
        }

        private byte[] loadWeights(string strName)
        {
            string strDir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\models\\mnist\\";
            string strFile = strDir + strName + ".mycaffemodel";

            if (!File.Exists(strFile))
                return null;

            using (FileStream fs = File.OpenRead(strFile))
            using (BinaryReader br = new BinaryReader(fs))
            {
                return br.ReadBytes((int)fs.Length);
            }
        }
    }
}
