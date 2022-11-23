using MyCaffe;
using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.converter.onnx;
using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// These samples show how to import and export from/to *.onnx ai model files.
/// </summary>
namespace OnnxExamples
{
    class Program
    {
        /// <summary>
        /// Get the test path where created files are stored.
        /// </summary>
        private static string TestDataPath
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\MyCaffe\\test_data\\models\\onnx"; }
        }

        /// <summary>
        /// Download a file if not already downloaded.
        /// </summary>
        /// <param name="strUrl">Specifies the file url.</param>
        /// <returns>The local file path of the file is returned.</returns>
        private static string downloadFile(string strUrl)
        {
            int nPos = strUrl.LastIndexOf('/');
            string strFile = strUrl.Substring(nPos + 1);

            string strTestPath = TestDataPath;
            if (!Directory.Exists(strTestPath))
                Directory.CreateDirectory(strTestPath);

            string strModelFile = strTestPath + "\\" + strFile;
            if (File.Exists(strModelFile))
                return strModelFile;

            using (WebClient client = new WebClient())
            {
                Console.WriteLine("Downloading '" + strUrl + "' - this may take awhile...");
                client.DownloadFile(strUrl, strModelFile);
            }

            return strModelFile;
        }

        /// <summary>
        /// Main demonstration function.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Get the ONNX file to import.
            string strOnnxModelUrl = "https://github.com/onnx/models/raw/main/vision/classification/alexnet/model/bvlcalexnet-9.onnx";
            string strOnnxFile = downloadFile(strOnnxModelUrl);

            // Create the MyCaffe conversion control
            MyCaffeConversionControl<float> convert = new MyCaffeConversionControl<float>();
            CudaDnn<float> cuda = new CudaDnn<float>(0);
            Log log = new Log("Onnx Test");

            // Convert an ONNX model file into the MyCaffe model description prototxt and weight protobuf.
            MyCaffeModelData modeldata = convert.ConvertOnnxToMyCaffeFromFile(cuda, log, strOnnxFile);

            // Use the model description prototxt (same format used by CAFFE)...
            string strModelDesc = modeldata.ModelDescription;
            // And weights in binary protbuf format (same format used by CAFFE)...
            byte[] rgWeights = modeldata.Weights;
            // along with the solver descriptor of your choice to use the model.

            Console.WriteLine("================================");
            Console.WriteLine("IMPORT: Model imported from *.onnx");
            Console.WriteLine("================================");
            Console.WriteLine(strModelDesc);
            Console.WriteLine("--done--");

            // Convert a MyCaffe model file (and weights) into the equivalent ONNX model file.
            Console.WriteLine("================================");
            Console.WriteLine("EXPORT: Model exported to *.onnx");
            Console.WriteLine("================================");
            string strOnnxOutFile = TestDataPath + "\\bvlc_allexnet.onnx";
            if (File.Exists(strOnnxOutFile))
                File.Delete(strOnnxOutFile);

            // Convert the MyCaffe model file (and weights) back into a new ONNX model file.
            convert.ConvertMyCaffeToOnnxFile(cuda, log, modeldata, strOnnxOutFile);
            Console.WriteLine("Exported model to '" + strOnnxOutFile + "'.");

            // Cleanup.
            cuda.Dispose();
            convert.Dispose();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
