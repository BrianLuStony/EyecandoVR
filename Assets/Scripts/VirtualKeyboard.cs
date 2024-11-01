using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // For standard UI
using TMPro; // For TextMeshPro, remove if not using

public class VirtualKeyboard : MonoBehaviour
{
    public InputField inputField; // Replace with 'public InputField inputField;' if using standard UI
    private bool isShiftActive = false;

    public void OnKeyPress(string key)
    {
        if (key == "SHIFT")
        {
            isShiftActive = !isShiftActive; // Toggle shift state
            return;
        }

        if (key == "BACKSPACE")
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
            }
            return;
        }

        if (key == "SPACE")
        {
            inputField.text += " ";
            return;
        }

        if (key == "ENTER")
        {
            // Add your logic for the Enter key if needed
            return;
        }

        // Append the key to the text field, converting to upper case if shift is active
        inputField.text += isShiftActive ? key.ToUpper() : key.ToLower();

        // Optionally reset shift after a key press
        isShiftActive = false;
    }
}
