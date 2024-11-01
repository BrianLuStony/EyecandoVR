using UnityEngine;
using System.Collections.Generic;

public class StreetViewTest : MonoBehaviour
{
    [SerializeField] private StreetViewAutoNavigator navigator;

    void Start()
    {
        // Example coordinates (New York -> Times Square -> Central Park)
        List<Vector3> path = new List<Vector3>
        {
            new Vector3(40.7580f, 0, -73.9855f),  // Times Square
            new Vector3(40.7829f, 0, -73.9654f),  // Central Park
            new Vector3(40.7527f, 0, -73.9772f)   // Empire State
        };
        
        navigator.SetNavigationPath(path);
    }
}