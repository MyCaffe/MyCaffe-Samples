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

namespace SinCurve
{
    /// <summary>
    /// The Trainer is a complete rewrite of the Caffe-LSTM-Mini-Tutorial which demonstrates how to use an LSTM model to learn a generated Sin curve
    /// using a two LSTM Layer Seq2Seq model.
    /// </summary>
    /// <remarks>
    /// @see [Corvus/Caffe-LSTM-Mini-Tutorial](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial) open-source project distributed under
    /// the [GNU License](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial/blob/master/LICENSE).
    /// 
    /// NOTE: This sample requires MyCaffe version 0.11.2.55 or greater.
    /// </remarks>
    public class Trainer : IDisposable
    {
        Parameters m_param = new Parameters();
        Log m_log = new Log("Test");
        CancelEvent m_evtCancel = new CancelEvent();
        SettingsCaffe m_settings = new SettingsCaffe();
        MyCaffeControl<float> m_mycaffeTrain;
        MyCaffeControl<float> m_mycaffeRun;
        Stopwatch m_swTraining = new Stopwatch();
        PlotCollection m_plots = new PlotCollection();
        string m_strOutputPath;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="p">Specifies the parameters that define how to train and run the model.</param>
        public Trainer(Parameters p)
        {
            m_strOutputPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            m_param = p;

            if (p.Mode == Parameters.MODE.TRAIN)
            {
                m_mycaffeTrain = new MyCaffeControl<float>(m_settings, m_log, m_evtCancel);
                m_mycaffeTrain.OnTrainingIteration += m_mycaffe_OnTrainingIteration;
            }

            m_mycaffeRun = new MyCaffeControl<float>(m_settings, m_log, m_evtCancel);

            m_swTraining.Start();
        }

        /// <summary>
        /// Cleanup resources used.
        /// </summary>
        public void Dispose()
        {
            if (m_mycaffeTrain != null)
                m_mycaffeTrain.Dispose();

            if (m_mycaffeRun != null)
                m_mycaffeRun.Dispose();
        }

        /// <summary>
        /// Event called on each training iterations.
        /// </summary>
        /// <param name="sender">Specifies who sent the event.</param>
        /// <param name="e">Specifies the event parameters.</param>
        private void m_mycaffe_OnTrainingIteration(object sender, TrainingIterationArgs<float> e)
        {
            if (m_swTraining.Elapsed.TotalMilliseconds > 1000)
            {
                Console.WriteLine("Iteration = " + e.Iteration.ToString("N0") + " Loss = " + e.SmoothedLoss.ToString());
                m_swTraining.Restart();
            }

            if (e.Iteration % 30 == 0)
                m_plots.Add(e.Iteration, e.SmoothedLoss);
        }

        /// <summary>
        /// Train the model.
        /// </summary>
        /// <param name="bNewWts">Specifies whether to use new weights or load existing ones (if they exist).</param>
        public void Train(bool bNewWts)
        {
            if (m_mycaffeTrain == null)
                return;

            byte[] rgWts = null;

            if (!bNewWts)
                rgWts = loadWeights();

            if (rgWts == null)
                Console.WriteLine("Starting with new weights...");

            SolverParameter solver = createSolver();
            NetParameter model = createModel();

            string strModel = model.ToProto("root").ToString();
            Console.WriteLine("Using Train Model:");
            Console.WriteLine(strModel);
            Console.WriteLine("Starting training...");

            m_mycaffeTrain.LoadLite(Phase.TRAIN, solver.ToProto("root").ToString(), model.ToProto("root").ToString(), rgWts, false, false);
            m_mycaffeTrain.SetOnTrainingStartOverride(new EventHandler(onTrainingStart));
            m_mycaffeTrain.SetOnTestingStartOverride(new EventHandler(onTestingStart));

            // Set clockwork weights.
            if (m_param.LstmEngine != EngineParameter.Engine.CUDNN)
            {
                Net<float> net = m_mycaffeTrain.GetInternalNet(Phase.TRAIN);
                Blob<float> lstm1 = net.parameters[2];
                lstm1.SetData(1, m_param.Hidden, m_param.Hidden);
            }

            m_mycaffeTrain.Train(m_param.Iterations);
            saveLstmState(m_mycaffeTrain);

            Image img = SimpleGraphingControl.QuickRender(m_plots, 1000, 600);
            showImage(img, "training.png");
            saveWeights(m_mycaffeTrain.GetWeights());
        }

        /// <summary>
        ///  This event is called by the Solver to get training data for this next training Step.
        /// </summary>
        /// <param name="sender">Specifies the sender of the event (e.g. the solver)</param>
        /// <param name="args">n/a</param>
        private void onTrainingStart(object sender, EventArgs args)
        {
            Blob<float> blobData = m_mycaffeTrain.GetInternalNet(Phase.TRAIN).FindBlob("data");
            Blob<float> blobLabel = m_mycaffeTrain.GetInternalNet(Phase.TRAIN).FindBlob("label");
            Blob<float> blobClip1 = m_mycaffeTrain.GetInternalNet(Phase.TRAIN).FindBlob("clip1");
            Blob<float> blobClip2 = m_mycaffeTrain.GetInternalNet(Phase.TRAIN).FindBlob("clip2");

            if (blobClip2 != null)
                blobClip2.SetData(1);

            Dictionary<string, float[]> data = generateSample(null, null, m_param.Batch, m_param.Output, m_param.TimeSteps);

            float[] rgY = data["Y"];
            rgY = SimpleDatum.Transpose(rgY, blobData.channels, blobData.num, blobData.count(2)); // Transpose for Sequence Major ordering.
            blobData.mutable_cpu_data = rgY;

            float[] rgFY = data["FY"];
            rgFY = SimpleDatum.Transpose(rgFY, blobLabel.channels, blobLabel.num, blobData.count(2)); // Transpose for Sequence Major ordering.
            blobLabel.mutable_cpu_data = rgFY;

            blobClip1.SetData(1);
            blobClip1.SetData(0, 0, m_param.Batch);
        }

        /// <summary>
        ///  This event is called by the Solver to get testing data for this next training Step.
        /// </summary>
        /// <param name="sender">Specifies the sender of the event (e.g. the solver)</param>
        /// <param name="args">n/a</param>
        private void onTestingStart(object sender, EventArgs args)
        {
        }

        /// <summary>
        /// Run the trained model on the generated Sin curve.
        /// </summary>
        /// <returns>Returns <i>false</i> if no trained model found.</returns>
        public bool Run()
        {
            // Load the run net with the previous weights.
            byte[] rgWts = loadWeights();
            if (rgWts == null)
            {
                Console.WriteLine("You must first train the network!");
                return false;
            }

            // Crate the model used to run indefinitely
            NetParameter model = createModelInfiniteInput();

            string strModel = model.ToProto("root").ToString();
            Console.WriteLine("Using Run Model:");
            Console.WriteLine(strModel);

            // Load the model for running with the trained weights.
            int nN = 1;
            m_mycaffeRun.LoadToRun(strModel, rgWts, new BlobShape(new List<int>() { nN, 1, 1 }), null, null, false, false);

            // Load the previously saved LSTM state (hy and cy) along with the previously
            // trained weights.
            loadLstmState(m_mycaffeRun);

            // Get the internal RUN net and associated blobs.
            Net<float> net = m_mycaffeRun.GetInternalNet(Phase.RUN);
            Blob<float> blobData = net.FindBlob("data");
            Blob<float> blobClip = net.FindBlob("clip2");
            Blob<float> blobIp1 = net.FindBlob("ip1");

            int nBatch = 1;
            
            // Run on 3 different, randomly selected Sin curves.
            for (int i = 0; i < 3; i++)
            {
                // Create the Sin data.
                Dictionary<string, float[]> data = generateSample(i + 1.1337f, null, nBatch, m_param.Output, m_param.TimeSteps);
                List<float> rgPrediction = new List<float>();

                // Set the clip to 1 for we are continuing from the 
                // last training session and want start with the last
                // cy and hy states.
                blobClip.SetData(1);
                float[] rgY = data["Y"];
                float[] rgFY = data["FY"];

                // Run the model on the data up to number of 
                // time steps.
                for (int t = 0; t < m_param.TimeSteps; t++)
                {
                    blobData.SetData(rgY[t]);
                    net.Forward();
                    rgPrediction.Add(blobIp1.GetData(0));
                }

                // Run the model on the last prediction for
                // the number of predicted output steps.
                for (int t = 0; t < m_param.Output; t++)
                {
                    blobData.SetData(rgPrediction[rgPrediction.Count - 1]);
                    //blobData.SetData(rgFY[t]);
                    net.Forward();
                    rgPrediction.Add(blobIp1.GetData(0));
                }

                // Graph and show the resupts.
                List<float> rgT2 = new List<float>(data["T"]);
                rgT2.AddRange(data["FT"]);

                // Plot the graph.
                PlotCollection plotsY = createPlots("Y", rgT2.ToArray(), new List<float[]>() { data["Y"], data["FY"] }, 0);
                PlotCollection plotsTarget = createPlots("Target", rgT2.ToArray(), new List<float[]>() { data["Y"], data["FY"] }, 1);
                PlotCollection plotsPrediction = createPlots("Predicted", rgT2.ToArray(), new List<float[]>() { rgPrediction.ToArray() }, 0);
                PlotCollectionSet set = new PlotCollectionSet(new List<PlotCollection>() { plotsY, plotsTarget, plotsPrediction });

                // Create the graph image and display
                Image img = SimpleGraphingControl.QuickRender(set, 2000, 600);
                showImage(img, "result_" + i.ToString() + ".png");
            }

            return true;
        }

        /// <summary>
        /// Create the ADAM solver used, setting the test interval > than the
        /// iterations to avoid testing.
        /// </summary>
        /// <returns>The SolverParameter is returned.</returns>
        private SolverParameter createSolver()
        {
            SolverParameter solver = new SolverParameter();

            solver.random_seed = 0xCAFFE;
            solver.test_interval = m_param.Iterations + 1;
            solver.test_iter.Add(100);
            solver.max_iter = m_param.Iterations;
            solver.snapshot = m_param.Iterations;
            solver.test_initialization = false;
            solver.display = m_param.Display;
            solver.type = SolverParameter.SolverType.ADAM;
            solver.lr_policy = "fixed";
            solver.base_lr = m_param.LearningRate;

            return solver;
        }

        /// <summary>
        /// Create the model used to train the Encoder/Decoder
        /// Seq2Seq model using two LSTM layers where the first
        /// acts as the Encoder and the second the Decoder.
        /// </summary>
        /// <returns>The NetParameter of the model is returned.</returns>
        private NetParameter createModel()
        {
            NetParameter net = new NetParameter();

            int nInput = m_param.Input;
            int nHidden = m_param.Hidden;
            int nOutput = m_param.Output;
            int nBatch = m_param.Batch;
            int nSteps = m_param.TimeSteps;

            // 100,batch,1,1
            LayerParameter data = new LayerParameter(LayerParameter.LayerType.INPUT);
            data.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch, nInput }));
            data.top.Add("data");
            net.layer.Add(data);

            // 50,batch,1,1  (pred count)
            LayerParameter label = new LayerParameter(LayerParameter.LayerType.INPUT);
            label.input_param.shape.Add(new BlobShape(new List<int>() { nOutput, nBatch, nInput }));
            label.top.Add("label");
            net.layer.Add(label);

            // 100,batch (0 for first batch, then all 1's)
            LayerParameter clip1 = new LayerParameter(LayerParameter.LayerType.INPUT);
            clip1.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch }));
            clip1.top.Add("clip1");
            net.layer.Add(clip1);

            // 50,batch (all 1's)
            LayerParameter clip2 = new LayerParameter(LayerParameter.LayerType.INPUT);
            clip2.input_param.shape.Add(new BlobShape(new List<int>() { nOutput, nBatch }));
            clip2.top.Add("clip2");
            net.layer.Add(clip2);

            // Create the encoder layer.
            LayerParameter lstm1 = new LayerParameter(LayerParameter.LayerType.LSTM);
            if (lstm1.recurrent_param != null)
            {
                lstm1.recurrent_param.dropout_ratio = m_param.Dropout;
                lstm1.recurrent_param.engine = m_param.LstmEngine;
                lstm1.recurrent_param.num_layers = (uint)m_param.Layers;
                lstm1.recurrent_param.num_output = (uint)nHidden;
                lstm1.recurrent_param.expose_hidden_output = true;
                lstm1.recurrent_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.5);
                lstm1.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            }
            lstm1.name = "encoder";
            lstm1.bottom.Add("data");
            lstm1.bottom.Add("clip1");
            lstm1.top.Add("lstm1");
            lstm1.top.Add("hy"); // order here matters, hy must be before cy
            lstm1.top.Add("cy"); // order here matters.
            net.layer.Add(lstm1);
            string strTop = "lstm1";

            // Get last output from encoder
            LayerParameter slice1 = new LayerParameter(LayerParameter.LayerType.SLICE);
            slice1.slice_param.axis = 0;
            slice1.slice_param.slice_point.Add((uint)(nSteps - 1));
            slice1.bottom.Add(strTop);
            slice1.top.Add("slice1a");
            slice1.top.Add("slice2a");
            net.layer.Add(slice1);

            LayerParameter silence1 = new LayerParameter(LayerParameter.LayerType.SILENCE);
            silence1.bottom.Add("slice1a");
            net.layer.Add(silence1);

            // Get first nPedictCount - 1 from target (for teacher training)
            LayerParameter slice2 = new LayerParameter(LayerParameter.LayerType.SLICE);
            slice2.slice_param.axis = 0;
            slice2.slice_param.slice_point.Add((uint)(nOutput - 1));
            slice2.bottom.Add("label");
            slice2.top.Add("slice1b");
            slice2.top.Add("slice2b");
            net.layer.Add(slice2);

            LayerParameter silence2 = new LayerParameter(LayerParameter.LayerType.SILENCE);
            silence2.bottom.Add("slice2b");
            net.layer.Add(silence2);

            // Expand the decoder input so that we can concat
            // with the last encoder output.
            LayerParameter ip1a = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1a.name = "ip1a";
            ip1a.inner_product_param.num_output = (uint)nHidden;
            ip1a.inner_product_param.axis = 2;
            ip1a.inner_product_param.bias_term = true;
            ip1a.inner_product_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.1);
            ip1a.bottom.Add("slice1b");
            ip1a.top.Add("ip1a");
            net.layer.Add(ip1a);
            strTop = "ip1a";

            if (m_param.LstmEngine == EngineParameter.Engine.CUDNN)
            {
                LayerParameter reshape = new LayerParameter(LayerParameter.LayerType.RESHAPE);
                reshape.reshape_param.axis = 3;
                reshape.reshape_param.num_axes = -1;
                reshape.reshape_param.shape = new BlobShape(new List<int>() { 1 });
                reshape.bottom.Add("ip1a");
                reshape.top.Add("ip1ar");
                net.layer.Add(reshape);
                strTop = "ip1ar";
            }

            // Concat the last encoder output with the decoder input (sans its last item)
            LayerParameter concat = new LayerParameter(LayerParameter.LayerType.CONCAT);
            concat.concat_param.axis = 0;
            concat.bottom.Add("slice2a");
            concat.bottom.Add(strTop);
            concat.top.Add("concat");
            net.layer.Add(concat);

            // Create the decoder layer.
            LayerParameter lstm2 = new LayerParameter(LayerParameter.LayerType.LSTM);
            lstm2.recurrent_param.dropout_ratio = m_param.Dropout;
            lstm2.recurrent_param.engine = m_param.LstmEngine;
            lstm2.recurrent_param.num_layers = (uint)m_param.Layers;
            lstm2.recurrent_param.num_output = (uint)nHidden;
            lstm2.recurrent_param.expose_hidden_input = true;
            lstm2.recurrent_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.5);
            lstm2.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            lstm2.name = "decoder";
            lstm2.bottom.Add("concat");
            lstm2.bottom.Add("clip2");
            lstm2.bottom.Add("hy"); // order here matters, hy must be before cy
            lstm2.bottom.Add("cy"); // order here matters.
            lstm2.top.Add("lstm2");
            net.layer.Add(lstm2);
            strTop = "lstm2";

            // Compbine the decoder output down to a single output per step.
            LayerParameter ip1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1.name = "ip1";
            ip1.inner_product_param.num_output = 1;
            ip1.inner_product_param.axis = 2;
            ip1.inner_product_param.bias_term = true;
            ip1.inner_product_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.1);
            ip1.bottom.Add(strTop);
            ip1.top.Add("ip1");
            net.layer.Add(ip1);

            // Calculate the loss.
            LayerParameter loss = new LayerParameter(LayerParameter.LayerType.EUCLIDEAN_LOSS);
            loss.bottom.Add("ip1");
            loss.bottom.Add("label");
            loss.top.Add("loss");
            net.layer.Add(loss);

            return net;
        }

        /// <summary>
        /// Create the indefinite model that consists only of the Decoder side of the model.
        /// </summary>
        /// <returns>The model NetParameter is returned.</returns>
        private NetParameter createModelInfiniteInput()
        {
            NetParameter net = new NetParameter();

            int nInput = 1;
            int nHidden = m_param.Hidden;
            int nOutput = 1;
            int nBatch = 1;
            int nSteps = 1;


            // 1,1,1
            LayerParameter data = new LayerParameter(LayerParameter.LayerType.INPUT);
            data.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch, nInput }));
            data.top.Add("data");
            net.layer.Add(data);

            // 1,1
            LayerParameter clip = new LayerParameter(LayerParameter.LayerType.INPUT);
            clip.input_param.shape.Add(new BlobShape(new List<int>() { nOutput, nBatch }));
            clip.top.Add("clip2");
            net.layer.Add(clip);

            // Expand the inputs in the same way we did with the decoder input in the
            // training model.
            LayerParameter ip1a = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1a.name = "ip1a";
            ip1a.inner_product_param.num_output = (uint)nHidden;
            ip1a.inner_product_param.axis = 2;
            ip1a.inner_product_param.bias_term = true;
            ip1a.inner_product_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.1);
            ip1a.bottom.Add("data");
            ip1a.top.Add("ip1a");
            net.layer.Add(ip1a);

            // Create the decoder layer.
            LayerParameter lstm2 = new LayerParameter(LayerParameter.LayerType.LSTM);
            if (lstm2.recurrent_param != null)
            {
                lstm2.recurrent_param.dropout_ratio = m_param.Dropout;
                lstm2.recurrent_param.engine = m_param.LstmEngine;
                lstm2.recurrent_param.num_layers = (uint)m_param.Layers;
                lstm2.recurrent_param.num_output = (uint)nHidden;
                lstm2.recurrent_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.5);
                lstm2.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            }
            lstm2.name = "decoder";
            lstm2.bottom.Add("ip1a");
            lstm2.bottom.Add("clip2");
            lstm2.top.Add("lstm2");
            net.layer.Add(lstm2);
            string strTop = "lstm2";

            // Combine the decoder outputs into a single output per step.
            LayerParameter ip1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1.name = "ip1";
            ip1.inner_product_param.num_output = 1;
            ip1.inner_product_param.axis = 2;
            ip1.inner_product_param.bias_term = true;
            ip1.inner_product_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.1);
            ip1.bottom.Add(strTop);
            ip1.top.Add("ip1");
            net.layer.Add(ip1);

            return net;
        }

        /// <summary>
        /// Display the image.
        /// </summary>
        /// <param name="img">Specifies the image to display</param>
        /// <param name="strFile">Specifies the name of the file where the image is saved temporarily.</param>
        private void showImage(Image img, string strFile)
        {
            string strDir = m_strOutputPath + "\\MyCaffe-Samples\\SinCurve\\";
            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            strFile = strDir + strFile;
            img.Save(strFile);

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(strFile);
            p.Start();
        }

        /// <summary>
        /// Get the weight file name for the model.
        /// </summary>
        /// <param name="strTag">Specifies a special tag used to designate the model file name.</param>
        /// <returns>The file name is returned.</returns>
        private string getWeightFileName(string strTag = "")
        {
            string strDir = m_strOutputPath + "\\MyCaffe-Samples\\SinCurve";
            if (!Directory.Exists(strDir))
                Directory.CreateDirectory(strDir);

            return strDir + "\\" + strTag + ".weights_" + LayerParameter.LayerType.LSTM.ToString() + "_" + m_param.LstmEngine.ToString() + "_" + m_param.Layers.ToString() + "_" + m_param.Hidden.ToString() + ".bin";
        }

        /// <summary>
        /// Save the model weights.
        /// </summary>
        /// <param name="rg">Specifies the weight data.</param>
        private void saveWeights(byte[] rg)
        {
            string strFile = getWeightFileName();
            Console.WriteLine("Saving weights to '" + strFile + "'...");
            using (FileStream fs = File.Create(strFile))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(rg);
            }
        }

        /// <summary>
        /// Load the weights from file.
        /// </summary>
        /// <returns>The weight data is returned.</returns>
        private byte[] loadWeights()
        {
            string strFile = getWeightFileName();
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
        /// Save the LSTM state (cy and hy) and associated weights from the ip1a, decoder and ip1
        /// layers.
        /// </summary>
        /// <param name="mycaffe">Specifies the instance of the MyCaffeControl used.</param>
        private void saveLstmState(MyCaffeControl<float> mycaffe)
        {
            Net<float> net = mycaffe.GetInternalNet(Phase.TRAIN);
            Layer<float> encoderLayer = net.FindLayer(LayerParameter.LayerType.LSTM, "encoder");
            Layer<float> decoderLayer = net.FindLayer(LayerParameter.LayerType.LSTM, "decoder");
            Layer<float> layerIp1a = net.FindLayer(LayerParameter.LayerType.INNERPRODUCT, "ip1a");
            Layer<float> layerIp1 = net.FindLayer(LayerParameter.LayerType.INNERPRODUCT, "ip1");

            string strFile = getWeightFileName("sin.lstm_state");
            Console.WriteLine("Saving LSTM state to '" + strFile + "'...");
            using (FileStream fs = File.Create(strFile))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                for (int i = 0; i < encoderLayer.internal_blobs.Count; i++)
                {
                    saveBlob(bw, encoderLayer.internal_blobs[i]);
                }

                for (int i = 0; i < decoderLayer.blobs.Count; i++)
                {
                    saveBlob(bw, decoderLayer.blobs[i]);
                }

                for (int i = 0; i < layerIp1a.blobs.Count; i++)
                {
                    saveBlob(bw, layerIp1a.blobs[i]);
                }

                for (int i = 0; i < layerIp1.blobs.Count; i++)
                {
                    saveBlob(bw, layerIp1.blobs[i]);
                }
            }
        }

        /// <summary>
        /// Save a single blob data to the BinaryWriter.
        /// </summary>
        /// <param name="bw">Specifies the BinaryWriter.</param>
        /// <param name="b">Specifies the Blob.</param>
        private void saveBlob(BinaryWriter bw, Blob<float> b)
        {
            if (b == null)
                return;

            float[] rg = b.update_cpu_data();
            bw.Write(b.num);
            bw.Write(b.channels);
            bw.Write(b.height);
            bw.Write(b.width);
            bw.Write(rg.Length);

            for (int i = 0; i < rg.Length; i++)
            {
                bw.Write(rg[i]);
            }
        }

        /// <summary>
        /// Load the previously saved lstm state (saved from the trained encoder/decoder
        /// model).  The encoder cy and hy are loaded along with the ip1a, decoder and ip1
        /// layer weights.  The Encoder state cy and hy are used to initialize the Decoder's
        /// initial state which is why the Decoder uses a Clip = 1 from the start.
        /// </summary>
        /// <param name="mycaffe">Specifies the instance of the MyCaffeControl.</param>
        private void loadLstmState(MyCaffeControl<float> mycaffe)
        {
            Net<float> net = mycaffe.GetInternalNet(Phase.RUN);
            Layer<float> decoderLayer = net.FindLayer(LayerParameter.LayerType.LSTM, "decoder");
            Layer<float> layerIp1a = net.FindLayer(LayerParameter.LayerType.INNERPRODUCT, "ip1a");
            Layer<float> layerIp1 = net.FindLayer(LayerParameter.LayerType.INNERPRODUCT, "ip1");

            string strFile = getWeightFileName("sin.lstm_state");
            Console.WriteLine("Loading LSTM state to '" + strFile + "'...");
            using (FileStream fs = File.Open(strFile, FileMode.Open, FileAccess.Read))
            using (BinaryReader br = new BinaryReader(fs))
            {
                for (int i = 0; i < decoderLayer.internal_blobs.Count; i++)
                {
                    loadBlob(br, decoderLayer.internal_blobs[i]);
                }

                for (int i = 0; i < decoderLayer.blobs.Count; i++)
                {
                    loadBlob(br, decoderLayer.blobs[i]);
                }

                for (int i = 0; i < layerIp1a.blobs.Count; i++)
                {
                    loadBlob(br, layerIp1a.blobs[i]);
                }

                for (int i = 0; i < layerIp1.blobs.Count; i++)
                {
                    loadBlob(br, layerIp1.blobs[i]);
                }
            }
        }

        /// <summary>
        /// Load the data for a Blob.
        /// </summary>
        /// <param name="br">Specifies the BinaryReader used to load the data.</param>
        /// <param name="b">Specifies the Blob that is loaded with the data.</param>
        private void loadBlob(BinaryReader br, Blob<float> b)
        {
            if (b == null)
                return;

            int nN = br.ReadInt32();
            int nC = br.ReadInt32();
            int nH = br.ReadInt32();
            int nW = br.ReadInt32();
            int nCount = br.ReadInt32();
            float[] rg = b.mutable_cpu_data;

            for (int i = 0; i < nCount; i++)
            {
                float fVal = br.ReadSingle();

                if (i < rg.Length)
                    rg[i] = fVal;
            }

            b.mutable_cpu_data = rg;
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
                    float fX = rgT[nTidx];
                    float fY = rgrgY[i][j];
                    bool bActive = (i == nYIdx) ? true : false;
                    plots.Add(fX, fY, bActive);
                    nTidx++;
                }
            }

            return plots;
        }

        /// <summary>
        /// Create the sample data.
        /// </summary>
        /// <param name="f">The frequency to use for all time series or null to randomize.</param>
        /// <param name="t0">The time offset to use for all time series or null to randomize.</param>
        /// <param name="nBatch">The number of time series to generate.</param>
        /// <param name="nPredict">The number of future samples to generate.</param>
        /// <param name="nSamples">The number of past (and current) samples to generate.</param>
        /// <returns>A dictionary containing the data is returned.</returns>
        private Dictionary<string, float[]> generateSample(float? f = 1.0f, float? t0 = null, int nBatch = 1, int nPredict = 50, int nSamples = 100)
        {
            Dictionary<string, float[]> data = new Dictionary<string, float[]>();
            float[] rgfT = new float[nBatch * nSamples];
            float[] rgfY = new float[nBatch * nSamples];
            float[] rgfFT = new float[nBatch * nPredict];
            float[] rgfFY = new float[nBatch * nPredict];
            Random random = new Random((int)DateTime.Now.Ticks);

            float? fT0 = t0;
            float fFs = 100.0f;

            for (int i = 0; i < nBatch; i++)
            {
                float[] rgft = arrange(0, nSamples + nPredict, fFs);

                if (!fT0.HasValue)
                    t0 = (float)(random.NextDouble() * 2 * Math.PI);
                else
                    t0 = fT0.Value + i / (float)nBatch;

                float? freq = f;
                if (!freq.HasValue)
                    freq = (float)(random.NextDouble() * 3.5 + 0.5);

                float[] rgY = createSample(freq.Value, rgft, t0.Value);

                Array.Copy(rgft, 0, rgfT, i * nSamples, nSamples);
                Array.Copy(rgY, 0, rgfY, i * nSamples, nSamples);
                Array.Copy(rgft, nSamples, rgfFT, i * nPredict, nPredict);
                Array.Copy(rgY, nSamples, rgfFY, i * nPredict, nPredict);
            }

            data.Add("T", rgfT);
            data.Add("Y", rgfY);
            data.Add("FT", rgfFT);
            data.Add("FY", rgfFY);

            return data;
        }

        /// <summary>
        /// Create a single Sin sample.
        /// </summary>
        /// <param name="fFreq">Specifies the frequency.</param>
        /// <param name="rgft">Specifies the time step.</param>
        /// <param name="fT0">Specifies the initial time step.</param>
        /// <returns></returns>
        private float[] createSample(float fFreq, float[] rgft, float fT0)
        {
            float[] rg = new float[rgft.Length];

            for (int i = 0; i < rg.Length; i++)
            {
                rg[i] = (float)Math.Sin(2 * Math.PI * fFreq * (rgft[i] + fT0));
            }

            return rg;
        }

        private float[] arrange(int nStart, int nEnd, float fScale)
        {
            float[] rg = new float[nStart + nEnd];

            for (int i = 0; i < rg.Length; i++)
            {
                rg[i] = i / fScale;
            }

            return rg;
        }
    }
}
