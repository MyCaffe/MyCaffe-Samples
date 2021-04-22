using MyCaffe.basecode;
using MyCaffe.basecode.descriptors;
using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqImageToSin
{
    /// <summary>
    /// The Model class manages the model and solver configurations.
    /// </summary>
    public class Model
    {
        int m_nIterations = 4000;
        int m_nDisplay = 100;
        double m_dfLearningRate = 0.002;
        int m_nInputData = 0;
        int m_nInputLabel = 0;
        int m_nHidden = 128;
        int m_nBatch = 64;
        int m_nTimeSteps = 12;  // sequenced images 0..9
        double m_dfDropout = 0.2;
        int m_nLayers = 1;
        EngineParameter.Engine m_lstmEngine = EngineParameter.Engine.CUDNN;

        /// <summary>
        /// The constructor.
        /// </summary>
        public Model()
        {
        }

        /// <summary>
        /// Specifies the number of training iterations to run (default = 1000).
        /// </summary>
        public int Iterations
        {
            get { return m_nIterations; }
        }

        /// <summary>
        /// Specifies the size of the batch (default = 1).
        /// </summary>
        public int Batch
        {
            get { return m_nBatch; }
        }

        /// <summary>
        /// Specifies the number of timesteps to use (default = 10).
        /// </summary>
        public int TimeSteps
        {
            get { return m_nTimeSteps; }
        }

        /// <summary>
        /// Specifies the data input count.
        /// </summary>
        public int InputData
        {
            get { return m_nInputData; }
        }

        /// <summary>
        /// Specifies the label input count.
        /// </summary>
        public int InputLabel
        {
            get { return m_nInputLabel; }
        }

        /// <summary>
        /// Specifies the number of hidden LSTM units to use (default = 1000).
        /// </summary>
        public int Hidden
        {
            get { return m_nHidden; }
        }

        /// <summary>
        /// Specifies how often to output the training status (default = 100).
        /// </summary>
        public int Display
        {
            get { return m_nDisplay; }
        }

        /// <summary>
        /// Specifies the number of layers to use (LSTM:CUDNN only, default = 1).
        /// </summary>
        public int Layers
        {
            get { return m_nLayers; }
        }

        /// <summary>
        /// Specifies the dropout ratio to use (default = 0.0).
        /// </summary>
        public double Dropout
        {
            get { return m_dfDropout; }
        }

        /// <summary>
        /// Specifies the learning rate (default = 0.002).
        /// </summary>
        public double LearningRate
        {
            get { return m_dfLearningRate; }
        }

        /// <summary>
        /// Specifies the LSTM engine to use (CAFFE or CUDNN).  
        /// </summary>
        public EngineParameter.Engine LstmEngine
        {
            get { return m_lstmEngine; }
        }

        /// <summary>
        /// Create the ADAM solver used, setting the test interval > than the
        /// iterations to avoid testing.
        /// </summary>
        /// <returns>The SolverParameter is returned.</returns>
        public SolverParameter CreateSolver()
        {
            SolverParameter solver = new SolverParameter();

            solver.random_seed = 0xCAFFE;
            solver.test_interval = m_nIterations + 1;
            solver.test_iter.Add(100);
            solver.max_iter = m_nIterations;
            solver.snapshot = m_nIterations;
            solver.test_initialization = false;
            solver.display = m_nDisplay;
            solver.type = SolverParameter.SolverType.ADAM;
            solver.lr_policy = "fixed";
            solver.base_lr = m_dfLearningRate;

            return solver;
        }

        /// <summary>
        /// Create the model used to train the Encoder/Decoder
        /// Seq2Seq model using two LSTM layers where the first
        /// acts as the Encoder and the second the Decoder.
        /// </summary>
        /// <param name="nInputData">Specifies the count of the input data.</param>
        /// <param name="nInputLabel">Specifies the count of the label data.</param>
        /// <param name="nBatchOverride">Specifies an override for the batch count.</param>
        /// <param name="nTimeStepOverride">Specifies an override for the time-step count.</param>
        /// <returns>The NetParameter of the model is returned.</returns>
        public NetParameter CreateModel(int nInputData, int nInputLabel, int? nBatchOverride = null, int? nTimeStepOverride = null)
        {
            NetParameter net = new NetParameter();

            int nHidden = m_nHidden;
            int nBatch = (nBatchOverride.HasValue) ? nBatchOverride.Value : m_nBatch;
            int nSteps = (nTimeStepOverride.HasValue) ? nTimeStepOverride.Value : m_nTimeSteps;

            m_nInputData = nInputData;
            m_nInputLabel = nInputLabel;

            // 10,batch,1,1
            LayerParameter data = new LayerParameter(LayerParameter.LayerType.INPUT);
            data.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch, nInputData }));
            data.top.Add("data");
            net.layer.Add(data);

            // 10,batch,1,1  (pred count)
            LayerParameter label = new LayerParameter(LayerParameter.LayerType.INPUT);
            label.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch, nInputLabel }));
            label.top.Add("label");
            net.layer.Add(label);

            // 10,batch (0 for first batch, then all 1's)
            LayerParameter clip1 = new LayerParameter(LayerParameter.LayerType.INPUT);
            clip1.input_param.shape.Add(new BlobShape(new List<int>() { nSteps, nBatch }));
            clip1.top.Add("clip1");
            net.layer.Add(clip1);

            // Create the encoder layer that encodes the input 'ip1' image representatons,
            // learned from the input model.
            LayerParameter lstm1 = new LayerParameter(LayerParameter.LayerType.LSTM);
            if (lstm1.recurrent_param != null)
            {
                lstm1.recurrent_param.dropout_ratio = m_dfDropout;
                lstm1.recurrent_param.engine = m_lstmEngine;
                lstm1.recurrent_param.num_layers = (uint)m_nLayers;
                lstm1.recurrent_param.num_output = (uint)nHidden;
                lstm1.recurrent_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.5);
                lstm1.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            }
            lstm1.name = "encoder";
            lstm1.bottom.Add("data");
            lstm1.bottom.Add("clip1");
            lstm1.top.Add("lstm1");
            net.layer.Add(lstm1);

            // Create the decoder layer used to decode the input encoding to the
            // data representing a section of the Sin curve.
            LayerParameter lstm2 = new LayerParameter(LayerParameter.LayerType.LSTM);
            lstm2.recurrent_param.dropout_ratio = m_dfDropout;
            lstm2.recurrent_param.engine = m_lstmEngine;
            lstm2.recurrent_param.num_layers = (uint)m_nLayers;
            lstm2.recurrent_param.num_output = (uint)nHidden;
            lstm2.recurrent_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.5);
            lstm2.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            lstm2.name = "decoder";
            lstm2.bottom.Add("lstm1");
            lstm2.bottom.Add("clip1");
            lstm2.top.Add("lstm2");
            net.layer.Add(lstm2);

            // Combine the decoder output down to the input label count per step,
            // which are the number of items in the Sin curve section.
            LayerParameter ip1 = new LayerParameter(LayerParameter.LayerType.INNERPRODUCT);
            ip1.name = "ip1";
            ip1.inner_product_param.num_output = (uint)nInputLabel;
            ip1.inner_product_param.axis = 2;
            ip1.inner_product_param.bias_term = true;
            ip1.inner_product_param.weight_filler = new FillerParameter("gaussian", 0, 0, 0.1);
            ip1.bottom.Add("lstm2");
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
        /// Load the MNIST LeNet model and set its sources to the MNIST dataset (already loaded
        /// in the database using the MyCaffeTestApplication).
        /// </summary>
        /// <param name="ds">Specifies the MNIST dataset descriptor.</param>
        /// <returns>The NetParameter for the LeNet is returned.</returns>
        public NetParameter CreateMnistModel(DatasetDescriptor ds)
        {
            string str = System.Text.Encoding.Default.GetString(Properties.Resources.lenet_train_test);
            RawProto proto = RawProto.Parse(str);
            NetParameter netParam = NetParameter.FromProto(proto);

            for (int i=0; i<netParam.layer.Count; i++)
            {
                LayerParameter layer = netParam.layer[i];

                if (layer.type == LayerParameter.LayerType.DATA)
                {
                    if (layer.include[0].phase == Phase.TRAIN)
                        layer.data_param.source = ds.TrainingSourceName;
                    else
                        layer.data_param.source = ds.TestingSourceName;
                }
            }

            return netParam;
        }

        /// <summary>
        /// Load and return the solver used with the MNIST LeNet input model.
        /// </summary>
        /// <returns>The SolverParameter is returned.</returns>
        public SolverParameter CreateMnistSolver()
        {
            string str = System.Text.Encoding.Default.GetString(Properties.Resources.lenet_solver);
            RawProto proto = RawProto.Parse(str);
            return SolverParameter.FromProto(proto);
        }
    }
}
