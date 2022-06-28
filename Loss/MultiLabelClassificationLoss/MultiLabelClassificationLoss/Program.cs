using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.basecode.descriptors;
using MyCaffe.common;
using MyCaffe.data;
using MyCaffe.db.image;
using MyCaffe.param;
using MyCaffe.solvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MultiLabelClassificationLoss
{
    internal class Program
    {
        /// <summary>
        /// Specifies the minimum version of MyCaffe supported.
        /// </summary>
        static string m_strExpectedMyCaffeVersion = "1.11.6.46";
        /// <summary>
        /// Specifies the batch size.
        /// </summary>
        static int m_nBatch = 128;
        static int m_nInputCount = 2;   // input items per each input.
        static int m_nClassCount = 8;   // classes

        public enum LOSS_TYPE
        {
            HINGE = 1,
            SIGMOIDCE = 2,
            SOFTMAXCE = 3,
            SOFTMAX = 4
        }

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Standard arguments, not used.</param>
        /// <remarks>
        /// Simple binary classification loss example with the following data
        /// input sizes.
        ///     Data (Batch, 1, 1, InputDim), in this sample InputDim = 2
        ///     Label (Batch, 1, 1, 1), where the label = 0, 1 or 2.
        /// </remarks>
        static void Main(string[] args)
        {
            // Create the output log.
            Log log = new Log("test");
            log.OnWriteLine += Log_OnWriteLine;

            // Create the Cancel event to abort training.
            CancelEvent evtCancel = new CancelEvent();
            // Create the object used to load the MyCaffe database.
            Dataset ds = new Dataset(log, evtCancel);

            // Get the mean error type to use from the user.
            LOSS_TYPE lossType = get_loss_type();

            // Create MyCaffe
            SettingsCaffe settings;
            MyCaffeControl<float> mycaffe = create_mycaffe(log, evtCancel, out settings);

            try
            {
                if (!version_check(mycaffe, m_strExpectedMyCaffeVersion))
                    throw new Exception("You need to use a version of MyCaffe >= '" + m_strExpectedMyCaffeVersion + "'!");

                // Load the dataset data into the MyCaffe database.
                int nDatasetId;
                IXImageDatabaseBase db = ds.LoadDatabase(settings, out nDatasetId);

                // Build the model description.
                string strModel = build_model(m_nBatch, m_nInputCount, m_nClassCount, lossType, db, nDatasetId);
                // Build the solver description.
                string strSolver = build_solver();

                // Load the model and solver for training.
                mycaffe.Load(Phase.TRAIN, strSolver, strModel, null, null, null, false, db);

                // Set the OnTestingIteration event so that we can track the progress.
                mycaffe.GetInternalSolver().OnTestResults += Program_OnTestResults;
                // Set the OnSnapshot event so that we can notify when the best accuracies are received.
                mycaffe.GetInternalSolver().OnSnapshot += Program_OnSnapshot;

                // Train for 2 epochs.
                int nTrainCount = db.GetDatasetById(nDatasetId).TrainingSource.ImageCount;
                int nIterations = (2 * nTrainCount) / m_nBatch;
                mycaffe.Train(nIterations);
            }
            catch (Exception excpt)
            {
                Console.WriteLine("ERROR: " + excpt.Message);
            }
            finally
            {
                Console.WriteLine("Press any key...");
                Console.ReadKey();

                if (mycaffe != null)
                    mycaffe.Dispose();
            }
        }

        /// <summary>
        /// Query the user for the binary loss type to use (default = SigmoidCrossEntropeLoss 'Softmax').
        /// </summary>
        /// <returns>The LOSS_TYPE type is returned.</returns>
        private static LOSS_TYPE get_loss_type()
        {
            Console.WriteLine("What loss type would you like to use? 1=Hinge, 2=SigmoidCE (default)");
            string strLoss = Console.ReadLine().Trim(' ', '\r', '\n');
            LOSS_TYPE lossType = LOSS_TYPE.SIGMOIDCE;

            switch (strLoss)
            {
                case "1":
                    lossType = LOSS_TYPE.HINGE;
                    break;
                case "2":
                    lossType = LOSS_TYPE.SIGMOIDCE;
                    break;
                default:
                    lossType = LOSS_TYPE.SIGMOIDCE;
                    break;
            }

            Console.WriteLine("Using '" + lossType.ToString() + "' loss type."); 
            return lossType;
        }

        /// <summary>
        /// Event called at the end of each test cycle to verify results.
        /// </summary>
        /// <param name="sender">Specifies the sender - the Solver</param>
        /// <param name="e">Specifies the testing arguments.</param>
        private static void Program_OnTestResults(object sender, TestResultArgs<float> e)
        {
            float[] rgTarget = e.Results[0].mutable_cpu_data;
            float[] rgPredicted = e.Results[1].mutable_cpu_data;
            int nMatchCount = 0;
            int nMatchTotal = 0;
            float fTopThreshold = 0.7f;
            float fBtmThreshold = 0.3f;

            for (int i = 0; i < m_nBatch; i++)
            {
                for (int j = 0; j < m_nClassCount; j++)
                {
                    int nIdx = i * m_nClassCount + j;
                    float fPredicted = rgPredicted[nIdx];
                    float fTarget = rgTarget[nIdx];

                    if (fPredicted > fTopThreshold && fTarget == 1)
                        nMatchCount++;
                    else if (fPredicted < fBtmThreshold && fTarget == 0)
                        nMatchCount++;

                    nMatchTotal++;
                }
            }

            e.Accuracy = (double)nMatchCount / nMatchTotal;
            e.AccuracyValid = true;
        }
        
        /// <summary>
        /// Event called each time a better accuracy is found.
        /// </summary>
        /// <param name="sender">Specifies the sender - the Solver</param>
        /// <param name="e">Specifies the snapshot arguments, including the current accuracy.</param>
        private static void Program_OnSnapshot(object sender, SnapshotArgs e)
        {
            Console.WriteLine("Accuracy = " + e.Accuracy.ToString("P") + " at iteraction " + e.Iteration.ToString());
        }

        /// <summary>
        /// Create MyCaffe
        /// </summary>
        /// <returns>An instance to the MyCaffeControl is returned.</returns>
        static MyCaffeControl<float> create_mycaffe(Log log, CancelEvent evtCancel, out SettingsCaffe settings)
        {
            // Run on GPU 0
            settings = new SettingsCaffe();
            settings.GpuIds = "0";
            settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ALL;

            // Return the MyCaffe instance.
            return new MyCaffeControl<float>(settings, log, evtCancel);
        }

        /// <summary>
        /// Output status to the console.
        /// </summary>
        /// <param name="sender">Specifies the sender, the Log</param>
        /// <param name="e">Specifies the output arguments.</param>
        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Console.WriteLine(e.Message);
        }

        /// <summary>
        /// Build the model
        /// </summary>
        /// <param name="nBatch">Specifies the batch size.</param>
        /// <param name="nInputCount">Specifies the input count.</param>
        /// <param name="nClassCount">Specifies the class count.</param>
        /// <param name="lossType">Specifies the loss type.</param>
        /// <returns>Return the model as a descriptor string.</returns>
        /// <remarks>The model is a simple linear regression model.</remarks>
        static string build_model(int nBatch, int nInputCount, int nClassCount, LOSS_TYPE lossType, IXImageDatabaseBase db, int nDatasetId)
        {
            DatasetDescriptor ds = db.GetDatasetById(nDatasetId);
            NetParameter net = new NetParameter();
            net.name = lossType.ToString().ToLower() + "_model";

            LayerParameter data = new LayerParameter(LayerParameter.LayerType.DATA);
            data.name = "data";
            data.top.Add("data");
            data.top.Add("label");
            data.include.Add(new NetStateRule(Phase.TRAIN));
            data.data_param.source = ds.TrainingSource.Name;
            data.data_param.batch_size = (uint)nBatch;
            data.data_param.enable_random_selection = true;
            data.data_param.one_hot_label_size = 8;
            data.data_param.label_type = LayerParameterBase.LABEL_TYPE.MULTIPLE;
            data.data_param.backend = DataParameter.DB.IMAGEDB;
            data.transform_param.scale = 1.0 / 255.0;
            net.layer.Add(data);

            data = new LayerParameter(LayerParameter.LayerType.DATA);
            data.name = "data";
            data.top.Add("data");
            data.top.Add("label");
            data.include.Add(new NetStateRule(Phase.TEST));
            data.data_param.source = ds.TestingSource.Name;
            data.data_param.batch_size = (uint)nBatch;
            data.data_param.enable_random_selection = true;
            data.data_param.one_hot_label_size = 8;
            data.data_param.label_type = LayerParameterBase.LABEL_TYPE.MULTIPLE;
            data.data_param.backend = DataParameter.DB.IMAGEDB;
            data.transform_param.scale = 1.0 / 255.0;
            net.layer.Add(data);

            // Add the convolution blocks.
            string strLayer = "data";            
            strLayer = build_model_add_block(net, "a", strLayer, 32, 0.2);
            strLayer = build_model_add_block(net, "b", strLayer, 64, 0.2);
            strLayer = build_model_add_block(net, "c", strLayer, 128, 0.2);
            
            // Create the first Dense layer with 50 outputs.
            LayerParameter dense1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            dense1.name = "dense1";
            dense1.bottom.Add(strLayer);
            dense1.top.Add("dense1");
            dense1.inner_product_param.num_output = 128;
            dense1.inner_product_param.weight_filler = new FillerParameter("xavier");
            dense1.inner_product_param.bias_filler = new FillerParameter("constant", 0.0);
            dense1.inner_product_param.bias_term = true;
            net.layer.Add(dense1);

            // Create the activation function.
            LayerParameter activation1 = new LayerParameter(LayerParameter.LayerType.RELU);
            activation1.name = "relud1";
            activation1.bottom.Add("dense1");
            activation1.top.Add("dense1");
            net.layer.Add(activation1);

            LayerParameter dropout = new LayerParameter(LayerParameter.LayerType.DROPOUT);
            dropout.name = "drop2";
            dropout.bottom.Add("dense1");
            dropout.top.Add("dense1");
            dropout.dropout_param.active = true;
            dropout.dropout_param.dropout_ratio = 0.5;
            net.layer.Add(dropout);

            // Create the second Dense layer with 1 output.
            LayerParameter dense2 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            dense2.name = "dense2";
            dense2.bottom.Add("dense1");
            dense2.top.Add("dense2");
            dense2.inner_product_param.num_output = (uint)nClassCount; // 7 classes
            dense2.inner_product_param.weight_filler = new FillerParameter("xavier");
            dense2.inner_product_param.bias_filler = new FillerParameter("constant", 0.0);
            dense2.inner_product_param.bias_term = true;
            net.layer.Add(dense2);

            // Create the loss layer.
            switch (lossType)
            {
                case LOSS_TYPE.HINGE:
                    {
                        LayerParameter loss = new LayerParameter(LayerParameter.LayerType.HINGE_LOSS);
                        loss.name = "loss";
                        loss.bottom.Add("dense2");
                        loss.bottom.Add("label");
                        loss.top.Add("loss");
                        loss.include.Add(new NetStateRule(Phase.TRAIN));
                        net.layer.Add(loss);
                    }
                    break;
                    
                case LOSS_TYPE.SIGMOIDCE:
                    {
                        LayerParameter loss = new LayerParameter(LayerParameter.LayerType.SIGMOIDCROSSENTROPY_LOSS);
                        loss.name = "loss";
                        loss.bottom.Add("dense2");
                        loss.bottom.Add("label");
                        loss.top.Add("loss");
                        loss.include.Add(new NetStateRule(Phase.TRAIN));
                        net.layer.Add(loss);
                    }
                    break;
            }

            if (lossType == LOSS_TYPE.SIGMOIDCE)
            {
                LayerParameter sigmoid = new LayerParameter(LayerParameter.LayerType.SIGMOID);
                sigmoid.name = "sigmoid";
                sigmoid.bottom.Add("dense2");
                sigmoid.top.Add("sigmoid");
                sigmoid.include.Add(new NetStateRule(Phase.TEST));
                net.layer.Add(sigmoid);
            }
            else
            {
                LayerParameter softmax = new LayerParameter(LayerParameter.LayerType.SOFTMAX);
                softmax.name = "softmax";
                softmax.bottom.Add("dense2");
                softmax.top.Add("softmax");
                softmax.include.Add(new NetStateRule(Phase.TEST));
                net.layer.Add(softmax);
            }

            return net.ToProto("root").ToString();
        }

        /// <summary>
        /// Build a convolution block (similar to VGG)
        /// </summary>
        /// <param name="net">Specifies the Net parameter</param>
        /// <param name="strName">Specifies the unique part of the name added to each layer.</param>
        /// <param name="strInputLayer">Specifies the input laye name.</param>
        /// <param name="nKernels">Specifies the convolution kernels.</param>
        /// <returns>Returns the output layer name.</returns>
        private static string build_model_add_block(NetParameter net, string strName, string strInputLayer, int nKernels, double dfDropout)
        {
            // Block 1
            LayerParameter conv1 = new LayerParameter(LayerParameter.LayerType.CONVOLUTION);
            conv1.name = "conv1" + strName;
            conv1.bottom.Add(strInputLayer);
            conv1.top.Add("conv1" + strName);
            conv1.convolution_param.kernel_size.Add(3);
            conv1.convolution_param.stride.Add(1);
            conv1.convolution_param.num_output = (uint)nKernels;
            net.layer.Add(conv1);

            LayerParameter relu1 = new LayerParameter(LayerParameter.LayerType.RELU);
            relu1.name = "relu1" + strName;
            relu1.bottom.Add("conv1" + strName);
            relu1.top.Add("conv1" + strName);
            net.layer.Add(relu1);

            LayerParameter conv2 = new LayerParameter(LayerParameter.LayerType.CONVOLUTION);
            conv2.name = "conv2" + strName;
            conv2.bottom.Add("conv1" + strName);
            conv2.top.Add("conv2" + strName);
            conv2.convolution_param.kernel_size.Add(3);
            conv2.convolution_param.stride.Add(1);
            conv2.convolution_param.num_output = (uint)nKernels;
            net.layer.Add(conv2);
            
            LayerParameter relu2 = new LayerParameter(LayerParameter.LayerType.RELU);
            relu2.name = "relu2" + strName;
            relu2.bottom.Add("conv2" + strName);
            relu2.top.Add("conv2" + strName);
            net.layer.Add(relu2);
            
            LayerParameter pool1 = new LayerParameter(LayerParameter.LayerType.POOLING);
            pool1.name = "pool1" + strName;
            pool1.bottom.Add("conv2" + strName);
            pool1.top.Add("pool1" + strName);
            pool1.pooling_param.pool = PoolingParameter.PoolingMethod.MAX;
            pool1.pooling_param.kernel_size.Add(2);
            pool1.pooling_param.stride.Add(1);
            net.layer.Add(pool1);
            
            if (dfDropout > 0)
            {
                LayerParameter dropout = new LayerParameter(LayerParameter.LayerType.DROPOUT);
                dropout.name = "drop1" + strName;
                dropout.bottom.Add("pool1" + strName);
                dropout.top.Add("pool1" + strName);
                dropout.dropout_param.active = true;
                dropout.dropout_param.dropout_ratio = dfDropout;
                net.layer.Add(dropout);
            }

            return pool1.name;
        }

        /// <summary>
        /// Create the solver description string.
        /// </summary>
        /// <returns>The solver description string is returned.</returns>
        static string build_solver()
        {           
            SolverParameter solver = new SolverParameter();
            // Use SGD with momentum.
            solver.type = SolverParameter.SolverType.SGD;
            // Learning rate = 0.01
            solver.base_lr = 0.01;
            solver.momentum = 0.9;
            // Keep the same learning rate.
            solver.LearningRatePolicy = SolverParameter.LearningRatePolicyType.FIXED;
            // Disable initial testing.
            solver.test_initialization = false;
            // Run testing every 100 intervals.
            solver.test_interval = 10;
            // Only run one test.
            solver.test_iter[0] = 10;
            // Add weight decay.
            solver.weight_decay = 0;
            // Display output every 1000 intervals.
            solver.display = 1000;
            // Disable snapshots after training.
            solver.snapshot_after_train = false;
            // Disable periodic snapshots (snapshots are 
            // still taken when accuracy improvements are found)
            solver.snapshot = -1;
            
            return solver.ToProto("root").ToString();
        }

        /// <summary>
        /// Verify the version of MyCaffe.
        /// </summary>
        /// <param name="mycaffe">Specifies the instance of MyCaffe.</param>
        /// <param name="strMinVersion">Specifies the minimum version.</param>
        /// <returns>If the version running is >= the minimum version, true is returned, otherwise false.</returns>
        /// <exception cref="Exception">An exception is thrown if the version string is not found.</exception>
        private static bool version_check(MyCaffeControl<float> mycaffe, string strMinVersion)
        {
            Type type = mycaffe.GetType();
            string strName = type.Assembly.FullName;
            string strTarget = "Version=";

            int nPos = strName.IndexOf(strTarget);
            if (nPos < 0)
                throw new Exception("Could not find the 'Version=' in the assembly name!");

            strName = strName.Substring(nPos + strTarget.Length);
            nPos = strName.IndexOf(',');
            if (nPos < 0)
                throw new Exception("Could not find the ',' after the 'Version=' in the assembly name.");

            string strVer = strName.Substring(0, nPos);
            string[] rgstrVer = strVer.Split('.');
            string[] rgstrMin = strMinVersion.Split('.');
            int[] rgnVer = rgstrVer.Select(p => int.Parse(p)).ToArray();
            int[] rgnMin = rgstrMin.Select(p => int.Parse(p)).ToArray();

            for (int i = 0; i < rgnVer.Length; i++)
            {
                if (rgnVer[i] < rgnMin[i])
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// The Dataset class loads and manages the dataset used for training and testing.
    /// </summary>
    class Dataset
    {
        CancelEvent m_evtCancel;
        Log m_log;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="log">Specifies the log used for output.</param>
        /// <param name="evtCancel">Specifies the cancel event to cancel processing.</param>
        public Dataset(Log log, CancelEvent evtCancel)
        {
            m_log = log;
            m_evtCancel = evtCancel;
        }

        /// <summary>
        /// Load the Kaggel Amazon Rainforest data into a MyCaffe in-memory database.
        /// </summary>
        /// <returns>The MyCaffe in-memory database is returned.</returns>
        public IXImageDatabaseBase LoadDatabase(SettingsCaffe settings, out int nDsId)
        {
            string strPath = getFullFilePath("");
            string strTrainingPath = strPath + "training";
            string strTestingPath = strPath + "testing";

            KARSDataParameters param = new KARSDataParameters(strTrainingPath, strTestingPath);
            KARSDataLoader loader = new KARSDataLoader(param, m_log, m_evtCancel);

            nDsId = loader.GetDatasetExists();
            if (nDsId == 0)
            {
                string[] rgstrFiles = Directory.GetFiles(strTrainingPath, "*.png");
                if (rgstrFiles.Length == 0)
                    throw new Exception("No training images were found in the directory '" + strTrainingPath + "'!");

                rgstrFiles = Directory.GetFiles(strTestingPath, "*.png");
                if (rgstrFiles.Length == 0)
                    throw new Exception("No testing images were found in the directory '" + strTrainingPath + "'!");

                loader.OnProgress += Loader_OnProgress;
                loader.OnError += Loader_OnError;
                loader.OnCompleted += Loader_OnCompleted;

                nDsId = loader.LoadDatabase();
            }

            m_log.WriteLine("Loading dataset into the MyCaffe in-memory database...");
            MyCaffeImageDatabase db = new MyCaffe.db.image.MyCaffeImageDatabase(m_log);

            db.InitializeWithDsId1(settings, nDsId);
            return db;
        }

        private void Loader_OnCompleted(object sender, EventArgs e)
        {
            m_log.WriteLine("Completed loading the dataset into SQL.");
        }

        private void Loader_OnError(object sender, ProgressArgs e)
        {
            m_log.WriteLine("ERROR: Loading the dataset into SQL.  " + e.Progress.Error.Message);
        }

        private void Loader_OnProgress(object sender, ProgressArgs e)
        {
            m_log.WriteLine("Loading data into SQL at " + e.Progress.Percentage.ToString("P") + "...");
        }

        private string getFullFilePath(string strFile)
        {
            string strPath = AssemblyDirectory;
            int nPos = strPath.LastIndexOf('\\');
            if (nPos < 0)
                throw new Exception("Could not find the full path of the file '" + strFile + "'.");

            strPath = strPath.Substring(0, nPos);
            nPos = strPath.LastIndexOf('\\');
            if (nPos < 0)
                throw new Exception("Could not find the full path of the file '" + strFile + "'.");

            strPath = strPath.Substring(0, nPos);
            strPath += "\\dataset\\" + strFile;

            return strPath;
        }

        /// <summary>
        /// Return the directory of the executing assembly.
        /// </summary>
        public string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
