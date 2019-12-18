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
            // Create the CudaDnn connection used.  NOTE: only one CudaDnn connection is needed 
            // per thread for each instance creates and manages its own low-level kernel state
            // which includes all memory allocated etc.  All memory handles allocated should
            // be used with the CudaDnn that allocated the memory.
            CudaDnn<float> cuda = new CudaDnn<float>(0, DEVINIT.CUBLAS | DEVINIT.CURAND);

            // Perform the addition test, passing in the main cuda instance.
            float fVal1 = 1.0f;
            float fVal2 = 5.0f;
            float fResult = myBlobAdditionTest(cuda, log, fVal1, fVal2)[0];
            Console.WriteLine("Result of " + fVal1.ToString() + " + " + fVal2.ToString() + " = " + fResult.ToString());

            cuda.Dispose();
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
    }
}
