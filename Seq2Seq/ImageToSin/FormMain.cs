using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.basecode.descriptors;
using MyCaffe.common;
using MyCaffe.db.image;
using MyCaffe.layers;
using MyCaffe.param;
using SimpleGraphing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageToSin
{
    public partial class FormMain : Form
    {
        bool m_bStopping = false;
        Log m_log = new Log("Seq2Seq Image");
        CancelEvent m_evtCancel = new CancelEvent();
        MyCaffeControl<float> m_mycaffeInput = null;
        MyCaffeControl<float> m_mycaffe = null;
        IXImageDatabaseBase m_imgDb = null;
        Model m_model = new Model();
        string m_strOutputPath = "";
        DatasetDescriptor m_ds;
        int m_nLabelSeq = 0;
        Stopwatch m_sw = new Stopwatch();
        PlotCollection m_plotsInputLoss = new PlotCollection("Input Training");
        PlotCollection m_plotsSequenceLoss = new PlotCollection("Sequence Training");
        List<ConfigurationTargetLine> m_rgZeroLine = new List<ConfigurationTargetLine>();
        string m_strInputOutputBlobName = "ip1";
        List<Tuple<Image, int>> m_rgInputImg = new List<Tuple<Image, int>>();
        AutoResetEvent m_evtForceError = new AutoResetEvent(false);
        OPERATION m_operation = OPERATION.TRAIN;

        /// <summary>
        /// The setstatus delegate is used to output text to the status window
        /// and update the progress.
        /// </summary>
        /// <param name="e"></param>
        public delegate void fnSetStatus(LogArg e);

        /// <summary>
        /// Defines the OPERATION to run.
        /// </summary>
        public enum OPERATION
        {
            /// <summary>
            /// Specifies to run a training operation in the BackgroundWorker thread.
            /// </summary>
            TRAIN,
            /// <summary>
            /// Specifies to run a 'run' operation in the BackgroundWorker thread.
            /// </summary>
            RUN
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        public FormMain()
        {
            copyCudaDnnDll();
            m_strOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            m_sw.Start();

            ConfigurationTargetLine zeroLine = new ConfigurationTargetLine(0, Color.Gray);
            m_rgZeroLine.Add(zeroLine);

            InitializeComponent();
        }

        private void copyCudaDnnDll()
        {
            string strDll = AssemblyDirectory + "\\CudaDnnDll.11.6.dll";

            if (!File.Exists(strDll))
            {
                string strTarget = "MyCaffe-Samples";
                int nPos = strDll.IndexOf(strTarget);
                if (nPos == -1)
                    return;

                string strSrc = strDll.Substring(0, nPos + strTarget.Length);
                strSrc += "\\Seq2Seq\\packages\\MyCaffe.1.11.6.38\\nativeBinaries\\x64";

                File.Copy(strSrc + "\\CudaDnnDll.11.6.dll", strDll);
            }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// The load event occurs when the application is loaded.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void FormMain_Load(object sender, EventArgs e)
        {
            m_log.OnWriteLine += Log_OnWriteLine;
        }

        /// <summary>
        /// The close event closes the application.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// The train event start training the model.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void btnTrain_Click(object sender, EventArgs e)
        {
            m_evtCancel.Reset();
            m_bw.RunWorkerAsync(OPERATION.TRAIN);
        }

        /// <summary>
        /// The run event starts running the model.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            m_evtCancel.Reset();
            m_bw.RunWorkerAsync(OPERATION.RUN);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            m_evtCancel.Set();
            m_bw.CancelAsync();
            m_bStopping = true;
        }

        /// <summary>
        /// The timer event is used to update the user interface based
        /// on the state of the BackgroundWorker.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void timerUI_Tick(object sender, EventArgs e)
        {
            btnRun.Enabled = !m_bw.IsBusy;
            btnTrain.Enabled = !m_bw.IsBusy;
            btnStop.Enabled = m_bw.IsBusy && !m_bStopping;
            btnDeleteWeights.Enabled = !m_bw.IsBusy;
            btnForceError.Enabled = (m_operation == OPERATION.RUN && m_bw.IsBusy);
        }

        /// <summary>
        /// The worker thread used to either train or run the models.
        /// </summary>
        /// <remarks>
        /// When training, first the input hand-written image model is trained
        /// using the LeNet model.
        /// 
        /// This input mode is then run in the onTrainingStart event to get the
        /// detected hand written character representation.  The outputs of layer
        /// 'ip1' from the input model are then fed as input to the sequence
        /// model which is then trained to encode the 'ip1' input data with one
        /// lstm and then decoded with another which is then trained to detect
        /// a section of the Sin curve data.
        /// 
        /// When running, the first input model is run to get its 'ip1' representation,
        /// which is then fed into the sequence model to detect the section of the
        /// Sin curve.
        /// </remarks>
        /// <param name="sender">Specifies the sender of the event (e.g. the BackgroundWorker)</param>
        /// <param name="args">Specifies the event args.</param>
        private void m_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            OPERATION op = (OPERATION)e.Argument;
            SettingsCaffe s = new SettingsCaffe();
            s.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ALL;

            m_operation = op;
            m_mycaffe = new MyCaffeControl<float>(s, m_log, m_evtCancel);
            m_mycaffeInput = new MyCaffeControl<float>(s, m_log, m_evtCancel);
            m_imgDb = new MyCaffeImageDatabase2(m_log);

            // Load the image database.
            m_imgDb.InitializeWithDsName1(s, "MNIST");
            m_ds = m_imgDb.GetDatasetByName("MNIST");

            // Create the MNIST image detection model
            NetParameter netParamMnist = m_model.CreateMnistModel(m_ds);
            SolverParameter solverParamMnist = m_model.CreateMnistSolver();

            byte[] rgWts = loadWeights("input");
            m_mycaffeInput.Load(Phase.TRAIN, solverParamMnist.ToProto("root").ToString(), netParamMnist.ToProto("root").ToString(), rgWts, null, null, false, m_imgDb);
            Net<float> netTrain = m_mycaffeInput.GetInternalNet(Phase.TRAIN);
            Blob<float> input_ip = netTrain.FindBlob(m_strInputOutputBlobName); // input model's second to last output (includes relu)

            // Run the train or run operation.
            if (op == OPERATION.TRAIN)
            {
                // Train the MNIST model first.
                m_mycaffeInput.OnTrainingIteration += m_mycaffeInput_OnTrainingIteration;
                m_plotsInputLoss = new PlotCollection("Input Loss");
                m_mycaffeInput.Train(2000);
                saveWeights("input", m_mycaffeInput.GetWeights());

                // Load the Seq2Seq training model.
                NetParameter netParam = m_model.CreateModel(input_ip.channels, 10);
                string strModel = netParam.ToProto("root").ToString();
                SolverParameter solverParam = m_model.CreateSolver();
                rgWts = loadWeights("sequence");

                m_mycaffe.OnTrainingIteration += m_mycaffe_OnTrainingIteration;
                m_mycaffe.LoadLite(Phase.TRAIN, solverParam.ToProto("root").ToString(), netParam.ToProto("root").ToString(), rgWts, false, false);
                m_mycaffe.SetOnTrainingStartOverride(new EventHandler(onTrainingStart));

                // Train the Seq2Seq model.
                m_plotsSequenceLoss = new PlotCollection("Sequence Loss");
                m_mycaffe.Train(m_model.Iterations);
                saveWeights("sequence", m_mycaffe.GetWeights());
            }
            else
            {
                NetParameter netParam = m_model.CreateModel(input_ip.channels, 10, 1, 1);
                string strModel = netParam.ToProto("root").ToString();
                rgWts = loadWeights("sequence");

                int nN = 1;
                m_mycaffe.LoadToRun(netParam.ToProto("root").ToString(), rgWts, new BlobShape(new List<int>() { nN, 1, 1, 1 }), null, null, false, false);
                runModel(m_mycaffe, bw);
            }

            // Cleanup.
            m_mycaffe.Dispose();
            m_mycaffe = null;
            m_mycaffeInput.Dispose();
            m_mycaffeInput = null;
        }

        /// <summary>
        ///  This event is called by the Solver to get training data for this next training Step.  
        ///  Within this event, the data is loaded for the next training step.
        /// </summary>
        /// <param name="sender">Specifies the sender of the event (e.g. the solver)</param>
        /// <param name="args">n/a</param>
        private void onTrainingStart(object sender, EventArgs args)
        {
            Blob<float> blobData = m_mycaffe.GetInternalNet(Phase.TRAIN).FindBlob("data");
            Blob<float> blobLabel = m_mycaffe.GetInternalNet(Phase.TRAIN).FindBlob("label");
            Blob<float> blobClip1 = m_mycaffe.GetInternalNet(Phase.TRAIN).FindBlob("clip1");

            // Load a batch of data where:
            // 'data' contains a batch of 10D detected images in sequence by label.
            // 'label' contains a batch of 1D future signals in sequence.
            List<float> rgYb = new List<float>();
            List<float> rgFYb = new List<float>();

            for (int i = 0; i < m_model.Batch; i++)
            {
                for (int t = 0; t < m_model.TimeSteps; t++)
                {
                    // Get images one number at a time, in order by label, but randomly selected.
                    SimpleDatum sd = m_imgDb.QueryImage(m_ds.TrainingSource.ID, 0, null, IMGDB_IMAGE_SELECTION_METHOD.RANDOM, m_nLabelSeq);
                    m_mycaffeInput.Run(sd);

                    Net<float> inputNet = m_mycaffeInput.GetInternalNet(Phase.RUN);
                    Blob<float> input_ip = inputNet.FindBlob(m_strInputOutputBlobName);
                    float[] rgY1 = input_ip.mutable_cpu_data;

                    rgYb.AddRange(rgY1);
                    Dictionary<string, float[]> data = Signal.GenerateSample(1, m_nLabelSeq / 10.0f, 1, m_model.InputLabel, m_model.TimeSteps);
                    float[] rgFY1 = data["FY"];

                    // Add future steps corresponding to m_nLabelSeq time step;
                    rgFYb.AddRange(rgFY1);

                    m_nLabelSeq++;
                    if (m_nLabelSeq > 9)
                        m_nLabelSeq = 0;
                }
            }

            float[] rgY = SimpleDatum.Transpose(rgYb.ToArray(), blobData.channels, blobData.num, blobData.count(2)); // Transpose for Sequence Major ordering.
            blobData.mutable_cpu_data = rgY;

            float[] rgFY = SimpleDatum.Transpose(rgFYb.ToArray(), blobLabel.channels, blobLabel.num, blobLabel.count(2)); // Transpose for Sequence Major ordering.
            blobLabel.mutable_cpu_data = rgFY;

            blobClip1.SetData(1);
            blobClip1.SetData(0, 0, m_model.Batch);
        }

        /// <summary>
        /// Run the trained model.  When run each hand-written image is fed in sequence (by label,
        /// e.g. 0,1,2,...,9 through the model, yet images within each label are selected at random.
        /// </summary>
        /// <param name="mycaffe">Specifies the mycaffe instance running the sequence run model.</param>
        /// <param name="bw">Specifies the background worker.</param>
        private void runModel(MyCaffeControl<float> mycaffe, BackgroundWorker bw)
        {
            Random random = new Random((int)DateTime.Now.Ticks);
            // Get the internal RUN net and associated blobs.
            Net<float> net = m_mycaffe.GetInternalNet(Phase.RUN);
            Blob<float> blobData = net.FindBlob("data");
            Blob<float> blobClip1 = net.FindBlob("clip1");
            Blob<float> blobIp1 = net.FindBlob("ip1");
            List<float> rgPrediction = new List<float>();
            List<float> rgTarget = new List<float>();
            List<float> rgT = new List<float>();

            m_mycaffeInput.UpdateRunWeights();
            blobClip1.SetData(0);

            bool bForcedError = false;

            for (int i = 0; i < 100; i++)
            {
                if (m_evtCancel.WaitOne(0))
                    return;

                int nLabelSeq = m_nLabelSeq;
                if (m_evtForceError.WaitOne(0))
                {
                    nLabelSeq = random.Next(10);
                    bForcedError = true;
                }
                else
                {
                    bForcedError = false;
                }

                // Get images one number at a time, in order by label, but randomly selected.
                SimpleDatum sd = m_imgDb.QueryImage(m_ds.TrainingSource.ID, 0, null, IMGDB_IMAGE_SELECTION_METHOD.RANDOM, nLabelSeq);
                ResultCollection res = m_mycaffeInput.Run(sd);

                Net<float> inputNet = m_mycaffeInput.GetInternalNet(Phase.RUN);
                Blob<float> input_ip = inputNet.FindBlob(m_strInputOutputBlobName);
                Dictionary<string, float[]> data = Signal.GenerateSample(1, m_nLabelSeq / 10.0f, 1, m_model.InputLabel, m_model.TimeSteps);

                float[] rgFY1 = data["FY"];

                // Run the model.
                blobClip1.SetData(1);
                blobData.mutable_cpu_data = input_ip.mutable_cpu_data;
                net.Forward();
                rgPrediction.AddRange(blobIp1.mutable_cpu_data);

                // Graph and show the results.
                float[] rgFT = data["FT"];
                float[] rgFY = data["FY"];
                for (int j = 0; j < rgFT.Length; j++)
                {
                    rgT.Add(rgFT[j]);
                    rgTarget.Add(rgFY[j]);
                }

                while (rgTarget.Count * 5 > pbImage.Width)
                {
                    rgTarget.RemoveAt(0);
                    rgPrediction.RemoveAt(0);
                }

                // Plot the graph.
                PlotCollection plotsTarget = createPlots("Target", rgT.ToArray(), new List<float[]>() { rgTarget.ToArray() }, 0);
                PlotCollection plotsPrediction = createPlots("Predicted", rgT.ToArray(), new List<float[]>() { rgPrediction.ToArray() }, 0);
                PlotCollection plotsAvePrediction = createPlotsAve("Predicted SMA", plotsPrediction, 10);
                PlotCollectionSet set = new PlotCollectionSet(new List<PlotCollection>() { plotsTarget, plotsPrediction, plotsAvePrediction });

                // Create the graph image and display
                Image img = SimpleGraphingControl.QuickRender(set, pbImage.Width, pbImage.Height);
                img = drawInput(img, sd, res.DetectedLabel, bForcedError);

                bw.ReportProgress(0, img);
                Thread.Sleep(1000);

                m_nLabelSeq++;
                if (m_nLabelSeq == 10)
                    m_nLabelSeq = 0;
            }
        }

        /// <summary>
        /// Draw the input images.
        /// </summary>
        /// <param name="img">Specifies the background graph image.</param>
        /// <param name="sd">Specifies the current hand written character data.</param>
        /// <param name="nPredictedLabel">Specifies the predicted label.</param>
        /// <returns>The image with the input images drawn on it is returned.</returns>
        private Image drawInput(Image img, SimpleDatum sd, int nPredictedLabel, bool bForcedError)
        {
            Image imgInput = ImageData.GetImage(sd);
            m_rgInputImg.Add(new Tuple<Image, int>(imgInput, (sd.Label == nPredictedLabel) ? (bForcedError) ? 1 : 0 : -1));

            while (m_rgInputImg.Count * 50 > (pbImage.Width - 30))
            {
                m_rgInputImg.RemoveAt(0);
            }

            int nX = 20;
            int nY = pbImage.Height - 40;
            using (Graphics g = Graphics.FromImage(img))
            {
                for (int i = 0; i < m_rgInputImg.Count; i++)
                {
                    Image img1 = m_rgInputImg[i].Item1;
                    int nResult = m_rgInputImg[i].Item2;

                    if (nResult < 0)
                        g.FillRectangle(Brushes.Red, nX - 4, nY - 4, img1.Width + 8, img1.Height + 8);
                    else if (nResult > 0)
                        g.FillRectangle(Brushes.Yellow, nX - 4, nY - 4, img1.Width + 8, img1.Height + 8);

                    g.DrawImage(img1, nX, nY);
                    nX += 50;
                }
            }

            return img;
        }

        /// <summary>
        /// Called on each training iteration of the sequence model used to encode each detected hand written character
        /// and then decode the encoding into the proper section of the Sin curve.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void m_mycaffe_OnTrainingIteration(object sender, TrainingIterationArgs<float> e)
        {
            if (m_sw.Elapsed.TotalMilliseconds > 1000)
            {
                m_log.Progress = e.Iteration / (double)m_model.Iterations;
                m_log.WriteLine("Seq2Seq Iteration " + e.Iteration.ToString() + " of " + m_model.Iterations.ToString() + ", loss = " + e.SmoothedLoss.ToString());
                m_sw.Restart();

                m_plotsSequenceLoss.Add(e.Iteration, e.SmoothedLoss);
                Image img = SimpleGraphingControl.QuickRender(m_plotsSequenceLoss, pbImage.Width, pbImage.Height, false, null, null, true, m_rgZeroLine);
                m_bw.ReportProgress(1, img);
            }
        }

        /// <summary>
        /// Called on each training iteration of the input model used to detect each hand written character.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void m_mycaffeInput_OnTrainingIteration(object sender, TrainingIterationArgs<float> e)
        {
            if (m_sw.Elapsed.TotalMilliseconds > 1000)
            {
                m_log.Progress = e.Iteration / (double)m_model.Iterations;
                m_log.WriteLine("MNIST Iteration " + e.Iteration.ToString() + " of " + m_model.Iterations.ToString() + ", loss = " + e.SmoothedLoss.ToString());
                m_sw.Restart();

                m_plotsInputLoss.Add(e.Iteration, e.SmoothedLoss);
                Image img = SimpleGraphingControl.QuickRender(m_plotsInputLoss, pbImage.Width, pbImage.Height, false, null, null, true, m_rgZeroLine);
                m_bw.ReportProgress(1, img);
            }
        }

        private void Log_OnWriteLine(object sender, LogArg e)
        {
            Invoke(new fnSetStatus(setStatus), e);
        }

        /// <summary>
        /// Called on each status report from the worker thread.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void m_bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Image img = e.UserState as Image;

            if (img != null)
                pbImage.Image = img;
        }

        /// <summary>
        /// Called upon the completion of the worker thread.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void m_bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            m_bStopping = false;

            if (e.Error != null)
                setStatus("ERROR: " + e.Error.Message);
            else if (e.Cancelled)
                setStatus("ABORTED!");
            else
                setStatus("Completed.");
        }

        /// <summary>
        /// Handle the closing event.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (m_bw.IsBusy)
                {
                    MessageBox.Show("You must stop the running operation first!", "Operation Running", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    e.Cancel = true;
                    return;
                }
            }

            if (m_mycaffe != null)
                m_mycaffe.Dispose();

            if (m_mycaffeInput != null)
                m_mycaffeInput.Dispose();
        }

        /// <summary>
        /// Sets the LogArg info in the status and progress controls.
        /// </summary>
        /// <param name="e">Specifies the log args.</param>
        private void setStatus(LogArg e)
        {
            setStatus(e.Message);

            if (e.Progress <= 1)
            {
                lblProgress.Text = e.Progress.ToString("P");
                pbProgress.Value = (int)(100 * e.Progress);
            }
        }

        /// <summary>
        /// Set a line of status text in the status window.
        /// </summary>
        /// <param name="str">Specifies the status string to display.</param>
        private void setStatus(string str)
        {
            edtStatus.Text += Environment.NewLine;
            edtStatus.Text += str;
            edtStatus.SelectionLength = 0;
            edtStatus.SelectionStart = edtStatus.Text.Length;
            edtStatus.ScrollToCaret();
        }

        /// <summary>
        /// Get the weight file name for the model.
        /// </summary>
        /// <param name="strTag">Specifies a special tag used to designate the model file name.</param>
        /// <returns>The file name is returned.</returns>
        private string getWeightFileName(string strTag = "")
        {
            string strDir = m_strOutputPath + "\\MyCaffe-Samples\\Seq2SeqImageToSin";
            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            return strDir + "\\" + strTag + ".weights_" + LayerParameter.LayerType.LSTM.ToString() + "_" + m_model.LstmEngine.ToString() + "_" + m_model.Layers.ToString() + "_" + m_model.Hidden.ToString() + ".bin";
        }

        /// <summary>
        /// Save the model weights.
        /// </summary>
        /// <param name="strTag">Specifies an identifying tag.</param>
        /// <param name="rg">Specifies the weight data.</param>
        private void saveWeights(string strTag, byte[] rg)
        {
            string strFile = getWeightFileName(strTag);
            Console.WriteLine("Saving weights to '" + strFile + "'...");
            using (FileStream fs = File.Create(strFile))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(rg);
            }
        }

        /// <summary>
        /// Delete the weights file.
        /// </summary>
        /// <param name="strTag">Specifies the identifying tag for the file.</param>
        private void clearWeights(string strTag)
        {
            string strFile = getWeightFileName(strTag);
            if (File.Exists(strFile))
                File.Delete(strFile);
        }

        /// <summary>
        /// Load the weights from file.
        /// </summary>
        /// <param name="strTag">Specifies an identifying tag.</param>
        /// <returns>The weight data is returned.</returns>
        private byte[] loadWeights(string strTag)
        {
            string strFile = getWeightFileName(strTag);
            if (!File.Exists(strFile))
                return null;

            Console.WriteLine("Loading weights from '" + strFile + "'...");
            using (FileStream fs = File.Open(strFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                return br.ReadBytes((int)fs.Length);
            }
        }

        /// <summary>
        /// Create the PlotCollection for display on the graph.
        /// </summary>
        /// <param name="strName">Specifies the name of the plot.</param>
        /// <param name="rgT">Specifies the X axis data (e.g. the time sequence)</param>
        /// <param name="rgrgY">Specifies the Y data.</param>
        /// <param name="nYIdx">Specifies which of the Y data items are to be activated in the plot.</param>
        /// <returns>The filled PlotCollection is returned.</returns>
        private PlotCollection createPlots(string strName, float[] rgT, List<float[]> rgrgY, int nYIdx)
        {
            PlotCollection plots = new PlotCollection();
            plots.Name = strName;

            int nTidx = 0;

            for (int i = 0; i < rgrgY.Count; i++)
            {
                for (int j = 0; j < rgrgY[i].Length; j++)
                {
                    float fX = rgT[nTidx] * 100;
                    float fY = rgrgY[i][j];
                    bool bActive = (i == nYIdx) ? true : false;
                    plots.Add(fX, fY, bActive);
                    nTidx++;
                }
            }

            return plots;
        }

        /// <summary>
        /// Create a simple moving average of the input plot.
        /// </summary>
        /// <param name="strName">Specifies the name of the new plot.</param>
        /// <param name="plotsSrc">Specifies the source plots.</param>
        /// <param name="nIterations">Specifies the iterations over which to average.</param>
        /// <returns>The plot of average values is returned.</returns>
        private PlotCollection createPlotsAve(string strName, PlotCollection plotsSrc, int nIterations)
        {
            PlotCollection plots = new PlotCollection();
            plots.Name = strName + nIterations.ToString();
            List<float> rgfVal = new List<float>();

            for (int i = 0; i < plotsSrc.Count; i++)
            {
                rgfVal.Add(plotsSrc[i].Y);

                if (rgfVal.Count > nIterations)
                    rgfVal.RemoveAt(0);

                bool bActive = (rgfVal.Count == nIterations) ? true : false;
                float fAve = rgfVal.Sum() / nIterations;

                plots.Add(plotsSrc[i].X, fAve, bActive);
            }

            return plots;
        }

        /// <summary>
        /// Clear the weights.
        /// </summary>
        /// <param name="sender">Specifies the sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void btnDeleteWeights_Click(object sender, EventArgs e)
        {
            clearWeights("input");
            clearWeights("sequence");
            setStatus("All weights are cleared.");
        }

        private void btnForceError_Click(object sender, EventArgs e)
        {
            m_evtForceError.Set();
        }
    }
}
