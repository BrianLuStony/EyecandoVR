using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchKeyboard : MonoBehaviour
{
    [SerializeField] private GameObject keyboard1;
    [SerializeField] private GameObject keyboard2;
    public void SwitchKeyboards()
    {
        if (keyboard1 != null && keyboard2 != null)
        {
            // Toggle the active state of both keyboards
            keyboard1.SetActive(!keyboard1.activeSelf);
            keyboard2.SetActive(!keyboard2.activeSelf);
        }
        else
        {
            Debug.LogError("Both keyboard GameObjects must be assigned in the inspector!");
        }
    }
}
