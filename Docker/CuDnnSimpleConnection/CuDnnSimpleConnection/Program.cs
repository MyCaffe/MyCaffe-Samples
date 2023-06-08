using MyCaffe.common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuDnnSimpleConnection
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CudaDnn<float> cuda = new CudaDnn<float>(0, (DEVINIT)3);

            int nDevCount = cuda.GetDeviceCount();
            for (int i = 0; i < nDevCount; i++)
            {
                Console.WriteLine("Device #" + i.ToString() + " = " + cuda.GetDeviceName(i));
            }
        }
    }
}
