using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Windows;
// using System.Windows.Media;
// using System.Windows.Media.Effects;
// using System.Windows.Shapes;
// using System.Windows.Threading;
//using Point = System.Drawing.Point;
using TMPro;

// checked

//namespace View
//{
    // This class visualize the input trajectory
    public class InputTrajectoryView : MonoBehaviour
    {
        public GameObject hitPointScriptObj;
        public GameObject textConsole;
        private RControllerUtil hitPointScript;
        private LineRenderer trajectory;
        public static int POINT_TO_DRAW = 60;

        void Start() {
          hitPointScript = hitPointScriptObj.GetComponent<RControllerUtil>();
          //textConsole.GetComponent<TextMeshPro>().SetText("World coordinates of the hit point.");
          trajectory = this.GetComponent<LineRenderer>();
          trajectory.positionCount = POINT_TO_DRAW;
          trajectory.SetColors(Color.blue, Color.blue);
          trajectory.SetWidth(0.005f, 0.01f);
        }

        void Update() {
          Vector3[] linePoints = hitPointScript.GethitPointForGestureView().ToArray();
          if (linePoints.Length > 0) {
            trajectory.positionCount = linePoints.Length;
            trajectory.SetPositions(linePoints);
          } else {
            trajectory.positionCount = 0;
            trajectory.SetVertexCount(0);
          }
          //textConsole.GetComponent<TextMeshPro>().SetText("World coordinates of the hit point:" + linePoints.Length);
        }
        // private Path _eyeTrace;
        // private Ellipse _halo;
        // private Panel _drawPane;

      //  public InputTrajectoryView()
      //  {
            // _drawPane = drawPane;

            // _eyeTrace = new Path
            // {
            //     Stroke = new SolidColorBrush(Color.FromArgb(102, 77, 182, 172)),
            //     StrokeStartLineCap = PenLineCap.Round,
            //     StrokeEndLineCap = PenLineCap.Round,
            //     StrokeThickness = 0,
            //     StrokeLineJoin = PenLineJoin.Round
            // };

            // int blurRadius = 5;
            // int radius = 3;
            // var blur = new GaussianBlurEffect {Radius = blurRadius};
            // _halo = new Ellipse {Width = radius * 2, Height = radius * 2, Fill = new SolidColorBrush(Color.FromArgb(234, 29, 128, 222)), Effect = blur};
        //}

        //public void DrawTrace(Point point)
        //{
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     int x = point.X, y = point.Y;
            //     if (_eyeTrace.Data == null)
            //     {
            //         _eyeTrace.Data = new PathGeometry(new List<PathFigure> {new PathFigure(new Point(x, y), new List<PathSegment>(), false)});
            //     }
            //     else
            //     {
            //         ((PathGeometry)_eyeTrace.Data).Figures[0].Segments.Add(new LineSegment(new Point(x, y), true));
            //     }
            // });

            // return point;
            // return null;
        //}

        //public void SetCursorWidth(int cursorWidth)
        //{
            // _halo = new Ellipse {Width = 0, Height = 0};
        //}

        //public void DrawPoint(Point point, double score)
        //{
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     double x = point.X, y = point.Y;
            //     Canvas.SetLeft(_halo, x - _halo.Width / 2);
            //     Canvas.SetTop(_halo, y - _halo.Height / 2);
            //     //halo.Radius = Math.Pow(score * 20, 0.8);
            // });
      //  }

        //public void StartDrawing()
        //{
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     _drawPane.Children.Clear();
            //     _eyeTrace.Data = null;
            //     _drawPane.Children.Add(_eyeTrace);
            //     _drawPane.Children.Add(_halo);
            // });
        //}

        //public void IsShowTrace(bool isShow)
        //{
            // _eyeTrace.StrokeThickness = isShow ? 5 : 0;
        //}

        //public void ClearTrace()
        //{
            // _eyeTrace.Data = null;
        //}

        //public void StopDrawing()
        //{
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     _eyeTrace.Data = null;
            //     _drawPane.Children.Clear();
            // });
        //}

        //public void DrawPoints(List<Point> points, Color color)
        //{
            // _drawPane.Children.Clear();
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     foreach (Point p in points)
            //     {
            //         var circle = new Ellipse {Width = 20, Height = 20, Fill = new SolidColorBrush(color)};
            //         Canvas.SetLeft(circle, p.X - circle.Width / 2);
            //         Canvas.SetTop(circle, p.Y - circle.Height / 2);
            //         _drawPane.Children.Add(circle);
            //     }
            // });
        //}

        //public void DrawPointsPair(List<KeyValuePair<Point, double>> points, Color color)
        //{
            // _drawPane.Children.Clear();
            // _drawPane.Dispatcher.Invoke(() =>
            // {
            //     foreach (KeyValuePair<Point, double> p in points)
            //     {
            //         var circle = new Ellipse { Width = p.Value * 20, Height = p.Value * 20, Fill = new SolidColorBrush(color) };
            //         Canvas.SetLeft(circle, p.Key.X - circle.Width / 2);
            //         Canvas.SetTop(circle, p.Key.Y - circle.Height / 2);
            //         _drawPane.Children.Add(circle);
            //     }
            // });
        //}

        //public void GetGazeTransferredAxis(double x, double y)
        //{
            // System.Windows.Rect screenBounds = SystemParameters.PrimaryScreenWidth * SystemParameters.PrimaryScreenHeight;
            // double screenX = (int)(screenBounds.Width * x);
            // double screenY = (int)(screenBounds.Height * y);
            // // Get relative position towards screen
            // var transform = _drawPane.TransformToVisual(Application.Current.MainWindow);
            // Point offset = transform.Transform(new Point(0, 0));
            // double startX = offset.X + 1;
            // double startY = offset.Y + 1;

            // return new Point((int)(screenX - startX), (int)(screenY - startY));
        //}
    }
//}
