// using UnityEngine;
// using UnityEngine.EventSystems;

// namespace TLab.Android.WebView
// {
//     public class WebViewInputListener : MonoBehaviour,
//         IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
//     {
//         [SerializeField] private TLabWebView m_webview;

//         private bool m_pointerDown = false;
//         private int? m_pointerId = null;
//         private RenderMode m_renderMode;
//         private Vector2Int m_inputPosition;

//         private enum WebTouchEvent
//         {
//             DOWN,
//             UP,
//             DRAG
//         };

//         private string THIS_NAME => "[" + this.GetType() + "] ";

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="eventData"></param>
//         /// <returns></returns>
//         private bool GetInputPosition(PointerEventData eventData)
//         {
//             Vector2 localPosition = Vector2.zero;

//             RectTransform rectTransform = (RectTransform)transform;

//             switch (m_renderMode)
//             {
//                 case RenderMode.ScreenSpaceOverlay:
//                     localPosition = transform.InverseTransformPoint(eventData.position);
//                     break;
//                 case RenderMode.ScreenSpaceCamera:
//                 case RenderMode.WorldSpace:
//                     RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
//                     break;
//             }

//             float x = localPosition.x / rectTransform.rect.width + rectTransform.pivot.x;
//             float y = 1f - (localPosition.y / rectTransform.rect.height + rectTransform.pivot.y);

//             if (Math.Range(x, 0, 1) && Math.Range(y, 0, 1))
//             {
//                 m_inputPosition = new Vector2Int((int)(x * m_webview.webWidth), (int)(y * m_webview.webHeight));

//                 return true;
//             }

//             m_inputPosition = Vector2Int.zero;

//             return false;
//         }

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="eventData"></param>
//         public void OnPointerDown(PointerEventData eventData)
//         {
//             if (m_pointerId == null && !m_pointerDown && GetInputPosition(eventData))
//             {
//                 m_pointerId = eventData.pointerId;

//                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DOWN);

//                 m_pointerDown = true;
//             }
//         }

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="eventData"></param>
//         public void OnDrag(PointerEventData eventData)
//         {
//             if ((m_pointerId == eventData.pointerId) && m_pointerDown && GetInputPosition(eventData))
//             {
//                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DRAG);
//             }
//         }

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="eventData"></param>
//         public void OnPointerUp(PointerEventData eventData)
//         {
//             if ((m_pointerId == eventData.pointerId) && m_pointerDown && GetInputPosition(eventData))
//             {
//                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP);

//                 m_pointerId = null;

//                 m_pointerDown = false;
//             }
//         }

//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="eventData"></param>
//         public void OnPointerExit(PointerEventData eventData)
//         {
//             if ((m_pointerId == eventData.pointerId) && m_pointerDown)
//             {
//                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP);

//                 m_pointerId = null;

//                 m_pointerDown = false;
//             }
//         }

//         private void OnEnable()
//         {
//             Canvas canvas = GetComponentInParent<Canvas>();

//             if (canvas == null)
//             {
//                 Debug.LogError(THIS_NAME + "canvas not found");
//                 return;
//             }

//             m_renderMode = canvas.renderMode;

//             m_pointerId = null;

//             m_pointerDown = false;
//         }

//         private void OnDisable()
//         {
//             m_pointerId = null;

//             m_pointerDown = false;
//         }
//     }

//     public static class Math
//     {
//         /// <summary>
//         /// 
//         /// </summary>
//         /// <param name="i"></param>
//         /// <param name="min"></param>
//         /// <param name="max"></param>
//         /// <returns></returns>
//         public static bool Range(float i, float min, float max)
//         {
//             if (min >= max)
//             {
//                 return false;
//             }

//             return i >= min && i <= max;
//         }
//     }
// }

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

namespace TLab.Android.WebView
{
    public class WebViewInputListener : MonoBehaviour
    {
        [Header("Eye Settings")]
        [SerializeField] private GameObject leftEye;
        [SerializeField] private GameObject rightEye;

        [Header("Cursor Settings")]
        [SerializeField] private RectTransform cursorIndicator;

        [Header("Keyboard Settings")]
        [SerializeField] private GameObject keyboardObject;

        [Header("WebView and Interaction Settings")]
        [SerializeField] private TLabWebView m_tlabWebView;
        [SerializeField] private Button webViewButton;
        [SerializeField] private RectTransform tlabRectTransform; // Parent of WebView
        [SerializeField] private RectTransform webViewRectTransform; // Actual WebView
        [SerializeField] private RectTransform cursorRectTransform; // Cursor
        [SerializeField] private Image loadingCircle;
        [SerializeField] private GameObject searchBarGameObject;
        public LineRenderer lineRenderer;
        // private float gazeTime = 0f;
        // private bool selectionInitiated = false;
        private Vector2 lastGazePosition = Vector2.zero;
        private const float gazeTriggerTime = 1.3f;
        private const float gazePositionTolerance = 10f;
        private const int TOUCH_DOWN = 0;
        private const int TOUCH_UP = 1;

        private List<OnScreenButton> keyboardButtons = new List<OnScreenButton>();
        private OnScreenButton currentGazeTargetButton = null;

        private bool cursorOutsideWebViewOnly = false;  

        private int uiLayerMask;

        private float gazeDurationThreshold = 2.0f;
        private float currentGazeTime = 0.0f;
        private Vector3 lastGazePoint = Vector3.zero;
        private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
        private int gazeFilterWindowSize = 10;

        private Vector2 lastStableCursorPos;
        private float cursorMoveThreshold = 50f;

        private bool isGazing = false;

        void Start()
        {
            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = 0.01f;
            lineRenderer.endWidth = 0.01f;
            lastStableCursorPos = Vector2.zero;
            InitializeKeyboardButtons(keyboardObject.transform);

            tlabRectTransform.pivot = new Vector2(0.5f, 0.5f);
            webViewRectTransform.pivot = new Vector2(0.5f, 0.5f);
            cursorRectTransform.pivot = new Vector2(0.5f, 0.5f);
        }

        private void InitializeKeyboardButtons(Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                if (child.gameObject.activeInHierarchy)
                {
                    Button uiButton = child.GetComponent<Button>();
                    if (uiButton != null)
                    {
                        OnScreenButton onScreenButton = new OnScreenButton(child.gameObject, child.name, OnScreenButton.ButtonType.FUNCTIONAL);
                        keyboardButtons.Add(onScreenButton);
                        Debug.Log("Initialized Keyboard Button: " + child.name);
                    }
                }
                InitializeKeyboardButtons(child);
            }
        }
        void Update()
        {
            RaycastHit hitLeft, hitRight;
            Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
            Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);

            if (Physics.Raycast(gazeRayLeft, out hitLeft) &&
                Physics.Raycast(gazeRayRight, out hitRight))
            {
                Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;

                Vector3 filteredGazePoint = GazeFilter(gazeHitPoint);

                Vector2 screenPoint = Camera.main.WorldToScreenPoint(filteredGazePoint);

                RectTransformUtility.ScreenPointToLocalPointInRectangle(tlabRectTransform, screenPoint, Camera.main, out Vector2 localPointInTLab);
                Vector2 localPointInWebView = ConvertLocalPointFromParentToChild(tlabRectTransform, webViewRectTransform, localPointInTLab);

                bool isInsideWebView = IsPointInsideWebView(localPointInWebView);
                if (!cursorOutsideWebViewOnly || (cursorOutsideWebViewOnly && !isInsideWebView))
                {
                    cursorIndicator.anchoredPosition = localPointInWebView ;
                    cursorIndicator.gameObject.SetActive(true);

                    float distanceFromLastStablePos = Vector2.Distance(lastStableCursorPos, localPointInTLab);

                    if (distanceFromLastStablePos > cursorMoveThreshold)
                    {
                        ResetGazeInteraction();
                    }

                    currentGazeTime += Time.deltaTime;
                    loadingCircle.fillAmount = currentGazeTime / gazeDurationThreshold;
                    if (currentGazeTime >= gazeDurationThreshold && !isGazing)
                    {
                        if (keyboardObject.activeInHierarchy && hitLeft.collider.gameObject == keyboardObject)
                        {
                            HandleKeyboardInteraction(screenPoint);
                        }
                        else if (!cursorOutsideWebViewOnly && isInsideWebView)
                        {
                            TriggerWebViewClick(localPointInWebView);
                        }
                        else
                        {
                            HandleUIInteraction(hitLeft.collider.gameObject);
                        }

                        loadingCircle.fillAmount = 0;
                        currentGazeTime = 0f;
                    }

                    lastStableCursorPos = localPointInTLab;
                }
                else
                {
                    cursorIndicator.gameObject.SetActive(false);
                    ResetGazeInteraction();
                }
            }
            else
            {
                ResetGazeInteraction();
            }
        }
         private Vector2 ConvertLocalPointFromParentToChild(RectTransform parent, RectTransform child, Vector2 localPointInParent)
        {
            Vector3 worldPoint = parent.TransformPoint(localPointInParent);
            return child.InverseTransformPoint(worldPoint);
        }

        private void HandleKeyboardInteraction(Vector2 screenPoint)
        {
            OnScreenButton hitButton = FindKeyboardButtonAtGazePoint(screenPoint.x, screenPoint.y);
            if (hitButton != null)
            {
                ProcessKeyboardButtonInteraction(hitButton);
            }
        }

        private OnScreenButton FindKeyboardButtonAtGazePoint(float x, float y)
        {
            Vector2 screenPoint = new Vector2(x, y);
            foreach (OnScreenButton button in keyboardButtons)
            {
                if (button.mButtonGameObject != null && button.mButtonGameObject.activeInHierarchy)
                {
                    RectTransform buttonRect = button.GetGameObject().GetComponent<RectTransform>();
                    Button buttonComponent = button.GetGameObject().GetComponent<Button>();
                    
                    if (buttonRect != null && buttonComponent != null && buttonComponent.interactable &&
                        RectTransformUtility.RectangleContainsScreenPoint(buttonRect, screenPoint, Camera.main))
                    {
                        return button;
                    }
                }
            }
            return null;
        }

        private bool IsPointInsideWebView(Vector2 localPoint)
        {
            return webViewRectTransform.rect.Contains(localPoint);
        }

        private void ProcessKeyboardButtonInteraction(OnScreenButton targetedButton)
        {
            if (targetedButton == null)
            {
                Debug.LogError("targetedButton is null");
                return;
            }

            if (currentGazeTargetButton != targetedButton)
            {
                currentGazeTargetButton = targetedButton;
                currentGazeTime = 0f;
                loadingCircle.fillAmount = 0;
            }
            else
            {
                currentGazeTime += Time.deltaTime;
                if (currentGazeTime >= gazeDurationThreshold)
                {
                    Button button = targetedButton.GetGameObject().GetComponent<Button>();
                    if (button != null)
                    {
                        button.onClick.Invoke();
                        Debug.Log("Clicked Keyboard Button: " + targetedButton.GetButtonName());
                    }
                    else
                    {
                        Debug.LogError("Button component not found on targetedButton.");
                    }

                    ResetGazeInteraction();
                }
                else
                {
                    loadingCircle.fillAmount = currentGazeTime / gazeDurationThreshold;
                }
            }
        }

        private void HandleUIInteraction(GameObject hitObject)
        {
            Button button = hitObject.GetComponent<Button>();
            UnityEngine.UI.InputField inputfield = hitObject.GetComponent<UnityEngine.UI.InputField>();
            Debug.Log("Interacted with: " + hitObject.name);
            if (button != null)
            {
                HandleUIButton(button);
            }
            else if(inputfield != null){
                HandleInputField(inputfield);
            }
            else
            {
                // Handle other UI elements if needed
                Debug.Log("Interacted with non-button UI element: " + hitObject.name);
            }
        }
        private void HandleUIButton(Button button)
        {
            button.onClick.Invoke();
            Debug.Log("Button clicked: " + button.name);
        }
        private void HandleInputField(UnityEngine.UI.InputField inputField)
        {
            inputField.Select(); // Focus on the input field
            inputField.ActivateInputField();
            Debug.Log("Focused InputField: " + inputField.name);
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

        IEnumerator TriggerWebViewClickCoroutine(Vector2 localPoint)
        {
            TriggerTouchEvent(localPoint, TOUCH_DOWN);
            yield return new WaitForSeconds(0.1f); // Add a short delay
            TriggerTouchEvent(localPoint, TOUCH_UP);
        }

        async void TriggerWebViewClick(Vector2 localPoint)
        {
            if (webViewButton != null)
            {
                webViewButton.onClick.Invoke();
            }
            TriggerTouchEvent(localPoint, TOUCH_DOWN);
            TriggerTouchEvent(localPoint, TOUCH_UP);
        }

        void TriggerTouchEvent(Vector2 position, int touchEvent)
        {
            float uvX = (position.x / webViewRectTransform.rect.width) + 0.5f;
            float uvY = 1.0f - ((position.y / webViewRectTransform.rect.height) + 0.5f);
            int xPos = (int)(uvX * m_tlabWebView.webWidth);
            int yPos = (int)(uvY * m_tlabWebView.webHeight);

            m_tlabWebView.TouchEvent(xPos, yPos, touchEvent);
        }

        private void ResetGazeInteraction()
        {
            cursorIndicator.gameObject.SetActive(false);

            loadingCircle.fillAmount = 0;
            currentGazeTime = 0f;
            isGazing = false;

            currentGazeTargetButton = null;
        }

        public void ToggleCursorOutsideWebViewOnly(bool outsideOnly)
        {
            cursorOutsideWebViewOnly = outsideOnly;
            if (outsideOnly)
            {
                ResetGazeInteraction();
            }
        }
    }
}


// // namespace TLab.Android.WebView
// // {
// //     public class WebViewInputListener : MonoBehaviour
// //     {
// //         [Header("Eye Settings")]
// //         [SerializeField] private GameObject leftEye;
// //         [SerializeField] private GameObject rightEye;

// //         [Header("Cursor Settings")]
// //         [SerializeField] private RectTransform cursorIndicator;

// //         [Header("WebView and Interaction Settings")]
// //         [SerializeField] private TLabWebView m_tlabWebView;
// //         [SerializeField] private RectTransform m_webViewRect; // Ensure this has a collider for raycasting.
// //         [SerializeField] private Image loadingCircle;

// //         private float gazeTime = 0f;
// //         private bool selectionInitiated = false;
// //         private Vector2 lastGazePosition = Vector2.zero;
// //         private const float gazeTriggerTime = 2f;
// //         private const float gazePositionTolerance = 10f;
// //         private const int TOUCH_DOWN = 0;
// //         private const int TOUCH_UP = 1;

// //         private float gazeDurationThreshold = 2.0f;
// //         private float currentGazeTime = 0.0f;
// //         private Vector3 lastGazePoint = Vector3.zero;
// //         private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
// //         private int gazeFilterWindowSize = 10;

// //         private bool isGazing = false;

// //         void Update()
// //         {

// //             RaycastHit hitLeft, hitRight;
// //             Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
// //             Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);

// //             if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
// //             {
// //                 Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
// //                 Vector3 filteredGazePoint = GazeFilter(gazeHitPoint);

// //                 // Convert from world point to screen point
// //                 Vector2 screenPoint = Camera.main.WorldToScreenPoint(filteredGazePoint);

// //                 // Assuming your Canvas is using Screen Space - Camera or World Space and cursorIndicator is a RectTransform under this Canvas
// //                 RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)cursorIndicator.parent, screenPoint, Camera.main, out Vector2 localPoint);

// //                 cursorIndicator.anchoredPosition = localPoint; // Set the local position in the canvas space
// //                 cursorIndicator.gameObject.SetActive(true);
// //                 // Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
// //                 // Vector3 filteredGazePoint = GazeFilter(gazeHitPoint);

// //                 // cursorIndicator.anchoredPosition = filteredGazePoint;
// //                 // cursorIndicator.gameObject.SetActive(true);

// //                 // cursorIndicator.transform.position = gazeHitPoint;
// //                 // cursorIndicator.SetActive(true);

// //                 // Check if the hit object is part of the web view
// //                 if (hitLeft.collider.transform == m_webViewRect || hitRight.collider.transform == m_webViewRect)
// //                 {
// //                     currentGazeTime += Time.deltaTime;
// //                     loadingCircle.fillAmount = currentGazeTime / gazeDurationThreshold;

// //                     if (currentGazeTime >= gazeDurationThreshold && !isGazing)
// //                     {
// //                         // Perform the click action on the WebView
// //                         TriggerWebViewClick(gazeHitPoint);
// //                         // isGazing = true; // Prevent multiple clicks
// //                         loadingCircle.fillAmount = 0;
// //                         currentGazeTime = 0f;
// //                     }
// //                 }
// //                 else
// //                 {
// //                     ResetGazeInteraction();
// //                 }
// //             }
// //             else
// //             {
// //                 ResetGazeInteraction();
// //             }
// //         }
// //         private Vector3 GazeFilter(Vector3 gazePoint)
// //         {
// //         gazePointWindow.Enqueue(gazePoint);
// //         while (gazePointWindow.Count > gazeFilterWindowSize)
// //         {
// //             gazePointWindow.Dequeue();
// //         }
        
// //         Vector3 filteredHitPoint = new Vector3(0f, 0f, 0f);
// //         int currentElementsInWindow = Math.Min(gazeFilterWindowSize, gazePointWindow.Count);
// //         Vector3[] gazePoints = gazePointWindow.ToArray();
// //         for (int i = 0; i < gazePoints.Length; ++i)
// //         {
// //             filteredHitPoint += gazePoints[i];
// //         }
// //         filteredHitPoint = filteredHitPoint / (float) currentElementsInWindow;
// //         return filteredHitPoint;
// //         }

// //         void TriggerWebViewClick(Vector3 gazePoint)
// //         {
// //             Vector3 localPoint = m_webViewRect.InverseTransformPoint(gazePoint);
// //             Vector2 currentPosition = new Vector2(localPoint.x, localPoint.y);
// //             TriggerTouchEvent(currentPosition, TOUCH_DOWN);
// //             TriggerTouchEvent(currentPosition, TOUCH_UP);
// //             // Normalize the local position to UV coordinates for the WebView
// //             // Vector2 uvPosition = new Vector2(
// //             //     (currentPosition.x + m_webViewRect.rect.width * 0.5f) / m_webViewRect.rect.width,
// //             //     (currentPosition.y + m_webViewRect.rect.height * 0.5f) / m_webViewRect.rect.height
// //             // );

// //             // // Convert to pixel coordinates based on the web view's resolution
// //             // int xPos = Mathf.Clamp((int)(uvPosition.x * m_tlabWebView.WebWidth), 0, m_tlabWebView.WebWidth);
// //             // int yPos = Mathf.Clamp((int)(uvPosition.y * m_tlabWebView.WebHeight), 0, m_tlabWebView.WebHeight);

// //             // Debug.Log($"Triggering WebView Click at: {xPos}, {yPos}");

// //             // // Trigger the touch event on the web view
// //             // m_tlabWebView.TouchEvent(xPos, yPos, TOUCH_DOWN);
// //             // m_tlabWebView.TouchEvent(xPos, yPos, TOUCH_UP);
// //         }
// //         void TriggerTouchEvent(Vector2 position, int touchEvent)
// //         {
// //             float uvX = (position.x / m_webViewRect.rect.width) + 0.5f;
// //             float uvY = 1.0f - ((position.y / m_webViewRect.rect.height) + 0.5f);
// //             int xPos = (int)(uvX * m_tlabWebView.WebWidth);
// //             int yPos = (int)(uvY * m_tlabWebView.WebHeight);

// //             m_tlabWebView.TouchEvent(xPos, yPos, touchEvent);
// //         }
// //         private void ResetGazeInteraction()
// //         {
// //             cursorIndicator.gameObject.SetActive(false);

// //             loadingCircle.fillAmount = 0;
// //             currentGazeTime = 0f;
// //             isGazing = false;
// //         }
// //     }
// // }

// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using System;
// // using UnityEngine.EventSystems;
// // using UnityEngine.UI;

// // namespace TLab.Android.WebView
// // {
// //     public class WebViewInputListener : MonoBehaviour
// //     {
// //         [SerializeField] private TLabWebView m_webview;
// //         [SerializeField] private GameObject leftEye;
// //         [SerializeField] private GameObject rightEye;
// //         [SerializeField] private GameObject cursorIndicator;
// //         [SerializeField] private Image gazeLoadingCircle;
// //         [SerializeField] private Camera uiCamera;
// //         [SerializeField] private RectTransform webViewRectTransform;

// //         private float gazeDurationThreshold = 2.0f;
// //         private float currentGazeTime = 0.0f;
// //         private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
// //         private int gazeFilterWindowSize = 10;
        
// //         private bool m_pointerDown = false;
// //         private Vector2Int m_inputPosition;
// //         private enum WebTouchEvent
// //         {
// //             DOWN,
// //             UP,
// //             DRAG
// //         };

// //         private void Update()
// //         {
// //             if (GetInputPosition())
// //             {
// //                 cursorIndicator.SetActive(true);
// //                 cursorIndicator.transform.position = ProjectPointOnCanvas(GetGazeHitPoint());
                
// //                 currentGazeTime += Time.deltaTime;
// //                 gazeLoadingCircle.fillAmount = currentGazeTime / gazeDurationThreshold;
                
// //                 if (currentGazeTime >= gazeDurationThreshold)
// //                 {
// //                     TriggerGazeClick();
// //                     currentGazeTime = 0.0f; // Reset after triggering click
// //                 }
// //             }
// //             else
// //             {
// //                 cursorIndicator.SetActive(false);
// //                 gazeLoadingCircle.fillAmount = 0;
// //                 currentGazeTime = 0.0f;
// //             }
// //         }

// //         private Vector2 ProjectPointOnCanvas(Vector3 gazeHitPoint3D)
// //         {
// //             Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, gazeHitPoint3D);
// //             Vector2 localPoint;
// //             RectTransformUtility.ScreenPointToLocalPointInRectangle(webViewRectTransform, screenPoint, uiCamera, out localPoint);
// //             return webViewRectTransform.TransformPoint(localPoint);
// //         }

// //         private void TriggerGazeClick()
// //         {
// //             Debug.Log("Gaze Click Triggered on Target");
// //             m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DOWN);
// //             m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP);
// //         }

// //         private Vector3 GetGazeHitPoint()
// //         {
// //             RaycastHit hitLeft, hitRight;
// //             Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
// //             Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);

// //             if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
// //             {
// //                 Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
// //                 return GazeFilter(gazeHitPoint);
// //             }

// //             return Vector3.zero;
// //         }

// //         private Vector3 GazeFilter(Vector3 gazePoint)
// //         {
// //             gazePointWindow.Enqueue(gazePoint);
// //             while (gazePointWindow.Count > gazeFilterWindowSize)
// //             {
// //                 gazePointWindow.Dequeue();
// //             }

// //             Vector3 filteredHitPoint = Vector3.zero;
// //             foreach (Vector3 point in gazePointWindow)
// //             {
// //                 filteredHitPoint += point;
// //             }
// //             return filteredHitPoint / gazePointWindow.Count;
// //         }

// //         private bool GetInputPosition()
// //         {
// //             Vector3 gazeHitPoint3D = GetGazeHitPoint();
// //             if (gazeHitPoint3D == Vector3.zero) return false;

// //             Vector2 gazePointOnCanvas = ProjectPointOnCanvas(gazeHitPoint3D);
// //             if (!RectTransformUtility.RectangleContainsScreenPoint(webViewRectTransform, gazePointOnCanvas, uiCamera)) return false;

// //             Vector2 localPosition;
// //             RectTransformUtility.ScreenPointToLocalPointInRectangle(webViewRectTransform, gazePointOnCanvas, uiCamera, out localPosition);

// //             float x = (localPosition.x - webViewRectTransform.rect.x) / webViewRectTransform.rect.width;
// //             float y = (localPosition.y - webViewRectTransform.rect.y) / webViewRectTransform.rect.height;

// //             m_inputPosition = new Vector2Int((int)(x * m_webview.WebWidth), (int)(y * m_webview.WebHeight));
// //             return true;
// //         }
// //     }
// // }

// // using System.Collections;
// // using System.Collections.Generic;
// // using UnityEngine;
// // using System;
// // using UnityEngine.EventSystems;
// // using UnityEngine.UI;

// // namespace TLab.Android.WebView
// // {
// //     public class WebViewInputListener : MonoBehaviour,
// //         IPointerDownHandler, IDragHandler, IPointerUpHandler, IPointerExitHandler
// //     {
// //         [SerializeField] private TLabWebView m_webview;
// //         [SerializeField] private GameObject leftEye; // Assign in Inspector
// //         [SerializeField] private GameObject rightEye;
// //         [SerializeField] private GameObject cursorIndicator; // Drag your cursor GameObject here
// //         [SerializeField] private Image gazeLoadingCircle; // Drag your loading circle Image component here
// //         private float gazeDurationThreshold = 2.0f; // Time in seconds to activate gaze click
// //         private float currentGazeTime = 0.0f; // Timer to track gaze duration
// //         [SerializeField] private Camera uiCamera; // Assign this in the Inspector
// //         [SerializeField] private RectTransform webViewRectTransform; // Assign this in the Inspector

// //         private Queue<Vector3> gazePointWindow = new Queue<Vector3>();
// //         private int gazeFilterWindowSize = 10; // Adjust this value based on your needs

// //         private const int GazePointerId = 1;

// //         // Additional fields to track the current gaze target
// //         // For demonstration, let's assume it's a simple boolean indicating if there's a target
// //         private bool currentGazeTarget = false;


// //         private bool m_pointerDown = false;
// //         private int? m_pointerId = null;
// //         private RenderMode m_renderMode;
// //         private Vector2Int m_inputPosition;

// //         private enum WebTouchEvent
// //         {
// //             DOWN,
// //             UP,
// //             DRAG
// //         };

// //         private string THIS_NAME => "[" + this.GetType() + "] ";

// //         void Update()
// //         {
// //             if (GetInputPosition()) // If the gaze is within the web view
// //             {
// //                 cursorIndicator.SetActive(true); // Show the cursor
// //                 cursorIndicator.transform.position = ProjectPointOnCanvas(GetGazeHitPoint()); // Move cursor to gaze point

// //                 if (currentGazeTarget) // Assuming you have a way to determine if a clickable target is being gazed at
// //                 {
// //                     currentGazeTime += Time.deltaTime;
// //                     gazeLoadingCircle.fillAmount = currentGazeTime / gazeDurationThreshold;

// //                     if (currentGazeTime >= gazeDurationThreshold) // Gaze click activated
// //                     {
// //                         TriggerGazeClick(); // Implement this method to handle gaze click action
// //                         currentGazeTime = 0.0f; // Reset timer
// //                     }
// //                 }
// //                 else
// //                 {
// //                     currentGazeTime = 0.0f; // Reset timer if gaze moves off the target
// //                     gazeLoadingCircle.fillAmount = 0; // Reset loading circle
// //                 }
// //             }
// //             else
// //             {
// //                 cursorIndicator.SetActive(false); // Hide the cursor if not gazing at the web view
// //                 gazeLoadingCircle.fillAmount = 0; // Reset loading circle
// //                 currentGazeTime = 0.0f; // Reset gaze timer
// //             }
// //         }
// //         private Vector2 ProjectPointOnCanvas(Vector3 gazeHitPoint3D)
// //         {
// //             Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, gazeHitPoint3D);
// //             return screenPoint;
// //         }
// //         private void TriggerGazeClick()
// //         {
// //             // Here, you might simulate a click or call a method on the currentGazeTarget.
// //             Debug.Log("Gaze Click Triggered on Target");
// //             if (m_webview != null && currentGazeTarget)
// //             {
// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DOWN); // Simulate touch down
// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP); // Simulate touch up
// //             }
// //         }

// //         private Vector3 GetGazeHitPoint()
// //         {
// //             RaycastHit hitLeft, hitRight;
// //             Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
// //             Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);

// //             if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
// //             {
// //                 // Average the hit points for both eyes to account for binocular vision
// //                 Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;

// //                 // Optionally, apply a filter to smooth out the gaze point
// //                 Vector3 filteredGazePoint = GazeFilter(gazeHitPoint);
                
// //                 return filteredGazePoint;
// //             }

// //             return Vector3.zero; // Return Vector3.zero if no hit is detected
// //         }
// //         private bool GetInputPosition()
// //         {
// //             Vector3 gazeHitPoint3D = GetGazeHitPoint(); // Implement based on your eye tracking system
// //             if (gazeHitPoint3D == Vector3.zero) return false; // Early exit if no hit point detected

// //             Vector2 gazePointOnCanvas = ProjectPointOnCanvas(gazeHitPoint3D);
// //             if (!RectTransformUtility.RectangleContainsScreenPoint(webViewRectTransform, gazePointOnCanvas, uiCamera))
// //             {
// //                 return false; // Gaze is outside the web view
// //             }

// //             // Convert gaze point to local position within the web view
// //             Vector2 localPosition;
// //             RectTransformUtility.ScreenPointToLocalPointInRectangle(webViewRectTransform, gazePointOnCanvas, uiCamera, out localPosition);

// //             // Calculate normalized position within the web view
// //             float x = (localPosition.x - webViewRectTransform.rect.x) / webViewRectTransform.rect.width;
// //             float y = (localPosition.y - webViewRectTransform.rect.y) / webViewRectTransform.rect.height;

// //             // Convert to pixel coordinates based on the web view's resolution
// //             m_inputPosition = new Vector2Int((int)(x * m_webview.WebWidth), (int)(y * m_webview.WebHeight));
// //             return true;
// //         }
// //         // private bool GetInputPosition(PointerEventData eventData)
// //         // {
// //         //     Vector2 localPosition = Vector2.zero;

// //         //     RectTransform rectTransform = (RectTransform)transform;

// //         //     switch (m_renderMode)
// //         //     {
// //         //         case RenderMode.ScreenSpaceOverlay:
// //         //             localPosition = transform.InverseTransformPoint(eventData.position);
// //         //             break;
// //         //         case RenderMode.ScreenSpaceCamera:
// //         //         case RenderMode.WorldSpace:
// //         //             RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPosition);
// //         //             break;
// //         //     }

// //         //     float x = localPosition.x / rectTransform.rect.width + rectTransform.pivot.x;
// //         //     float y = 1f - (localPosition.y / rectTransform.rect.height + rectTransform.pivot.y);

// //         //     if (Math.Range(x, 0, 1) && Math.Range(y, 0, 1))
// //         //     {
// //         //         m_inputPosition = new Vector2Int((int)(x * m_webview.WebWidth), (int)(y * m_webview.WebHeight));

// //         //         return true;
// //         //     }

// //         //     m_inputPosition = Vector2Int.zero;

// //         //     return false;
// //         // }

// //         private Vector3 GazeFilter(Vector3 gazePoint)
// //         {
// //         gazePointWindow.Enqueue(gazePoint);
// //         while (gazePointWindow.Count > gazeFilterWindowSize)
// //         {
// //             gazePointWindow.Dequeue();
// //         }
        
// //         Vector3 filteredHitPoint = new Vector3(0f, 0f, 0f);
// //         int currentElementsInWindow = System.Math.Min(gazeFilterWindowSize, gazePointWindow.Count);
// //         Vector3[] gazePoints = gazePointWindow.ToArray();
// //         for (int i = 0; i < gazePoints.Length; ++i)
// //         {
// //             filteredHitPoint += gazePoints[i];
// //         }
// //         filteredHitPoint = filteredHitPoint / (float) currentElementsInWindow;
// //         return filteredHitPoint;
// //         }

// //         public void OnPointerDown()
// //         {
// //             if (m_pointerId == null && !m_pointerDown && GetInputPosition())
// //             {
// //                 m_pointerId = GazePointerId;

// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DOWN);

// //                 m_pointerDown = true;
// //             }
// //         }

// //         public void OnDrag()
// //         {
// //             if ((m_pointerId == GazePointerId) && m_pointerDown && GetInputPosition())
// //             {
// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.DRAG);
// //             }
// //         }

// //         public void OnPointerUp()
// //         {
// //             if ((m_pointerId == GazePointerId) && m_pointerDown && GetInputPosition())
// //             {
// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP);

// //                 m_pointerId = null;

// //                 m_pointerDown = false;
// //             }
// //         }

// //         public void OnPointerExit()
// //         {
// //             if ((m_pointerId == GazePointerId) && m_pointerDown)
// //             {
// //                 m_webview.TouchEvent(m_inputPosition.x, m_inputPosition.y, (int)WebTouchEvent.UP);

// //                 m_pointerId = null;

// //                 m_pointerDown = false;
// //             }
// //         }

// //         private void OnEnable()
// //         {
// //             Canvas canvas = GetComponentInParent<Canvas>();

// //             if (canvas == null)
// //             {
// //                 Debug.LogError(THIS_NAME + "canvas not found");
// //                 return;
// //             }

// //             m_renderMode = canvas.renderMode;

// //             m_pointerId = null;

// //             m_pointerDown = false;
// //         }

// //         private void OnDisable()
// //         {
// //             m_pointerId = null;

// //             m_pointerDown = false;
// //         }
// //     }

// //     public static class Math
// //     {
// //         public static bool Range(float i, float min, float max)
// //         {
// //             if (min >= max)
// //             {
// //                 return false;
// //             }

// //             return i >= min && i <= max;
// //         }
// //     }
// // }
