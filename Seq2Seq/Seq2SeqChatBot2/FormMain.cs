using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.layers;
using MyCaffe.layers.beta;
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
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Seq2SeqChatBot
{
    /// <summary>
    /// The FormMain class manages the main window of the application.
    /// </summary>
    public partial class FormMain : Form
    {
        bool m_bStopping = false;
        Log m_log = new Log("Seq2Seq Image");
        CancelEvent m_evtCancel = new CancelEvent();
        MyCaffeControl<float> m_mycaffe = null;
        Model m_model = new Model();
        string m_strOutputPath = "";
        Stopwatch m_sw = new Stopwatch();
        PlotCollection m_plotsSequenceLoss = new PlotCollection("Sequence Training");
        PlotCollection m_plotsSequenceAccuracyTest = new PlotCollection("Sequence Testing");
        PlotCollection m_plotsSequenceAccuracyTrain = new PlotCollection("Sequence Training");
        List<ConfigurationTargetLine> m_rgZeroLine = new List<ConfigurationTargetLine>();
        InputData m_input = null;
        Blob<float> m_blobProbs = null;
        Blob<float> m_blobScale = null;
        int m_nCorrectCount = 0;
        List<float> m_rgAccuracyTraining = new List<float>();
        List<float> m_rgAccuracyTesting = new List<float>();
        float m_fTotalCost = 0;
        int m_nTotalIter1 = 0;
        int m_nTotalSequences = 0;
        int m_nTotalEpochs = 0;

        /// <summary>
        /// The setstatus delegate is used to output text to the status window
        /// and update the progress.
        /// </summary>
        /// <param name="e"></param>
        public delegate void fnSetStatus(LogArg e);

        /// <summary>
        /// The constructor.
        /// </summary>
        public FormMain()
        {
            m_strOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            m_sw.Start();

            ConfigurationTargetLine zeroLine = new ConfigurationTargetLine(0, Color.Gray);
            m_rgZeroLine.Add(zeroLine);

            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            m_log.OnWriteLine += Log_OnWriteLine;

            edtIterations.Text = Properties.Settings.Default.Iterations.ToString();
            edtBatch.Text = Properties.Settings.Default.Batch.ToString();
            edtHidden.Text = Properties.Settings.Default.Hidden.ToString();
            edtWordSize.Text = Properties.Settings.Default.WordSize.ToString();

            edtInputTextFile.Text = AssemblyDirectory + "\\human_text.txt";
            edtTargetTextFile.Text = AssemblyDirectory + "\\robot_text.txt";
            edtInput.Select();
        }

        private void Log_OnWriteLine(object sender, LogArg e)
        {
            Invoke(new fnSetStatus(setStatus), e);
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

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timerUI_Tick(object sender, EventArgs e)
        {
            m_log.Enable = btnEnableVerboseOutput.Checked;
            btnRun.Enabled = !m_bw.IsBusy;
            btnTrain.Enabled = !m_bw.IsBusy;
            btnStop.Enabled = m_bw.IsBusy && !m_bStopping;
            btnDeleteWeights.Enabled = !m_bw.IsBusy;

            bool bSelectInput = false;
            if (!splitContainer2.Panel1.Enabled && !m_bw.IsBusy)
                bSelectInput = true;

            splitContainer2.Panel1.Enabled = !m_bw.IsBusy;

            if (bSelectInput)
                edtInput.Select();
        }

        private InputData getInput(InputData.OPERATION op)
        {
            InputData input = null;

            try
            {
                input = new InputData();
                input.SetData(op, edtInputTextFile.Text, edtTargetTextFile.Text, edtIterations.Text, edtInput.Text, edtBatch.Text, edtHidden.Text, edtWordSize.Text, edtLearningRate.Text);
                edtInput.Text = "";

                if (input.HiddenSize != Properties.Settings.Default.Hidden ||
                    input.WordSize != Properties.Settings.Default.WordSize)
                {
                    DialogResult res = MessageBox.Show("The hidden size and/or word size have changed which requires deleting any previously trained weights - do you want to delete the weights?", "Sizing Change", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                    if (res == DialogResult.Cancel)
                        return null;

                    if (res == DialogResult.Yes)
                        btnDeleteWeights_Click(this, new EventArgs());
                    else
                        m_log.WriteLine("The weights were not deleted - sizing errors may occur.");
                }

                Properties.Settings.Default.Hidden = input.HiddenSize;
                Properties.Settings.Default.WordSize = input.WordSize;
                Properties.Settings.Default.Save();

                for (int i = 0; i < input.EpochSize; i++)
                {
                    m_rgAccuracyTesting.Add(0);
                    m_rgAccuracyTraining.Add(0);
                }

                return input;
            }
            catch (Exception excpt)
            {
                setStatus("ERROR: " + excpt.Message);
                return null;
            }
        }

        /// <summary>
        /// Train the model.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void btnTrain_Click(object sender, EventArgs e)
        {
            m_evtCancel.Reset();
            InputData input = getInput(InputData.OPERATION.TRAIN);

            if (input != null)
                m_bw.RunWorkerAsync(input);
        }

        /// <summary>
        /// Run the trained model.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void btnRun_Click(object sender, EventArgs e)
        {
            m_evtCancel.Reset();
            InputData input = getInput(InputData.OPERATION.RUN);

            if (input != null)
                m_bw.RunWorkerAsync(input);
        }

        /// <summary>
        /// Stop training or running.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            m_evtCancel.Set();
            m_bw.CancelAsync();
            m_bStopping = true;
        }

        /// <summary>
        /// Delete all saved weights.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void btnDeleteWeights_Click(object sender, EventArgs e)
        {
            int nHidden = m_model.Hidden;
            int.TryParse(edtHidden.Text, out nHidden);

            clearWeights("sequence", nHidden);
            clearWeights("sequence.run", nHidden);
            setStatus("All weights are cleared.");
        }

        /// <summary>
        /// The DoWork thread is the main tread used to train or run the model depending on the operation selected.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void m_bw_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            m_input = e.Argument as InputData;
            SettingsCaffe s = new SettingsCaffe();
            s.ImageDbLoadMethod = IMAGEDB_LOAD_METHOD.LOAD_ALL;

            try
            {
                m_model.Batch = m_input.Batch;
                m_mycaffe = new MyCaffeControl<float>(s, m_log, m_evtCancel);

                // Train the model.
                if (m_input.Operation == InputData.OPERATION.TRAIN)
                {
                    m_model.Iterations = (int)((m_input.Epochs * 7000) / m_model.Batch);
                    m_log.WriteLine("Training for " + m_input.Epochs.ToString() + " epochs (" + m_model.Iterations.ToString("N0") + " iterations).", true);
                    m_log.WriteLine("INFO: " + m_model.Iterations.ToString("N0") + " iterations.", true);
                    m_log.WriteLine("Using hidden = " + m_input.HiddenSize.ToString() + ", and word size = " + m_input.WordSize.ToString() + ".", true);

                    // Load the Seq2Seq training model.
                    NetParameter netParam = m_model.CreateModel(m_input.InputFileName, m_input.TargetFileName, m_input.HiddenSize, m_input.WordSize);
                    string strModel = netParam.ToProto("root").ToString();
                    SolverParameter solverParam = m_model.CreateSolver(m_input.LearningRate);
                    string strSolver = solverParam.ToProto("root").ToString();
                    byte[] rgWts = loadWeights("sequence");

                    m_mycaffe.OnTrainingIteration += m_mycaffe_OnTrainingIteration;
                    m_mycaffe.OnTestingIteration += m_mycaffe_OnTestingIteration;
                    m_mycaffe.LoadLite(Phase.TRAIN, strSolver, strModel, rgWts, false, false);

                    m_blobProbs = new Blob<float>(m_mycaffe.Cuda, m_mycaffe.Log);
                    m_blobScale = new Blob<float>(m_mycaffe.Cuda, m_mycaffe.Log);

                    MemoryLossLayer<float> lossLayerTraining = m_mycaffe.GetInternalNet(Phase.TRAIN).FindLayer(LayerParameter.LayerType.MEMORY_LOSS, "loss") as MemoryLossLayer<float>;
                    if(lossLayerTraining != null)
                        lossLayerTraining.OnGetLoss += LossLayer_OnGetLossTraining;
                    MemoryLossLayer<float> lossLayerTesting = m_mycaffe.GetInternalNet(Phase.TEST).FindLayer(LayerParameter.LayerType.MEMORY_LOSS, "loss") as MemoryLossLayer<float>;
                    if (lossLayerTesting != null)
                        lossLayerTesting.OnGetLoss += LossLayer_OnGetLossTesting;

                    TextDataLayer<float> dataLayerTraining = m_mycaffe.GetInternalNet(Phase.TRAIN).FindLayer(LayerParameter.LayerType.TEXT_DATA, "data") as TextDataLayer<float>;
                    if (dataLayerTraining != null)
                        dataLayerTraining.OnGetData += DataLayerTraining_OnGetDataTraining;

                    // Train the Seq2Seq model.
                    m_plotsSequenceLoss = new PlotCollection("Sequence Loss");
                    m_plotsSequenceAccuracyTest = new PlotCollection("Sequence Accuracy Test");
                    m_plotsSequenceAccuracyTrain = new PlotCollection("Sequence Accuracy Train");
                    m_mycaffe.Train(m_model.Iterations);
                    saveWeights("sequence", m_mycaffe);
                }

                // Run a trained model.
                else
                {
                    NetParameter netParam = m_model.CreateModel(m_input.InputFileName, m_input.TargetFileName, m_input.HiddenSize, m_input.WordSize, Phase.RUN);
                    string strModel = netParam.ToProto("root").ToString();
                    byte[] rgWts = loadWeights("sequence");

                    int nN = m_model.TimeSteps;
                    m_mycaffe.LoadToRun(strModel, rgWts, new BlobShape(new List<int>() { nN, 1, 1, 1 }), null, null, false, false);

                    m_blobProbs = new Blob<float>(m_mycaffe.Cuda, m_mycaffe.Log);
                    m_blobScale = new Blob<float>(m_mycaffe.Cuda, m_mycaffe.Log);

                    runModel(m_mycaffe, bw, edtInput.Text);
                }
            }
            catch (Exception excpt)
            {
                throw excpt;
            }
            finally
            {
                // Cleanup.
                if (m_mycaffe != null)
                {
                    m_mycaffe.Dispose();
                    m_mycaffe = null;
                }
            }
        }

        private void DataLayerTraining_OnGetDataTraining(object sender, OnGetDataArgs e)
        {
            if (e.IterationInfo.NewSequence)
            {
                if (e.IterationInfo.OutputCount > 0)
                {
                    float fAccuracy = (float)m_nCorrectCount / e.IterationInfo.OutputCount;
                    m_rgAccuracyTraining.Add(fAccuracy);
                    m_rgAccuracyTraining.RemoveAt(0);
                    m_nCorrectCount = 0;
                }

                m_nTotalSequences++;

                if (e.IterationInfo.NewEpoch)
                    m_nTotalEpochs++;
            }
        }

        private void softmax_fwd(Blob<float> blobBottom, Blob<float> blobClip, Blob<float> blobScale, Blob<float> blobTop, int nAxis)
        {
            int nCount = blobBottom.count();
            int nOuterNum = blobBottom.count(0, nAxis);
            int nInnerNum = blobBottom.count(nAxis + 1);
            int nChannels = blobTop.shape(nAxis);
            long hBottomData = blobBottom.gpu_data;
            long hTopData = blobTop.mutable_gpu_data;
            long hScaleData = blobScale.mutable_gpu_data;
            CudaDnn<float> cuda = m_mycaffe.Cuda;

            cuda.copy(nCount, hBottomData, hTopData);

            // Apply clip.
            if (blobClip != null)
                cuda.channel_scale(nCount, blobTop.num, blobTop.channels, blobTop.count(2), blobTop.gpu_data, blobClip.gpu_data, blobTop.mutable_gpu_data);

            // We need to subtract the max to avoid numerical issues, compute the exp
            // and then normalize.
            // compute max.
            cuda.channel_max(nOuterNum * nInnerNum, nOuterNum, nChannels, nInnerNum, hTopData, hScaleData);

            // subtract
            cuda.channel_sub(nCount, nOuterNum, nChannels, nInnerNum, hScaleData, hTopData);

            // exponentiate
            cuda.exp(nCount, hTopData, hTopData);

            // Apply clip to remove 1's.
            if (blobClip != null)
                cuda.channel_scale(nCount, blobTop.num, blobTop.channels, blobTop.count(2), blobTop.gpu_data, blobClip.gpu_data, blobTop.mutable_gpu_data);

            // Sum after exp
            cuda.channel_sum(nOuterNum * nInnerNum, nOuterNum, nChannels, nInnerNum, hTopData, hScaleData);

            // divide
            cuda.channel_div(nCount, nOuterNum, nChannels, nInnerNum, hScaleData, hTopData);

            // Denan for divide by zero.
            cuda.denan(nCount, blobTop.mutable_gpu_data, 0);
        }

        /// <summary>
        /// Calculate the loss when training.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void LossLayer_OnGetLossTraining(object sender, MemoryLossLayerGetLossArgs<float> e)
        {
            Phase phase = (e.Tag == null) ? Phase.TRAIN : (Phase)e.Tag;

            Blob<float> btm = e.Bottom[0];
            Blob<float> blobTarget = e.Bottom[1];
            CudaDnn<float> cuda = m_mycaffe.Cuda;
            Net<float> net = m_mycaffe.GetInternalNet(Phase.TRAIN);

            int nIxTarget = (int)blobTarget.GetData(0);

            m_blobProbs.ReshapeLike(btm);
            m_blobScale.ReshapeLike(btm);
            softmax_fwd(btm, null, m_blobScale, m_blobProbs, 2);

            int nCount = btm.count(2);
            cuda.copy(nCount, m_blobProbs.gpu_data, btm.mutable_gpu_diff);

            long lPos;
            cuda.max(nCount, btm.gpu_data, out lPos);

            int nCorrectCount = 0;
            if ((int)lPos == nIxTarget)
                nCorrectCount++;

            float fData = btm.GetDiff(nIxTarget);
            e.Loss += (-(float)Math.Log(fData));

            if (phase == Phase.TRAIN)
            {
                fData -= 1;
                btm.SetDiff(fData, nIxTarget);
            }

            m_nCorrectCount += nCorrectCount;

            e.EnableLossUpdate = false;
        }

        /// <summary>
        /// Calculate the loss when testing.
        /// </summary>
        /// <param name="sender">Specifies the sender</param>
        /// <param name="e">specifies the arguments.</param>
        private void LossLayer_OnGetLossTesting(object sender, MemoryLossLayerGetLossArgs<float> e)
        {
            e.Tag = Phase.TEST;
            LossLayer_OnGetLossTraining(sender, e);
            e.Tag = null;
        }

        /// <summary>
        /// Load the data into the model.
        /// </summary>
        /// <param name="strInput">Specifies the encoder input sentence.</param>
        /// <param name="nIxInput">Specifies the decoder current input word index.</param>
        private BlobCollection<float> loadData(string strInput, int nIxInput)
        {
            Net<float> net = m_mycaffe.GetInternalNet(Phase.RUN);
            TextDataLayer<float> layer = net.FindLayer("TextData", "data") as TextDataLayer<float>;
            Blob<float> blobData = net.FindBlob("data");
            Blob<float> blobDatar = net.FindBlob("datar");
            Blob<float> blobClipE = net.FindBlob("clipE");
            Blob<float> blobDecInput = net.FindBlob("dec_input");
            BlobCollection<float> colBtm = new BlobCollection<float>();

            colBtm.Add(blobDecInput);
            colBtm.Add(blobData);
            colBtm.Add(blobDatar);
            colBtm.Add(blobClipE);

            layer.PreProcessInput(strInput, nIxInput, colBtm);

            return colBtm;
        }

        /// <summary>
        /// Run the trained model.  
        /// </summary>
        /// <param name="mycaffe">Specifies the mycaffe instance running the sequence run model.</param>
        /// <param name="bw">Specifies the background worker.</param>
        /// <param name="strInput">Specifies the input data to run the model on.</param>
        private void runModel(MyCaffeControl<float> mycaffe, BackgroundWorker bw, string strInput)
        {
            Net<float> net = m_mycaffe.GetInternalNet(Phase.RUN);
            TextDataLayer<float> layer = net.FindLayer("TextData", "data") as TextDataLayer<float>;           
            int nDecInputLayerIdx = net.layer_index_by_name("dec_input_embed");
            int nIxInput = 1;

            try
            {
                BlobCollection<float> colBtm = loadData(strInput, nIxInput);

                double dfLoss;
                BlobCollection<float> colTop = net.Forward(colBtm, out dfLoss);

                Tuple<string, int> res = layer.PostProcessOutput(colTop[0]);
                int nCount = 0;

                nIxInput = res.Item2;
                string strWord = res.Item1;
                string strOut = "";

                while (nIxInput != 0 && strWord.Length > 0 && nCount < 80)
                {
                    strOut += strWord + " ";

                    colBtm = loadData(strInput, nIxInput);
                    colTop = net.Forward(colBtm, out dfLoss);

                    res = layer.PostProcessOutput(colTop[0]);

                    nIxInput = res.Item2;
                    strWord = res.Item1;

                    nCount++;
                }

                m_log.WriteLine("Robot: " + strOut.Trim(), true);
            }
            catch (Exception excpt)
            {
                m_log.WriteLine("Robot: " + excpt.Message);
            }
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
                m_log.WriteLine("Seq2Seq Epoch " + m_nTotalEpochs.ToString() + " Sequence " + m_nTotalSequences.ToString() + " Iteration " + e.Iteration.ToString() + " of " + m_model.Iterations.ToString() + ", loss = " + e.SmoothedLoss.ToString(), true);
                m_sw.Restart();

                m_fTotalCost += (float)e.SmoothedLoss;
                m_nTotalIter1++;
                float fLoss = m_fTotalCost / m_nTotalIter1;

                m_plotsSequenceLoss.Add(m_nTotalSequences, fLoss);
                if (m_plotsSequenceLoss.Count > 2000)
                    m_plotsSequenceLoss.RemoveAt(0);

                Image img = SimpleGraphingControl.QuickRender(m_plotsSequenceLoss, pbImageLoss.Width, pbImageLoss.Height, false, null, null, true, m_rgZeroLine);
                m_bw.ReportProgress(1, new Tuple<Image, int>(img, 0));
            }
        }

        /// <summary>
        /// Called on each testing iteration of the sequence model.
        /// </summary>
        /// <param name="sender">Specifies the event sender.</param>
        /// <param name="e">Specifies the event args.</param>
        private void m_mycaffe_OnTestingIteration(object sender, TestingIterationArgs<float> e)
        {
            float fAccuracy = m_rgAccuracyTraining.Average();
            m_plotsSequenceAccuracyTrain.Add(m_nTotalSequences, fAccuracy * 100);
            if (m_plotsSequenceAccuracyTrain.Count > 100)
                m_plotsSequenceAccuracyTrain.RemoveAt(0);

            m_rgAccuracyTesting.Add((float)e.Accuracy);
            m_rgAccuracyTesting.RemoveAt(0);
            fAccuracy = m_rgAccuracyTesting.Average();

            m_plotsSequenceAccuracyTest.Add(m_nTotalSequences, fAccuracy * 100);
            if (m_plotsSequenceAccuracyTest.Count > 100)
                m_plotsSequenceAccuracyTest.RemoveAt(0);

            PlotCollectionSet set = new PlotCollectionSet();
            set.Add(m_plotsSequenceAccuracyTrain);
            set.Add(m_plotsSequenceAccuracyTest);

            Image img = SimpleGraphingControl.QuickRender(set, pbImageAccuracy.Width, pbImageAccuracy.Height, false, null, null, false, m_rgZeroLine);
            m_bw.ReportProgress(1, new Tuple<Image, int>(img, 1));
        }

        private void m_bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Tuple<Image, int> res = e.UserState as Tuple<Image, int>;
            if (res == null)
                return;

            Image img = res.Item1;
            int nDst = res.Item2;

            if (img != null)
            {
                if (nDst == 0)
                    pbImageLoss.Image = img;
                else
                    pbImageAccuracy.Image = img;
            }
        }

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

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.WindowsShutDown)
            {
                if (m_bw.IsBusy)
                {
                    MessageBox.Show("You must stop the running operation first!", "Operation Running", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    e.Cancel = true;
                    return;
                }
            }

            if (m_mycaffe != null)
                m_mycaffe.Dispose();

            if (m_input != null)
                Properties.Settings.Default.Iterations = m_input.Epochs;

            Properties.Settings.Default.Batch = m_model.Batch;
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Sets the LogArg info in the status and progress controls.
        /// </summary>
        /// <param name="e">Specifies the log args.</param>
        private void setStatus(LogArg e)
        {
            setStatus(e.Message);

            int nPos = e.Message.IndexOf("INFO:");
            if (nPos == 0)
            {
                string str = e.Message.Substring(5);

                nPos = str.IndexOf("Accuracy = ");
                if (nPos >= 0)
                    lblLastAccuracy.Text = str.Trim();
                else
                    lblIterations.Text = "(" + str.Trim() + ")";
            }

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
            ListViewItem lvi = new ListViewItem(str);

            ListViewEx lv = lvStatus;

            if (str.StartsWith("You: ") || str.StartsWith("Robot: "))
                lv = lvDiscussion;

            lv.Items.Add(lvi);
            lvi.EnsureVisible();

            while (lv.Items.Count > 2000)
            {
                lv.Items.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get the weight file name for the model.
        /// </summary>
        /// <param name="strTag">Specifies a special tag used to designate the model file name.</param>
        /// <param name="nHiddenOverride">Specifies the hidden override.</param>
        /// <returns>The file name is returned.</returns>
        private string getWeightFileName(string strTag = "", int? nHiddenOverride = null)
        {
            int nHidden = nHiddenOverride.GetValueOrDefault(m_model.Hidden);

            string strDir = m_strOutputPath + "\\MyCaffe-Samples\\Seq2SeqChatBot";
            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            return strDir + "\\" + strTag + ".weights_" + LayerParameter.LayerType.LSTM.ToString() + "_ATTN_" + m_model.Layers.ToString() + "_" + nHidden.ToString() + ".bin";
        }

        /// <summary>
        /// Save the weights to a file.
        /// </summary>
        /// <param name="strTag">Specifies the tag applied to the filename.</param>
        /// <param name="mycaffe">Specifies the instance of mycaffe whos weights are to be saved.</param>
        private void saveWeights(string strTag, MyCaffeControl<float> mycaffe)
        {
            string strFile = getWeightFileName(strTag);

            byte[] rgWts = mycaffe.GetWeights();

            using (FileStream fs = File.Create(strFile))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(rgWts);
            }
        }

        /// <summary>
        /// Load the weights from file into a byte array.
        /// </summary>
        /// <param name="strTag">Specifies the tag applied to the filename.</param>
        /// <returns>The byte array of weights is returned.</returns>
        private byte[] loadWeights(string strTag)
        {
            string strFile = getWeightFileName(strTag);

            if (!File.Exists(strFile))
                return null;

            using (FileStream fs = File.OpenRead(strFile))
            using (BinaryReader br = new BinaryReader(fs))
            {
                byte[] rgWts = new byte[fs.Length];
                br.Read(rgWts, 0, (int)fs.Length);
                return rgWts;
            }
        }

        /// <summary>
        /// Delete the weights file.
        /// </summary>
        /// <param name="strTag">Specifies the identifying tag for the file.</param>
        /// <param name="nHiddenOverride">Specifies the hidden size override.</param>
        private void clearWeights(string strTag, int? nHiddenOverride = null)
        {
            string strFile = getWeightFileName(strTag, nHiddenOverride);
            if (File.Exists(strFile))
                File.Delete(strFile);
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
                double fAve = rgfVal.Sum() / nIterations;

                plots.Add(plotsSrc[i].X, fAve, bActive);
            }

            return plots;
        }

        private void btnBrowseInputTextFile_Click(object sender, EventArgs e)
        {
            openFileDialogTxt.Title = "Select the input text file.";
            if (openFileDialogTxt.ShowDialog() == DialogResult.OK)
                edtInputTextFile.Text = openFileDialogTxt.FileName;
        }

        private void btnBrowseTargetTextFile_Click(object sender, EventArgs e)
        {
            openFileDialogTxt.Title = "Select the target text file.";
            if (openFileDialogTxt.ShowDialog() == DialogResult.OK)
                edtTargetTextFile.Text = openFileDialogTxt.FileName;
        }

        private void lvStatus_Resize(object sender, EventArgs e)
        {
            lvStatus.Columns[0].Width = lvStatus.Width - 20;
        }

        private void edtInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                m_log.WriteLine("You: " + edtInput.Text, true);
                btnRun_Click(sender, new EventArgs());
                edtInput.Select();
            }
        }

        private void edtHidden_TextChanged(object sender, EventArgs e)
        {
            int nVal;

            if (int.TryParse(edtHidden.Text, out nVal))
                edtWordSize.Text = (nVal * 2).ToString();
        }

        private void btnSetDefaults_Click(object sender, EventArgs e)
        {
            edtIterations.Text = "300";
            edtBatch.Text = "1";
            edtHidden.Text = "128";
            edtLearningRate.Text = "0.001";
        }
    }
}
