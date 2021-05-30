using MyCaffe.basecode;
using MyCaffe.basecode.descriptors;
using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqChatBot
{
    /// <summary>
    /// The Model class manages the model and solver configurations.
    /// </summary>
    public class Model
    {
        int m_nIterations = 400000;
        int m_nDisplay = 100;
        int m_nHidden = 16;
        int m_nBatch = 1;
        double m_dfLearningRate = 0.001;
        double m_dfDecayRate = 0.000001;
        int m_nTimeSteps = 80;  // maximum sentence length
        double m_dfDropout = 0.0;
        int m_nLayers = 1;
        FillerParameter m_fillerParam = new FillerParameter("gaussian", 0, 0, 0.08);
        int m_nSampleSize = 1000;

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
            set { m_nIterations = value; }
        }

        /// <summary>
        /// Specifies the size of the batch (default = 1).
        /// </summary>
        public int Batch
        {
            get { return m_nBatch; }
            set { m_nBatch = value; }
        }

        /// <summary>
        /// Returns the sample size over which training runs.
        /// </summary>
        public int SampleSize
        {
            get { return m_nSampleSize; }
        }

        /// <summary>
        /// Specifies the number of timesteps to use (default = 10).
        /// </summary>
        public int TimeSteps
        {
            get { return m_nTimeSteps; }
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
        /// Specifies the learning rate (default = 0.001).
        /// </summary>
        public double LearningRate
        {
            get { return m_dfLearningRate; }
        }

        /// <summary>
        /// Specifies the decay rate (default = 0.000001)
        /// </summary>
        public double DecayRate
        {
            get { return m_dfDecayRate; }
        }

        /// <summary>
        /// Create the ADAM solver used, setting the test interval > than the
        /// iterations to avoid testing.
        /// </summary>
        /// <param name="dfLr">Specifies the learning rate.</param>
        /// <returns>The SolverParameter is returned.</returns>
        public SolverParameter CreateSolver(double dfLr)
        {
            SolverParameter solver = new SolverParameter();

            m_dfLearningRate = dfLr;

            solver.random_seed = 0xCAFFE;
            solver.test_interval = 100;
            solver.test_iter[0] =  1;
            solver.max_iter = m_nIterations;
            solver.snapshot = m_nIterations;
            solver.test_initialization = false;
            solver.display = m_nDisplay;
            solver.momentum = 0;
            solver.rms_decay = 0.999;
            solver.weight_decay = m_dfDecayRate;
            solver.clip_gradients = 5;
            solver.regularization_type = "L2";
            solver.type = SolverParameter.SolverType.RMSPROP;
            solver.lr_policy = "multistep";
            solver.stepvalue = new List<int>() { 100000, 200000 };
            solver.gamma = 0.5;
            solver.base_lr = m_dfLearningRate;

            return solver;
        }

        /// <summary>
        /// Create the model used to train the Encoder/Decoder using the TextData Layer as input.
        /// Seq2Seq model using two LSTM layers where the first
        /// acts as the Encoder and the second the Decoder.
        /// </summary>
        /// <param name="strInputFile">Specifies the input data.</param>
        /// <param name="strTargetFile">Specifies the target data.</param>
        /// <param name="nHiddenCount">Specifies hidden data count.</param>
        /// <param name="nWordSize">Specifies the size of the word embeddings.</param>
        /// <param name="phase">Specifies phase of the model to create.</param>
        /// <returns>The NetParameter of the model is returned.</returns>
        public NetParameter CreateModel(string strInputFile, string strTargetFile, int nHiddenCount, int nWordSize, Phase phase = Phase.TRAIN)
        {
            m_nHidden = nHiddenCount;
            NetParameter net = new NetParameter();

            // Add data input layer that takes care of loading inputs and feeding the data
            // to the network.
            LayerParameter data = new LayerParameter(LayerParameter.LayerType.TEXT_DATA);
            data.name = "data";
            data.text_data_param.time_steps = (uint)m_nTimeSteps;
            data.text_data_param.batch_size = (uint)m_nBatch;
            data.text_data_param.enable_normal_encoder_output = true;
            data.text_data_param.enable_reverse_encoder_output = true;
            data.text_data_param.encoder_source = strInputFile;
            data.text_data_param.decoder_source = strTargetFile;
            data.text_data_param.sample_size = (uint)m_nSampleSize;
            data.text_data_param.shuffle = true;
            data.top.Add("dec_input");
            data.top.Add("clipD");
            data.top.Add("data");
            data.top.Add("datar");
            data.top.Add("clipE");
            data.top.Add("vocabcount");
            data.top.Add("label");
            net.layer.Add(data);

            // Create the embedding layer that converts sentence word indexes into an embedding of
            // size nWordSize for each word in the sentence.
            LayerParameter embed1 = new LayerParameter(LayerParameter.LayerType.EMBED);
            embed1.embed_param.input_dim = 1; // (uint)nVocabCount + 2;
            embed1.embed_param.num_output = (uint)nWordSize; // Word size.
            embed1.embed_param.bias_term = true;
            embed1.embed_param.weight_filler = m_fillerParam;
            embed1.parameters.Add(new ParamSpec("embed_wts"));
            embed1.parameters.Add(new ParamSpec("embed_bias"));
            embed1.bottom.Add("data");
            embed1.bottom.Add("vocabcount");
            embed1.top.Add("embed1");
            net.layer.Add(embed1);

            // Create the encoder layer that encodes the input 'ip1' image representatons,
            // learned from the input model.
            LayerParameter lstm1 = new LayerParameter(LayerParameter.LayerType.LSTM);
            lstm1.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            lstm1.recurrent_param.weight_filler = m_fillerParam;
            lstm1.recurrent_param.engine = EngineParameter.Engine.CUDNN;
            lstm1.recurrent_param.num_output = (uint)m_nHidden;
            lstm1.name = "encoder1";
            lstm1.bottom.Add("embed1");
            lstm1.bottom.Add("clipE");
            lstm1.top.Add("lstm1");
            net.layer.Add(lstm1);

            // Create the embedding layer that converts sentence word indexes into an embedding of
            // size nWordSize for each word in the sentence.
            LayerParameter embed2 = new LayerParameter(LayerParameter.LayerType.EMBED);
            embed2.embed_param.input_dim = 1; // (uint)nVocabCount + 2;
            embed2.embed_param.num_output = (uint)nWordSize; // Word size.
            embed2.embed_param.bias_term = true;
            embed2.embed_param.weight_filler = m_fillerParam;
            embed2.parameters.Add(new ParamSpec("embed_wts"));
            embed2.parameters.Add(new ParamSpec("embed_bias"));
            embed2.bottom.Add("datar");
            embed2.bottom.Add("vocabcount");
            embed2.top.Add("embed2");
            net.layer.Add(embed2);

            // Create the encoder layer that encodes the input 'ip1' image representatons,
            // learned from the input model.
            LayerParameter lstm2 = new LayerParameter(LayerParameter.LayerType.LSTM);
            lstm2.recurrent_param.bias_filler = new FillerParameter("constant", 0);
            lstm2.recurrent_param.weight_filler = m_fillerParam;
            lstm2.recurrent_param.engine = EngineParameter.Engine.CUDNN;
            lstm2.recurrent_param.num_output = (uint)m_nHidden;
            lstm2.name = "encoder2";
            lstm2.bottom.Add("embed2");
            lstm2.bottom.Add("clipE");
            lstm2.top.Add("lstm2");
            net.layer.Add(lstm2);

            LayerParameter concat = new LayerParameter(LayerParameter.LayerType.CONCAT);
            concat.concat_param.axis = 2;
            concat.bottom.Add("lstm1");
            concat.bottom.Add("lstm2");
            concat.top.Add("encoded");
            net.layer.Add(concat);

            // Create embedding for decoder input.
            LayerParameter embed3 = new LayerParameter(LayerParameter.LayerType.EMBED);
            embed3.name = "dec_input_embed";
            embed3.embed_param.input_dim = 1; // (uint)nVocabCount + 2;
            embed3.embed_param.num_output = (uint)nWordSize; // Word size.
            embed3.embed_param.bias_term = true;
            embed3.embed_param.weight_filler = m_fillerParam;
            embed3.bottom.Add("dec_input");
            embed3.bottom.Add("vocabcount");
            embed3.top.Add("dec_input_embed");
            net.layer.Add(embed3);

            LayerParameter lstm3 = new LayerParameter(LayerParameter.LayerType.LSTM_ATTENTION);
            lstm3.lstm_attention_param.bias_filler = new FillerParameter("constant", 0);
            lstm3.lstm_attention_param.weight_filler = m_fillerParam;
            lstm3.lstm_attention_param.num_output = (uint)m_nHidden;
            lstm3.lstm_attention_param.num_output_ip = 1; // (uint)nVocabCount + 2;
            lstm3.lstm_attention_param.enable_attention = true;
            lstm3.name = "decoder";
            lstm3.bottom.Add("dec_input_embed");
            lstm3.bottom.Add("clipD");
            lstm3.bottom.Add("encoded");
            lstm3.bottom.Add("clipE");
            lstm3.bottom.Add("vocabcount");
            lstm3.top.Add("ip1");
            net.layer.Add(lstm3);

            if (phase != Phase.RUN)
            {
                LayerParameter loss = new LayerParameter(LayerParameter.LayerType.MEMORY_LOSS);
                loss.name = "loss";
                loss.loss_param.normalization = LossParameter.NormalizationMode.NONE;
                loss.bottom.Add("ip1");
                loss.bottom.Add("label");
                loss.top.Add("loss");
                net.layer.Add(loss);

                LayerParameter accuracy = new LayerParameter(LayerParameter.LayerType.ACCURACY);
                accuracy.accuracy_param.axis = 2;
                accuracy.accuracy_param.ignore_label = 0;
                accuracy.bottom.Add("ip1");
                accuracy.bottom.Add("label");
                accuracy.top.Add("accuracy");
                accuracy.include.Add(new NetStateRule(Phase.TEST));
                net.layer.Add(accuracy);
            }
            else
            {
                LayerParameter output = new LayerParameter(LayerParameter.LayerType.SOFTMAX);
                output.softmax_param.axis = 2;
                output.bottom.Add("ip1");
                output.top.Add("softmax");
                net.layer.Add(output);
            }

            return net;
        }
    }
}
