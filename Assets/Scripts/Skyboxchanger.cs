using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skyboxchanger : MonoBehaviour
{
    public Material[] skyboxes; // Array of skybox materials

    public void ChangeSkybox(int index)
    {
        if (index >= 0 && index < skyboxes.Length)
        {
            RenderSettings.skybox = skyboxes[index];
            DynamicGI.UpdateEnvironment(); // Update global illumination if needed
        }
    }
}   
