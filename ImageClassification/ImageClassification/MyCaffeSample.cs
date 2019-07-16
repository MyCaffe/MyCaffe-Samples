using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.basecode.descriptors;
using MyCaffe.common;
using MyCaffe.gym;
using MyCaffe.db.image;
using MyCaffe.trainers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MyCaffe.db.stream;
using MyCaffe.extras;
using System.Net;

namespace MyCaffeSample
{
    //-----------------------------------------------------------------------------------
    // MyCaffeSample.MyCaffeSample
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// This sample is provided to show how to initialize and use MyCaffe.  However, before
    /// using this sample, you should complete the configuration steps noted in the remarks.
    /// </summary>
    /// <remarks>
    /// This sample expects that the following configuration steps have been completed.
    /// 
    /// 1.) Download and install Microsoft SQL Express 2016 (or Standard, etc.),
    ///     see https://www.microsoft.com/en-us/sql-server/sql-server-editions-express.
    /// 2.) Download and install the MyCaffe Test Application,
    ///     see https://github.com/MyCaffe/MyCaffe/releases.
    /// 3.) From the MyCaffe Test Application create the MyCaffe database by selecting
    ///     the 'Database | Create Database' menu.
    /// 4.) From the MyCaffe Test Application load the MNIST dataset by selecting
    ///     the 'Database | Load MNIST...' menu.
    /// 5.) Register the file 'packages\CudaControl.x.x.x.x\nativeBinaries\x64\CudaControl.dll'
    ///     by running the command 'regsvr32.exe CudaControl.dll' from a CMD window with
    ///     Administrative privileges.
    /// 6.) Copy the file 'packages\MyCaffe.x.x.x.x\nativeBinaries\x64\CudaDnnDll.10.0.dll'
    ///     to the output 'bin\x64\Debug' and 'bin\x64\Release' directories for this
    ///     DLL provides your connection to CUDA.
    /// 7.) IMPORTANT: Setup your project to build to the x64 Platform Target by unchecking
    ///     the 'Prefer 32-bit' build option.
    ///     
    /// NOTE: To use MyCaffe you must have at least one NVIDIA GPU installed that supports
    /// CUDA - see https://www.nvidia.com/en-us/geforce/.
    /// </remarks>
    public class MyCaffeSample : IDisposable
    {
        MyCaffeControl<float> m_caffe = null;
        CancelEvent m_evtCancel = new CancelEvent();    // Allows for cancelling training and testing operations.
        Log m_log = null;                               // Provides output of testing and training operations.

        public MyCaffeSample()
        {
        }

        /// <summary>
        /// When using MyCaffe it important to explicitly dispose of it when finished,
        /// for this will free all GPU memory used by MyCaffe.
        /// </summary>
        public void Dispose()
        {
            if (m_caffe != null)
            {
                m_caffe.Dispose();
                m_caffe = null;
            }
        }

        /// <summary>
        /// The Initialize function shows how to create MyCaffe and load a project into it. 
        /// </summary>
        /// <remarks>
        /// MyCaffe organizes datasets and models by project where each project contains 
        /// the following:
        ///     a.) The Solver Description
        ///     b.) The Model Descrption
        ///     c.) The Dataset (a reference to the Dataset in the database)
        ///     d.) The Model Results (trained weights)
        ///     
        /// The example function below loads a new project used to run the LeNet model
        /// on the MNIST dataset.
        /// </remarks>
        /// <param name="loadMethod">Specifies how the images are to be loaded (e.g. on demand, all at once, etc.)</param>
        public void Initialize(IMAGEDB_LOAD_METHOD loadMethod)
        {
            try
            {
                //---------------------------------------------------
                // The Default SQL instance is used by 'default'.
                // To change to a different instance, uncomment the
                //  line below which will then use the 'SQLEXPRESS' 
                //  instance.
                //---------------------------------------------------
                // EntitiesConnection.GlobalDatabaseServerName = ".\\SQLEXPRESS";

                // Load the MNIST dataset descriptor (not the images)
                DatasetFactory factory = new DatasetFactory();
                DatasetDescriptor ds = factory.LoadDataset("MNIST");

                // Create the LeNet project.
                ProjectEx prj = new ProjectEx("LeNet");

                // Load the LeNet model and solver desciptions into the project.
                string strDir = Path.GetFullPath("..\\..\\..\\models\\mnist\\");
                if (!Directory.Exists(strDir))
                    strDir = Path.GetFullPath("..\\..\\models\\mnist\\");

                prj.LoadModelFile(strDir + "lenet_train_test.prototxt");
                prj.LoadSolverFile(strDir + "lenet_solver.prototxt");
                // Set the project dataset to the MNIST dataset.
                prj.SetDataset(ds);

                // Setup the MyCaffe initialization settings.
                SettingsCaffe settings = new SettingsCaffe();
                settings.GpuIds = "0";  // use GPU 0.
                settings.ImageDbLoadMethod = loadMethod;

                // Setup the MyCaffe output log.
                m_log = new Log("Test");
                m_log.OnWriteLine += Log_OnWriteLine;

                // Create the MyCaffeControl 
                m_caffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

                // Load the project into MyCaffe.  This steps will load the images
                // into the MyCafffe in-memory Image Database and create a connection
                // to the GPU used.
                m_caffe.Load(Phase.TRAIN, prj);
            }
            catch (Exception excpt)
            {
                MessageBox.Show("ERROR: " + excpt.Message + "\n\nMake sure that you are building this project for x64!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Output from the training and testing operations will go to this even.
        /// </summary>
        /// <remarks>
        /// This output is sent to the Visual Studio Debug Output window. 
        /// </remarks>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the log arguments such as the message and progress.</param>
        private void Log_OnWriteLine(object sender, LogArg e)
        {
            Trace.WriteLine("(" + e.Progress.ToString("P") + ") " + e.Message);
        }

        /// <summary>
        /// Train the loaded project for 1000 iterations.
        /// </summary>
        public void Train()
        {
            m_evtCancel.Reset();
            m_caffe.Train(1000);
        }

        /// <summary>
        /// Test the loaded project for 1000 iterations.
        /// </summary>
        /// <returns></returns>
        public double Test()
        {
            m_evtCancel.Reset();
            return m_caffe.Test(1000);
        }

        /// <summary>
        /// Run an image through the trained network.
        /// </summary>
        /// <remarks>
        /// Note, the MNIST model expects black and white images of size 28x28 pixels.
        /// </remarks>
        /// <param name="bmp">Specifies the input image.</param>
        /// <returns>
        /// The results of the run are returned.  See the 'DetectedLabel' field 
        /// for the label detected during the run.
        /// </returns>
        public ResultCollection Run(Bitmap bmp)
        {
            m_evtCancel.Reset();
            return m_caffe.Run(bmp);
        }

        /// <summary>
        /// Abort the current operation (training or testing).
        /// </summary>
        /// <remarks>
        /// NOTE: Make sure to reset the event before your next operation.
        /// </remarks>
        public void Abort()
        {
            m_evtCancel.Set();
        }

        /// <summary>
        /// This static function puts it all together for a very simple sample.
        /// Call this function from your application.
        /// </summary>
        /// <param name="loadMethod">Specifies how the images are to be loaded (default = LOAD_ALL).
        /// The following load methods are available:
        ///   LOAD_ON_DEMAND - loads the images into memory as they are needed during training (slower training).
        ///   LOAD_ALL - loads all images into memory first then trains (fastest training).
        /// </param>
        public static void RunSample(IMAGEDB_LOAD_METHOD loadMethod = IMAGEDB_LOAD_METHOD.LOAD_ALL)
        {
            MyCaffeSample sample = new MyCaffeSample();

            sample.Initialize(loadMethod);
            sample.Train();
            double dfAccuracy = sample.Test();
            sample.Dispose();

            MessageBox.Show("The MNIST trained accuracy = " + dfAccuracy.ToString("P"));
        }
    }


    //-----------------------------------------------------------------------------------
    // MyCaffeSample.MyCaffeRLpgSample
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// This sample is provided to show how to initialize and use MyCaffe with the Policy
    /// Gradient reinforcement trainer.  However, before using this sample, you should complete
    /// the configuration steps noted in the remarks.
    /// </summary>
    /// <remarks>
    /// This sample expects that the following configuration steps have been completed.
    /// 
    /// 1.) Download and install Microsoft SQL Express 2016 (or Standard, etc.),
    ///     see https://www.microsoft.com/en-us/sql-server/sql-server-editions-express.
    /// 2.) Download and install the MyCaffe Test Application,
    ///     see https://github.com/MyCaffe/MyCaffe/releases.
    /// 3.) From the MyCaffe Test Application create the MyCaffe database by selecting
    ///     the 'Database | Create Database' menu.
    /// 4.) Register the file 'packages\CudaControl.x.x.x.x\nativeBinaries\x64\CudaControl.dll'
    ///     by running the command 'regsvr32.exe CudaControl.dll' from a CMD window with
    ///     Administrative privileges.
    /// 5.) Copy the file 'packages\MyCaffe.x.x.x.x\nativeBinaries\x64\CudaDnnDll.10.1.dll'
    ///     to the output 'bin\x64\Debug' and 'bin\x64\Release' directories for this
    ///     DLL provides your connection to CUDA.
    /// 6.) IMPORTANT: Setup your project to build to the x64 Platform Target by unchecking
    ///     the 'Prefer 32-bit' build option.
    ///     
    /// NOTE: To use MyCaffe you must have at least one NVIDIA GPU installed that supports
    /// CUDA - see https://www.nvidia.com/en-us/geforce/.
    /// </remarks>
    public class MyCaffeRLpgSample : IDisposable
    {
        MyCaffeControl<float> m_mycaffe = null;
        IXMyCaffeCustomTrainer m_itrainer = null;
        CancelEvent m_evtCancel = new CancelEvent();    // Allows for cancelling training and testing operations.
        Log m_log = null;
        string m_strInit;
        ITERATOR_TYPE m_iteratorType = ITERATOR_TYPE.ITERATION;

        public enum GYM
        {
            CARTPOLE,
            ATARI
        }

        public MyCaffeRLpgSample(Log log)
        {
            m_log = log;
        }

        /// <summary>
        /// When using MyCaffe it important to explicitly dispose of it when finished,
        /// for this will free all GPU memory used by MyCaffe.
        /// </summary>
        public void Dispose()
        {
            if (m_mycaffe != null)
            {
                m_mycaffe.Dispose();
                m_mycaffe = null;
            }
        }

        /// <summary>
        /// The Initialize function shows how to create MyCaffe and load a project into it. 
        /// </summary>
        /// <remarks>
        /// MyCaffe organizes datasets and models by project where each project contains 
        /// the following:
        ///     a.) The Solver Description
        ///     b.) The Model Descrption
        ///     c.) The Dataset (a reference to the Dataset in the database)
        ///     d.) The Model Results (trained weights)
        ///     
        /// The example function below loads a new project used to run the RL model
        /// on the GYM selected.
        /// </remarks>
        /// <
        public void Initialize(GYM gym = GYM.CARTPOLE)
        {
            try
            {
                string strName = "MyCaffeCartPoleTrainer";

                // Setup the MyCaffe initialization settings.
                SettingsCaffe settings = new SettingsCaffe();
                settings.GpuIds = "0";  // use GPU 0.
                settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ON_DEMAND; // images are not used.

                // Create the MyCaffeControl 
                m_mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

                // Create the Policy Gradient project.
                ProjectEx prj = new ProjectEx("test");
                string strDir = "";

                if (gym == GYM.ATARI)
                {
                    // Create the MyCaffe Atari trainer.
                    m_itrainer = new MyCaffeAtariTrainer();

                    // Set the initialization parameters
                    //  - TraingerType = 'PG.MT' ('PG.MT' = use multi-threaded Policy Gradient trainer)
                    //  - RewardType = VAl (display the actual rewards received, a setting of MAX displays the maximum reward received)
                    //  - Gamma = 0.99 (discounting factor)
                    //  - Threads = 1 (only use 1 thread if multi-threading is supported)
                    //  - EnableBinaryActions = True (use left or right actions, only applies to ATARI gym)
                    //  - UseAcceleratedTraining = True (apply gradient on each step AND apply accumulated gradients on end of mini-batch which defaults to 10).
                    //  - GameROM = 'path to game ROM'
                    string strRom = Path.GetFullPath("..\\..\\..\\roms\\pong.bin");
                    if (!File.Exists(strRom))
                        strRom = Path.GetFullPath("..\\..\\roms\\pong.bin");
                    m_strInit = "TrainerType=PG.MT;RewardType=VAL;Gamma=0.99;EnableBinaryActions=True;UseAcceleratedTraining=False;AllowDiscountReset=True;GameROM=" + strRom;

                    // Load the Atari model and solver desciptions into the project.
                    strDir = Path.GetFullPath("..\\..\\..\\models\\pg\\atari\\");
                    if (!Directory.Exists(strDir))
                        strDir = Path.GetFullPath("..\\..\\models\\pg\\atari\\");

                    m_iteratorType = ITERATOR_TYPE.EPISODE;
                }
                else // Cart-Pole
                {
                    // Create the MyCaffe cart-pole trainer.
                    m_itrainer = new MyCaffeCartPoleTrainer();

                    // Set the initialization parameters
                    //  - TraingerType = PG>MT (use multi-threaded Policy Gradient trainer)
                    //  - RewardType = VAL (display the actual rewards, other options are MAX and AVE)
                    //  - Gamma = 0.99 (discounting factor)
                    //  - Init1 = default force of 10.
                    //  - Init2 = do not use additive force.                    
                    m_strInit = "TrainerType=PG.MT;RewardType=VAL;Gamma=0.99;Init1=10;Init2=0;UseAcceleratedTraining=True;AllowDiscountReset=False;";

                    // Load the Cart-Pole model and solver desciptions into the project.
                    strDir = Path.GetFullPath("..\\..\\..\\models\\pg\\cartpole\\");
                    if (!Directory.Exists(strDir))
                        strDir = Path.GetFullPath("..\\..\\models\\pg\\cartpole\\");
                }

                prj.LoadModelFile(strDir + "train_val.prototxt");
                prj.LoadSolverFile(strDir + "solver.prototxt");

                // Set the project dataset to the Gym dataset.
                DatasetDescriptor ds = m_itrainer.GetDatasetOverride(0);
                m_log.CHECK(ds != null, "The " + strName + " should return its dataset override returned by the Gym that it uses.");
                prj.SetDataset(ds);

                // Load the project to train (note the project must use the MemoryDataLayer for input).
                m_mycaffe.Load(Phase.TRAIN, prj, IMGDB_LABEL_SELECTION_METHOD.NONE, IMGDB_IMAGE_SELECTION_METHOD.NONE, false, null, (ds == null) ? true : false);
            }
            catch (Exception excpt)
            {
                MessageBox.Show("ERROR: " + excpt.Message + "\n\nMake sure that you are building this project for x64!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Output from the training and testing operations will go to this even.
        /// </summary>
        /// <remarks>
        /// This output is sent to the Visual Studio Debug Output window. 
        /// </remarks>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the log arguments such as the message and progress.</param>
        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Trace.WriteLine("(" + e.Progress.ToString("P") + ") " + e.Message);
        }

        /// <summary>
        /// Train the loaded project for 1000 iterations.
        /// </summary>
        /// <param name="nIterations">Specifies the maximum number of episodes to run.</param>
        /// <param name="bShowUi">Show the gym user interface.</param>
        public void Train(int nIterations, bool bShowUi)
        {
            m_evtCancel.Reset();

            // Train the network using the custom trainer
            //  - Iterations (maximum frames cumulative across all threads) = 1000 (normally this would be much higher such as 500,000)
            //  - Learning rate = 0.001 (defined in solver.prototxt)
            //  - Mini Batch Size = 10 (defined in train_val.prototxt for MemoryDataLayer)
            m_itrainer.Initialize(m_strInit, null);

            if (bShowUi)
                m_itrainer.OpenUi();

            m_itrainer.Train(m_mycaffe, nIterations, m_iteratorType, TRAIN_STEP.NONE);
            m_itrainer.CleanUp();
        }

        /// <summary>
        /// Abort the current operation (training or testing).
        /// </summary>
        /// <remarks>
        /// NOTE: Make sure to reset the event before your next operation.
        /// </remarks>
        public void Abort()
        {
            m_evtCancel.Set();
        }

        /// <summary>
        /// This static function puts it all together for a very simple sample.
        /// Call this function from your application.
        /// </summary>
        /// <param name="ctrlParent">Specifies the parent control where the sample is run, typically use 'RunSample(this)' when running from within a Form.</param>
        /// <param name="evtCancel">Optionally, specifies the cancel event used to abort the training.</param>
        /// <param name="gym">Optionally, specifies the gym to use: CARTPOLE or ATARI (default = CARTPOLE).</param>
        /// <param name="log">Optionally, specifies an external log to use.</param>
        public static void RunSample(Control ctrlParent, CancelEvent evtCancel = null, GYM gym = GYM.CARTPOLE, Log log = null)
        {
            if (evtCancel != null)
                evtCancel.Reset();

            MyCaffeGymUiServiceHost gymHost = new MyCaffeGymUiServiceHost();

            // Setup the MyCaffe output log.
            if (log == null)
                log = new Log("Test");

            log.OnWriteLine += Log_OnWriteLine;

            gymHost = new MyCaffeGymUiServiceHost();

            try
            {
                gymHost.Open();
            }
            catch (Exception excpt)
            {
                log.WriteError(excpt);
                gymHost = null;
            }


            // IMPORTANT: When using the Gym the trainer must run in a separate thread from the thread
            // where the Gym is registered (e.g. the MyCaffeGymRegistrar.Initialize and trainer must
            // run on separate threads, with the MyCaffeGymRegistrar.Initialize being called on the main
            // user-interface thread that contains the message pump.
            Task task = Task.Factory.StartNew(new Action<object>(run), new Tuple<Log, CancelEvent, GYM>(log, evtCancel, gym));
        }

        private static void run(object obj)
        {
            Tuple<Log, CancelEvent, GYM> args = obj as Tuple<Log, CancelEvent, GYM>;
            MyCaffeRLpgSample sample = new MyCaffeRLpgSample(args.Item1);

            // Use the cancel event passed in.
            if (args.Item2 != null)
                sample.m_evtCancel.AddCancelOverride(args.Item2);

            sample.Initialize(args.Item3);
            sample.Train(50000, true);
            sample.Dispose();
        }
    }

    /// <summary>
    /// This sample is provided to show how to initialize and use MyCaffe with the Deep Q-Learning
    /// Network reinforcement trainer.  However, before using this sample, you should complete
    /// the configuration steps noted in the remarks.
    /// </summary>
    /// <remarks>
    /// This sample expects that the following configuration steps have been completed.
    /// 
    /// 1.) Download and install Microsoft SQL Express 2016 (or Standard, etc.),
    ///     see https://www.microsoft.com/en-us/sql-server/sql-server-editions-express.
    /// 2.) Download and install the MyCaffe Test Application,
    ///     see https://github.com/MyCaffe/MyCaffe/releases.
    /// 3.) From the MyCaffe Test Application create the MyCaffe database by selecting
    ///     the 'Database | Create Database' menu.
    /// 4.) Register the file 'packages\CudaControl.x.x.x.x\nativeBinaries\x64\CudaControl.dll'
    ///     by running the command 'regsvr32.exe CudaControl.dll' from a CMD window with
    ///     Administrative privileges.
    /// 5.) Copy the file 'packages\MyCaffe.x.x.x.x\nativeBinaries\x64\CudaDnnDll.10.1.dll'
    ///     to the output 'bin\x64\Debug' and 'bin\x64\Release' directories for this
    ///     DLL provides your connection to CUDA.
    /// 6.) IMPORTANT: Setup your project to build to the x64 Platform Target by unchecking
    ///     the 'Prefer 32-bit' build option.
    ///     
    /// NOTE: To use MyCaffe you must have at least one NVIDIA GPU installed that supports
    /// CUDA - see https://www.nvidia.com/en-us/geforce/.
    /// </remarks>
    public class MyCaffeRLdqnSample : IDisposable
    {
        MyCaffeControl<float> m_mycaffe = null;
        IXMyCaffeCustomTrainer m_itrainer = null;
        CancelEvent m_evtCancel = new CancelEvent();    // Allows for cancelling training and testing operations.
        Log m_log = null;
        string m_strInit;
        ITERATOR_TYPE m_iteratorType = ITERATOR_TYPE.ITERATION;

        public enum GYM
        {
            CARTPOLE,
            ATARI
        }

        public MyCaffeRLdqnSample(Log log)
        {
            m_log = log;
        }

        /// <summary>
        /// When using MyCaffe it important to explicitly dispose of it when finished,
        /// for this will free all GPU memory used by MyCaffe.
        /// </summary>
        public void Dispose()
        {
            if (m_mycaffe != null)
            {
                m_mycaffe.Dispose();
                m_mycaffe = null;
            }
        }

        /// <summary>
        /// The Initialize function shows how to create MyCaffe and load a project into it. 
        /// </summary>
        /// <remarks>
        /// MyCaffe organizes datasets and models by project where each project contains 
        /// the following:
        ///     a.) The Solver Description
        ///     b.) The Model Descrption
        ///     c.) The Dataset (a reference to the Dataset in the database)
        ///     d.) The Model Results (trained weights)
        ///     
        /// The example function below loads a new project used to run the RL model
        /// on the GYM selected.
        /// 
        /// NOTE: Training the DQN to beat ATARI breakout can take several days to complete.  Initially, you will see 
        /// the rewards drop from around zero down to around -1 as the experience buffer fills.  However after episode
        /// 500 or so, you should start to see the rewards slowly start to increase towards the positive and eventually
        /// move into the positive.
        /// </remarks>
        public void Initialize(GYM gym = GYM.CARTPOLE)
        {
            try
            {
                string strName = "MyCaffeCartPoleTrainer";

                // Setup the MyCaffe initialization settings.
                SettingsCaffe settings = new SettingsCaffe();
                settings.GpuIds = "0";  // use GPU 0.
                settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ON_DEMAND; // images are not used.

                // Create the MyCaffeControl 
                m_mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

                // Create the Policy Gradient project.
                ProjectEx prj = new ProjectEx("test");
                string strDir = "";

                if (gym == GYM.ATARI)
                {
                    // Create the MyCaffe Atari trainer.
                    m_itrainer = new MyCaffeAtariTrainer();

                    // Set the initialization parameters
                    //  - TraingerType = 'DQN.ST' ('DQN.ST' = use single-threaded Deep Q-Learning trainer)
                    //  - RewardType = VAl (display the actual rewards received, a setting of MAX displays the maximum reward received)
                    //  - Gamma = 0.99 (discounting factor)
                    //  - Threads = 1 (only use 1 thread if multi-threading is supported)
                    //  - EnableBinaryActions = False (do not use binary actions for we also what the fire action, only applies to ATARI gym)
                    //  - AllowNegativeRewards = True (produce negative rewards when ball is missed, only applies to ATARI gym)
                    //  - TerminateOnRallyEnd = True (set termination state at end of rally instead of end of game, only applies to ATARI gym)
                    //  - MiniBatch = 1 (do not use a mini batch for gradient accumulation)
                    //  - UseAcceleratedTraining = False (disable accelerated training, only applies when MiniBatch > 1).
                    //  - GameROM = 'path to game ROM'
                    string strRom = Path.GetFullPath("..\\..\\..\\roms\\breakout.bin");
                    if (!File.Exists(strRom))
                        strRom = Path.GetFullPath("..\\..\\roms\\breakout.bin");
                    m_strInit = "TrainerType=DQN.ST;RewardType=VAL;Gamma=0.99;EnableBinaryActions=False;AllowNegativeRewards=True;TerminateOnRallyEnd=True;MiniBatch=1;UseAcceleratedTraining=False;AllowDiscountReset=True;GameROM=" + strRom;

                    // Load the Atari model and solver desciptions into the project.
                    strDir = Path.GetFullPath("..\\..\\..\\models\\dqn\\atari\\");
                    if (!Directory.Exists(strDir))
                        strDir = Path.GetFullPath("..\\..\\models\\dqn\\atari\\");

                    m_iteratorType = ITERATOR_TYPE.EPISODE;
                }
                else // Cart-Pole
                {
                    // Create the MyCaffe cart-pole trainer.
                    m_itrainer = new MyCaffeCartPoleTrainer();

                    // Set the initialization parameters
                    //  - TraingerType = 'DQN.SIMPLE' ('DQN.SIMPLE' = use simple (single frame) Deep Q-Learning trainer)
                    //  - RewardType = VAL (display the value of rewards received, a setting of MAX displays the maximum reward received)
                    //  - Gamma = 0.99 (discounting factor)
                    //  - UseRawInput = True (use the raw input data without preprocessing by subtracting the current values from the previous received)
                    //  - Threads = 1 (only use 1 thread if multi-threading is supported)
                    //  - UseAcceleratedTraining = False (disable accelerated training).
                    //  - Init1 = default force of 10.
                    //  - Init2 = 0 = do not use additive force.
                    //  - GameROM = 'path to breakout game ROM'
                    string strRom = Path.GetFullPath("..\\..\\..\\roms\\breakout.bin");
                    if (!File.Exists(strRom))
                        strRom = Path.GetFullPath("..\\..\\roms\\breakout.bin");
                    m_strInit = "TrainerType=DQN.SIMPLE;RewardType=VAL;Gamma=0.99;UseRawInput=True;UseAcceleratedTraining=False;AllowDiscountReset=False;Init1=10;Init2=0;GameROM=" + strRom;

                    // Load the Atari model and solver desciptions into the project.
                    strDir = Path.GetFullPath("..\\..\\..\\models\\dqn\\cartpole\\");
                    if (!Directory.Exists(strDir))
                        strDir = Path.GetFullPath("..\\..\\models\\dqn\\cartpole\\");
                }

                prj.LoadModelFile(strDir + "train_val.prototxt");
                prj.LoadSolverFile(strDir + "solver.prototxt");

                // Set the project dataset to the Gym dataset.
                DatasetDescriptor ds = m_itrainer.GetDatasetOverride(0);
                m_log.CHECK(ds != null, "The " + strName + " should return its dataset override returned by the Gym that it uses.");
                prj.SetDataset(ds);

                // Load the project to train (note the project must use the MemoryDataLayer for input).
                m_mycaffe.Load(Phase.TRAIN, prj, IMGDB_LABEL_SELECTION_METHOD.NONE, IMGDB_IMAGE_SELECTION_METHOD.NONE, false, null, (ds == null) ? true : false);
            }
            catch (Exception excpt)
            {
                MessageBox.Show("ERROR: " + excpt.Message + "\n\nMake sure that you are building this project for x64!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Output from the training and testing operations will go to this even.
        /// </summary>
        /// <remarks>
        /// This output is sent to the Visual Studio Debug Output window. 
        /// </remarks>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the log arguments such as the message and progress.</param>
        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Trace.WriteLine("(" + e.Progress.ToString("P") + ") " + e.Message);
        }

        /// <summary>
        /// Train the loaded project for 1000 iterations.
        /// </summary>
        /// <param name="nIterations">Specifies the maximum number of episodes to run.</param>
        /// <param name="bShowUi">Show the gym user interface.</param>
        public void Train(int nIterations, bool bShowUi)
        {
            m_evtCancel.Reset();

            // Train the network using the custom trainer
            //  - Iterations (maximum frames cumulative across all threads) = 1000 (normally this would be much higher such as 500,000)
            //  - Learning rate = 0.001 (defined in solver.prototxt)
            //  - Mini Batch Size = 10 (defined in train_val.prototxt for MemoryDataLayer)
            m_itrainer.Initialize(m_strInit, null);

            if (bShowUi)
                m_itrainer.OpenUi();

            m_itrainer.Train(m_mycaffe, nIterations, m_iteratorType, TRAIN_STEP.NONE);
            m_itrainer.CleanUp();
        }

        /// <summary>
        /// Abort the current operation (training or testing).
        /// </summary>
        /// <remarks>
        /// NOTE: Make sure to reset the event before your next operation.
        /// </remarks>
        public void Abort()
        {
            m_evtCancel.Set();
        }

        /// <summary>
        /// This static function puts it all together for a very simple sample.
        /// Call this function from your application.
        /// </summary>
        /// <param name="ctrlParent">Specifies the parent control where the sample is run, typically use 'RunSample(this)' when running from within a Form.</param>
        /// <param name="evtCancel">Optionally, specifies the cancel event used to abort the training.</param>
        /// <param name="gym">Optionally, specifies the gym to use: CARTPOLE or ATARI (default = CARTPOLE).</param>
        /// <param name="log">Optionally, specifies an external log to use.</param>
        public static void RunSample(Control ctrlParent, CancelEvent evtCancel = null, GYM gym = GYM.CARTPOLE, Log log = null)
        {
            if (evtCancel != null)
                evtCancel.Reset();


            // Setup the MyCaffe output log.
            if (log == null)
                log = new Log("Test");

            log.OnWriteLine += Log_OnWriteLine;

            MyCaffeGymUiServiceHost gymHost = new MyCaffeGymUiServiceHost();

            try
            {
                gymHost.Open();
            }
            catch (Exception excpt)
            {
                log.WriteError(excpt);
                gymHost = null;
            }

            // IMPORTANT: When using the Gym the trainer must run in a separate thread from the thread
            // where the Gym is registered (e.g. the MyCaffeGymRegistrar.Initialize and trainer must
            // run on separate threads, with the MyCaffeGymRegistrar.Initialize being called on the main
            // user-interface thread that contains the message pump.
            Task task = Task.Factory.StartNew(new Action<object>(run), new Tuple<Log, CancelEvent, GYM>(log, evtCancel, gym));
        }

        private static void run(object obj)
        {
            Tuple<Log, CancelEvent, GYM> args = obj as Tuple<Log, CancelEvent, GYM>;
            MyCaffeRLdqnSample sample = new MyCaffeRLdqnSample(args.Item1);

            // Use the cancel event passed in.
            if (args.Item2 != null)
                sample.m_evtCancel.AddCancelOverride(args.Item2);

            sample.Initialize(args.Item3);
            sample.Train(50000, true);
            sample.Dispose();
        }
    }

    /// <summary>
    /// The MyCaffeCartPole Trainer is the Cart-Pole specific trainer used to provide the
    /// cart-pole data to the underlying MyCaffeRLTrainer.
    /// </summary>
    class MyCaffeCartPoleTrainer : MyCaffeTrainerDual, IXMyCaffeGymUiCallback
    {
        Stopwatch m_sw = new Stopwatch();
        IXMyCaffeGym m_igym;
        Log m_log;
        bool m_bNormalizeInput = false;
        int m_nUiId = -1;
        MyCaffeGymUiProxy m_gymui = null;
        string m_strName = "Cart-Pole";
        GymCollection m_colGyms = new GymCollection();

        public MyCaffeCartPoleTrainer()
            : base()
        {
            m_colGyms.Load();
        }

        protected override void initialize(InitializeArgs e)
        {
            m_igym = m_colGyms.Find(m_strName);
            m_log = e.OutputLog;

            m_bNormalizeInput = m_properties.GetPropertyAsBool("NormalizeInput", false);
            m_igym.Initialize(m_log, m_properties);

            m_sw.Start();
        }

        protected override void shutdown()
        {
            if (m_igym != null)
            {
                m_igym.Close();
                m_igym = null;
            }
        }

        protected override void dispose()
        {
            base.dispose();
        }

        protected override string name
        {
            get { return "RL.Trainer"; }
        }

        protected override DatasetDescriptor get_dataset_override(int nProjectID)
        {
            if (m_igym == null)
                m_igym = m_colGyms.Find(m_strName);

            return m_igym.GetDataset(DATA_TYPE.VALUES);
        }

        protected override bool getData(GetDataArgs e)
        {
            Tuple<State, double, bool> state = null;

            if (e.Reset)
                state = m_igym.Reset();

            if (e.Action >= 0)
                state = m_igym.Step(e.Action);

            bool bShowUi = (m_nUiId >= 0) ? true : false;
            Tuple<Bitmap, SimpleDatum> data = m_igym.Render(bShowUi, 512, 512, true);
            int nDataLen = 0;
            SimpleDatum stateData = state.Item1.GetData(m_bNormalizeInput, out nDataLen);
            Observation obs = new Observation(null, ImageData.GetImage(data.Item2), m_igym.RequiresDisplayImage, stateData.RealData, state.Item2, state.Item3);

            e.State = new StateBase(m_igym.GetActionSpace().Count());
            e.State.Reward = obs.Reward;
            e.State.Data = new SimpleDatum(true, nDataLen, 1, 1, -1, DateTime.Now, null, stateData.RealData.ToList(), 0, false, 0);
            e.State.Done = obs.Done;
            e.State.IsValid = true;

            if (m_gymui != null && m_nUiId >= 0)
            {
                m_gymui.Render(m_nUiId, obs);
                Thread.Sleep(m_igym.UiDelay);
            }

            if (m_sw.Elapsed.TotalMilliseconds > 1000)
            {
                double dfPct = (GlobalEpisodeMax == 0) ? 0 : (double)GlobalEpisodeCount / (double)GlobalEpisodeMax;
                e.OutputLog.Progress = dfPct;
                e.OutputLog.WriteLine("(" + dfPct.ToString("P") + ") Global Episode #" + GlobalEpisodeCount.ToString() + "  Global Reward = " + GlobalRewards.ToString() + " Exploration Rate = " + ExplorationRate.ToString("P"));
                m_sw.Restart();
            }

            return true;
        }

        protected override void openUi()
        {
            m_gymui = new MyCaffeGymUiProxy(new InstanceContext(this));
            m_gymui.Open();
            m_nUiId = m_gymui.OpenUi(m_strName, m_nUiId);
        }

        public void Closing()
        {
            m_nUiId = -1;
            m_gymui.Close();
            m_gymui = null;
        }
    }

    /// <summary>
    /// The MyCaffeAtari Trainer is the Atari specific trainer used to provide the
    /// ATARI data (via the AleControl) to the underlying MyCaffeRLTrainer.
    /// </summary>
    class MyCaffeAtariTrainer : MyCaffeTrainerDual, IXMyCaffeGymUiCallback
    {
        Stopwatch m_sw = new Stopwatch();
        IXMyCaffeGym m_igym;
        Log m_log;
        bool m_bNormalizeInput = false;
        int m_nUiId = -1;
        MyCaffeGymUiProxy m_gymui = null;
        string m_strName = "ATARI";
        GymCollection m_colGyms = new GymCollection();

        public MyCaffeAtariTrainer()
            : base()
        {
            m_colGyms.Load();
        }

        protected override void initialize(InitializeArgs e)
        {
            m_igym = m_colGyms.Find(m_strName);
            m_log = e.OutputLog;

            m_bNormalizeInput = m_properties.GetPropertyAsBool("NormalizeInput", false);
            m_igym.Initialize(m_log, m_properties);

            m_sw.Start();
        }

        protected override void shutdown()
        {
            if (m_igym != null)
            {
                m_igym.Close();
                m_igym = null;
            }
        }

        protected override void dispose()
        {
            base.dispose();
        }

        protected override string name
        {
            get { return "RL.Trainer"; }
        }

        protected override DatasetDescriptor get_dataset_override(int nProjectID)
        {
            if (m_igym == null)
                m_igym = m_colGyms.Find(m_strName);

            return m_igym.GetDataset(DATA_TYPE.BLOB);
        }

        protected override bool getData(GetDataArgs e)
        {
            Tuple<State, double, bool> state = null;

            if (e.Reset)
                state = m_igym.Reset();

            if (e.Action >= 0)
                state = m_igym.Step(e.Action);

            bool bShowUi = (m_nUiId >= 0) ? true : false;
            Tuple<Bitmap, SimpleDatum> data = m_igym.Render(bShowUi, 512, 512, true);
            int nDataLen = 0;
            SimpleDatum stateData = state.Item1.GetData(false, out nDataLen);
            Observation obs = new Observation(data.Item1, ImageData.GetImage(data.Item2), m_igym.RequiresDisplayImage, stateData.RealData, state.Item2, state.Item3);

            e.State = new StateBase(m_igym.GetActionSpace().Count());
            e.State.Reward = obs.Reward;
            e.State.Data = data.Item2;
            e.State.Done = obs.Done;
            e.State.IsValid = true;

            if (m_gymui != null && m_nUiId >= 0)
            {
                if (e.GetDataCallback != null)
                {
                    OverlayArgs args = new OverlayArgs(obs.ImageDisplay);
                    e.GetDataCallback.OnOverlay(args);
                    obs.ImageDisplay = args.DisplayImage;
                }

                m_gymui.Render(m_nUiId, obs);
                Thread.Sleep(m_igym.UiDelay);
            }

            if (m_sw.Elapsed.TotalMilliseconds > 1000)
            {
                double dfPct = (GlobalEpisodeMax == 0) ? 0 : (double)GlobalEpisodeCount / (double)GlobalEpisodeMax;
                e.OutputLog.Progress = dfPct;
                e.OutputLog.WriteLine("(" + dfPct.ToString("P") + ") Global Episode #" + GlobalEpisodeCount.ToString() + "  Global Reward = " + GlobalRewards.ToString() + " Exploration Rate = " + ExplorationRate.ToString("P"));
                m_sw.Restart();
            }

            return true;
        }

        protected override void openUi()
        {
            m_gymui = new MyCaffeGymUiProxy(new InstanceContext(this));
            m_gymui.Open();
            m_nUiId = m_gymui.OpenUi(m_strName, m_nUiId);
        }

        public void Closing()
        {
            m_nUiId = -1;
            m_gymui.Close();
            m_gymui = null;
        }
    }


    //-----------------------------------------------------------------------------------
    // MyCaffeSample.MyCaffeRNNSample
    //-----------------------------------------------------------------------------------

    /// <summary>
    /// This sample is provided to show how to initialize and use MyCaffe with the Recurrent 
    /// Learning trainer to solve Char-RNN and create Shakespeare.  However, before using  
    /// this sample, you should complete the configuration steps noted in the remarks.
    /// </summary>
    /// <remarks>
    /// This sample expects that the following configuration steps have been completed.
    /// 
    /// 1.) Download and install Microsoft SQL Express 2016 (or Standard, etc.),
    ///     see https://www.microsoft.com/en-us/sql-server/sql-server-editions-express.
    /// 2.) Download and install the MyCaffe Test Application,
    ///     see https://github.com/MyCaffe/MyCaffe/releases.
    /// 3.) From the MyCaffe Test Application create the MyCaffe database by selecting
    ///     the 'Database | Create Database' menu.
    /// 4.) Register the file 'packages\CudaControl.x.x.x.x\nativeBinaries\x64\CudaControl.dll'
    ///     by running the command 'regsvr32.exe CudaControl.dll' from a CMD window with
    ///     Administrative privileges.
    /// 5.) Copy the file 'packages\MyCaffe.x.x.x.x\nativeBinaries\x64\CudaDnnDll.10.0.dll'
    ///     to the output 'bin\x64\Debug' and 'bin\x64\Release' directories for this
    ///     DLL provides your connection to CUDA.
    /// 6.) IMPORTANT: Setup your project to build to the x64 Platform Target by unchecking
    ///     the 'Prefer 32-bit' build option.
    ///     
    /// NOTE: To use MyCaffe you must have at least one NVIDIA GPU installed that supports
    /// CUDA - see https://www.nvidia.com/en-us/geforce/.
    /// </remarks>
    public class MyCaffeRNNSample : IDisposable, IXMyCaffeCustomTrainerCallbackRNN
    {
        MyCaffeControl<float> m_mycaffe = null;
        IXMyCaffeCustomTrainerRNN m_itrainer = null;
        CancelEvent m_evtCancel = new CancelEvent();    // Allows for cancelling training and testing operations.
        Log m_log = null;
        string m_strInit;

        delegate void fnDone(string strResult);

        public MyCaffeRNNSample(Log log)
        {
            m_log = log;
        }

        /// <summary>
        /// When using MyCaffe it important to explicitly dispose of it when finished,
        /// for this will free all GPU memory used by MyCaffe.
        /// </summary>
        public void Dispose()
        {
            if (m_mycaffe != null)
            {
                m_mycaffe.Dispose();
                m_mycaffe = null;
            }
        }

        /// <summary>
        /// The Initialize function shows how to create MyCaffe and load a project into it. 
        /// </summary>
        /// <remarks>
        /// MyCaffe organizes datasets and models by project where each project contains 
        /// the following:
        ///     a.) The Solver Description
        ///     b.) The Model Descrption
        ///     c.) The Dataset (a reference to the Dataset in the database)
        ///     d.) The Model Results (trained weights)
        ///     
        /// The example function below loads a new project used to run the RL model
        /// on the GYM selected.
        /// </remarks>
        /// <
        public void Initialize()
        {
            try
            {
                string strName = "MyCaffeRnnTrainer";

                // Setup the MyCaffe initialization settings.
                SettingsCaffe settings = new SettingsCaffe();
                settings.GpuIds = "0";  // use GPU 0.
                settings.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ON_DEMAND; // images are not used.

                // Create the MyCaffeControl 
                m_mycaffe = new MyCaffeControl<float>(settings, m_log, m_evtCancel);

                // Create the Policy Gradient project.
                ProjectEx prj = new ProjectEx("test");
                string strDir = "";

                // Create the MyCaffe Atari trainer.
                m_itrainer = new MyCaffeDataGeneralTrainer();

                // Train the network using the custom trainer
                //  - Iterations (maximum frames cumulative across all threads) = 1000 (normally this would be much higher such as 500,000)
                //  - Learning rate = 0.05 (defined in solver.prototxt)
                //  - Mini Batch Size = 25 (defined in train_val_cudnn.prototxt InputLayer)
                //  - Sequence Length = 75 (defined in train_val_cudnn.prototxt InputLayer)
                //
                //  - TrainerType = 'RNN.SIMPLE' (currently only one supported)
                //  - UseAcceleratedTraining = False (disable accelerated training).
                //  - ConnectionCount=1 (using one query)
                //  - Connection0_CustomQueryName=StdTextFileQuery (using standard text file query to read the text files)
                //  - Connection0_CustomQueryParam=params (set the custom query parameters to the packed parameters containing the FilePath where the text files are to be loaded).
                string strSchema = "ConnectionCount=1;";

                // Set the data path to the directory where the training text
                // file(s) reside - this is where the 4MB Shakespeare.txt file
                // is located.
                string strDataPath = Path.GetFullPath("..\\..\\..\\data\\char-rnn\\");
                if (!Directory.Exists(strDataPath))
                    strDataPath = Path.GetFullPath("..\\..\\data\\char-rnn\\");

                string strParam = "FilePath=" + strDataPath + ";";

                strParam = ParamPacker.Pack(strParam);
                strSchema += "Connection0_CustomQueryName=StdTextFileQuery;";
                strSchema += "Connection0_CustomQueryParam=" + strParam + ";";

                m_strInit = "TrainerType=RNN.SIMPLE;UseAcceleratedTraining=false;" + strSchema;

                // Load the Char-RNN model and solver desciptions into the project.
                strDir = Path.GetFullPath("..\\..\\..\\models\\rnn\\char_rnn\\");
                if (!Directory.Exists(strDir))
                    strDir = Path.GetFullPath("..\\..\\models\\rnn\\char_rnn\\");

                prj.LoadModelFile(strDir + "train_val_cudnn.prototxt");
                prj.LoadSolverFile(strDir + "solver.prototxt");

                // Initialize network using the custom trainer
                m_itrainer.Initialize(m_strInit, this);

                // Pre-load the data used for training to get the vocabulary.
                BucketCollection rgVocabulary = m_itrainer.PreloadData(m_log, m_evtCancel, prj.ID);
    
                // Set the project dataset to the Gym dataset.
                DatasetDescriptor ds = m_itrainer.GetDatasetOverride(0);
                m_log.CHECK(ds != null, "The " + strName + " should return its dataset override returned by the Gym that it uses.");
                prj.SetDataset(ds);

                // Resize the model with the vocabulary.
                prj.ModelDescription = m_itrainer.ResizeModel(m_log, prj.ModelDescription, rgVocabulary);

                // Load the project to train (note the project must use the MemoryDataLayer for input).
                m_mycaffe.Load(Phase.TRAIN, prj, IMGDB_LABEL_SELECTION_METHOD.NONE, IMGDB_IMAGE_SELECTION_METHOD.NONE, false, null, (ds == null) ? true : false);
            }
            catch (Exception excpt)
            {
                MessageBox.Show("ERROR: " + excpt.Message + "\n\nMake sure that you are building this project for x64!", "ERROR!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Output from the training and testing operations will go to this even.
        /// </summary>
        /// <remarks>
        /// This output is sent to the Visual Studio Debug Output window. 
        /// </remarks>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the log arguments such as the message and progress.</param>
        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Trace.WriteLine("(" + e.Progress.ToString("P") + ") " + e.Message);
        }

        /// <summary>
        /// Train the loaded project for 1000 iterations.
        /// </summary>
        /// <param name="nIterations">Specifies the maximum number of episodes to run.</param>
        public void Train(int nIterations)
        {
            m_evtCancel.Reset();
            m_itrainer.Train(m_mycaffe, nIterations, ITERATOR_TYPE.ITERATION, TRAIN_STEP.NONE);
            m_itrainer.CleanUp();
        }

        /// <summary>
        /// Run the model to create a specified number of characters.
        /// </summary>
        /// <param name="nCharCount">Specifies the number of characters to create.</param>
        /// <returns>The string of generated Shakespeare-like text is returned.</returns>
        public string Run(int nCharCount)
        {
            string strType;
            byte[] rgOutput = ((IXMyCaffeCustomTrainerRNN)m_itrainer).Run(m_mycaffe, nCharCount, out strType);
            m_itrainer.CleanUp();
            return Encoding.ASCII.GetString(rgOutput);
        }

        /// <summary>
        /// Abort the current operation (training or testing).
        /// </summary>
        /// <remarks>
        /// NOTE: Make sure to reset the event before your next operation.
        /// </remarks>
        public void Abort()
        {
            m_evtCancel.Set();
        }

        /// <summary>
        /// This static function puts it all together for a very simple sample.
        /// Call this function from your application.
        /// </summary>
        /// <param name="ctrlParent">Specifies the parent control where the sample is run, typically use 'RunSample(this)' when running from within a Form.</param>
        /// <param name="evtCancel">Optionally, specifies the cancel event used to abort the training.</param>
        /// <param name="log">Optionally, specifies an external log to use.</param>
        public static void RunSample(Control ctrlParent, CancelEvent evtCancel = null, Log log = null)
        {
            if (evtCancel != null)
                evtCancel.Reset();

            // Setup the MyCaffe output log.
            if (log == null)
                log = new Log("Test");

            log.OnWriteLine += Log_OnWriteLine;

            // Start training the gym.
            Task task = Task.Factory.StartNew(new Action<object>(run), new Tuple<Log, CancelEvent, Control>(log, evtCancel, ctrlParent));
        }

        private static void run(object obj)
        {
            Tuple<Log, CancelEvent, Control> args = obj as Tuple<Log, CancelEvent, Control>;
            MyCaffeRNNSample sample = new MyCaffeRNNSample(args.Item1);

            // Use the cancel event passed in.
            if (args.Item2 != null)
            {
                args.Item2.Reset();
                sample.m_evtCancel.AddCancelOverride(args.Item2);
            }

            // Initialize and train the model for 10,000 iterations.
            sample.Initialize();
            sample.Train(10000);

            // Create a string of 1000 characters of Shakespeare-like text.
            string strGeneratedShakespeare = sample.Run(1000);
            args.Item1.WriteLine(strGeneratedShakespeare);

            args.Item3.Invoke(new fnDone(done), strGeneratedShakespeare);

            sample.Dispose();
        }

        private static void done(string strResult)
        {
            MessageBox.Show(strResult, "Result Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        /// <summary>
        /// Run the run properties used when calling Run.  These are retrieved via the GetRunProperties call-back.
        /// </summary>
        /// <returns>Return the temperature to use when creating the character data.</returns>
        public string GetRunProperties()
        {
            return "Temperature=0.5";
        }

        /// <summary>
        /// Callback called during the training process.
        /// </summary>
        /// <param name="cat">Specifies the training category.</param>
        /// <param name="rgValues">Specifies the update values.</param>
        public void Update(TRAINING_CATEGORY cat, Dictionary<string, double> rgValues)
        {
            int nIteration = (int)rgValues["GlobalIteration"];
            double dfGlobalLoss = rgValues["GlobalLoss"];
            double dfGlobalAccuracy = rgValues["GlobalAccuracy"];
            double dfLearningRate = rgValues["LearningRate"];

            m_log.WriteLine("Iteration = " + nIteration.ToString() + " Loss = " + dfGlobalLoss + " Accuracy = " + dfGlobalAccuracy + " Learning Rate = " + dfLearningRate.ToString());
        }
    }

    /// <summary>
    /// The MyCaffeDataGeneral Trainer is the DataGeneral specific trainer used to
    /// provide data from the MyCaffe streaming database to the underlying MyCaffeRNNTrainer.
    /// </summary>
    class MyCaffeDataGeneralTrainer : MyCaffeTrainerDual, IXMyCaffeGymUiCallback
    {
        Stopwatch m_sw = new Stopwatch();
        IXMyCaffeGym m_igym;
        Log m_log;
        int m_nUiId = -1;
        string m_strName = "DataGeneral";
        GymCollection m_colGyms = new GymCollection();
        DatasetDescriptor m_ds;
        Tuple<State, double, bool> m_firststate = null;

        public MyCaffeDataGeneralTrainer()
            : base()
        {
            m_colGyms.Load();
        }

        protected override void initialize(InitializeArgs e)
        {
            initialize(e.OutputLog);
            m_sw.Start();
        }

        private void initialize(Log log)
        {
            if (m_igym == null)
            {
                m_log = log;
                m_igym = m_colGyms.Find(m_strName);
                m_igym.Initialize(m_log, m_properties);
            }
        }

        protected override void shutdown()
        {
            if (m_igym != null)
            {
                m_igym.Close();
                m_igym = null;
            }
        }

        protected override void dispose()
        {
            base.dispose();
        }

        protected override string name
        {
            get { return "RNN.Trainer"; }
        }

        protected override DatasetDescriptor get_dataset_override(int nProjectID)
        {
            IXMyCaffeGym igym = m_igym;

            if (igym == null)
                igym = m_colGyms.Find(m_strName);

            m_ds = igym.GetDataset(DATA_TYPE.BLOB);

            return m_ds;
        }

        protected override bool getData(GetDataArgs e)
        {
            Tuple<State, double, bool> state = null;

            if (e.Reset)
            {
                if (m_firststate != null)
                {
                    state = m_firststate;
                    m_firststate = null;
                }
                else
                {
                    state = m_igym.Reset();
                }
            }

            if (e.Action >= 0)
                state = m_igym.Step(e.Action);

            bool bIsOpen = (m_nUiId >= 0) ? true : false;
            int nDataLen = 0;
            SimpleDatum stateData = state.Item1.GetData(false, out nDataLen);

            e.State = new StateBase(m_igym.GetActionSpace().Count());
            e.State.Reward = 0;
            e.State.Data = stateData;
            e.State.Done = state.Item3;
            e.State.IsValid = true;

            if (m_sw.Elapsed.TotalMilliseconds > 1000)
            {
                int nMax = (int)GetProperty("GlobalMaxIterations");
                int nIteration = (int)GetProperty("GlobalIteration");
                double dfPct = (nMax == 0) ? 0 : (double)nIteration / (double)nMax;
                e.OutputLog.Progress = dfPct;
                e.OutputLog.WriteLine("(" + dfPct.ToString("P") + ") Global Iteration #" + nIteration.ToString());
                m_sw.Restart();
            }

            return true;
        }

        protected override bool convertOutput(ConvertOutputArgs e)
        {
            IXMyCaffeGymData igym = m_igym as IXMyCaffeGymData;
            if (igym == null)
                throw new Exception("Output data conversion requires a gym that implements the IXMyCaffeGymData interface.");

            string type;
            byte[] rgOutput = igym.ConvertOutput(Stage, e.Output.Length, e.Output, out type);
            e.SetRawOutput(rgOutput, type);

            return true;
        }

        protected override void openUi()
        {
        }

        protected override BucketCollection preloaddata(Log log, CancelEvent evtCancel, int nProjectID, out bool bUsePreload)
        {
            initialize(log);
            IXMyCaffeGymData igym = m_igym as IXMyCaffeGymData;
            Tuple<State, double, bool> state = igym.Reset();
            int nDataLen;
            SimpleDatum sd = state.Item1.GetData(false, out nDataLen);
            BucketCollection rgBucketCollection = null;

            bUsePreload = true;

            if (sd.IsRealData)
            {
                // Create the vocabulary bucket collection.
                rgBucketCollection = BucketCollection.Bucketize("Building vocabulary", 128, sd, log, evtCancel);
                if (rgBucketCollection == null)
                    return null;
            }
            else
            {
                List<int> rgVocabulary = new List<int>();

                for (int i = 0; i < sd.ByteData.Length; i++)
                {
                    int nVal = (int)sd.ByteData[i];

                    if (!rgVocabulary.Contains(nVal))
                        rgVocabulary.Add(nVal);
                }

                rgBucketCollection = new BucketCollection(rgVocabulary);
            }

            m_firststate = state;

            return rgBucketCollection;
        }

        public void Closing()
        {
        }
    }

    /// <summary>
    /// The MyCaffeNeuralStyle sample applies the style of one image to the content
    /// of another using the Neural Style Transfer algorithm.
    /// </summary>
    /// <remarks>
    /// @see [A Neural Algorithm of Artistic Style](https://arxiv.org/abs/1508.06576) by Gatys, Ecker and Bethge, 2015, arXiv:1508.06576
    /// @see [Very Deep Convolutional Networks for Large-Scale Image Recognition](https://arxiv.org/pdf/1409.1556.pdf) by K. Simonyan, A. Zisserman, arXiv:1409.1556 
    /// </remarks>
    public class MyCaffeNeuralStyleSample : IDisposable
    {
        bool m_bProcessing = false;
        Control m_ctrlParent = null;
        Stopwatch m_swDownload = new Stopwatch();
        CancelEvent m_evtCancel = new CancelEvent();
        AutoResetEvent m_evtDownloadDone = new AutoResetEvent(false);
        Log m_log;
        string m_strSyleImagePath;
        string m_strContentImagePath;
        string m_strResultImagePath;
        CudaDnn<float> m_cuda = null;
        NeuralStyleTransfer<float> m_neuralStyle = null;

        delegate void fnDone(string strResult, string strImgFile);
        delegate void fnBreathe();

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="log">Specifies the output log, which current outputs to the Visual Studio output window.</param>
        public MyCaffeNeuralStyleSample(Log log)
        {
            m_log = log;
        }

        /// <summary>
        /// Release all resources used.
        /// </summary>
        public void Dispose()
        {
            if (m_neuralStyle != null)
            {
                m_neuralStyle.Dispose();
                m_neuralStyle = null;
            }

            if (m_cuda != null)
            {
                m_cuda.Dispose();
                m_cuda = null;
            }
        }

        /// <summary>
        /// Returns the style image path.
        /// </summary>
        public string StyleImagePath
        {
            get { return m_strSyleImagePath; }
        }

        /// <summary>
        /// Returns the content image path.
        /// </summary>
        public string ContentImagePath
        {
            get { return m_strContentImagePath; }
        }

        /// <summary>
        /// Returns the result image path.
        /// </summary>
        public string ResultImagePath
        {
            get { return m_strResultImagePath; }
        }

        private Exception downloadFile(string strUrl, string strFile)
        {
            try
            {
                WebClient webClient = new WebClient();

                m_swDownload.Start();

                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
                webClient.DownloadFileAsync(new Uri(strUrl), strFile);
                
                List<WaitHandle> rgWait = new List<WaitHandle>();
                rgWait.AddRange(m_evtCancel.Handles);
                rgWait.Add(m_evtDownloadDone);

                int nWait = WaitHandle.WaitAny(rgWait.ToArray());

                if (nWait < rgWait.Count - 1)
                {
                    webClient.CancelAsync();

                    if (File.Exists(strFile))
                        File.Delete(strFile);

                    return new Exception("Download Aborted!");
                }
            }
            catch (Exception excpt)
            {
                return excpt;
            }

            return null;
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            m_evtDownloadDone.Set();
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (m_bProcessing)
                return;

            try
            {
                m_bProcessing = true;

                if (m_swDownload.Elapsed.TotalMilliseconds > 1000)
                {
                    double dfPct = (double)e.BytesReceived / (double)e.TotalBytesToReceive;
                    string strMsg = "(" + dfPct.ToString("P") + ") Downloading weight file...";
                    Trace.WriteLine(strMsg);

                    if (m_ctrlParent != null)
                        m_ctrlParent.Invoke(new fnBreathe(breathe));

                    m_swDownload.Restart();
                }
            }
            catch (Exception excpt)
            {
                throw excpt;
            }
            finally
            {
                m_bProcessing = false;
            }
        }

        private void breathe()
        {
            Application.DoEvents();
        }

        /// <summary>
        /// Initialize the sample, by loading the style and content images, loading
        /// the model and weights, and creating the NeuralStyleTransfer object.
        /// </summary>
        /// <remarks>
        /// @see [Very Deep Convolutional Networks for Large-Scale Image Recognition](https://arxiv.org/pdf/1409.1556.pdf) by K. Simonyan, A. Zisserman, arXiv:1409.1556 
        /// </remarks>
        /// <param name="ctrlParent">Specifies the parent control.</param>
        /// <returns>Returns an error description if an error occurred, otherwise <i>null</i> on success.</returns>
        public string Initialize(Control ctrlParent)
        {
            try
            {
                m_ctrlParent = ctrlParent;

                string strPath = Path.GetFullPath("..\\..\\..\\data\\neuralstyle\\");
                if (!Directory.Exists(strPath))
                    strPath = Path.GetFullPath("..\\..\\data\\neuralstyle\\");

                // Get the style image path.
                m_strSyleImagePath = strPath + "style.jpg";

                // Get the content image path.
                m_strContentImagePath = strPath + "content.jpg";

                // Get the result image path.
                m_strResultImagePath = strPath + "result.png";

                // Load the VGG19 model and solver desciptions into the project.
                strPath = Path.GetFullPath("..\\..\\..\\models\\vgg\\vgg19\\neuralstyle\\");
                if (!Directory.Exists(strPath))
                    strPath = Path.GetFullPath("..\\..\\models\\vgg\\vgg19\\neuralstyle\\");

                string strModelFile = strPath + "deploy.prototxt";
                string strWeightFile = strPath + "weights.caffemodel";
                string strModelDesc;
                byte[] rgWeights = null;
                long nWeightFileSize = 0;
                long nExpectedWeightFileSize = 574671192;

                if (File.Exists(strWeightFile))
                {
                    FileInfo fi = new FileInfo(strWeightFile);
                    nWeightFileSize = fi.Length;
                }

                if (nWeightFileSize != nExpectedWeightFileSize)
                {
                    string strWeightsUrl = "http://www.robots.ox.ac.uk/~vgg/software/very_deep/caffe/VGG_ILSVRC_19_layers.caffemodel";
                    string strMsg;

                    if (nWeightFileSize == 0)
                        strMsg = "It appears that the weight file does not exist and must be downloaded which will take a few minutes.  You will only need to do this download once.";
                    else
                        strMsg = "A weight file was found but does not have the correct size.   You will need to re-download the file.";

                    strMsg += Environment.NewLine;
                    strMsg += Environment.NewLine;
                    strMsg += "Download URL: " + strWeightsUrl;
                    strMsg += Environment.NewLine;
                    strMsg += Environment.NewLine;
                    strMsg += "Expected Size: " + nExpectedWeightFileSize.ToString("N0") + " bytes";
                    strMsg += Environment.NewLine;
                    strMsg += Environment.NewLine;
                    strMsg += "Source: http://www.robots.ox.ac.uk/~vgg/research/very_deep/";
                    strMsg += Environment.NewLine;
                    strMsg += Environment.NewLine;
                    strMsg += "The download progress is shown in your Visual Studio output window.  Do you want to proceed?";

                    if (MessageBox.Show(strMsg, "Weight File Download Needed", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                        return "Weight file download cancelled.  You must download the weight file in order to perform the neural style transfer.";

                    Exception err = downloadFile(strWeightsUrl, strWeightFile);
                    if (err != null)
                        return "Failed to download the weights file from '" + strWeightFile + "'! " + err.Message;
                }

                if (File.Exists(strWeightFile))
                {
                    using (FileStream fs = new FileStream(strWeightFile, FileMode.Open, FileAccess.Read))
                    {
                        using (BinaryReader br = new BinaryReader(fs))
                        {
                            rgWeights = br.ReadBytes((int)fs.Length);
                        }
                    }
                }

                using (StreamReader sr = new StreamReader(strModelFile))
                {
                    strModelDesc = sr.ReadToEnd();
                }

                // Create the CUDA connection.
                m_cuda = new CudaDnn<float>(0, DEVINIT.CUBLAS | DEVINIT.CURAND);
                // Create the MyCaffe NeuralStyleTransfer object.
                m_neuralStyle = new NeuralStyleTransfer<float>(m_cuda, m_log, m_evtCancel, "vgg19", strModelDesc, rgWeights, false, MyCaffe.param.SolverParameter.SolverType.LBFGS, 1.5);
            }
            catch (Exception excpt)
            {
                return excpt.Message;
            }

            return null;
        }

        /// <summary>
        /// Run the neural style transfer for a specified number of iterations.
        /// </summary>
        /// <param name="nIterations">Specifies the number of style transfer iterations to run.</param>
        /// <param name="bmpStyle">Specifies the style image to use.</param>
        /// <param name="bmpContent">Specifies the content image to use.</param>
        /// <returns>The resulting image which has the style applied to the content is returned.</returns>
        public Bitmap Run(int nIterations, Bitmap bmpStyle, Bitmap bmpContent)
        {
            return m_neuralStyle.Process(bmpStyle, bmpContent, nIterations);
        }

        /// <summary>
        /// Output from the training and testing operations will go to this even.
        /// </summary>
        /// <remarks>
        /// This output is sent to the Visual Studio Debug Output window. 
        /// </remarks>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the log arguments such as the message and progress.</param>
        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            Trace.WriteLine("(" + e.Progress.ToString("P") + ") " + e.Message);
        }

        /// <summary>
        /// Run the Neural Style Transfer sample.
        /// </summary>
        /// <param name="ctrlParent">Specifies the parent control.</param>
        /// <param name="evtCancel">Specifies the CancelEvent used to abort the style transfer.</param>
        /// <param name="log">Optionally, specifies an external log to use.</param>
        public static void RunSample(Control ctrlParent, CancelEvent evtCancel = null, Log log = null)
        {
            if (evtCancel != null)
                evtCancel.Reset();

            // Setup the MyCaffe output log.
            if (log == null)
                log = new Log("Test");

            log.OnWriteLine += Log_OnWriteLine;

            // Start training the gym.
            Task task = Task.Factory.StartNew(new Action<object>(run), new Tuple<Log, CancelEvent, Control>(log, evtCancel, ctrlParent));
        }

        private static void run(object obj)
        {
            Tuple<Log, CancelEvent, Control> args = obj as Tuple<Log, CancelEvent, Control>;
            MyCaffeNeuralStyleSample sample = new MyCaffeNeuralStyleSample(args.Item1);

            // Use the cancel event passed in.
            if (args.Item2 != null)
            {
                args.Item2.Reset();
                sample.m_evtCancel.AddCancelOverride(args.Item2);
            }

            // Initialize and run the model for 200 iterations.
            string strErr = sample.Initialize(args.Item3);
            if (!string.IsNullOrEmpty(strErr))
            {
                string strDone = "ERROR: " + strErr;
                args.Item3.Invoke(new fnDone(done), strDone, null);
                return;
            }
            
            string strStyleImg = sample.StyleImagePath;
            string strContentImg = sample.ContentImagePath;
            string strResultImg = sample.ResultImagePath;
            Bitmap bmpStyle = new Bitmap(strStyleImg);
            Bitmap bmpContent = new Bitmap(strContentImg);
            Bitmap bmpResult = sample.Run(200, bmpStyle, bmpContent);

            if (sample.m_evtCancel.WaitOne(0))
            {
                string strDone = "Aborted the Neural Style Transfer!";
                args.Item3.Invoke(new fnDone(done), strDone, null);
            }
            else if (bmpResult != null)
            {
                if (File.Exists(strResultImg))
                    File.Delete(strResultImg);

                bmpResult.Save(strResultImg);

                string strDone = "Resulting style image located at: '" + strResultImg + "'";
                args.Item3.Invoke(new fnDone(done), strDone, strResultImg);
            }
            else
            {
                string strDone = "The Neural Style Transfer failed to create an image!";
                args.Item3.Invoke(new fnDone(done), strDone, null);
            }

            if (bmpResult != null)
                bmpResult.Dispose();

            bmpStyle.Dispose();
            bmpContent.Dispose();
            sample.Dispose();
        }

        private static void done(string strResult, string strImgFile)
        {
            if (strImgFile == null)
            {
                MessageBox.Show(strResult, "Result Output", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (MessageBox.Show(strResult + "  Do you want to see the image?", "Result Output", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = strImgFile;
                    p.Start();
                }
            }
        }
    }
}
