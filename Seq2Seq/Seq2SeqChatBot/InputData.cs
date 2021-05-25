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
        Vocabulary m_vocab;
        Random m_random = new Random(1234);
        string m_strInputFileName;
        string m_strTargetFileName;
        string m_strInput;
        int m_nBatch;
        int m_nEpochs;
        OPERATION m_operation = OPERATION.TRAIN;
        List<List<string>> m_rgrgstrInput = new List<List<string>>();
        List<List<string>> m_rgrgstrTarget = new List<List<string>>();
        int m_nEpochSize = 1000;
        int m_nHidden = 256;
        int m_nWordSize = 128;

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
        /// Returns the input data as a list of list of strings where each inner list is an input sentence 
        /// broken up into words.
        /// </summary>
        public List<List<string>> Input
        {
            get { return m_rgrgstrInput; }
        }

        /// <summary>
        /// Returns the target data as a list of list of strings where each inner list is an target sentence 
        /// broken up into words.
        /// </summary>
        public List<List<string>> Target
        {
            get { return m_rgrgstrTarget; }
        }

        /// <summary>
        /// Returns the input text file name.
        /// </summary>
        public string InputFileName
        {
            get { return m_strInputFileName; }
            set { m_strInputFileName = value; }
        }

        /// <summary>
        /// Returns the target text file name.
        /// </summary>
        public string TargetFileName
        {
            get { return m_strTargetFileName; }
            set { m_strTargetFileName = value; }
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
        /// Sets the initial input and target data files which are then loaded into the data sets.
        /// </summary>
        /// <param name="op">Specifies the operation to perform.</param>
        /// <param name="strInputFile">Specifies the input file containing the input sentences.</param>
        /// <param name="strTargetFile">Specifies the target file containing the target sentences.</param>
        /// <param name="strIter">Specifies the iterations to run.</param>
        /// <param name="strInput">Specifies the input text used when running the model.</param>
        /// <param name="strBatch">Specifies the batch size, current = "1"</param>
        /// <param name="strHidden">Specifies the hidden size.</param>
        /// <param name="strWordSize">Specifies the word size.</param>
        public void SetData(OPERATION op, string strInputFile, string strTargetFile, string strIter, string strInput, string strBatch, string strHidden, string strWordSize)
        {
            m_operation = op;
            m_strInput = strInput;

            if (!File.Exists(strInputFile))
                throw new Exception("Could not find the input file '" + strInputFile + "'!");

            m_strInputFileName = strInputFile;

            if (!File.Exists(strTargetFile))
                throw new Exception("Could not find the target file '" + strTargetFile + "'!");

            m_strTargetFileName = strTargetFile;

            if (!int.TryParse(strIter, out m_nEpochs) || m_nEpochs < 1)
                throw new Exception("Invalid iterations, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strBatch, out m_nBatch) || m_nBatch < 1)
                throw new Exception("Invalid batch, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strHidden, out m_nHidden) || m_nHidden < 1)
                throw new Exception("Invalid hidden size, please enter a valid integer in the range [1,+].");

            if (!int.TryParse(strWordSize, out m_nWordSize) || m_nWordSize < 1)
                throw new Exception("Invalid word size, please enter a valid integer in the range [1,+].");
        }

        /// <summary>
        /// Load the input and target files and convert each into a list of lines each containing a list of words per line.
        /// </summary>
        public Data PreProcessInputFiles()
        {
            m_rgrgstrInput = new List<List<string>>();
            m_rgrgstrTarget = new List<List<string>>();

            string[] rgstrInput = File.ReadAllLines(m_strInputFileName);
            string[] rgstrTarget = File.ReadAllLines(m_strTargetFileName);

            if (rgstrInput.Length != rgstrTarget.Length)
                throw new Exception("Both the input and target files must contains the same number of lines!");

            for (int i = 0; i < m_nEpochSize; i++)
            {
                int nMaxLenInput = 0;
                int nMaxLenTarget = 0;

                List<string> rgstrInput1 = preprocess(rgstrInput[i], nMaxLenInput);
                List<string> rgstrTarget1 = preprocess(rgstrTarget[i], nMaxLenTarget);

                if (rgstrInput1 != null && rgstrTarget1 != null)
                {
                    m_rgrgstrInput.Add(rgstrInput1);
                    m_rgrgstrTarget.Add(rgstrTarget1);
                }
            }

            Vocabulary vocab = new Vocabulary();

            vocab.Load(m_rgrgstrInput, m_rgrgstrTarget);
            m_vocab = vocab;

            Data data = new Data(m_rgrgstrInput, m_rgrgstrTarget, vocab);

            return data;
        }

        private List<string> preprocess(string str, int nMaxLen = 0)
        {
            string strInput = clean(str);
            List<string> rgstr = strInput.ToLower().Trim().Split(' ').ToList();

            if (nMaxLen > 0)
            {
                rgstr = rgstr.Take(nMaxLen).ToList();
                if (rgstr.Count < nMaxLen)
                    return null;
            }

            return rgstr;
        }

        public Data PreProcessInputText()
        {
            List<string> rgstrInput = preprocess(m_strInput);
            List<List<string>> rgrgstrInput = new List<List<string>>();
            rgrgstrInput.Add(rgstrInput);

            return new Data(rgrgstrInput, null, m_vocab);
        }

        private string clean(string str)
        {
            string strOut = "";

            foreach (char ch in str)
            {
                if (ch == 'á')
                    strOut += 'a';
                else if (ch == 'é')
                    strOut += 'e';
                else if (ch == 'í')
                    strOut += 'i';
                else if (ch == 'ó')
                    strOut += 'o';
                else if (ch == 'ú')
                    strOut += 'u';
                else if (ch == 'Á')
                    strOut += 'A';
                else if (ch == 'É')
                    strOut += 'E';
                else if (ch == 'Í')
                    strOut += 'I';
                else if (ch == 'Ó')
                    strOut += 'O';
                else if (ch == 'Ú')
                    strOut += 'U';
                else
                    strOut += ch;
            }

            return strOut;
        }
    }
}
