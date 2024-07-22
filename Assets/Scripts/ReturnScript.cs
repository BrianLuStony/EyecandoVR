using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnScript : MonoBehaviour
{

    public GameObject savedObject;
    private GameObject returnObject;
    // Start is called before the first frame update
    public void SaveObject(GameObject after)
    {
        savedObject.SetActive(false);
        returnObject = after;
        returnObject.SetActive(true);
    }

    public void ReturnObject()
    {
        if (savedObject != null)
        {
            savedObject.SetActive(true);
            returnObject.SetActive(false);
            returnObject = null;
        }
    }
}
