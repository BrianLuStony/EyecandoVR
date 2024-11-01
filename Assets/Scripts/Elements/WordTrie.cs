// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;

// checking TrieNode and WordTrie ruiliu1 5/3/23

// public class WordTrie : MonoBehaviour
// {
//     // Start is called before the first frame update
//     void Start()
//     {
        
//     }

//     // Update is called once per frame
//     void Update()
//     {
        
//     }
// }

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
// using GestureInterface;

namespace Elements
{
    public class WordTrie
    {
        private TrieNode root;
        private Queue<KeyValuePair<string, float>> currentCandidates;
        private List<TrieNode> hoverNodes;
        // private IGestureInterface gestureInterface;

        public WordTrie()
        {
            root = new TrieNode();
            currentCandidates = new Queue<KeyValuePair<string, float>>();
            hoverNodes = new List<TrieNode>();

            // var assembly = Assembly.GetExecutingAssembly();
            // using (StreamReader sr = new StreamReader(assembly.GetManifestResourceStream("Recourses.LanguagueModel.dick10k.txt")))
            // {
            //     string line;
            //     while ((line = sr.ReadLine()) != null)
            //     {
            //         string word = line.ToLower();
            //         AddWord(word);
            //     }
            // }
            string fileName = "LanguageModel/dick10k";
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
                    string word = line.ToLower();
                    AddWord(word);
                }
            }

            Debug.Log("WordTrie initialized ruiliu1");
        }

        // public void SetGestureInterface(IGestureInterface _gestureInterface)
        // {
        //     gestureInterface = _gestureInterface;
        // }

        public Queue<KeyValuePair<string, float>> UpdateCandidatesByKeys(List<KeyScore> keys, bool isScanRoot)
        {
            if (keys.Count == 0)
            {
                return currentCandidates;
            }

            keys.Sort((a, b) => b.Possibility.CompareTo(a.Possibility));

            TrieNode node = root;
            if (isScanRoot)
            {
                foreach (KeyScore ks in keys)
                {
                    char key = ks.Key;
                    float score = (float) ks.Possibility;
                    int keyIndex = key - 'a';

                    if (keyIndex >= 0 && keyIndex <= 25
                        && node.Children[keyIndex] != null && node.Children[keyIndex].NodeScore == 0)
                    {
                        node.Children[keyIndex].SetNodeScore(score, node.SumScore);
                        hoverNodes.Add(node.Children[keyIndex]);
                    }
                }
            }

            hoverNodes.Sort((a, b) => b.SumScore.CompareTo(a.SumScore));

            int hoverNodeSize = Math.Min(hoverNodes.Count, 50);
            for (int i = 0; i < hoverNodeSize; i++)
            {
                node = hoverNodes[i];
                bool isHit = false;

                foreach (KeyScore ks in keys)
                {
                    int keyIndex = ks.Key - 'a';
                    if (keyIndex < 0 || keyIndex >= 26)
                    {
                        continue;
                    }

                    if (node.Letter == ks.Key)
                    {
                        isHit = true;
                        node.UpdateNodeScoreIfLarger((float) ks.Possibility);
                        break;
                    }
                }

                if (!isHit)
                {
                    foreach (KeyScore ks in keys)
                    {
                        int keyIndex = ks.Key - 'a';
                        char nextKey = ks.Key;
                        float nextScore = (float) ks.Possibility;
                        if (keyIndex >= 0 && keyIndex <= 25 && node.Children[keyIndex] != null && node.Children[keyIndex].NodeScore == 0)
                        {
                            node.Children[keyIndex].SetNodeScore(nextScore, node.SumScore);
                            hoverNodes.Add(node.Children[keyIndex]);
                        }
                    }
                }
                if (node.Text.Count > 0)
                {
                    foreach (KeyValuePair<string, float> p in currentCandidates.ToList())
                    {
                        foreach (string s in node.Text)
                        {
                            if (s.Equals(p.Key))
                            {
                                currentCandidates = new Queue<KeyValuePair<string, float>>(currentCandidates.Where(x => !x.Key.Equals(p.Key)));
                            }
                        }
                    }
                    if (!isHit)
                        node.MisCount++;
                    if (node.MisCount < 100)
                    {
                        foreach (string str in node.Text)
                        {
                            currentCandidates.Enqueue(new KeyValuePair<string, float>(str, node.SumScore));
                        }
                    }

                }
            }
            return currentCandidates;
        }

        public void AddWord(string word)
        {
            if (!IsAlpha(word))
            {
                return;
            }
            TrieNode node = root;
            int i = 0;
            for (i = 0; i < word.Length; i++)
            {
                if (i + 1 < word.Length && word[i + 1] == word[i])
                {
                    continue;
                }
                char c = word[i];
                int index = c - 'a';
                if (node.Children[index] == null)
                {
                    node.Children[index] = new TrieNode();
                    node.Children[index].Parent = node;
                    node.Children[index].Length = i + 1;
                    node.Children[index].Letter = c;
                }
                node = node.Children[c - 'a'];
            }

            node.Text.Add(word);
        } 

        public void Clear()
        {
            foreach (TrieNode node in hoverNodes)
            {
                node.ClearStatus();
            }
            hoverNodes.Clear();
            currentCandidates.Clear();
        }

        private bool IsAlpha(string name)
        {
            char[] chars = name.ToCharArray();

            foreach (char c in chars)
            {
                if (!Char.IsLetter(c))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
