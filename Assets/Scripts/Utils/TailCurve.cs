using System.Collections;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Utils
{
    public class TailCurve
    {
        private static readonly string TAG = typeof(TailCurve).Name;
        // keepLength set to 0.028f
        private static float keepLength = 0.028f;

        private static int GetTailPointsNumber(List<Vector3> points)
        {
            float accLength = 0.0f;
            int count = 0;
            if (points.Count < 2)
            {
                return 0;
            }
            for (int i = points.Count - 1; i > 0; i--)
            {
                count += 1;
                accLength = (float)Math.Sqrt(Math.Pow(points[i].x - points[i - 1].x, 2) + Math.Pow(points[i].y - points[i - 1].y, 2));
                if (accLength > keepLength)
                {
                    break;
                }
            }
            return count;
        }

        public static float GetTailCurve(List<Vector3> trace)
        {
            int n = GetTailPointsNumber(trace);
            List<Vector3> tail = trace.GetRange(trace.Count - n, n);
            int degree = 180;
            if (n > 2)
            {
                int mid = n / 2;
                Vector3 P1 = tail[0], P2 = tail[mid], P3 = tail[n - 1];
                degree = GetDegree(P2.x, P2.y, P1.x, P1.y, P3.x, P3.y);
            }
            degree = degree == 0 ? 180 : degree;
            float result = (198f - degree) / 180.0f;
            return result;
        }

        public static float GetTailStability(List<Vector3> trace)
        {
            int n = GetTailPointsNumber(trace);
            float stability = 0f;
            if (n > 2)
            {
                // filter 0.014f
                stability = (float) n * 0.014f / keepLength;
            }
            return stability;
        }

        private static int GetDegree(float vertexPointX, float vertexPointY, float point0X, float point0Y, float point1X, float point1Y)
        {
            float vector = (point0X - vertexPointX) * (point1X - vertexPointX) + (point0Y - vertexPointY) * (point1Y - vertexPointY);
            float sqrt = (float) (Math.Sqrt(Math.Pow(point0X - vertexPointX, 2) + Math.Pow(point0Y - vertexPointY, 2)) *
                          Math.Sqrt(Math.Pow(point1X - vertexPointX, 2) + Math.Pow(point1Y - vertexPointY, 2)));
            float radian = (float) Math.Acos(vector / sqrt);
            return (int)(180 * radian / Math.PI);
        }
    }
}
