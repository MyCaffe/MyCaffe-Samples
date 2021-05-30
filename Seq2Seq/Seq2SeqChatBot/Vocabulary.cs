using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seq2SeqChatBot
{
    /// <summary>
    /// The Vocabulary object manages the overall word dictionary and word to index and index to word mappings.
    /// </summary>
    public class Vocabulary
    {
        Dictionary<string, int> m_rgDictionary = new Dictionary<string, int>();
        Dictionary<string, int> m_rgWordToIndex = new Dictionary<string, int>();
        Dictionary<int, string> m_rgIndexToWord = new Dictionary<int, string>();
        List<string> m_rgstrVocabulary = new List<string>();

        /// <summary>
        /// The constructor.
        /// </summary>
        public Vocabulary()
        {
        }

        /// <summary>
        /// The WordToIndex method maps a word to its corresponding index value.
        /// </summary>
        /// <param name="strWord">Specifies the word to map.</param>
        /// <returns>The word index is returned.</returns>
        public int WordToIndex(string strWord)
        {
            if (!m_rgWordToIndex.ContainsKey(strWord))
                throw new Exception("I do not know the word '" + strWord + "'!");

            return m_rgWordToIndex[strWord];
        }

        /// <summary>
        /// The IndexToWord method maps an index value to its corresponding word.
        /// </summary>
        /// <param name="nIdx">Specifies the index value.</param>
        /// <returns>The word corresponding to the index is returned.</returns>
        public string IndexToWord(int nIdx)
        {
            if (!m_rgIndexToWord.ContainsKey(nIdx))
                return "";

            return m_rgIndexToWord[nIdx];
        }

        /// <summary>
        /// Returns the number of words in the vocabulary.
        /// </summary>
        public int VocabularCount
        {
            get { return m_rgstrVocabulary.Count; }
        }

        /// <summary>
        /// Loads the word to index mappings.
        /// </summary>
        /// <param name="rgrgstrInput">Specifies the input sentences where each inner array is one sentence of words.</param>
        /// <param name="rgrgstrTarget">Specifies the target sentences where each inner array is one sentence of words.</param>
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
