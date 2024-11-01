using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimplifiedGazeInteraction : MonoBehaviour
{
    public GameObject leftEye, rightEye;
    public RectTransform cursorIndicator; // Visual indicator for the gaze point
    public Image gazeLoadingCircle; // UI element for the loading circle
    public float gazeInteractionTime = 2f; // Time in seconds for gaze interaction
    public GameObject appContainer;

    public GameObject secondContainer;
    public RectTransform apps; // Container with all buttons

    private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
    private List<OnScreenButton> buttons = new List<OnScreenButton>(); // List to hold OnScreenButton instances
    private List<OnScreenInputField> inputFields = new List<OnScreenInputField>(); // List to hold OnScreenInputField instances
    private int gazeFilterWindowSize = 10;
    private Vector3 hitPoint;
    private bool isGaze = true;
    private float gazeTimer = 0f;
    private OnScreenButton currentGazeTargetButton = null; // Current target being gazed at, as OnScreenButton
    private OnScreenInputField currentGazeTargetInputField = null; // Current input field being gazed at
    private LineRenderer rayLine;

    private Vector2 lastStableCursorPos;
    // private float cursorMoveThreshold = 50f;

    void Start()
    {
        buttons.Clear();
        inputFields.Clear();
        RefreshActiveButtons();
        rayLine = GetComponent<LineRenderer>();
        rayLine.positionCount = 0;
        if (gazeLoadingCircle != null) gazeLoadingCircle.fillAmount = 0;
        if (cursorIndicator != null) cursorIndicator.gameObject.SetActive(false);
        lastStableCursorPos = Vector2.zero;
    }

    void Update()
    {
        if (isGaze) HandleGazeInteraction();
    }

    private void InitializeUIElements(Transform parentTransform)
    {
        foreach (Transform child in parentTransform)
        {
            if (child.gameObject.activeInHierarchy){
                Button uiButton = child.GetComponent<Button>();
                InputField uiInputField = child.GetComponent<InputField>();

                if (uiButton != null)
                {
                    OnScreenButton onScreenButton = new OnScreenButton(child.gameObject, child.name, OnScreenButton.ButtonType.FUNCTIONAL);
                    buttons.Add(onScreenButton);
                    Debug.Log("Initialized Button: " + child.name);
                }
                else if (uiInputField != null)
                {
                    OnScreenInputField onScreenInputField = new OnScreenInputField(child.gameObject, child.name);
                    inputFields.Add(onScreenInputField);
                    Debug.Log("Initialized InputField: " + child.name);
                }
            }
            InitializeUIElements(child);
        }
        Debug.Log("Total Buttons Initialized: " + buttons.Count);
        Debug.Log("Total InputFields Initialized: " + inputFields.Count);
    }

    public void RefreshActiveButtons()
    {
        buttons.Clear(); // Clear the existing list
        InitializeUIElements(appContainer.transform);
        InitializeUIElements(secondContainer.transform);
        Debug.Log($"Refreshed active buttons. Total active buttons: {buttons.Count}");
    }


    private void HandleGazeInteraction()
    {
        rayLine.positionCount = 0;
        RaycastHit hitLeft, hitRight;
        Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
        Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);

        if (Physics.Raycast(gazeRayLeft, out hitLeft, Mathf.Infinity) &&
            Physics.Raycast(gazeRayRight, out hitRight, Mathf.Infinity))
        {
            Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
            Vector3 filteredGazePoint = GazeFilter(gazeHitPoint);

            Vector2 screenPoint = Camera.main.WorldToScreenPoint(filteredGazePoint);

            OnScreenButton hitButton = FindButtonAtGazePoint(screenPoint.x, screenPoint.y);
            OnScreenInputField hitInputField = FindInputFieldAtGazePoint(screenPoint.x, screenPoint.y);
            if (hitButton != null)
            {
                RectTransform buttonRect = hitButton.GetGameObject().GetComponent<RectTransform>();
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(apps, screenPoint, Camera.main, out localPoint);
                
                if (cursorIndicator != null)
                {
                    cursorIndicator.anchoredPosition = localPoint;
                    cursorIndicator.gameObject.SetActive(true);
                }

                ProcessGazeInteraction(hitButton);
            }else if(hitInputField != null){
                RectTransform buttonRect = hitInputField.GetGameObject().GetComponent<RectTransform>();
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(apps, screenPoint, Camera.main, out localPoint);
                
                if (cursorIndicator != null)
                {
                    cursorIndicator.anchoredPosition = localPoint;
                    cursorIndicator.gameObject.SetActive(true);
                }

                ProcessGazeInteraction(hitInputField);
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
        filteredHitPoint = filteredHitPoint / (float)currentElementsInWindow;
        return filteredHitPoint;
    }

    private OnScreenButton FindButtonAtGazePoint(float x, float y)
    {
        Vector2 screenPoint = new Vector2(x, y);
        foreach (OnScreenButton button in buttons)
        {
            if (button.mButtonGameObject != null && button.mButtonGameObject.activeInHierarchy)
            {
                RectTransform buttonRect = button.GetGameObject().GetComponent<RectTransform>();
                Button buttonComponent = button.GetGameObject().GetComponent<Button>();
                
                if (buttonRect != null && buttonComponent != null && buttonComponent.interactable &&
                    RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPoint, Camera.main))
                {
                    Debug.Log($"Hit button: {button.GetButtonName()} at screen point: {screenPoint}");
                    return button;
                }
            }
        }
        return null;
    }

    
    private OnScreenInputField FindInputFieldAtGazePoint(float x, float y)
    {
        Vector2 screenPoint = new Vector2(x, y);
        foreach (OnScreenInputField inputField in inputFields)
        {   
            if(inputField.mInputFieldGameObject.activeInHierarchy && inputField.mInputFieldGameObject != null){
                RectTransform inputRect = inputField.GetGameObject().GetComponent<RectTransform>();
                InputField inputComponent = inputField.GetGameObject().GetComponent<InputField>();
                if (inputRect != null && inputComponent != null && inputComponent.interactable &&
                    RectTransformUtility.RectangleContainsScreenPoint(inputRect, screenPoint, Camera.main))
                {
                    return inputField;
                }
            }
        }
        return null;
    }
    private void ProcessGazeInteraction(OnScreenButton targetedButton)
    {
        Debug.Log("Processing gaze interaction with: " + targetedButton.GetButtonName());
        if (targetedButton == null)
        {
            Debug.LogError("targetedButton is null");
            return;
        }
        if (currentGazeTargetButton != targetedButton)
        {
            currentGazeTargetButton = targetedButton;
            gazeTimer = 0f; // Reset gaze timer for the new target
            if (gazeLoadingCircle != null)
            {
                gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
            }
            else
            {
                Debug.LogError("gazeLoadingCircle is not assigned.");
            }
        }
        else
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeInteractionTime)
            {
                Button button = targetedButton.GetGameObject().GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke(); // Trigger the button event
                    Debug.Log("Clicked Button: " + targetedButton.GetButtonName());

                    // Check if the button is the "ReturnButton"
                    // if (targetedButton.GetGameObject().CompareTag("ReturnButton"))
                    // {
                    //     // Call the method to return to the previous page
                    //     UIManager uiManager = FindObjectOfType<UIManager>();
                    //     if (uiManager != null)
                    //     {
                    //         uiManager.ReturnToPreviousPage();
                    //     }
                    //     else
                    //     {
                    //         Debug.LogError("UIManager instance not found.");
                    //     }
                    // }
                    // // Check if the button is tagged as an "AppPage"
                    // else if (targetedButton.GetGameObject().CompareTag("AppPage"))
                    // {
                    //     // Call the method to open a new page
                    //     UIManager uiManager = FindObjectOfType<UIManager>();
                    //     if (uiManager != null)
                    //     {
                    //         uiManager.OpenPage(targetedButton.GetGameObject());
                    //     }
                    //     else
                    //     {
                    //         Debug.LogError("UIManager instance not found.");
                    //     }
                    // }

                    // ResetGazeInteraction();
                }
                else
                {
                    Debug.LogError("Button component not found on targetedButton.");
                }

                gazeTimer = 0f; // Reset timer after triggering
            }
            else
            {
                if (gazeLoadingCircle != null)
                {
                    gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
                }
                else
                {
                    Debug.LogError("gazeLoadingCircle is not assigned.");
                }
            }
        }
    }

    private void ProcessGazeInteraction(OnScreenInputField targetedInputField)
    {
        Debug.Log("Processing gaze interaction with: " + targetedInputField.GetInputFieldName());
        if (currentGazeTargetInputField != targetedInputField)
        {
            currentGazeTargetInputField = targetedInputField;
            gazeTimer = 0f; // Reset gaze timer for the new target
            if (gazeLoadingCircle != null)
            {
                gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
            }
            else
            {
                Debug.LogError("gazeLoadingCircle is not assigned.");
            }
        }
        else
        {
            gazeTimer += Time.deltaTime;
            if (gazeTimer >= gazeInteractionTime)
            {
                InputField inputField = targetedInputField.GetGameObject().GetComponent<InputField>();
                if (inputField != null)
                {
                    inputField.Select(); // Focus on the input field
                    inputField.ActivateInputField();
                    Debug.Log("Focused InputField: " + targetedInputField.GetInputFieldName());
                    // ResetGazeInteraction();
                }
                gazeTimer = 0f; // Reset timer after triggering
            }
            else
            {
                gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
            }
        }
    }

    private void ResetGazeInteraction()
    {
        currentGazeTargetButton = null;
        currentGazeTargetInputField = null;
        gazeTimer = 0f;
        cursorIndicator.gameObject.SetActive(false); // Hide the cursor
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

// using System.Collections;
// using System.Collections.Generic;
// using System;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class SimplifiedGazeInteraction : MonoBehaviour
// {
//     public GameObject leftEye, rightEye;
//     public GameObject cursorIndicator; // Visual indicator for the gaze point
//     public Image gazeLoadingCircle; // UI element for the loading circle
//     public float gazeInteractionTime = 2f; // Time in seconds for gaze interaction
//     public GameObject appContainer; // Container with all buttons

//     private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
//     private List<OnScreenButton> buttons = new List<OnScreenButton>(); // List to hold OnScreenButton instances
//     private List<OnScreenInputField> inputFields = new List<OnScreenInputField>(); // List to hold OnScreenInputField instances
//     private int gazeFilterWindowSize = 10;
//     private Vector3 hitPoint;
//     private bool isGaze = true;
//     private float gazeTimer = 0f;
//     private OnScreenButton currentGazeTargetButton = null; // Current target being gazed at, as OnScreenButton
//     private OnScreenInputField currentGazeTargetInputField = null; // Current input field being gazed at
//     private LineRenderer rayLine;
    
//     void Start()
//     {
//         InitializeUIElements(appContainer.transform);
//         rayLine = GetComponent<LineRenderer>();
//         rayLine.positionCount = 0;
//         if (gazeLoadingCircle != null) gazeLoadingCircle.fillAmount = 0;
//         cursorIndicator.SetActive(false);
//     }

//     void Update()
//     {
//         if (isGaze) HandleGazeInteraction();
//     }

//     private void InitializeUIElements(Transform parentTransform)
//     {
//         foreach (Transform child in parentTransform)
//         {
//             Button uiButton = child.GetComponent<Button>();
//             InputField uiInputField = child.GetComponent<InputField>();
            
//             if (uiButton != null)
//             {
//                 OnScreenButton onScreenButton = new OnScreenButton(child.gameObject, child.name, OnScreenButton.ButtonType.FUNCTIONAL);
//                 buttons.Add(onScreenButton);
//                 Debug.Log("Initialized Button: " + child.name);
//             }
//             else if (uiInputField != null)
//             {
//                 OnScreenInputField onScreenInputField = new OnScreenInputField(child.gameObject, child.name);
//                 inputFields.Add(onScreenInputField);
//                 Debug.Log("Initialized InputField: " + child.name);
//             }

//             InitializeUIElements(child);
//         }
//         Debug.Log("Total Buttons Initialized: " + buttons.Count);
//         Debug.Log("Total InputFields Initialized: " + inputFields.Count);
//     }

//     private void HandleGazeInteraction()
//     {
//         rayLine.positionCount = 0;
//         RaycastHit hitLeft;
//         RaycastHit hitRight;
//         Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
//         Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);
//         if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
//         {
//             Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
//             Vector3 filteredGamePoint = GazeFilter(gazeHitPoint);
//             cursorIndicator.SetActive(true);
//             cursorIndicator.transform.position = filteredGamePoint;
//             OnScreenButton hitButton = FindButtonAtGazePoint(filteredGamePoint.x, filteredGamePoint.y);
//             OnScreenInputField hitInputField = FindInputFieldAtGazePoint(filteredGamePoint.x, filteredGamePoint.y);

//             if (hitButton != null)
//             {
//                 ProcessGazeInteraction(hitButton);
//             }
//             else if (hitInputField != null)
//             {
//                 ProcessGazeInteraction(hitInputField);
//             }
//             else
//             {
//                 ResetGazeInteraction();
//             }
//         }
//         else
//         {
//             ResetGazeInteraction();
//         }
//     }
    
//     private Vector3 GazeFilter(Vector3 gazePoint)
//     {
//         gazePointWindow.Enqueue(gazePoint);
//         while (gazePointWindow.Count > gazeFilterWindowSize)
//         {
//             gazePointWindow.Dequeue();
//         }
      
//         Vector3 filteredHitPoint = new Vector3(0f, 0f, 0f);
//         int currentElementsInWindow = Math.Min(gazeFilterWindowSize, gazePointWindow.Count);
//         Vector3[] gazePoints = gazePointWindow.ToArray();
//         for (int i = 0; i < gazePoints.Length; ++i)
//         {
//             filteredHitPoint += gazePoints[i];
//         }
//         filteredHitPoint = filteredHitPoint / (float) currentElementsInWindow;
//         return filteredHitPoint;
//     }

//     private OnScreenButton FindButtonAtGazePoint(float x, float y)
//     {
//         foreach (OnScreenButton button in buttons)
//         {
//             if (button.mButtonGameObject.activeSelf && button.containPoint(x, y))
//             {
//                 return button;
//             }
//         }
//         return null;
//     }

//     private OnScreenInputField FindInputFieldAtGazePoint(float x, float y)
//     {
//         foreach (OnScreenInputField inputField in inputFields)
//         {
//             if (inputField.mInputFieldGameObject.activeSelf && inputField.containPoint(x, y))
//             {
//                 return inputField;
//             }
//         }
//         return null;
//     }

//     private void ProcessGazeInteraction(OnScreenButton targetedButton)
//     {
//         if (currentGazeTargetButton != targetedButton)
//         {
//             currentGazeTargetButton = targetedButton;
//             gazeTimer = 0f; // Reset gaze timer for the new target
//             gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
//         }
//         else
//         {
//             gazeTimer += Time.deltaTime;
//             if (gazeTimer >= gazeInteractionTime)
//             {
//                 Button button = targetedButton.GetGameObject().GetComponent<Button>();
//                 if (button != null)
//                 {
//                     button.onClick.Invoke(); // Trigger the button event
//                     Debug.Log("Clicked Button: " + targetedButton.GetButtonName());
//                     ResetGazeInteraction();
//                 }
//                 gazeTimer = 0f; // Reset timer after triggering
//             }
//             else
//             {
//                 gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
//             }
//         }
//     }

//     private void ProcessGazeInteraction(OnScreenInputField targetedInputField)
//     {
//         if (currentGazeTargetInputField != targetedInputField)
//         {
//             currentGazeTargetInputField = targetedInputField;
//             gazeTimer = 0f; // Reset gaze timer for the new target
//             gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
//         }
//         else
//         {
//             gazeTimer += Time.deltaTime;
//             if (gazeTimer >= gazeInteractionTime)
//             {
//                 InputField inputField = targetedInputField.GetInputFieldComponent();
//                 if (inputField != null)
//                 {
//                     inputField.Select(); // Focus on the input field
//                     Debug.Log("Focused InputField: " + targetedInputField.GetInputFieldName());
//                     ResetGazeInteraction();
//                 }
//                 gazeTimer = 0f; // Reset timer after triggering
//             }
//             else
//             {
//                 gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
//             }
//         }
//     }

//     private void ResetGazeInteraction()
//     {
//         currentGazeTargetButton = null;
//         currentGazeTargetInputField = null;
//         gazeTimer = 0f;
//         cursorIndicator.SetActive(false); // Hide the cursor
//         gazeLoadingCircle.fillAmount = 0; // Reset loading circle
//     }   

//     public void SetIsGaze(bool modality)
//     {
//         isGaze = modality;
//         ResetGazeInteraction(); // Reset interaction state if gaze is disabled/enabled
//         gazePointWindow.Clear();
//     }

//     public bool GetIsGaze()
//     {
//         return isGaze;
//     }
// }



// using System.Collections;
// using System.Collections.Generic;
// using System;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class SimplifiedGazeInteraction : MonoBehaviour
// {
//     public GameObject leftEye, rightEye;
//     public GameObject cursorIndicator; // Visual indicator for the gaze point
//     public Image gazeLoadingCircle; // UI element for the loading circle
//     public float gazeInteractionTime = 2f; // Time in seconds for gaze interaction
//     public GameObject appContainer; // Container with all buttons

//     private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
//     private List<OnScreenButton> buttons = new List<OnScreenButton>(); // List to hold OnScreenButton instances
//      private int gazeFilterWindowSize = 10;
//     private Vector3 hitPoint;
//     private bool isGaze = true;
//     private float gazeTimer = 0f;
//     private OnScreenButton currentGazeTarget = null; // Current target being gazed at, as OnScreenButton
//     private LineRenderer rayLine;
//     void Start()
//     {
//         InitializeButtons(appContainer.transform);
//         rayLine = GetComponent<LineRenderer>();
//         rayLine.positionCount = 0;
//         if (gazeLoadingCircle != null) gazeLoadingCircle.fillAmount = 0;
//         cursorIndicator.SetActive(false);
//     }

//     void Update()
//     {
//         if (isGaze) HandleGazeInteraction();
//     }

//     private void InitializeButtons(Transform parentTransform)
//     {
//         // Iterate over all children of appContainer and initialize OnScreenButton objects
//         foreach (Transform child in parentTransform)
//         {
//             Button uiButton = child.GetComponent<Button>();
//             if (uiButton != null)
//             {
//                 // Assuming FUNCTIONAL for simplicity, adjust as needed
//                 OnScreenButton onScreenButton = new OnScreenButton(child.gameObject, child.name, OnScreenButton.ButtonType.FUNCTIONAL);
//                 buttons.Add(onScreenButton);
//                 Debug.Log("Initialized Button: " + child.name);
//             }
//             InitializeButtons(child);
//         }
//          Debug.Log("Total Buttons Initialized: " + buttons.Count);
//     }
    
//     private void HandleGazeInteraction()
//     {
//         rayLine.positionCount = 0;
//         RaycastHit hitLeft;
//         RaycastHit hitRight;
//         Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
//         Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);
//         if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
//         {
//             // Convert the hit point to local space of the appContainer to determine which button is being gazed at
    
//             Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
//             Vector3 filteredGamePoint = GazeFilter(gazeHitPoint);
//             cursorIndicator.SetActive(true);
//             cursorIndicator.transform.position = filteredGamePoint;
//             OnScreenButton hitButton = FindButtonClicked(filteredGamePoint.x, filteredGamePoint.y);
//             if (hitButton != null)
//             {
//                 Debug.Log("I'm getting press");
//                 ProcessGazeInteraction(hitButton);
//             }
//             else
//             {
//                 ResetGazeInteraction();
//             }
//         }
//         else
//         {
//             ResetGazeInteraction();
//         }
//     }
//     private Vector3 GazeFilter(Vector3 gazePoint)
//     {
//       gazePointWindow.Enqueue(gazePoint);
//       while (gazePointWindow.Count > gazeFilterWindowSize)
//       {
//         gazePointWindow.Dequeue();
//       }
      
//       Vector3 filteredHitPoint = new Vector3(0f, 0f, 0f);
//       int currentElementsInWindow = Math.Min(gazeFilterWindowSize, gazePointWindow.Count);
//       Vector3[] gazePoints = gazePointWindow.ToArray();
//       for (int i = 0; i < gazePoints.Length; ++i)
//       {
//         filteredHitPoint += gazePoints[i];
//       }
//       filteredHitPoint = filteredHitPoint / (float) currentElementsInWindow;
//       return filteredHitPoint;
//     }

//     private void HandleHitPoint(Vector3 point) {
//       cursorIndicator.SetActive(true);
//       cursorIndicator.transform.position = hitPoint;
//     }
//     public OnScreenButton FindButtonClicked(float x, float y) {
//       foreach (OnScreenButton button in buttons)
//       {
//         if (button.mButtonGameObject.activeSelf && button.containPoint(x, y)) {
//           return button;
//         }
//       }
//       return null;
//     }


//     private void ProcessGazeInteraction(OnScreenButton targetedButton)
//     {
//         Debug.Log("Processing gaze interaction with: " + targetedButton.GetButtonName());
//         if (currentGazeTarget != targetedButton)
//         {
//             currentGazeTarget = targetedButton;
//             gazeTimer = 0f; // Reset gaze timer for the new target
//             gazeLoadingCircle.fillAmount = 0; // Reset loading circle for the new target
//         }
//         else
//         {
//             gazeTimer += Time.deltaTime;
//             if (gazeTimer >= gazeInteractionTime)
//             {
//                 // Trigger the button event once gaze duration meets the threshold
//                 TriggerButtonEvent(targetedButton.GetGameObject());
//                 gazeTimer = 0f; // Reset timer after triggering
//             }
//             else
//             {
//                 // Update loading circle based on gaze duration
//                 gazeLoadingCircle.fillAmount = gazeTimer / gazeInteractionTime;
//             }
//         }
//     }

//     private void TriggerButtonEvent(GameObject buttonObject)
//     {
//         Button button = buttonObject.GetComponent<Button>();
//         if (button != null)
//         {
//             button.onClick.Invoke();
//             Debug.Log("Clicked Button: " + button.gameObject.name);
//             ResetGazeInteraction();
//         }
//     }
//     public Vector3 GetHitPoint() {
//       return hitPoint;
//     }
//     private void ResetGazeInteraction()
//     {
//         currentGazeTarget = null;
//         gazeTimer = 0f;
//         cursorIndicator.SetActive(false); // Hide the cursor
//         gazeLoadingCircle.fillAmount = 0; // Reset loading circle
//     }   

//     public void SetIsGaze(bool modality)
//     {
//         isGaze = modality;
//         ResetGazeInteraction(); // Reset interaction state if gaze is disabled/enabled
//         gazePointWindow.Clear();
//     }

//         public bool GetIsGaze()
//     {
//         return isGaze;
//     }
// }

