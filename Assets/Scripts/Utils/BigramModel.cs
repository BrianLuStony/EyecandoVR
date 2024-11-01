using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace Elements
{
    public class BigramModel
    {
        // private static readonly string Tag = typeof(BigramModel).Name; // no usage
        private Dictionary<string, Dictionary<string, float>> BigramMap { get; set; }

        public BigramModel()
        {
            // Build bigram map
            BigramMap = new Dictionary<string, Dictionary<string, float>>();
            string fileName = "LanguageModel/sorted_bigram";

            TextAsset textAsset = Resources.Load<TextAsset>(fileName);

            if (textAsset == null)
            {
                throw new FileNotFoundException($"Resource '{fileName}' not found.");
            }

            using (StringReader sr = new StringReader(textAsset.text))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    string text = line.ToLower();
                    string[] words = text.Split('\t');
                    if (words.Length == 3)
                    {
                        if (!BigramMap.ContainsKey(words[0]))
                        {
                            BigramMap[words[0]] = new Dictionary<string, float>();
                        }
                        if (!BigramMap[words[0]].ContainsKey(words[1]))
                        {
                            BigramMap[words[0]][words[1]] = float.Parse(words[2]);
                        }
                    }
                }
            }
        }

        public float GetBigramScore(string str1, string str2)
        {
            if (str1.Length == 0)
            {
                str1 = "_HEAD_";
            }

            if (BigramMap.TryGetValue(str1, out Dictionary<string, float> subitem))
            {
                if (subitem.TryGetValue(str2, out float score))
                {
                    return score;
                }
                else
                {
                    return subitem["_minimum_"];
                }
            }

            return 1;
        }
    }
}
