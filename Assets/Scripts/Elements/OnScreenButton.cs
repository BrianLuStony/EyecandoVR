using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class OnScreenButton
{
    public enum ButtonType { SUGGESTION, FUNCTIONAL, KEY };

    public GameObject mButtonGameObject;
    public ButtonType mButtonType;
    public string mButtonName;
    public float bottom;
    public float top;
    public float left;
    public float right;
    public float centerX;
    public float centerY;
    public float width;
    public float height;
    public float diag;
    public float accumulatedFrames = 0;

    public OnScreenButton(GameObject gameObject, string buttonName, ButtonType buttonType)
    {
        mButtonGameObject = gameObject;
        mButtonName = buttonName;
        mButtonType = buttonType;
        ComputeGameObjectInfo(mButtonGameObject);
    }

    public GameObject GetGameObject()
    {
        return mButtonGameObject;
    }

    public void ComputeGameObjectInfo(GameObject gameObject)
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        Vector3[] worldCorners = new Vector3[4];
        rectTransform.GetWorldCorners(worldCorners);
        Vector3 bottomLeft = worldCorners[0];
        Vector3 topRight = worldCorners[2];
        bottom = bottomLeft.y;
        top = topRight.y;
        left = bottomLeft.x;
        right = topRight.x;
        centerX = (left + right) / 2f;
        centerY = (top + bottom) / 2f;
        width = Math.Abs(right - left);
        height = Math.Abs(bottom - top);
        diag = (float) Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
    }

    public bool containPoint(float x, float y)
    {
        if (x <= right && x >= left && y <= top && y >= bottom)
        {
            return true;
        }
        return false;
    }

    public String GetButtonName() {
      return mButtonName;
    }

    public Vector4 GetButtonBoundary() {
      return new Vector4(left, right, bottom, top);
    }

    public bool ContainPointWithPadding(float x, float y, float widthPadding, float heightPadding)
    {
        float paddingLeft = left - (widthPadding * width);
        float paddingRight = right + (widthPadding * width);
        float paddingTop = top + (heightPadding * height);
        float paddingBottom = bottom - (heightPadding * height);

        return x >= paddingLeft && x < paddingRight && y < paddingTop && y >= paddingBottom;
    }

    public float GetDiagDistanceToKey(float x, float y)
    {
        return (float) Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2)) / (diag / 2);
    }
}
