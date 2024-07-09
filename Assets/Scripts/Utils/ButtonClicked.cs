using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ButtonClicked : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject decoderObj;
    public GameObject keyboardObj;
    public GameObject controllerObj;
    public GameObject suggestionObj;
    private Controller.GlanceWriterController decoderScript;
    private View.BasicKeyboardView keyboardScript;
    private Controller.SuggestionView suggestionScript;
    private RControllerUtil hitPointScript;
    public GameObject textConsole;
    EventSystem m_EventSystem;

    void Start()
    {
      decoderScript = decoderObj.GetComponent<Controller.GlanceWriterController>();
      keyboardScript = keyboardObj.GetComponent<View.BasicKeyboardView>();
      hitPointScript = controllerObj.GetComponent<RControllerUtil>();
      suggestionScript = suggestionObj.GetComponent<Controller.SuggestionView>();
      m_EventSystem = EventSystem.current;
    }

    // Update is called once per frame
    void Update()
    {
      Vector3 hitPoint = hitPointScript.GetHitPoint();
      if (OVRInput.GetDown(OVRInput.RawButton.A) || OVRInput.GetDown(OVRInput.RawButton.X)) {

        OnScreenButton selectedButton = FindButtonClicked(hitPoint.x, hitPoint.y);
        textConsole.GetComponent<TextMeshPro>().SetText("here" + hitPoint + selectedButton.GetButtonName());
        OnButtonClicked(selectedButton);
      }

      if (hitPoint.y < hitPointScript.GetDividerBottom()) {
        decoderScript.ClearSuggestion();
      }
    }

    public void OnButtonClicked(OnScreenButton selectedButton)
    {
        GameObject selectedButtonObj = selectedButton.GetGameObject();
        String bttnName = selectedButton.GetButtonName();

        m_EventSystem.SetSelectedGameObject(selectedButtonObj);

        // Reset the accumualted 'time' for Dwell
        selectedButton.accumulatedFrames = 0;

        if (bttnName == "delete") {
          OnDeleteClicked(selectedButtonObj);
        } else if (bttnName == "enter") {
          OnEnterClicked(selectedButtonObj);
        } else if (bttnName == "SwitchModality"){
          //textConsole.GetComponent<TextMeshPro>().SetText("here");
          OnModalitySwitched(selectedButtonObj);
        } else {
          OnSuggestionClicked(selectedButtonObj);
        }
        m_EventSystem.SetSelectedGameObject(null);
    }

    public OnScreenButton FindButtonClicked(float x, float y) {
      List<OnScreenButton> functionBttns = keyboardScript.GetFunctionalKeys();
      List<OnScreenButton> suggestionBttns = suggestionScript.GetSuggestionButtons();
      foreach (OnScreenButton button in functionBttns)
      {
        if (button.mButtonGameObject.activeSelf && button.containPoint(x, y)) {
          //textConsole.GetComponent<TextMeshPro>().SetText("here" + x + y + button.GetButtonName() + button.containPoint(x, y) + button.GetButtonBoundary() + functionBttns.Count);
          return button;
        }
      }

      foreach (OnScreenButton button in suggestionBttns)
      {
        if (button.mButtonGameObject.activeSelf && button.containPoint(x, y)) {
          //textConsole.GetComponent<TextMeshPro>().SetText("here" + x + y + button.GetButtonName() + button.containPoint(x, y) + button.GetButtonBoundary() + functionBttns.Count);
          return button;
        }
      }
      return null;
    }

    public void OnEnterClicked(GameObject selectedButtonObj) {
      //selectedButtonObj.GetComponentInChildren<TMP_Text>().text = "enter button successfully clicked";
      decoderScript.ClearInputWord();
    }

    public void OnDeleteClicked(GameObject selectedButtonObj) {
      decoderScript.UndoInputWord();
    }

    public void OnSuggestionClicked(GameObject selectedButtonObj) {
      //selectedButtonObj.GetComponentInChildren<TMP_Text>().text = "Sugg button successfully clicked";
      decoderScript.UndoInputWord();
      String selectedWord = selectedButtonObj.GetComponentInChildren<TMP_Text>().text;
      decoderScript.InputWord(selectedWord);
      decoderScript.ClearSuggestion();
    }

    public void OnModalitySwitched(GameObject selectedButtonObj) {
      if (hitPointScript.GetIsGaze() == true) {
        selectedButtonObj.GetComponentInChildren<TMP_Text>().text = "Input mode: Controller\nClick to switch to Gaze";
        bool isGaze = false;
        hitPointScript.SetIsGaze(isGaze);
        // decoderScript.OnModalityChange(isGaze);
      } else {
        selectedButtonObj.GetComponentInChildren<TMP_Text>().text = "Input mode: Gaze\nClick to switch to Controller";
        bool isGaze = true;
        hitPointScript.SetIsGaze(isGaze);
        // decoderScript.OnModalityChange(isGaze);
      }
    }

}
