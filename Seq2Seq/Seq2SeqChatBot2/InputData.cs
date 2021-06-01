using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqChatBot
{
    /// <summary>
    /// The InputData manages the input and target data sets.
    /// </summary>
    public class InputData
    {
        string m_strInputFile;
        string m_strTargetFile;
        string m_strInput;
        int m_nBatch;
        int m_nEpochs;
        OPERATION m_operation = OPERATION.TRAIN;
        int m_nEpochSize = 1000;
        int m_nHidden = 256;
        int m_nWordSize = 128;
        double m_dfLearningRate = 0.001;
        bool m_bUseSoftmax = false;
        bool m_bUseExtIp = false;

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
        public InputData()
        {
        }

        /// <summary>
        /// Specifies the input filename.
        /// </summary>
        public string InputFileName
        {
            get { return m_strInputFile; }
        }

        /// <summary>
        /// Specifies the target filename.
        /// </summary>
        public string TargetFileName
        {
            get { return m_strTargetFile; }
        }

        /// <summary>
        /// Get/set the number of epochs where one epoch equals
        /// one pass through the list of senetences in the training set.
        /// </summary>
        public int Epochs
        {
            get { return m_nEpochs; }
            set { m_nEpochs = value; }
        }

        /// <summary>
        /// Returns the number of sentences in the training set.
        /// </summary>
        public int EpochSize
        {
            get { return m_nEpochSize; }
        }

        /// <summary>
        /// Returns the hidden size of each LSTM layer.
        /// </summary>
        public int HiddenSize
        {
            get { return m_nHidden; }
        }

        /// <summary>
        /// Returns the word size used to size the output of each embedding layer.
        /// </summary>
        public int WordSize
        {
            get { return m_nWordSize; }
        }

        /// <summary>
        /// Returns the batch size, current = 1.
        /// </summary>
        public int Batch
        {
            get { return m_nBatch; }
            set { m_nBatch = value; }
        }

        /// <summary>
        /// Returns the learning rate.
        /// </summary>
        public double LearningRate
        {
            get { return m_dfLearningRate; }
        }

        /// <summary>
        /// Get/set the input text use when running the trainied model.
        /// </summary>
        public string InputText
        {
            get { return m_strInput; }
            set { m_strInput = value; }
        }

        /// <summary>
        /// Returns the operation to run.
        /// </summary>
        public OPERATION Operation
        {
            get { return m_operation; }
        }

        /// <summary>
        /// Returns whether or not to use a softmax layer vs memory_loss layer.
        /// </summary>
        public bool UseSoftmax
        {
            get { return m_bUseSoftmax; }
        }

        /// <summary>
        /// Returns whether or not to us an external inner-product layer.
        /// </summary>
        public bool UseExternalIp
        {
            get { return m_bUseExtIp; }
        }

        /// <summary>
        /// Sets the initial input and target data files which are then loaded into the data sets.
        /// </summary>
        /// <param name="op">Specifies the operation to perform.</param>
        /// <param name="strInputFile">Specifies the input filename.</param>
        /// <param name="strTargetFile">Specifies the target filename.</param>
        /// <param name="strIter">Specifies the iterations to run.</param>
        /// <param name="strInput">Specifies the input text used when running the model.</param>
        /// <param name="strBatch">Specifies the batch size, current = "1"</param>
        /// <param name="strHidden">Specifies the hidden size.</param>
        /// <param name="strWordSize">Specifies the word size.</param>
        /// <param name="strLr">Specifies the learning rate.</param>
        public void SetData(OPERATION op, string strInputFile, string strTargetFile, string strIter, string strInput, string strBatch, string strHidden, string strWordSize, string strLr, bool bUseSoftmax, bool bUseExtIp)
        {
            m_operation = op;
            m_strInput = strInput;

            m_bUseSoftmax = bUseSoftmax;
            m_bUseExtIp = bUseExtIp;

            if (!File.Exists(strInputFile))
                throw new Exception("Could not find the input filename '" + strInputFile + "'!");

            m_strInputFile = strInputFile;

            if (!File.Exists(strTargetFile))
                throw new Exception("Could not find the target filename '" + strTargetFile + "'!");

            m_strTargetFile = strTargetFile;

            if (!int.TryParse(strIter, out m_nEpochs) || m_nEpochs < 1)
                throw new Exception("Invalid iterations, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strBatch, out m_nBatch) || m_nBatch < 1)
                throw new Exception("Invalid batch, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strHidden, out m_nHidden) || m_nHidden < 1)
                throw new Exception("Invalid hidden size, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strWordSize, out m_nWordSize) || m_nWordSize < 1)
                throw new Exception("Invalid word size, please enter a valid integer in the range [1,+].");

            if (!double.TryParse(strLr, out m_dfLearningRate) || m_dfLearningRate < 0)
                throw new Exception("Invalid learning rate, please enter a valid integer in the range [0,+].");
        }
    }
}
