using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class RatingManager : MonoBehaviour
{
    // List of toggles representing each rating option
    public List<Toggle> ratingToggles;

    private void Start()
    {
        // Add listeners to each toggle
        foreach (Toggle toggle in ratingToggles)
        {
            toggle.onValueChanged.AddListener(delegate { OnToggleChanged(toggle); });
        }
    }

    private void OnToggleChanged(Toggle changedToggle)
    {
        // Ensure only one toggle is selected at a time
        if (changedToggle.isOn)
        {
            foreach (Toggle toggle in ratingToggles)
            {
                if (toggle != changedToggle)
                {
                    toggle.isOn = false;
                }
            }
        }
    }

    public int GetSelectedRating()
    {
        // Loop through toggles and find the selected rating
        for (int i = 0; i < ratingToggles.Count; i++)
        {
            if (ratingToggles[i].isOn)
            {
                return i + 1; // Return the rating as 1-based (1 to 5)
            }
        }
        return 0; // Return 0 if no rating is selected
    }
}
