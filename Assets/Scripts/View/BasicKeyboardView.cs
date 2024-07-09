using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
// using System.Drawing;
// using System.Windows;
// using VirtualKey = Elements.VirtualKey;

// checked

namespace View {

    public class BasicKeyboardView : MonoBehaviour
    {
        private RectTransform keyboardRectTransform;
        // public static float[] keyXArr = new float[26];
        // public static float[] keyYArr = new float[26];
        public List<OnScreenButton> virtualKeys;
        public List<OnScreenButton> functionalKeys;
        public float SPLIT_LINE_Y_UPPER;
        public float SPLIT_LINE_Y_LOWER;
        public GameObject divider;
        //public GameObject textConsole;



        // Start is called before the first frame update
        void Start()
        {
          virtualKeys = new List<OnScreenButton>();
          functionalKeys = new List<OnScreenButton>();
          SetupKeyboard();
          Debug.Log("There are " + functionalKeys.Count + " functional keys.");
        }

        // Update is called once per frame
        void Update()
        {

        }

        void GetDividerInfo()
        {
          RectTransform dividerRectTransform = divider.GetComponent<RectTransform>();
          Vector3[] dividerWorldCorners = new Vector3[4];
          dividerRectTransform.GetWorldCorners(dividerWorldCorners);
          SPLIT_LINE_Y_LOWER = dividerWorldCorners[0].y;
          SPLIT_LINE_Y_UPPER = dividerWorldCorners[2].y;
        }

        private void SetupKeyboard()
        {
          for (char i = 'a'; i <= 'z'; i++){
            GameObject keyObj = GameObject.Find(i.ToString());
            OnScreenButton keyButton = new OnScreenButton(keyObj, i.ToString(), OnScreenButton.ButtonType.KEY);
            virtualKeys.Add(keyButton);
          }

          GameObject deleteObject = GameObject.Find("delete");
          OnScreenButton deleteButton = new OnScreenButton(deleteObject, "delete", OnScreenButton.ButtonType.FUNCTIONAL);
          //textConsole.GetComponent<TextMeshPro>().SetText("here" + deleteButton);
          functionalKeys.Add(deleteButton);
          GameObject enterObject = GameObject.Find("enter");
          OnScreenButton enterButton = new OnScreenButton(enterObject, "enter", OnScreenButton.ButtonType.FUNCTIONAL);
          functionalKeys.Add(enterButton);
          GameObject switchObject = GameObject.Find("SwitchModality");
          OnScreenButton switchButton = new OnScreenButton(switchObject, "SwitchModality", OnScreenButton.ButtonType.FUNCTIONAL);
          functionalKeys.Add(switchButton);

        }

        public List<OnScreenButton> GetFunctionalKeys(){
          return functionalKeys;
        }

        public List<OnScreenButton> GetVirtualKeys(){
          return virtualKeys;
        }
    }

}
