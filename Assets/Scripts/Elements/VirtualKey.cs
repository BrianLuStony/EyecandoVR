using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Drawing;

// checked

namespace Elements
{
    public class VirtualKey
    {
        public char Key { get; set;}
        private int top;
        private int bottom;
        private int left;
        private int right;
        private int width;
        private int height;
        private int centerX;
        private int centerY;
        private double diag;

        // private ProgressSquare progressSquare; // Their corresponding view?

        public VirtualKey(char key, int centerX, int centerY, int top, int bottom, int left, int right)
        {
            Key = key;
            this.top = top;
            this.bottom = bottom;
            this.left = left;
            this.right = right;
            width = Math.Abs(right - left);
            height = Math.Abs(bottom - top);
            diag = Math.Sqrt(Math.Pow(width, 2) + Math.Pow(height, 2));
            this.centerX = centerX;
            this.centerY = centerY;
            // this.progressSquare = progressSquare;
        }

        // Commented because of no usage
        // public bool IsPointInKey(Point point)
        // {
        //     int x = point.X, y = point.Y;
        //     return x >= left && x < right && y >= top && y < bottom;
        // }

        public int GetCenterX()
        {
            return centerX;
        }

        public int GetCenterY()
        {
            return centerY;
        }

        public bool IsPointInKeyWithPadding(int x, int y, double widthPadding, double heightPadding)
        {
            double paddingLeft = left - (widthPadding * width);
            double paddingRight = right + (widthPadding * width);
            double paddingTop = top - (heightPadding * height);
            double paddingBottom = bottom + (heightPadding * height);

            return x >= paddingLeft && x < paddingRight && y >= paddingTop && y < paddingBottom;
        }

        public double GetDiagDistanceToKey(int x, int y)
        {
            return Math.Sqrt(Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2)) / (diag / 2);
        }

        // public ProgressSquare GetProgressSquare()
        // {
        //     return progressSquare;
        // }
    }
}
