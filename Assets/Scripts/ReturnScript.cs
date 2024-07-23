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
        if (savedObject != null)
        {
            Debug.Log("Saving object: " + savedObject.name);
            savedObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("savedObject is null!");
        }

        if (after != null)
        {
            returnObject = after;
            Debug.Log("Activating return object: " + returnObject.name);
            returnObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("after object is null!");
        }
    }

    public void ReturnObject()
    {
         if (savedObject != null)
        {
            Debug.Log("Returning object: " + savedObject.name);
            savedObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("savedObject is null!");
        }

        if (returnObject != null)
        {
            Debug.Log("Deactivating return object: " + returnObject.name);
            returnObject.SetActive(false);
            returnObject = null;
        }
        else
        {
            Debug.LogWarning("returnObject is null!");
        }
    }
}
