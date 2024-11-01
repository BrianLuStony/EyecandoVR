using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DwellBasedSelection 
{

    public static float framePerSecond = 60f;
    public static float DWELL_THRESHOLD = 1.0f * framePerSecond; // selection time in frame
    public List<OnScreenButton> listeningOnScreenButtons;

    public DwellBasedSelection()
    {

    }

    public void RegisterTargets(List<OnScreenButton> buttons)
    {
        listeningOnScreenButtons = buttons;
    }

    public OnScreenButton OnPoint(Vector3 inputPoint)
    {
        float x = inputPoint.x;
        float y = inputPoint.y;
        foreach (OnScreenButton button in listeningOnScreenButtons)
        {
            if (button.mButtonGameObject.activeSelf && button.containPoint(x, y)) {
                button.accumulatedFrames += 1;
                if (button.accumulatedFrames >= DWELL_THRESHOLD) 
                {
                    ResetAccumulatedTime();
                    return button;
                }
            } else {
                button.accumulatedFrames = 0;
            }
        }

        return null;
    }

    public void ResetAccumulatedTime() {
        foreach (OnScreenButton button in listeningOnScreenButtons) {
            button.accumulatedFrames = 0;
        }
    }
}
