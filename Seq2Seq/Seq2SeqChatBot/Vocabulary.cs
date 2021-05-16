using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqChatBot
{
    public class Vocabulary
    {
        Dictionary<string, int> m_rgDictionary = new Dictionary<string, int>();
        Dictionary<string, int> m_rgWordToIndex = new Dictionary<string, int>();
        Dictionary<int, string> m_rgIndexToWord = new Dictionary<int, string>();
        List<string> m_rgstrVocabulary = new List<string>();

        public Vocabulary()
        {
        }

        public int WordToIndex(string strWord)
        {
            if (!m_rgWordToIndex.ContainsKey(strWord))
                throw new Exception("I do not know the word '" + strWord + "'!");

            return m_rgWordToIndex[strWord];
        }

        public string IndexToWord(int nIdx)
        {
            if (!m_rgIndexToWord.ContainsKey(nIdx))
                return "";

            return m_rgIndexToWord[nIdx];
        }

        public int VocabularCount
        {
            get { return m_rgstrVocabulary.Count; }
        }

        public void Load(List<List<string>> rgrgstrInput, List<List<string>> rgrgstrTarget)
        {
            m_rgDictionary = new Dictionary<string, int>();

            // Count up all words.
            for (int i = 0; i < rgrgstrInput.Count; i++)
            {
                for (int j = 0; j < rgrgstrInput[i].Count; j++)
                {
                    string strWord = rgrgstrInput[i][j];

                    if (!m_rgDictionary.ContainsKey(strWord))
                        m_rgDictionary.Add(strWord, 1);
                    else
                        m_rgDictionary[strWord]++;
                }

                for (int j = 0; j < rgrgstrTarget[i].Count; j++)
                {
                    string strWord = rgrgstrTarget[i][j];

                    if (!m_rgDictionary.ContainsKey(strWord))
                        m_rgDictionary.Add(strWord, 1);
                    else
                        m_rgDictionary[strWord]++;
                }
            }

            // NOTE: Start at one to save room for START and END tokens where
            // START = 0 in the model word vectors and 
            // END = 0 in the next word softmax.
            int nIdx = 2;
            foreach (KeyValuePair<string, int> kv in m_rgDictionary)
            {
                if (kv.Value > 0)
                {
                    // Add word to vocabulary.
                    m_rgWordToIndex[kv.Key] = nIdx;
                    m_rgIndexToWord[nIdx] = kv.Key;
                    m_rgstrVocabulary.Add(kv.Key);
                    nIdx++;
                }
            }
        }
    }
}
