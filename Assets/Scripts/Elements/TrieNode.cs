using System.Collections;
using UnityEngine;

// checking TrieNode and WordTrie ruiliu1 5/3/23


// public class NewBehaviourScript : MonoBehaviour
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

    public class TrieNode
    {
        public TrieNode Parent = null;
        public TrieNode[] Children = new TrieNode[26];
        public char Letter = '\0';
        public  float NodeScore = 0.0f;
        public  float SumScore = 0.0f;
        public int Length = 0;
        public int MisCount = 0;
        public List<string> Text = new List<string>();

        public void ClearStatus()
        {
            NodeScore = 0;
            MisCount = 0;
            SumScore = 0;
        }

        public void SetNodeScore(float score, float prevSumScore)
        {
            NodeScore = score;
            SumScore = score + prevSumScore;
        }

        public void UpdateNodeScoreIfLarger(float score)
        {
            if (score > NodeScore)
            {
                SumScore = SumScore - NodeScore + score;
                NodeScore = score;
            }
        }
    }
}