using MyCaffe.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CudaDnnTest
{
    class Program
    {
        static void Main(string[] args)
        {
            CudaDnn<float> cuda = new CudaDnn<float>(0);

            List<long> rghGpuMem = new List<long>();
            long lOffset = 0;

            // This memory will reside on the GPU.
            long hGpuMem = 0;

            // You must first allocate the GPU memory to use.
            // Below we will allocate an array of 1000 float values.
            hGpuMem = cuda.AllocMemory(1000);
            cuda.set(1000, hGpuMem, 0.0);

            for (int i = 0; i < 10; i++)
            {
                long hMem1 = cuda.CreateMemoryPointer(hGpuMem, lOffset, 100);
                cuda.set(100, hMem1, (double)(i + 1));
                rghGpuMem.Add(hMem1);
                lOffset += 100;
            }

            for (int i = 0; i < 10; i++)
            {
                long hMem1 = rghGpuMem[i];
                float[] rgData = cuda.GetMemoryFloat(hMem1);

                if (rgData.Length != 100)
                    throw new Exception("The data length should = 100!");

                for (int j = 0; j < 100; j++)
                {
                    if (rgData[j] != (float)(i + 1))
                        throw new Exception("The data at index " + j.ToString() + " is not correct!");
                }
            }

            // Clean-up and release all GPU memory used.
            cuda.FreeMemory(hGpuMem);
            hGpuMem = 0;

            cuda.Dispose();
        }
    }
}
