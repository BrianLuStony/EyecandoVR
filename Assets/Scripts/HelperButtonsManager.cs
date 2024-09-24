using System.Collections.Generic;
using UnityEngine;
using Elements;

namespace Controller
{
    public class HelperButtonsManager : MonoBehaviour
    {
        public List<OnScreenButton> helperButtons;

        void Start()
        {
            helperButtons = new List<OnScreenButton>();
            SetupHelperButtons();
            Debug.Log("There are " + helperButtons.Count + " helper buttons.");
        }

        private void SetupHelperButtons()
        {
            // Add your helper buttons here
            AddHelperButton("StopButton");
            AddHelperButton("Normal");
            AddHelperButton("GlanceWriter");
            AddHelperButton("SpeakButton");
            // Add more helper buttons as needed
        }

        private void AddHelperButton(string buttonName)
        {
            GameObject buttonObj = GameObject.Find(buttonName);
            if (buttonObj != null)
            {
                OnScreenButton helperButton = new OnScreenButton(buttonObj, buttonName, OnScreenButton.ButtonType.FUNCTIONAL);
                helperButtons.Add(helperButton);
            }
            else
            {
                Debug.LogWarning($"Helper button '{buttonName}' not found in the scene.");
            }
        }

        public List<OnScreenButton> GetHelperButtons()
        {
            return helperButtons;
        }
    }
}