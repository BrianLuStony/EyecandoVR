using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimplifiedGazeInteraction : MonoBehaviour
{
    public GameObject leftEye, rightEye;
    public GameObject cursorIndicator; // Visual indicator for the gaze point
    public Image gazeLoadingCircle; // UI element for the loading circle
    public float gazeInteractionTime = 2f; // Time in seconds for gaze interaction
    public GameObject appContainer; // Container with all buttons

    private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
    private List<OnScreenButton> buttons = new List<OnScreenButton>(); // List to hold OnScreenButton instances
     private int gazeFilterWindowSize = 10;
    private Vector3 hitPoint;
    private bool isGaze = true;
    private float gazeTimer = 0f;
    private OnScreenButton currentGazeTarget = null; // Current target being gazed at, as OnScreenButton
    private LineRenderer rayLine;
    void Start()
    {
        InitializeButtons(appContainer.transform);
        rayLine = GetComponent<LineRenderer>();
        rayLine.positionCount = 0;
        if (gazeLoadingCircle != null) gazeLoadingCircle.fillAmount = 0;
        cursorIndicator.SetActive(false);
    }

    void Update()
    {
        if (isGaze) HandleGazeInteraction();
    }

    private void InitializeButtons(Transform parentTransform)
    {
        // Iterate over all children of appContainer and initialize OnScreenButton objects
        foreach (Transform child in parentTransform)
        {
            Button uiButton = child.GetComponent<Button>();
            if (uiButton != null)
            {
                // Assuming FUNCTIONAL for simplicity, adjust as needed
                OnScreenButton onScreenButton = new OnScreenButton(child.gameObject, child.name, OnScreenButton.ButtonType.FUNCTIONAL);
                buttons.Add(onScreenButton);
                Debug.Log("Initialized Button: " + child.name);
            }
            InitializeButtons(child);
        }
         Debug.Log("Total Buttons Initialized: " + buttons.Count);
    }
    
    private void HandleGazeInteraction()
    {
        rayLine.positionCount = 0;
        RaycastHit hitLeft;
        RaycastHit hitRight;
        Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
        Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);
        if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
        {
            // Convert the hit point to local space of the appContainer to determine which button is being gazed at
    
            Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
            Vector3 filteredGamePoint = GazeFilter(gazeHitPoint);
            cursorIndicator.SetActive(true);
            cursorIndicator.transform.position = filteredGamePoint;
            OnScreenButton hitButton = FindButtonClicked(filteredGamePoint.x, filteredGamePoint.y);
            if (hitButton != null)
            {
                Debug.Log("I'm getting press");
                ProcessGazeInteraction(hitButton);
            }
            else
            {
                ResetGazeInteraction();
            }
        }
        else
        {
            ResetGazeInteraction();
        }
    }
    private Vector3 GazeFilter(Vector3 gazePoint)
    {
      gazePointWindow.Enqueue(gazePoint);
      while (gazePointWindow.Count > gazeFilterWindowSize)
      {
        gazePointWindow.Dequeue();
      }
      
      Vector3 filteredHitPoint = new Vector3(0f, 0f, 0f);
      int currentElementsInWindow = Math.Min(gazeFilterWindowSize, gazePointWindow.Count);
      Vector3[] gazePoints = gazePointWindow.ToArray();
      for (int i = 0; i < gazePoints.Length; ++i)
      {
        filteredHitPoint += gazePoints[i];
      }
      filteredHitPoint = filteredHitPoint / (float) currentElementsInWindow;
      return filteredHitPoint;
    }

    private void HandleHitPoint(Vector3 point) {
      cursorIndicator.SetActive(true);
      cursorIndicator.transform.position = hitPoint;
    }
    public OnScreenButton FindButtonClicked(float x, float y) {
      foreach (OnScreenButton button in buttons)
      {
        if (button.mButtonGameObject.activeSelf && button.containPoint(x, y)) {
          return button;
        }
      }
      return null;
    }


    private void ProcessGazeInteraction(OnScreenButton targetedButton)
    {
        Debug.Log("Processing gaze interaction with: " + targetedButton.GetButtonName());
        if (currentGazeTarget != targetedButton)
        {
            currentGazeTarget = targetedButton;
            gazeTimer = 0f; // Reset gaze timer for the new target
            gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
        }
        else
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeInteractionTime)
            {
                // Trigger the button event once gaze duration meets the threshold
                TriggerButtonEvent(targetedButton.GetGameObject());
                gazeTimer = 0f; // Reset timer after triggering
            }
            else
            {
                // Update loading circle based on gaze duration
                gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
            }
        }
    }

    private void TriggerButtonEvent(GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke();
            Debug.Log("Clicked Button: " + button.gameObject.name);
            ResetGazeInteraction();
        }
    }
    public Vector3 GetHitPoint() {
      return hitPoint;
    }
    private void ResetGazeInteraction()
    {
        currentGazeTarget = null;
        gazeTimer = 0f;
        cursorIndicator.SetActive(false); // Hide the cursor
        gazeLoadingCircle.fillAmount = 0; // Reset loading circle
    }   

    public void SetIsGaze(bool modality)
    {
        isGaze = modality;
        ResetGazeInteraction(); // Reset interaction state if gaze is disabled/enabled
        gazePointWindow.Clear();
    }

        public bool GetIsGaze()
    {
        return isGaze;
    }
}

