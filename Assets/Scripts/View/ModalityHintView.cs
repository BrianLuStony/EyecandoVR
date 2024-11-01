using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ModalityHintView : MonoBehaviour
{

    private GameObject modalityHintObject;
    // Start is called before the first frame update
    void Start()
    {
        modalityHintObject = GameObject.Find("ModalityHintView");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetModalityHintText(string modalityHint)
    {
        modalityHintObject.GetComponent<TMP_Text>().text = modalityHint;
    }
}
