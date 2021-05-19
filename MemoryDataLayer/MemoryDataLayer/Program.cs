using MyCaffe.basecode;
using MyCaffe.common;
using MyCaffe.layers;
using MyCaffe.param;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// These samples show how to use the MemoryDataLayer.
/// </summary>
namespace MemoryDataLayer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create the output log used.
            Log log = new Log("Test");
            // Create the CudaDnn connection used.  NOTE: only one CudaDnn connection is needed 
            // per thread for each instance creates and manages its own low-level kernel state
            // which includes all memory allocated etc.  All memory handles allocated should
            // be used with the CudaDnn that allocated the memory.
            CudaDnn<float> cuda = new CudaDnn<float>(0, DEVINIT.CUBLAS | DEVINIT.CURAND);
            MemoryDataLayer<float> layer = createMemoryDataLayer(cuda, log);
            List<Datum> rgData = dataSetter();
            Blob<float> blobData = new Blob<float>(cuda, log);
            Blob<float> blobLabel = new Blob<float>(cuda, log);
            BlobCollection<float> colBottom = new BlobCollection<float>();
            BlobCollection<float> colTop = new BlobCollection<float>();

            // Set the top blob for MemoryDataLayers only have tops (e.g. no bottoms).
            colTop.Add(blobData);
            colTop.Add(blobLabel);

            layer.Setup(colBottom, colTop);
            layer.AddDatumVector(rgData);


            // Run Pass 1 - memory data layer advances intern index by batch size after forward completes.
            layer.Forward(colBottom, colTop);

            float[] rgDataPass1 = colTop[0].mutable_cpu_data;
            float[] rgLabelPass1 = colTop[1].mutable_cpu_data;

            log.CHECK_EQ(rgDataPass1.Length, 60, "There should be 60 data items.");
            for (int i = 0; i < rgDataPass1.Length; i++)
            {
                log.CHECK_EQ(rgDataPass1[i], 10, "The data value should = 10.");
            }

            log.CHECK_EQ(rgLabelPass1.Length, 1, "There should only be one label, for the batch size = 1.");
            log.CHECK_EQ(rgLabelPass1[0], 0, "The label of the first item should = 0.");
            Console.WriteLine("First Pass - label = " + rgLabelPass1[0].ToString());


            // Pass 2 - memory data layer advances intern index by batch size after forward completes.
            layer.Forward(colBottom, colTop);

            float[] rgDataPass2 = colTop[0].mutable_cpu_data;
            float[] rgLabelPass2 = colTop[1].mutable_cpu_data;

            log.CHECK_EQ(rgDataPass2.Length, 60, "There should be 60 data items.");
            for (int i = 0; i < rgDataPass2.Length; i++)
            {
                log.CHECK_EQ(rgDataPass2[i], 10, "The data value should = 10.");
            }

            log.CHECK_EQ(rgLabelPass2.Length, 1, "There should only be one label, for the batch size = 1.");
            log.CHECK_EQ(rgLabelPass2[0], 1, "The label of the first item should = 1.");
            Console.WriteLine("Second Pass - label = " + rgLabelPass2[0].ToString());

            // Pass 3 - memory data layer advances intern index by batch size after forward completes.
            layer.Forward(colBottom, colTop);

            float[] rgDataPass3 = colTop[0].mutable_cpu_data;
            float[] rgLabelPass3 = colTop[1].mutable_cpu_data;

            log.CHECK_EQ(rgDataPass3.Length, 60, "There should be 60 data items.");
            for (int i = 0; i < rgDataPass3.Length; i++)
            {
                log.CHECK_EQ(rgDataPass3[i], 10, "The data value should = 10.");
            }

            log.CHECK_EQ(rgLabelPass3.Length, 1, "There should only be one label, for the batch size = 1.");
            log.CHECK_EQ(rgLabelPass3[0], 2, "The label of the first item should = 2.");
            Console.WriteLine("Third Pass - label = " + rgLabelPass3[0].ToString());

            layer.Dispose();
            blobData.Dispose();
            blobLabel.Dispose();
            cuda.Dispose();

            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        static MemoryDataLayer<float> createMemoryDataLayer(CudaDnn<float> cuda, Log log)
        {
            // Setup the MemoryDataLayer parameters.
            LayerParameter p = new LayerParameter(LayerParameter.LayerType.MEMORYDATA);
            p.memory_data_param.label_type = LayerParameterBase.LABEL_TYPE.SINGLE;
            p.memory_data_param.batch_size = 1;
            p.memory_data_param.channels = 1;
            p.memory_data_param.height = 1;
            p.memory_data_param.width = 60;

            // Create the MemoryLayer.
            MemoryDataLayer<float> layer = Layer<float>.Create(cuda, log, p, null) as MemoryDataLayer<float>;
           
            return layer;
        }

        /// <summary>
        /// The dataSetter loads the raw data on the CPU side where the data is loaded into a list of Datums.
        /// </summary>
        /// <returns>The list of Datums containing the CPU data is returned.</returns>
        static List<Datum> dataSetter()
        {
            List<Datum> rgData = new List<Datum>();
            List<byte> rgRawData = new List<byte>();

            // Load raw data with all 10's.
            for (int i = 0; i < 60; i++)
            {
                rgRawData.Add(10);
            }

            // Load a set of 10 datums with the same raw data.
            for (int i = 0; i < 10; i++)
            {
                Datum d = new Datum(false, 1, 60, 1, i, DateTime.MinValue, rgRawData, 0, false, i);
                rgData.Add(d);
            }

            return rgData;
        }
    }
}
