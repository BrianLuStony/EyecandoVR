using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PromptView : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject promptObject;
    private GameObject inputFieldObject;

    void Start()
    {
        promptObject = GameObject.Find("PromptTextField");
        inputFieldObject = GameObject.Find("GlanceWriterInputField");
        // SetPromptText("test prompt!");
        //SetInputText("hello world!");
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetPromptText(string prompt)
    {
        if (prompt != null)
        {
            promptObject.GetComponent<TMP_Text>().text = prompt;
        }
    }

    public string GetPromptText()
    {
        return promptObject.GetComponent<TMP_Text>().text;
    }

    public void SetInputText(string input)
    {
        if (input != null)
        {
            TMP_InputField inputTextInputField = inputFieldObject.GetComponent<TMP_InputField>();
            inputTextInputField.text = input;
            // inputTextInputField.caretPosition = input.Length;
            // inputTextInputField.ForceLabelUpdate();
        }
    }

    public string GetInputText()
    {
        return inputFieldObject.GetComponent<TMP_InputField>().text;
    }

}
