using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RControllerUtil : MonoBehaviour
{
    // Start is called before the first frame update
    private LineRenderer rayLine;
    private float rayLen = 0.3f;
    public GameObject RController;
    public GameObject keyboardPanel;
    public GameObject divider;
    public GameObject textConsole;
    private Queue<Vector3> hitPointForGestureViewQueue = new Queue<Vector3>(); // contain InputTrajectoryView.POINT_TO_DRAW points
    private List<Vector3> hitPointForDecoder = new List<Vector3>(); // contain all points
    private Queue<Vector3> gazePointWindow = new Queue<Vector3>(); // filter

    private int gazeFilterWindowSize = 10;
    // private int countPoint = 0;
    private Vector3 hitPoint;
    public GameObject cursorIndicator;
    public GameObject rightEye;
    public GameObject leftEye;
    private float dividerTop;
    private float dividerBottom;
    private bool isGaze;
    private static bool IS_GAZE_DEFAULT = true;


    void Start()
    {
        //Debug.Log(TextConsole);
        isGaze = IS_GAZE_DEFAULT;
        rayLine = GetComponent<LineRenderer>();
        rayLine.positionCount = 0;
        cursorIndicator.SetActive(false);
        GetDividerInfo();
        HandelControllerModality();
        Debug.Log("RControllerUtil Start");
    }

    void GetDividerInfo()
    {
      RectTransform dividerRectTransform = divider.GetComponent<RectTransform>();
      Vector3[] dividerWorldCorners = new Vector3[4];
      dividerRectTransform.GetWorldCorners(dividerWorldCorners);
      dividerBottom = dividerWorldCorners[0].y;
      dividerTop = dividerWorldCorners[2].y;
    }

    // Update is called once per frame
    void Update()
    {
        if (isGaze) {
          HandleGazeModality();
        } else {
          HandelControllerModality();
        }
    }

    private void HandleGazeModality()
    {
      rayLine.positionCount = 0;
      RaycastHit hitLeft;
      RaycastHit hitRight;
      Ray gazeRayLeft = new Ray(leftEye.transform.position, leftEye.transform.forward);
      Ray gazeRayRight = new Ray(rightEye.transform.position, rightEye.transform.forward);
      if (Physics.Raycast(gazeRayLeft, out hitLeft) && Physics.Raycast(gazeRayRight, out hitRight))
      {
        Vector3 gazeHitPoint = (hitLeft.point + hitRight.point) / 2;
        Vector3 filteredGamePoint = GazeFilter(gazeHitPoint);
        HandleHitPoint(filteredGamePoint);
      } else {
        cursorIndicator.SetActive(false);
        hitPointForGestureViewQueue.Clear();
        // countPoint = 0;
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

    private void HandelControllerModality()
    {
      RaycastHit hitController;
      Vector3 rayOriginController = RController.transform.position;
      Vector3 direction = RController.transform.forward;
      Ray controllerRay = new Ray(rayOriginController, direction);
      Vector3 rayEndController = rayOriginController + rayLen * direction;

      if (Physics.Raycast(controllerRay, out hitController))
      {
        HandleHitPoint(hitController.point);
      } else {
        hitPointForGestureViewQueue.Clear();
        // countPoint = 0;
        cursorIndicator.SetActive(false);
      }

      Vector3 displayRayEnd = rayOriginController + rayLen * direction;
      Vector3[] linePoints = { rayOriginController, displayRayEnd };
      rayLine.SetColors(Color.grey, Color.grey);
      rayLine.positionCount = linePoints.Length;
      rayLine.SetWidth(0.003f, 0.005f);
      rayLine.SetPositions(linePoints);
    }

    private void HandleHitPoint(Vector3 point) {
      Vector3 rayEnd = point;
      if (rayEnd != hitPoint) {
        hitPointForDecoder.Add(rayEnd);
        // We only handle the points that are below the divider, for drawing gesture view 
        if (rayEnd.y < dividerBottom) {
          if (hitPointForGestureViewQueue.Count < InputTrajectoryView.POINT_TO_DRAW){
            hitPointForGestureViewQueue.Enqueue(rayEnd);
            // countPoint += 1;
          } else {
            hitPointForGestureViewQueue.Dequeue();
            hitPointForGestureViewQueue.Enqueue(rayEnd);
          }
          // hitPointForDecoder.Add(rayEnd);
        } else {
          // Clear the queue otherwise
          hitPointForGestureViewQueue.Clear();
          // countPoint = 0;
          // hitPointForDecoder.Clear();
        }
        // textConsole.GetComponent<TextMeshPro>().SetText("World coordinates of the hit point:" + rayEnd + "Points count:" + countPoint + "ruiliu1");
        hitPoint = rayEnd;
      }
      cursorIndicator.SetActive(true);
      cursorIndicator.transform.position = hitPoint;
    }

    public Queue<Vector3> GethitPointForGestureView(){
      return hitPointForGestureViewQueue;
    }

    // call this method in the decoder
    public List<Vector3> GetHitPointQueueForDecoder()
    {
      return hitPointForDecoder;
    }

    public Vector3 GetHitPoint() {
      return hitPoint;
    }

    public void SetIsGaze(bool modality) {
      isGaze = modality;
      // We should clear the existing points
      hitPointForGestureViewQueue.Clear();
      hitPointForDecoder.Clear();
      gazePointWindow.Clear();
    }
    
    public bool GetIsGaze()
    {
      return isGaze;
    }

    public float GetDividerBottom() {
      return dividerBottom;
    }

}
