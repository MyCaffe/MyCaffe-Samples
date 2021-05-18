using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinCurve
{
    /// <summary>
    /// Specifies and manages the command line parameters that are available.
    /// </summary>
    public class Parameters
    {
        EngineParameter.Engine m_lstmEngine = EngineParameter.Engine.CUDNN;
        int m_nIterations = 4000;
        int m_nBatch = 256;
        int m_nTimeSteps = 400;
        int m_nInput = 1;
        int m_nHidden = 32;
        int m_nLayers = 1;
        int m_nOutput = 50;
        int m_nDisplay = 100;
        double m_dfDropout = 0.0;
        double m_dfLearningRate = 0.002;
        string m_strType = "CUDNN";
        bool m_bHelp = false;
        bool m_nNewWeights = true;
        MODE m_mode = MODE.TRAIN;

        /// <summary>
        /// Specifies the mode to run.
        /// </summary>
        public enum MODE
        {
            /// <summary>
            /// Specifies to train the model (which then runs afterwards).
            /// </summary>
            TRAIN,
            /// <summary>
            /// Specifies to only run the model (must have already been trained).
            /// </summary>
            RUN
        }

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="args">Optionally, specifies the arguments to parse.</param>
        public Parameters(string[] args = null)
        {
            if (args == null)
                return;

            foreach (string strArg in args)
            {
                if (strArg == "-help")
                {
                    Console.WriteLine("Command Line: -iter:4000 -batch:32 -steps:100 -hidden:15 -dropout:0.2 -lr:0.002 -type:CAFFE|CUDNN -newwts:True -mode:[TRAIN|RUN]");
                    m_bHelp = true;
                    return;
                }

                if (getIntParam("-iter", strArg, ref m_nIterations))
                    continue;

                if (getIntParam("-batch", strArg, ref m_nBatch))
                    continue;

                if (getIntParam("-steps", strArg, ref m_nTimeSteps))
                    continue;

                if (getIntParam("-hidden", strArg, ref m_nHidden))
                    continue;

                if (getDoubleParam("-dropout", strArg, ref m_dfDropout))
                    continue;

                if (getDoubleParam("-lr", strArg, ref m_dfLearningRate))
                    continue;

                if (getStringParam("-type", strArg, ref m_strType))
                    continue;

                string strMode = "";
                if (getStringParam("-mode", strArg, ref strMode))
                {
                    if (strMode == MODE.RUN.ToString())
                        m_mode = MODE.RUN;
                    else
                        m_mode = MODE.TRAIN;
                    continue;
                }

                if (getBoolParam("-newwts", strArg, ref m_nNewWeights))
                    continue;
            }

            getLstmType();
        }

        /// <summary>
        /// Parses the LSTM and Engine options.
        /// </summary>
        private void getLstmType()
        {
            if (m_strType == "CAFFE")
                m_lstmEngine = EngineParameter.Engine.CAFFE;
            else if (m_strType == "CUDNN")
                m_lstmEngine = EngineParameter.Engine.CUDNN;
            else
                throw new Exception("Unknown Engine type '" + m_strType + "', expected <CAFFE|CUDNN> format.");
        }

        /// <summary>
        /// Parses an Int input.
        /// </summary>
        /// <param name="strName">Specifies the parameter name to look for.</param>
        /// <param name="str">Specifies the parameter.</param>
        /// <param name="nVal">Outputs the parameter value.</param>
        /// <returns>If the parameter is found, <i>true</i> is returned.</returns>
        private bool getIntParam(string strName, string str, ref int nVal)
        {
            string[] rgstr = str.Split(':');
            if (rgstr.Length < 2)
                throw new Exception("Invalid int parameter '" + strName + "', expected an integer value to follow.");

            if (rgstr[0] != strName)
                return false;

            if (!int.TryParse(rgstr[1], out nVal))
                throw new Exception("Invalid int parameter '" + strName + "', expected an integer value to follow.");

            return true;
        }

        /// <summary>
        /// Parses a Bool input.
        /// </summary>
        /// <param name="strName">Specifies the parameter name to look for.</param>
        /// <param name="str">Specifies the parameter.</param>
        /// <param name="nVal">Outputs the parameter value.</param>
        /// <returns>If the parameter is found, <i>true</i> is returned.</returns>
        private bool getBoolParam(string strName, string str, ref bool nVal)
        {
            string[] rgstr = str.Split(':');
            if (rgstr.Length < 2)
                throw new Exception("Invalid bool parameter '" + strName + "', expected an boolean value to follow.");

            if (rgstr[0] != strName)
                return false;

            if (!bool.TryParse(rgstr[1], out nVal))
                throw new Exception("Invalid bool parameter '" + strName + "', expected an boolean value to follow.");

            return true;
        }

        /// <summary>
        /// Parses an Double input.
        /// </summary>
        /// <param name="strName">Specifies the parameter name to look for.</param>
        /// <param name="str">Specifies the parameter.</param>
        /// <param name="nVal">Outputs the parameter value.</param>
        /// <returns>If the parameter is found, <i>true</i> is returned.</returns>
        private bool getDoubleParam(string strName, string str, ref double dfVal)
        {
            string[] rgstr = str.Split(':');
            if (rgstr.Length < 2)
                throw new Exception("Invalid int parameter '" + strName + "', expected an integer value to follow.");

            if (rgstr[0] != strName)
                return false;

            if (!double.TryParse(rgstr[1], out dfVal))
                throw new Exception("Invalid int parameter '" + strName + "', expected an double value to follow.");

            return true;
        }

        /// <summary>
        /// Parses an String input.
        /// </summary>
        /// <param name="strName">Specifies the parameter name to look for.</param>
        /// <param name="str">Specifies the parameter.</param>
        /// <param name="nVal">Outputs the parameter value.</param>
        /// <returns>If the parameter is found, <i>true</i> is returned.</returns>
        private bool getStringParam(string strName, string str, ref string strVal)
        {
            string[] rgstr = str.Split(':');
            if (rgstr.Length < 2)
                throw new Exception("Invalid int parameter '" + strName + "', expected an integer value to follow.");

            if (rgstr[0] != strName)
                return false;

            strVal = "";

            for (int i = 1; i < rgstr.Length; i++)
            {
                strVal += rgstr[i];
                strVal += ":";
            }

            strVal = strVal.TrimEnd(':');

            return true;
        }

        /// <summary>
        /// Returns true if this is just a request for command line help.
        /// </summary>
        public bool IsHelp
        {
            get { return m_bHelp; }
        }

        /// <summary>
        /// Specifies the mode to TRAIN or RUN.
        /// </summary>
        public MODE Mode
        {
            get { return m_mode; }
        }

        /// <summary>
        /// Specifies to use new weights even when existing trained weights exist.
        /// </summary>
        public bool NewWeights
        {
            get { return m_nNewWeights; }
        }

        /// <summary>
        /// Specifies the number of training iterations to run (default = 4000).
        /// </summary>
        public int Iterations
        {
            get { return m_nIterations; }
        }

        /// <summary>
        /// Specifies the size of the batch (default = 20).
        /// </summary>
        public int Batch
        {
            get { return m_nBatch; }
        }

        /// <summary>
        /// Specifies the number of timesteps to use (default = 100).
        /// </summary>
        public int TimeSteps
        {
            get { return m_nTimeSteps; }
        }

        /// <summary>
        /// Specifies the number of inputs (set at 1).
        /// </summary>
        public int Input
        {
            get { return m_nInput; }
        }

        /// <summary>
        /// Specifies the number of hidden LSTM units to use (default = 15).
        /// </summary>
        public int Hidden
        {
            get { return m_nHidden; }
        }

        /// <summary>
        /// Specifies the number of predicted values (default = 50).
        /// </summary>
        public int Output
        {
            get { return m_nOutput; }
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
        /// Specifies the dropout ratio to use (default = 0.2).
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
        /// Output the current settings to string.
        /// </summary>
        /// <returns>A string representation of the options is output.</returns>
        public override string ToString()
        {
            return "Command Line: -iter:" + m_nIterations.ToString() + " -batch:" + m_nBatch.ToString() + " -steps:" + m_nTimeSteps.ToString() + " -hidden:" + m_nHidden.ToString() + " -dropout:" + m_dfDropout.ToString() + " -lr:" + m_dfLearningRate.ToString() + " -type:" + m_strType + " -newwts:" + m_nNewWeights.ToString() + " -mode:" + m_mode.ToString();
        }
    }
}
