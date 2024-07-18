using System;
using UnityEngine;
using UnityEngine.UI;

public class OnScreenInputField
{
    public GameObject mInputFieldGameObject;
    public string mInputFieldName;
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
    private InputField inputFieldComponent;

    public OnScreenInputField(GameObject gameObject, string inputFieldName)
    {
        mInputFieldGameObject = gameObject;
        mInputFieldName = inputFieldName;
        inputFieldComponent = gameObject.GetComponent<InputField>();
        ComputeGameObjectInfo(mInputFieldGameObject);
    }

    public GameObject GetGameObject()
    {
        return mInputFieldGameObject;
    }

    public InputField GetInputFieldComponent()
    {
        return inputFieldComponent;
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
        diag = (float)Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
    }

    public bool containPoint(float x, float y)
    {
        if (x <= right && x >= left && y <= top && y >= bottom)
        {
            return true;
        }
        return false;
    }

    public string GetInputFieldName()
    {
        return mInputFieldName;
    }

    public Vector4 GetInputFieldBoundary()
    {
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

    public float GetDiagDistanceToInputField(float x, float y)
    {
        return (float)Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2)) / (diag / 2);
    }
}
