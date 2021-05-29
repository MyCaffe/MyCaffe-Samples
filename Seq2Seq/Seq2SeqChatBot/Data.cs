using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqChatBot
{
    public class Data
    {
        Random m_random = new Random((int)DateTime.Now.Ticks);
        List<List<string>> m_rgInput;
        List<List<string>> m_rgOutput;
        int m_nCurrentSequence = -1;
        int m_nCurrentOutputIdx = 0;
        int m_nIxInput = 1;
        int m_nIterations = 0;
        Vocabulary m_vocab;

        public Data(List<List<string>> rgInput, List<List<string>> rgOutput, Vocabulary vocab)
        {
            m_vocab = vocab;
            m_rgInput = rgInput;
            m_rgOutput = rgOutput;
        }

        public Vocabulary Vocabulary
        {
            get { return m_vocab; }
        }

        public int VocabularyCount
        {
            get { return m_vocab.VocabularCount; }
        }

        public Tuple<List<int>, int> GetInputData()
        {
            List<int> rgInput = new List<int>();
            foreach (string str in m_rgInput[0])
            {
                rgInput.Add(m_vocab.WordToIndex(str));
            }

            return new Tuple<List<int>, int>(rgInput, 1);
        }

        public Tuple<List<int>, int, int, int> GetNextData(out bool bNewEpoch, out bool bNewSequence, ref int nOutputCount)
        {
            int nDecClip = 1;

            bNewSequence = false;
            bNewEpoch = false;

            if (m_nCurrentSequence == -1)
            {
                m_nIterations++;
                bNewSequence = true;
                m_nCurrentSequence = m_random.Next(m_rgInput.Count);
                nOutputCount = m_rgOutput[m_nCurrentSequence].Count;
                nDecClip = 0;

                if (m_nIterations == m_rgOutput.Count)
                {
                    bNewEpoch = true;
                    m_nIterations = 0;
                }
            }

            List<string> rgstrInput = m_rgInput[m_nCurrentSequence];
            List<int> rgInput = new List<int>();
            foreach (string str in rgstrInput)
            {
                rgInput.Add(m_vocab.WordToIndex(str));
            }

            int nIxTarget = 0;

            if (m_nCurrentOutputIdx < m_rgOutput[m_nCurrentSequence].Count)
            {
                string strTarget = m_rgOutput[m_nCurrentSequence][m_nCurrentOutputIdx];
                nIxTarget = m_vocab.WordToIndex(strTarget);
            }

            Tuple<List<int>, int, int, int> data = new Tuple<List<int>, int, int, int>(rgInput, m_nIxInput, nIxTarget, nDecClip);
            m_nIxInput = nIxTarget;

            m_nCurrentOutputIdx++;

            if (m_nCurrentOutputIdx == m_rgOutput[m_nCurrentSequence].Count)
            {
                m_nCurrentSequence = -1;
                m_nCurrentOutputIdx = 0;
                m_nIxInput = 1;
            }

            return data;
        }
    }
}
