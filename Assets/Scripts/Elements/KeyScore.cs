using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// checked

namespace Elements
{
    public class KeyScore
    {
        public char Key { get; set; }
        // Key score is probability
        public float Possibility { get; set; }

        public KeyScore(char key, float possibility)
        {
            Key = key;
            Possibility = possibility;
        }

        public override string ToString()
        {
            return $"\nGKey{{ key={Key}, p={Possibility} }}";
        }
    }
}