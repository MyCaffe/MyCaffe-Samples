// Copyright (c) 2018-2020 SignalPop LLC and contributors. All rights reserved.
// License: Apache 2.0
// License: https://github.com/MyCaffe/MyCaffe/blob/master/LICENSE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyCaffe.basecode;
using MyCaffe.db.image;
using MyCaffe.basecode.descriptors;
using System.Threading;
using System.Drawing;

namespace MyCaffe.data
{
    /// <summary>
    /// The KARSDataLoader is used to create the Kaggel Amazon Rainforest Datset.
    /// </summary>
    /// <remarks>
    /// @see [Planet: Understanding the Amazon from Space Dataset](https://www.kaggle.com/c/planet-understanding-the-amazon-from-space/data)
    /// </remarks>
    public class KARSDataLoader
    {
        KARSDataParameters m_param;
        Log m_log;
        CancelEvent m_evtCancel;
        DatasetFactory m_factory = new DatasetFactory();

        /// <summary>
        /// The OnProgress event fires during the creation process to show the progress.
        /// </summary>
        public event EventHandler<ProgressArgs> OnProgress;
        /// <summary>
        /// The OnError event fires when an error occurs.
        /// </summary>
        public event EventHandler<ProgressArgs> OnError;
        /// <summary>
        /// The OnComplete event fires once the dataset creation has completed.
        /// </summary>
        public event EventHandler OnCompleted;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="param">Specifies the creation parameters.</param>
        /// <param name="log">Specifies the output log used to show status updates.</param>
        /// <param name="evtCancel">Specifies the cancel event used to abort the creation process.</param>
        public KARSDataLoader(KARSDataParameters param, Log log, CancelEvent evtCancel)
        {
            m_param = param;
            m_log = log;
            m_evtCancel = evtCancel;
            m_evtCancel.Reset();
        }

        private void m_extractor_OnError(object sender, ProgressArgs e)
        {
            if (OnError != null)
                OnError(sender, e);
        }

        private void m_extractor_OnProgress(object sender, ProgressArgs e)
        {
            if (OnProgress != null)
                OnProgress(sender, e);
        }

        /// <summary>
        /// Return the dataset name.
        /// </summary>
        public static string dataset_name
        {
            get { return "KARS"; }
        }

        /// <summary>
        /// Check to see if the dataset exists in the SQL database.
        /// </summary>
        /// <param name="strDsName">Specifies the dataset name.</param>
        /// <returns>Returns the datset ID if it exists.</returns>
        public int GetDatasetExists(string strDsName = null)
        {
            if (strDsName == null)
                strDsName = dataset_name;
            string strTrainSrc = strDsName + ".training";
            string strTestSrc = strDsName + ".testing";

            int nTrainSrcId = m_factory.GetSourceID(strTrainSrc);
            if (nTrainSrcId == 0)
                return 0;
            
            int nTestSrcId = m_factory.GetSourceID(strTestSrc);
            if (nTestSrcId == 0)
                return 0;

            return m_factory.GetDatasetID(strDsName);
        }

        /// <summary>
        /// Return the source name given a source ID.
        /// </summary>
        /// <param name="nSrcID">Specifies the source ID</param>
        /// <returns>Returns the source name.</returns>
        public string GetSourceName(int nSrcID)
        {
            return m_factory.GetSourceName(nSrcID);
        }

        /// <summary>
        /// Create the dataset and load it into the database.
        /// </summary>
        /// <param name="nCreatorID">Specifies the creator ID.</param>
        /// <returns>On successful creation, the DatasetID is returned, otherwise 0 is returned on abort.</returns>
        public int LoadDatabase(int nCreatorID = 0)
        {
            try
            {
                reportProgress(0, 0, "Loading database " + dataset_name + "...");

                string strTrainingSrcName = dataset_name + ".training";
                SourceDescriptor srcTrain = loadDataset(m_factory, m_param.TrainingPath, strTrainingSrcName);
                if (srcTrain == null)
                    return 0;

                string strTestingSrcName = dataset_name + ".testing";
                SourceDescriptor srcTest = loadDataset(m_factory, m_param.TestingPath, strTestingSrcName);
                if (srcTest == null)
                    return 0;

                DatasetDescriptor ds = new DatasetDescriptor(nCreatorID, dataset_name, null, null, srcTrain, srcTest, dataset_name, dataset_name + " Dataset");
                m_factory.AddDataset(ds);
                m_factory.UpdateDatasetCounts(ds.ID);
                
                return ds.ID;
            }
            catch (Exception excpt)
            {
                throw excpt;
            }
            finally
            {
                if (OnCompleted != null)
                    OnCompleted(this, new EventArgs());
            }
        }

        private SourceDescriptor loadDataset(DatasetFactory factory, string strPath, string strName)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int nSrcId = factory.GetSourceID(strName);
            if (nSrcId != 0)
                factory.DeleteSourceData(nSrcId);
            
            nSrcId = factory.AddSource(strName, 1, 28, 28, false, 0, true);
            string[] rgstrFiles = Directory.GetFiles(strPath, "*.png");

            SimpleDatum sdMean = new SimpleDatum(1, 28, 28);
            double[] rgMeanData = null;

            factory.Open(nSrcId);

            for (int i = 0; i < rgstrFiles.Length; i++)
            {
                Bitmap bmp = new Bitmap(rgstrFiles[i]);
                SimpleDatum sd = ImageData.GetImageData(bmp, sdMean);

                sd.SetLabel(getLabel(rgstrFiles[i]));
                factory.PutRawImageCache(i, sd);
                
                if (sw.Elapsed.TotalMilliseconds > 1000)
                {
                    if (m_evtCancel.WaitOne(0))
                        return null;

                    double dfPct = (double)i / rgstrFiles.Length;
                    reportProgress(i, rgstrFiles.Length, "(" + dfPct.ToString("P") + ") Loading files into '" + strName + "'...");
                    sw.Restart();
                }

                SimpleDatum.AccumulateMean(ref rgMeanData, sd, rgstrFiles.Length);
            }

            factory.ClearImageCache(true);
            factory.UpdateSourceCounts();
            factory.Close();

            SourceDescriptor src = factory.LoadSource(strName);
            sdMean.SetData(rgMeanData, -1);
            factory.SaveImageMean(sdMean, true, src.ID);

            return src;
        }

        /// <summary>
        /// Temporarily returning labels from the MNIST dataset while we wait for the amazon dataset to download.
        /// </summary>
        /// <param name="strFile">Specifies the file name of the hand written character.</param>
        /// <returns>The label returned is a one-hot vector (hex) of the features described in the remarks.</returns>
        /// <remarks>
        /// The features set make up a 7 bit one-hot vector with the following attributes:
        /// character        0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9
        ///            
        /// Loop             1   0   0   0   0   0   0   0   0   0
        /// Upper Loop       0   0   0   0   0   0   0   0   1   1
        /// Bottom Loop      0   0   0   0   0   0   1   0   1   0
        /// 
        /// Angle            0   0   1   0   0   0   0   1   0   0
        /// Top Curve        0   0   1   1   0   0   0   0   1   1
        /// Bottom Curve     0   0   0   1   0   1   1   0   1   0
        /// Lines            0   1   0   0   1   1   0   1   0   0
        /// </remarks>
        private int getLabel(string strFile)
        {
            int nPos = strFile.LastIndexOf('.');
            if (nPos < 0)
                throw new Exception("Invalid file name '" + strFile + "'");

            strFile = strFile.Substring(0, nPos);
            nPos = strFile.LastIndexOf('-');
            if (nPos < 0)
                throw new Exception("Invalid file name '" + strFile + "'");

            string strLabel = strFile.Substring(nPos + 1);
            int nLabel = int.Parse(strLabel);

            switch (nLabel)
            {
                case 0:
                    return 0x40; // 0100 0000 Loop
                    
                case 1:
                    return 0x01; // 0000 0001 Lines

                case 2:
                    return 0x0C; // 0000 1100 Angle, Top Curve

                case 3:
                    return 0x06; // 0000 0110 Top Curve, Bottom Curve

                case 4:
                    return 0x01; // 0000 0001 Lines

                case 5:
                    return 0x03; // 0000 0011 Lines, Bottom Curve

                case 6:
                    return 0x12; // 0001 0010 Bottom Curve

                case 7:
                    return 0x09; // 0000 1001 Lines, Angle

                case 8:
                    return 0x36; // 0011 0110 Top Curve, Bottom Curve, Upper Loop, Bottom Loop

                case 9:
                    return 0x24; // 0010 0100 Top Curve, Upper Loop
            }

            return 0;
        }

        private void Log_OnWriteLine(object sender, LogArg e)
        {
            reportProgress((int)(e.Progress * 1000), 1000, e.Message);
        }

        private void reportProgress(int nIdx, int nTotal, string strMsg)
        {
            if (OnProgress != null)
                OnProgress(this, new ProgressArgs(new ProgressInfo(nIdx, nTotal, strMsg)));
        }

        private void reportError(int nIdx, int nTotal, Exception err)
        {
            if (OnError != null)
                OnError(this, new ProgressArgs(new ProgressInfo(nIdx, nTotal, "ERROR", err)));
        }
    }

    /// <summary>
    /// The KARSDataParameters class contains the parameters used to create the Kaggel Amazon Rainforest Satellite dataset.
    /// </summary>
    public class KARSDataParameters
    {
        string m_strTrainingPath;
        string m_strTestingPath;

        /// <summary>
        /// The constructor.
        /// </summary>
        /// <param name="strTrainingPath">Specifies the path to the training images.</param>
        /// <param name="strTestingPath">Specifies the path to the testing images.</param>
        public KARSDataParameters(string strTrainingPath, string strTestingPath)
        {
            m_strTrainingPath = strTrainingPath;
            m_strTestingPath = strTestingPath;
        }

        /// <summary>
        /// Returns the path to the training images.
        /// </summary>
        public string TrainingPath
        {
            get { return m_strTrainingPath; }
        }

        /// <summary>
        /// Returns the path to the testing images.
        /// </summary>
        public string TestingPath
        {
            get { return m_strTestingPath; }
        }
    }
}
