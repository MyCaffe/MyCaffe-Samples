using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.param;
using MyCaffe.solvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BinaryClassificationLoss
{
    internal class Program
    {
        /// <summary>
        /// Specifies the minimum version of MyCaffe supported.
        /// </summary>
        static string m_strExpectedMyCaffeVersion = "1.11.6.46";
        /// <summary>
        /// Specifies the training dataset.
        /// </summary>
        static Dataset m_dsTrain;
        /// <summary>
        /// Specifies the testingn dataset.
        /// </summary>
        static Dataset m_dsTest;
        /// <summary>
        /// Specifies the batch size.
        /// </summary>
        static int m_nBatch = 32;
        static int m_nInputCount = 2;   // input items per each input.
        static int m_nClassCount = 3;   // classes 0, 1, 2

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
            Dataset ds = new Dataset();

            // Load the dataset which contains the y (1000) and X (1000,2) values.
            ds.Load("blobs_dataset_raw.txt");
            // Split the dataset in half into training and testing sets.
            Tuple<Dataset, Dataset> data = ds.Split(0.5);
            m_dsTrain = data.Item1;
            m_dsTest = data.Item2;

            // Get the mean error type to use from the user.
            LOSS_TYPE lossType = get_loss_type();

            // Build the model description.
            string strModel = build_model(m_nBatch, m_nInputCount, m_nClassCount, lossType);
            // Build the solver description.
            string strSolver = build_solver();

            // Create MyCaffe
            MyCaffeControl<float> mycaffe = create_mycaffe();

            try
            {
                if (!version_check(mycaffe, m_strExpectedMyCaffeVersion))
                    throw new Exception("You need to use a version of MyCaffe >= '" + m_strExpectedMyCaffeVersion + "'!");

                // Load the model and solver for training.
                mycaffe.LoadLite(Phase.TRAIN, strSolver, strModel);

                // Set the OnStart event so that we can load the training data here.
                mycaffe.GetInternalSolver().OnStart += Program_OnStart;
                // Set the OnTrainingIteration event so that we can track the progress.
                mycaffe.GetInternalSolver().OnTrainingIteration += Program_OnTrainingIteration;

                // Set the OnTestStart event so that we can load the testing data here.
                mycaffe.GetInternalSolver().OnTestStart += Program_OnTestStart;
                // Set the OnTestingIteration event so that we can track the progress.
                mycaffe.GetInternalSolver().OnTestingIteration += Program_OnTestingIteration;
                // Set the OnSnapshot event so that we can notify when the best accuracies are received.
                mycaffe.GetInternalSolver().OnSnapshot += Program_OnSnapshot;

                // Train for 300 epochs.
                int nIterations = (300 * m_dsTrain.Count) / m_nBatch;
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
            Console.WriteLine("What loss type would you like to use? 1=Hinge, 2=SigmoidCE, 3=SoftmaxCE, 4=Softmax (default)");
            string strLoss = Console.ReadLine().Trim(' ', '\r', '\n');
            LOSS_TYPE lossType = LOSS_TYPE.SOFTMAX;

            switch (strLoss)
            {
                case "1":
                    lossType = LOSS_TYPE.HINGE;
                    break;
                case "2":
                    lossType = LOSS_TYPE.SIGMOIDCE;
                    break;
                case "3":
                    lossType = LOSS_TYPE.SOFTMAXCE;
                    break;
                case "4":
                    lossType = LOSS_TYPE.SOFTMAX;
                    break;
                default:
                    lossType = LOSS_TYPE.SOFTMAX;
                    break;
            }

            Console.WriteLine("Using '" + lossType.ToString() + "' loss type."); 
            return lossType;
        }

        /// <summary>
        /// Event called just before each Training Step - load the training data here.
        /// </summary>
        /// <param name="sender">Specifies the sender, the Solver</param>
        /// <param name="e">Event arguments, not used.</param>
        private static void Program_OnStart(object sender, EventArgs e)
        {
            Solver<float> solver = sender as Solver<float>;

            // Get the Data and Label blobs from the Training Net.
            Net<float> net = solver.TrainingNet;
            Blob<float> dataBlob = net.blob_by_name("data");
            Blob<float> labelBlob = net.blob_by_name("label");

            // Get a batch of random training data.
            int nYItemCount = (labelBlob.count() == m_nBatch * m_nClassCount) ? m_nClassCount : 1;
            Tuple<float[], float[]> data = m_dsTrain.GetData(m_nBatch, nYItemCount);

            // Load the training data into the X->data, and y->label.
            dataBlob.mutable_cpu_data = data.Item1;
            labelBlob.mutable_cpu_data = data.Item2;
        }

        /// <summary>
        /// Event called just after each Training Step - track any progress here.
        /// </summary>
        /// <param name="sender">Specifies the sender, the Solver</param>
        /// <param name="e">Specifies the training arguments for the step.</param>
        private static void Program_OnTrainingIteration(object sender, MyCaffe.common.TrainingIterationArgs<float> e)
        {
        }

        /// <summary>
        /// Event called just before each Testing Step - load the testing data here.
        /// </summary>
        /// <param name="sender">Specifies the sender - the Solver</param>
        /// <param name="e">Event arguments, not used.</param>
        private static void Program_OnTestStart(object sender, EventArgs e)
        {
            Solver<float> solver = sender as Solver<float>;

            Net<float> net = solver.TestingNet;
            Blob<float> dataBlob = net.blob_by_name("data");
            Blob<float> labelBlob = net.blob_by_name("label");

            int nYItemCount = (labelBlob.count() == m_nBatch * m_nClassCount) ? m_nClassCount : 1;
            Tuple<float[], float[]> data = m_dsTest.GetData(m_nBatch, nYItemCount);
            dataBlob.mutable_cpu_data = data.Item1;
            labelBlob.mutable_cpu_data = data.Item2;
        }

        /// <summary>
        /// Event called at the end of each testing step - track any testing progress here.
        /// </summary>
        /// <param name="sender">Specifies the sender - the Solver</param>
        /// <param name="e">Specifies the testing arguments.</param>
        private static void Program_OnTestingIteration(object sender, MyCaffe.common.TestingIterationArgs<float> e)
        {
        }

        /// <summary>
        /// Event called each time a better accuracy is found.
        /// </summary>
        /// <param name="sender">Specifies the sender - the Solver</param>
        /// <param name="e">Specifies the snapshot arguments, including the current accuracy.</param>
        private static void Program_OnSnapshot(object sender, SnapshotArgs e)
        {
            Console.WriteLine("Accuracy = " + e.Accuracy.ToString("P"));
        }

        /// <summary>
        /// Create MyCaffe
        /// </summary>
        /// <returns>An instance to the MyCaffeControl is returned.</returns>
        static MyCaffeControl<float> create_mycaffe()
        {
            // Run on GPU 0
            SettingsCaffe settings = new SettingsCaffe();
            settings.GpuIds = "0";

            // Create the output log.
            Log log = new Log("test");
            log.OnWriteLine += Log_OnWriteLine;

            // Create the Cancel event to abort training.
            CancelEvent evtCancel = new CancelEvent();

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
        static string build_model(int nBatch, int nInputCount, int nClassCount, LOSS_TYPE lossType)
        {
            int nLabelItemCount = 1;
            NetParameter net = new NetParameter();
            net.name = lossType.ToString().ToLower() + "_model";

            // Create the input layer with inputs data->(32,1,1,2), label->(32,1,1,1)
            LayerParameter input = new LayerParameter(LayerParameter.LayerType.INPUT);
            input.name = "input";
            input.top.Add("data");
            input.top.Add("label");
            input.include.Add(new NetStateRule(Phase.TRAIN));
            input.input_param.shape = new List<BlobShape>()
            {
                new BlobShape(nBatch, 1, 1, nInputCount), // data, e.g., batch of 32
                new BlobShape(nBatch, 1, 1, nLabelItemCount)   // label, e.g., batch of 32 
            };
            net.layer.Add(input);
            input = new LayerParameter(LayerParameter.LayerType.INPUT);
            input.name = "input";
            input.top.Add("data");
            input.top.Add("label");
            input.include.Add(new NetStateRule(Phase.TEST));
            input.input_param.shape = new List<BlobShape>()
            {
                new BlobShape(nBatch, 1, 1, nInputCount), // data, e.g., batch of 32
                new BlobShape(nBatch, 1, 1, nLabelItemCount)   // label, e.g., batch of 32 
            };
            net.layer.Add(input);

            // Create the first Dense layer with 50 outputs.
            LayerParameter dense1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            dense1.name = "dense1";
            dense1.bottom.Add("data");
            dense1.top.Add("dense1");
            dense1.inner_product_param.num_output = 128;
            dense1.inner_product_param.weight_filler = new FillerParameter("xavier");
            dense1.inner_product_param.bias_filler = new FillerParameter("constant", 0.0);
            dense1.inner_product_param.bias_term = true;
            net.layer.Add(dense1);

            // Create the activation function.
            LayerParameter activation1 = new LayerParameter(LayerParameter.LayerType.SIGMOID);
            activation1.name = "relu1";
            activation1.bottom.Add("dense1");
            activation1.top.Add("dense1");
            net.layer.Add(activation1);

            // Create the second Dense layer with 1 output.
            LayerParameter dense2 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            dense2.name = "dense2";
            dense2.bottom.Add("dense1");
            dense2.top.Add("dense2");
            dense2.inner_product_param.num_output = (uint)nClassCount; // 3 classes; 0, 1 or 2
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

                case LOSS_TYPE.SOFTMAXCE:
                    {
                        LayerParameter loss = new LayerParameter(LayerParameter.LayerType.SOFTMAXCROSSENTROPY_LOSS);
                        loss.name = "loss";
                        loss.bottom.Add("dense2");
                        loss.bottom.Add("label");
                        loss.top.Add("loss");
                        loss.include.Add(new NetStateRule(Phase.TRAIN));
                        net.layer.Add(loss);
                    }
                    break;

                case LOSS_TYPE.SOFTMAX:
                    {
                        LayerParameter loss = new LayerParameter(LayerParameter.LayerType.SOFTMAXWITH_LOSS);
                        loss.name = "loss";
                        loss.bottom.Add("dense2");
                        loss.bottom.Add("label");
                        loss.top.Add("loss");
                        loss.include.Add(new NetStateRule(Phase.TRAIN));
                        net.layer.Add(loss);
                    }
                    break;
            }

            string strBottom = "dense2";
            if (lossType == LOSS_TYPE.SIGMOIDCE)
            {
                LayerParameter sigmoid = new LayerParameter(LayerParameter.LayerType.SIGMOID);
                sigmoid.name = "sigmoid";
                sigmoid.bottom.Add("dense2");
                sigmoid.top.Add("sigmoid");
                sigmoid.include.Add(new NetStateRule(Phase.TEST));
                net.layer.Add(sigmoid);
                strBottom = "sigmoid"; 
            }
            else
            {
                LayerParameter softmax = new LayerParameter(LayerParameter.LayerType.SOFTMAX);
                softmax.name = "softmax";
                softmax.bottom.Add("dense2");
                softmax.top.Add("softmax");
                softmax.include.Add(new NetStateRule(Phase.TEST));
                net.layer.Add(softmax);
                strBottom = "softmax";
            }

            LayerParameter accuracy = new LayerParameter(LayerParameter.LayerType.ACCURACY);
            accuracy.name = "accuracy";
            accuracy.bottom.Add(strBottom);
            accuracy.bottom.Add("label");
            accuracy.top.Add("accuracy");
            accuracy.include.Add(new NetStateRule(Phase.TEST));
            net.layer.Add(accuracy);

            return net.ToProto("root").ToString();
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
        float m_fAveX = 0;
        float m_fStdevX = 0;
        float m_fAvey = 0;
        float m_fStdevy = 0;
        List<List<float>> m_rgX = new List<List<float>>();
        List<float> m_rgy = new List<float>();
        int m_nIdx = 0;
        Random m_random = null;
        
        /// <summary>
        /// The constructor.
        /// </summary>
        public Dataset()
        {
        }

        /// <summary>
        /// Return the number of items in the X and y data.
        /// </summary>
        public int Count
        {
            get { return m_rgy.Count; }
        }

        /// <summary>
        /// Returns arrays filled with X,y data pairs randomly selected
        /// from the data set.
        /// </summary>
        /// <param name="nBatch">Specifies the batch size.</param>
        /// <param name="nYItemCount">Returns the y values as y itself, when nYItemCount = 1, or a one-hot vector when > 1.</param>
        /// <returns>The X array returned is X(item_len) * nBatch long, and the y array is nBatch long.</returns>
        public Tuple<float[], float[]> GetData(int nBatch, int nYItemCount = 1)
        {
            int nItemCount = m_rgX[0].Count;
            float[] rgXbatch = new float[nBatch * nItemCount];
            float[] rgYbatch = new float[nBatch * nYItemCount];

            if (m_random == null)
                m_random = new Random();

            for (int i = 0; i < nBatch; i++)
            {
                m_nIdx = m_random.Next(Count);

                float[] rgX = m_rgX[m_nIdx].ToArray();
                Array.Copy(rgX, 0, rgXbatch, i * nItemCount, nItemCount);

                float fY = m_rgy[m_nIdx];
                if (nYItemCount > 1)
                {
                    float[] rgy = new float[nYItemCount];
                    rgy[(int)fY] = 1;
                    Array.Copy(rgy, 0, rgYbatch, i * nYItemCount, nYItemCount);
                }
                else
                {
                    rgYbatch[i] = fY;
                }
            }

            return new Tuple<float[], float[]>(rgXbatch, rgYbatch);
        }

        /// <summary>
        /// Split the dataset into two separate and distinct datasets based on the percent given.
        /// </summary>
        /// <param name="dfPct">Specifies the percentage of the dataset to split.</param>
        /// <returns>Returns a Tuple containing the first and second data sets slit from the original.</returns>
        public Tuple<Dataset, Dataset> Split(double dfPct)
        {
            Dataset dsTrain = new Dataset();
            Dataset dsTest = new Dataset();
            int nIdx = (int)(Count * dfPct);

            for (int i = 0; i < Count; i++)
            {
                if (i < nIdx)
                {
                    dsTrain.m_rgy.Add(m_rgy[i]);
                    dsTrain.m_rgX.Add(m_rgX[i]);
                }
                else
                {
                    dsTest.m_rgy.Add(m_rgy[i]);
                    dsTest.m_rgX.Add(m_rgX[i]);
                }
            }

            return new Tuple<Dataset, Dataset>(dsTrain, dsTest);
        }

        /// <summary>
        ///  Compare one dataset with another.
        /// </summary>
        /// <param name="ds">Specifies the dataset to compare with this one.</param>
        /// <returns>If the datasets are the same, true is returned, otherwise false.</returns>
        public bool Compare(Dataset ds)
        {
            if (m_rgX.Count != ds.m_rgX.Count)
                return false;
            
            if (m_rgy.Count != ds.m_rgy.Count)
                return false;
            
            for (int i = 0; i < m_rgX.Count; i++)
            {
                if (m_rgX[i].Count != ds.m_rgX[i].Count)
                    return false;
                
                for (int j = 0; j < m_rgX[i].Count; j++)
                {
                    if (m_rgX[i][j] != ds.m_rgX[i][j])
                        return false;
                }
            }
            
            for (int i = 0; i < m_rgy.Count; i++)
            {
                if (m_rgy[i] != ds.m_rgy[i])
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Returns the X input data.
        /// </summary>
        public List<List<float>> X
        {
            get { return m_rgX; }
        }

        /// <summary>
        /// Returns the y target data.
        /// </summary>
        public List<float> y
        {
            get { return m_rgy; }
        }

        /// <summary>
        /// Loads the dataset from a file.
        /// </summary>
        /// <param name="strFile">Specifies the file name</param>
        /// <remarks>The expected file format is as follows:
        /// Count
        /// Count y vales, one item per line
        /// Count X values, one item per line, where each item is separated by a space.
        /// 
        /// For example:
        /// 5               
        /// 0.1
        /// 0.2
        /// 0.3
        /// 0.4
        /// 0.5
        /// 1 2 3 4
        /// 5 6 7 8
        /// 9 10 11 12
        /// 13 14 15 16
        /// 17 18 19 20
        /// </remarks>
        public void Load(string strFile)
        {
            strFile = getFullFilePath(strFile);

            using (StreamReader sr = new StreamReader(strFile))
            {
                string strLine = sr.ReadLine();
                int nCount = (int)float.Parse(strLine);

                // Read in y
                float fTotaly = 0;
                int nTotaly = 0;
                
                for (int i = 0; i < nCount; i++)
                {
                    strLine = sr.ReadLine();
                    float fy = float.Parse(strLine);
                    m_rgy.Add(fy);

                    fTotaly += fy;
                    nTotaly++;
                }

                m_fAvey = fTotaly / nTotaly;

                // Read in X
                float fTotalX = 0;
                int nTotalX = 0;
                
                for (int i = 0; i < nCount; i++)
                {
                    strLine = sr.ReadLine();
                    string[] rgstrX = strLine.Split(' ');

                    List<float> rgX = new List<float>();
                    for (int j = 0; j < rgstrX.Length; j++)
                    {
                        float fX = float.Parse(rgstrX[j]);
                        rgX.Add(fX);

                        fTotalX += fX;
                        nTotalX++;
                    }

                    m_rgX.Add(rgX);
                }

                m_fAveX = fTotalX / nTotalX;
            }
        }

        /// <summary>
        /// Center the data by subtracting the mean and dividing by the standard deviation.
        /// </summary>
        public void Center()
        {
            float fSumysq = 0;
            for (int i=0; i<m_rgy.Count; i++)
            {
                float fDiffy = m_rgy[i] - m_fAvey;
                fSumysq += fDiffy * fDiffy;                
            }

            m_fStdevy = (float)Math.Sqrt(fSumysq / m_rgy.Count);

            float fSumXsq = 0;
            for (int i = 0; i < m_rgX.Count; i++)
            {
                for (int j = 0; j < m_rgX[i].Count; j++)
                {
                    float fDiffX = m_rgX[i][j] - m_fAveX;
                    fSumXsq += fDiffX * fDiffX;
                }
            }

            m_fStdevX = (float)Math.Sqrt(fSumXsq / m_rgX.Count);

            // Center the Y data.
            for (int i = 0; i < m_rgy.Count; i++)
            {
                m_rgy[i] = (m_rgy[i] - m_fAvey) / m_fStdevy;
            }

            // Center the X data.
            for (int i = 0; i < m_rgX.Count; i++)
            {
                for (int j = 0; j < m_rgX[i].Count; j++)
                {
                    m_rgX[i][j] = (m_rgX[i][j] - m_fAveX) / m_fStdevX;
                }
            }
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
