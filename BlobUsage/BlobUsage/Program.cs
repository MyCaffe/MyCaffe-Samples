using MyCaffe.basecode;
using MyCaffe.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// These samples show how to use blobs.
/// </summary>
namespace BlobUsage
{
    /// <summary>
    /// This sample demonstrates how load a blob with data and performa simple addition.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Create the output log used.
            Log log = new Log("Test");
            log.OnWriteLine += Log_OnWriteLine;

            // Create the CudaDnn connection used.  NOTE: only one CudaDnn connection is needed 
            // per thread for each instance creates and manages its own low-level kernel state
            // which includes all memory allocated etc.  All memory handles allocated should
            // be used with the CudaDnn that allocated the memory.
            CudaDnn<float> cuda = new CudaDnn<float>(0, DEVINIT.CUBLAS | DEVINIT.CURAND);

            log.WriteLine("CudaDnn created.");

            // Run super simple sample.
            runSuperSimpleSample(cuda, log);

            // Run Blob sample #1
            runSimpleBlobExample1(cuda, log);

            // Run Blob sample #2
            runSimpleBlobExample2(cuda, log);

            // Run Blob sample #3
            runSimpleBlobExample3(cuda, log);

            // Release all GPU memory and other state data used.
            cuda.Dispose();

            Console.WriteLine("Success!");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static void runSuperSimpleSample(CudaDnn<float> cuda, Log log)
        {
            // Create Blob, allocating 1 x 1 x 1 x 2 floats of GPU memory.
            Blob<float> blob = new Blob<float>(cuda, log, 1, 1, 1, 2);

            // Transfer data from CPU to GPU.
            blob.mutable_cpu_data = new float[] { 1.0f, 2.0f };
            blob.mutable_cpu_diff = new float[] { 0.5f, 0.5f };

            // Blob gpu_data = gpu_data - gpu_diff
            cuda.sub(blob.count(), blob.gpu_data, blob.gpu_diff, blob.mutable_gpu_data);

            // Transfer data from GPU to CPU.
            float[] rgResult = blob.mutable_cpu_data;

            Console.WriteLine("1.0 - 0.5 = " + rgResult[0].ToString());
            Console.WriteLine("2.0 - 0.5 = " + rgResult[1].ToString());

            blob.Dispose();
        }

        private static void Log_OnWriteLine(object sender, LogArg e)
        {
            string strMsg = e.Message;
            // output message to user.
        }

        //=====================================================================
        //  Simple Blob Example #1
        //=====================================================================
        public static void runSimpleBlobExample1(CudaDnn<float> cuda, Log log)
        {
            // Perform the addition test, passing in the main cuda instance.
            float fVal1 = 1.0f;
            float fVal2 = 5.0f;
            float fResult = myBlobAdditionTest(cuda, log, fVal1, fVal2)[0];
            Console.WriteLine("Result of " + fVal1.ToString() + " + " + fVal2.ToString() + " = " + fResult.ToString());
        }


        public static Blob<float> CuSca(CudaDnn<float> cuda, Log log, float fInput)
        {
            float[] rgInput = new float[1];
            rgInput[0] = fInput;

            // Load a simple datum.
            SimpleDatum myData = new SimpleDatum(1, 1, 1, rgInput, 0, 1);

            // Load the blob, which transfers the cpu data to the gpu.
            // NOTE: Alternatively, the data can be transferred to the gpu
            // by using a call to Reshape (which allocates the GPU memory)
            // and then calling blob.mutable_cpu_data = rgInput;
            return new Blob<float>(cuda, log, myData, true, true, false);
        }

        public static float[] myBlobAdditionTest(CudaDnn<float> cuda, Log log, float fInput1, float fInput2)
        {
            // Create the blobs and load their input data.
            Blob<float> scalar1 = CuSca(cuda, log, fInput1);
            Console.WriteLine("Scalar 1 gpu_data = {0}", scalar1.gpu_data);
            Blob<float> scalar2 = CuSca(cuda, log, fInput2);
            Console.WriteLine("Scalar 2 gpu_data = {0}", scalar2.gpu_data);

            // Do the add.
            Blob<float> blobResult = scalar2.Clone();
            cuda.add(scalar1.count(), scalar1.gpu_data, scalar2.gpu_data, blobResult.mutable_gpu_data);

            // Transfer the data back to CPU memory.
            float[] rgRes = blobResult.mutable_cpu_data;

            // Free up any resources used (including any GPU memory used).
            scalar1.Dispose();
            scalar2.Dispose();
            blobResult.Dispose();

            // Return the result.
            return rgRes;
        }

        //=====================================================================
        //  Simple Blob Example #2 - performing a simple addition
        //=====================================================================
        public static void runSimpleBlobExample2(CudaDnn<float> cuda, Log log)
        {
            float[] rgInput = new float[3];
            rgInput[0] = 1.0f;
            rgInput[1] = 2.0f;
            rgInput[2] = 3.0f;

            // Load SimpleDatum which holds data in CPU memory.
            SimpleDatum data = new SimpleDatum(1, 3, 1, rgInput, 0, 3);

            // Load Blob which holds data in GPU memory.
            Blob<float> blob = new Blob<float>(cuda, log, data, true);

            // Blob gpu_data holds the handle to the GPU data memory 
            // (which actually resides in the low-level CudaDnnDll)
            long hData = blob.gpu_data;

            // Blob gpu_diff holds the handle to the GPU diff memory 
            // (which also resides in the low-level CudaDnnDll)
            long hDiff = blob.gpu_diff;

            // Set all diff values to 1.0f
            blob.SetDiff(1.0);

            // Use CudaDnn to add the data = data + diff.
            cuda.add(blob.count(), hData, hDiff, hData);

            // Transfer the data from the GPU back to the CPU.
            float[] rgResult = blob.mutable_cpu_data;

            log.CHECK_EQ(rgResult[0], 1.0f + 1.0f, "incorrect values.");
            Console.WriteLine("1.0 + 1.0 = " + rgResult[0].ToString());

            log.CHECK_EQ(rgResult[1], 2.0f + 1.0f, "incorrect values.");
            Console.WriteLine("2.0 + 1.0 = " + rgResult[1].ToString());

            log.CHECK_EQ(rgResult[2], 3.0f + 1.0f, "incorrect values.");
            Console.WriteLine("3.0 + 1.0 = " + rgResult[2].ToString());

            // Free all GPU memory used.
            blob.Dispose();
        }

        //=====================================================================
        //  Simple Blob Example #3 - performing a simple addition w/o SimpelDatum
        //=====================================================================
        public static void runSimpleBlobExample3(CudaDnn<float> cuda, Log log)
        {
            float[] rgInput = new float[3];
            rgInput[0] = 1.0f;
            rgInput[1] = 2.0f;
            rgInput[2] = 3.0f;

            // Load Blob which holds data in GPU memory.
            Blob<float> blob = new Blob<float>(cuda, log, true);

            // Reshape the blob (to allocate the GPU memory to
            // match the size of the CPU data that will be copied)
            blob.Reshape(1, 1, 1, 3);

            // Transfer the CPU data to the GPU data of Blob's data
            blob.mutable_cpu_data = rgInput;

            // Blob gpu_data holds the handle to the GPU data memory 
            // (which actually resides in the low-level CudaDnnDll)
            long hData = blob.gpu_data;

            // Blob gpu_diff holds the handle to the GPU diff memory 
            // (which also resides in the low-level CudaDnnDll)
            long hDiff = blob.gpu_diff;

            // Set all diff values to 1.0f
            blob.SetDiff(1.0);

            // Use CudaDnn to add the data = data + diff.
            cuda.add(blob.count(), hData, hDiff, hData);

            // Transfer the data from the GPU back to the CPU.
            float[] rgResult = blob.mutable_cpu_data;

            log.CHECK_EQ(rgResult[0], 1.0f + 1.0f, "incorrect values.");
            Console.WriteLine("1.0 + 1.0 = " + rgResult[0].ToString());

            log.CHECK_EQ(rgResult[1], 2.0f + 1.0f, "incorrect values.");
            Console.WriteLine("2.0 + 1.0 = " + rgResult[1].ToString());

            log.CHECK_EQ(rgResult[2], 3.0f + 1.0f, "incorrect values.");
            Console.WriteLine("3.0 + 1.0 = " + rgResult[2].ToString());

            // Free all GPU memory used.
            blob.Dispose();
        }
    }
}
