using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
// public class GlanceWriterDecoder : MonoBehaviour
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
using System.Drawing;
using Elements;
using TailCurve = Utils.TailCurve;
// using Decoder.Interfaces;

namespace Decoder
{
    public class GlanceWriterDecoder
    {
        private List<Vector3> trace;
        private List<OnScreenButton> keys;
        private WordTrie wordTrie;
        private bool isScanRoot;
        private BigramModel bigramModel;
        private List<KeyValuePair<string, float>> decodeResult;

        public GameObject textConsole;

        public GlanceWriterDecoder(List<OnScreenButton> _keys, GameObject textConsole)
        {
            trace = new List<Vector3>();
            keys = _keys;
            wordTrie = new WordTrie();
            bigramModel = new BigramModel();
            decodeResult = new List<KeyValuePair<string, float>>();
            isScanRoot = true;
            textConsole = textConsole;
        }

        void printDebug(string info)
        {
            textConsole.GetComponent<TextMeshPro>().SetText(info);
        }

        public float UpdateGesture(Vector3 point, string preText)
        {
            float pointScore = 0.0f;
            float filterScore = 0.0f;
            if (point.y > 0)
            {
                trace.Clear();
            }
            else
            {
                trace.Add(point);
                pointScore = TailCurve.GetTailStability(trace);
                filterScore = TailCurve.GetTailCurve(trace);

                if (filterScore > 0.2f)
                {
                    isScanRoot = false;
                }
            }
            List<KeyScore> keyScores = GetLettersByPoint(point, pointScore, keys);
            
            List<KeyValuePair<string, float>> res = new List<KeyValuePair<string, float>>(
                    wordTrie.UpdateCandidatesByKeys(keyScores, true));
            res.Sort((a, b) => b.Value.CompareTo(a.Value));

            List<KeyValuePair<string, KeyValuePair<float, float>>> rawResult = new List<KeyValuePair<string, KeyValuePair<float, float>>>();

            float sumStateScore = 0.0f, sumLanguageScore = 0.0f;
            int limit = Math.Min(res.Count, 10);
            for (int i = 0; i < limit; i++)
            {
                KeyValuePair<string, float> p = res[i];
                float stateScore = p.Value;
                float lmScore = bigramModel.GetBigramScore(GetPreWord(preText), p.Key);
                sumStateScore += stateScore;
                sumLanguageScore += lmScore;
                rawResult.Add(new KeyValuePair<string, KeyValuePair<float, float>>(p.Key, new KeyValuePair<float, float>(stateScore, lmScore)));
            }
            decodeResult.Clear();
            float ALPHA = 0.9f;
            for (int i = 0; i < limit; i++)
            {
                float stateScore = rawResult[i].Value.Key;
                float languageScore = rawResult[i].Value.Value;

                // float combineScore = stateScore * (float) Math.Pow(languageScore, 0.3f);
                // float combineScore = (float) Math.Pow(stateScore, ALPHA) * (float) Math.Pow(languageScore, 1 - ALPHA);
                
                float combineScore = ALPHA * stateScore / sumStateScore + (1 - ALPHA) * languageScore / sumLanguageScore;
                decodeResult.Add(new KeyValuePair<string, float>(rawResult[i].Key, combineScore));
            }
            decodeResult.Sort((a, b) => b.Value.CompareTo(a.Value));

            return filterScore;
        }

        private List<KeyScore> GetLettersByPoint(Vector3 point, float gazeScore, List<OnScreenButton> vkeys)
        {
            List<KeyScore> passingKeys = new List<KeyScore>();
            float x = point.x, y = point.y;

            foreach (OnScreenButton vkey in vkeys)
            {
                if (vkey.ContainPointWithPadding(x, y, 0.1f, 0.15f))
                {
                    float diagDiatance = vkey.GetDiagDistanceToKey(x, y);
                    passingKeys.Add(new KeyScore(vkey.mButtonName[0], (float) GaussianScore(diagDiatance, 0f, 0.4f) * gazeScore));
                }
            }
            return passingKeys;
        }

        public List<KeyValuePair<string, float>> GetDecodeResult()
        {
            return decodeResult;
        }

        public void StopDecoding()
        {
            isScanRoot = true;
            wordTrie.Clear();
        }

        public static float GaussianScore(float value, float u, float sigma)
        {
            float x = (value - u) / sigma;
            return (float) Math.Exp(-x * x / 2) / (float) Math.Sqrt(2 * Math.PI) / (float) sigma;
        }

        private string GetPreWord(string preText)
        {
            string[] words = preText.Trim().Split(' ');
            int n = words.Length;
            return n > 0 ? words[n - 1] : "";
        }
    }
}
