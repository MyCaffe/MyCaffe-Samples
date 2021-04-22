using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqImageToSin
{
    /// <summary>
    /// The Signal class is used to generate the Sin curve.
    /// </summary>
    /// <remarks>
    /// The Sin curve generation is a re-write from a portion of the Caffe-LSTM-Mini-Tutorial which demonstrates how to use an LSTM model to learn a generated Sin curve.
    /// @see [Corvus/Caffe-LSTM-Mini-Tutorial](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial) open-source project distributed under
    /// the [GNU License](https://github.com/CorvusCorax/Caffe-LSTM-Mini-Tutorial/blob/master/LICENSE).
    /// </remarks>
    public class Signal
    {
        /// <summary>
        /// The constructor.
        /// </summary>
        public Signal()
        {
        }

        /// <summary>
        /// Create the sample data.
        /// </summary>
        /// <param name="f">The frequency to use for all time series or null to randomize.</param>
        /// <param name="t0">The time offset to use for all time series or null to randomize.</param>
        /// <param name="nBatch">The number of time series to generate.</param>
        /// <param name="nPredict">The number of future samples to generate.</param>
        /// <param name="nSamples">The number of past (and current) samples to generate.</param>
        /// <returns>A dictionary containing the data is returned.</returns>
        public static Dictionary<string, float[]> GenerateSample(float? f = 1.0f, float? t0 = null, int nBatch = 1, int nPredict = 50, int nSamples = 100)
        {
            Dictionary<string, float[]> data = new Dictionary<string, float[]>();
            float[] rgfT = new float[nBatch * nSamples];
            float[] rgfY = new float[nBatch * nSamples];
            float[] rgfFT = new float[nBatch * nPredict];
            float[] rgfFY = new float[nBatch * nPredict];
            Random random = new Random((int)DateTime.Now.Ticks);

            float? fT0 = t0;
            float fFs = 100.0f;

            for (int i = 0; i < nBatch; i++)
            {
                float[] rgft = arrange(0, nSamples + nPredict, fFs);

                if (!fT0.HasValue)
                    t0 = (float)(random.NextDouble() * 2 * Math.PI);
                else
                    t0 = fT0.Value + i / (float)nBatch;

                float? freq = f;
                if (!freq.HasValue)
                    freq = (float)(random.NextDouble() * 3.5 + 0.5);

                float[] rgY = createSample(freq.Value, rgft, t0.Value);

                Array.Copy(rgft, 0, rgfT, i * nSamples, nSamples);
                Array.Copy(rgY, 0, rgfY, i * nSamples, nSamples);
                Array.Copy(rgft, nSamples, rgfFT, i * nPredict, nPredict);
                Array.Copy(rgY, nSamples, rgfFY, i * nPredict, nPredict);
            }

            data.Add("T", rgfT);
            data.Add("Y", rgfY);
            data.Add("FT", rgfFT);
            data.Add("FY", rgfFY);

            List<float> rgTAll = new List<float>();
            rgTAll.AddRange(rgfT);
            rgTAll.AddRange(rgfFT);

            data.Add("T-full", rgTAll.ToArray());

            List<float> rgYAll = new List<float>();
            rgYAll.AddRange(rgfY);
            rgYAll.AddRange(rgfFY);

            data.Add("Y-full", rgYAll.ToArray());

            return data;
        }

        /// <summary>
        /// Create a single Sin sample.
        /// </summary>
        /// <param name="fFreq">Specifies the frequency.</param>
        /// <param name="rgft">Specifies the time step.</param>
        /// <param name="fT0">Specifies the initial time step.</param>
        /// <returns></returns>
        private static float[] createSample(float fFreq, float[] rgft, float fT0)
        {
            float[] rg = new float[rgft.Length];

            for (int i = 0; i < rg.Length; i++)
            {
                rg[i] = (float)Math.Sin(2 * Math.PI * fFreq * (rgft[i] + fT0));
            }

            return rg;
        }

        private static float[] arrange(int nStart, int nEnd, float fScale)
        {
            float[] rg = new float[nStart + nEnd];

            for (int i = 0; i < rg.Length; i++)
            {
                rg[i] = i / fScale;
            }

            return rg;
        }
    }
}
