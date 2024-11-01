using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
// using System.Windows.Forms;

namespace Controller
{

    public class SuggestionView: MonoBehaviour
    {
        private List<KeyValuePair<string, float>> Suggestions;
        private string BestCandidate;

        private List<OnScreenButton> suggestionButtons;
        
        void Start()
        {
            suggestionButtons = new List<OnScreenButton>();
            Initialize();
            DisplaySuggestion();
        }

        void Update()
        {
            // DisplaySuggestion();
        }

        private void Initialize() {
            Suggestions = new List<KeyValuePair<string, float>>();
            for(int i = 1; i <= 5; ++i) {
                GameObject suggestionObject = GameObject.Find("Suggestion"+i);
                OnScreenButton suggestionButton = new OnScreenButton(suggestionObject, "Suggestion"+i, OnScreenButton.ButtonType.SUGGESTION);
                suggestionButtons.Add(suggestionButton);
            }
        }

        public void UpdateSuggestion(List<KeyValuePair<string, float>> newSuggestions)
        {
            Suggestions = new List<KeyValuePair<string, float>>(newSuggestions);
            if (Suggestions.Count > 0)
            {
                BestCandidate = Suggestions[0].Key;
            }
            DisplaySuggestion();
        }

        public void DisplaySuggestion()
        {
            for (int i = 0; i < 5; ++i) {
                // Usage of Suggestions[i+1], we put the 2nd, 3rd, ... suggestions in the suggestion area, and input the 1st suggestion by default
                if (i < Suggestions.Count && Suggestions[i+1].Key.Length > 0) {
                    Debug.Log(suggestionButtons[i].GetGameObject());
                    suggestionButtons[i].GetGameObject().GetComponentInChildren<TMP_Text>().text = Suggestions[i+1].Key;
                    suggestionButtons[i].GetGameObject().SetActive(true);
                } else {
                    suggestionButtons[i].GetGameObject().SetActive(false);
                }
            }
        }

        public string GetBestCandidate()
        {
            return BestCandidate;
        }

        public List<KeyValuePair<string, float>> GetSuggestions()
        {
            return new List<KeyValuePair<string, float>>(Suggestions);
        }

        public void RemoveSuggestion()
        {
            Suggestions = new List<KeyValuePair<string, float>>();
            for (int i = 0; i < 5; ++i) {
                suggestionButtons[i].GetGameObject().SetActive(false);
            }
        }

        public List<OnScreenButton> GetSuggestionButtons()
        {
            return suggestionButtons;
        }
    }
}
