using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearRegression;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public sealed class AngleShape : Shape
    {
        public static readonly DependencyProperty StartpointProperty = DependencyProperty.Register(nameof(StartPoint), typeof(Point), typeof(AngleShape), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(PropertyChangedEvent)));

        public static readonly DependencyProperty EndpointProperty = DependencyProperty.Register(nameof(EndPoint), typeof(Point), typeof(AngleShape), new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(PropertyChangedEvent)));

        public static readonly DependencyProperty IncludedAngleProperty = DependencyProperty.Register(nameof(IncludedAngle), typeof(double), typeof(AngleShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(PropertyChangedEvent)));

        public static readonly DependencyProperty IsAngleAdjustableProperty = DependencyProperty.Register(nameof(IsAngleAdjustable), typeof(bool), typeof(AngleShape), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(PropertyChangedEvent)));

        /// <summary>
        /// 取得及設定夾角圓心點
        /// </summary>
        public Point StartPoint
        {
            get { return (Point)GetValue(StartpointProperty); }
            set { SetValue(StartpointProperty, value); }
        }

        /// <summary>
        /// 取得及設定線段終點
        /// </summary>
        public Point EndPoint
        {
            get { return (Point)GetValue(EndpointProperty); }
            set { SetValue(EndpointProperty, value); }
        }

        /// <summary>
        /// 取得及設定夾角大小夾角
        /// </summary>
        public double IncludedAngle
        {
            get { return (double)GetValue(IncludedAngleProperty); }
            set { SetValue(IncludedAngleProperty, value); }
        }

        /// <summary>
        /// 取得及設定角度是否可調節
        /// </summary>
        public bool IsAngleAdjustable
        {
            get { return (bool)GetValue(IsAngleAdjustableProperty); }
            set { SetValue(IsAngleAdjustableProperty, value); }
        }

        /// <summary>
        /// 弧線半徑
        /// </summary>
        private double Radius;

        /// <summary>
        /// 弧線起點
        /// 以半徑Radius與線條焦點
        /// </summary>
        private Point ArcStartPoint;

        /// <summary>
        /// 弧線終點
        /// 以半徑Radius與線條焦點
        /// </summary>
        private Point ArcEndPoint;

        /// <summary>
        /// (計算出來的)夾角另外一條線段的終點
        /// </summary>
        private Point ThirdPoint;

        /// <summary>
        /// 組成此夾角的三個點的Dictionary
        /// </summary>
        private Dictionary<Point, Point> dicOfPoints;

        private static void PropertyChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var _this = d as AngleShape;
            if (_this.StartPoint != null && _this.EndPoint != null && _this.IncludedAngle != 0.0)
            {
                RotateTransform trans = new RotateTransform(-_this.IncludedAngle, _this.StartPoint.X, _this.StartPoint.Y);
                _this.ThirdPoint = trans.Transform(_this.EndPoint);
                double length = (_this.StartPoint - _this.EndPoint).Length;
                _this.Radius = length / 3;
                _this.dicOfPoints = new Dictionary<Point, Point>()
                {
                    [_this.ThirdPoint] = _this.StartPoint,
                    [_this.StartPoint] = _this.EndPoint,
                    [_this.EndPoint] = _this.ThirdPoint
                };
            }
        }

        public AngleShape(Point center, Point point, double includedAngle)
        {
            EndPoint = point;
            StartPoint = center;
            IncludedAngle = Math.Round(includedAngle, 4);
            RotateTransform trans = new RotateTransform(-includedAngle, StartPoint.X, StartPoint.Y);
            ThirdPoint = trans.Transform(EndPoint);
            double length = (StartPoint - EndPoint).Length;
            Radius = length / 3;
        }

        public AngleShape() { }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
                Point mousePosition = e.GetPosition(this);
                //Drag points
                if ((mousePosition - EndPoint).Length < 10)
                {
                    Vector? offset = mousePosition - EndPoint;
                    DragToChangeLength(EndPoint, (Point)offset);
                    e.Handled = true;
                    return;
                }
                else if ((mousePosition - StartPoint).Length < 10)
                {
                    Vector? offset = mousePosition - StartPoint;
                    DragToMove((Point)offset);
                    e.Handled = true;
                    return;
                }
                else if ((mousePosition - ThirdPoint).Length < 10)
                {
                    Vector? offset = mousePosition - ThirdPoint;
                    DragToChangeLength(ThirdPoint, (Point)offset);
                    e.Handled = true;
                    return;
                }
                Point? pos = GetPointOnLine(mousePosition);
                //Drag lines
                if (pos != null && (mousePosition - StartPoint).Length > 10)
                {
                    DragToChangeAngle(mousePosition);
                    e.Handled = true;
                    return;
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        private Point? GetPointOnLine(Point mousePoint)
        {
            if (StartPoint == ThirdPoint) return null;
            Point2D mouse = new Point2D(mousePoint.X, mousePoint.Y);
            Line2D line1 = new Line2D(new Point2D(StartPoint.X, StartPoint.Y), new Point2D(EndPoint.X, EndPoint.Y));
            Line2D line2 = new Line2D(new Point2D(StartPoint.X, StartPoint.Y), new Point2D(ThirdPoint.X, ThirdPoint.Y));
            Point point1 = new Point(line1.ClosestPointTo(mouse, false).X, line1.ClosestPointTo(mouse, false).Y);
            Point point2 = new Point(line2.ClosestPointTo(mouse, false).X, line2.ClosestPointTo(mouse, false).Y);
            double len1 = (mousePoint - point1).Length;
            double len2 = (mousePoint - point2).Length;
            if (len1 < 5) return point1;
            else if (len2 < 5) return point2;
            else return null;
        }

        /// <summary>
        /// 改變夾角大小
        /// </summary>
        /// <param name="mousePosition"></param>
        private void DragToChangeAngle(Point mousePosition)
        {
            InvalidateVisual();
            if (!IsAngleAdjustable) return;

            Point point = GetIntersectionOnLine(mousePosition, StartPoint, (StartPoint - EndPoint).Length);
            Point tempPoint;
            if ((mousePosition - EndPoint).Length < (mousePosition - ThirdPoint).Length)
            {
                tempPoint = ThirdPoint;
                EndPoint = point;
                ThirdPoint = tempPoint;
                Vector vector1 = (Vector)StartPoint - (Vector)EndPoint;
                Vector vector2 = (Vector)ThirdPoint - (Vector)StartPoint;
                double angle = Vector.AngleBetween(vector1, vector2);
                IncludedAngle = Math.Round(180 - angle, 4);
            }
            else
            {
                tempPoint = EndPoint;
                ThirdPoint = point;
                EndPoint = tempPoint;
                Vector vector1 = (Vector)StartPoint - (Vector)EndPoint;
                Vector vector2 = (Vector)StartPoint - (Vector)ThirdPoint;
                double angle = (Vector.AngleBetween(vector1, vector2));
                if (angle < 0) IncludedAngle = Math.Round(-angle, 4);
                else if (angle > 0) IncludedAngle = Math.Round(360 - angle, 4);
            }
            ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius);
            ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius);
            if ((ArcEndPoint - ArcStartPoint).Length > (ThirdPoint - EndPoint).Length || double.IsNaN(ArcStartPoint.X) || double.IsNaN(ArcStartPoint.Y) || double.IsNaN(ArcEndPoint.X) || double.IsNaN(ArcEndPoint.Y))
            {
                ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius, true);
                ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius, true);
            }
        }

        /// <summary>
        /// 改變線段長度(兩條線等長)
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="offset"></param>
        private void DragToChangeLength(Point pos, Point offset)
        {
            InvalidateVisual();

            int index = dicOfPoints.Select(_ => _.Value).ToList().IndexOf(pos);
            Point point = dicOfPoints.Values.ToArray()[(index + 1) % 3];
            RotateTransform trans;
            if (pos == EndPoint) trans = new RotateTransform(-IncludedAngle, StartPoint.X, StartPoint.Y);
            else trans = new RotateTransform(IncludedAngle, StartPoint.X, StartPoint.Y);

            Point check = trans.Transform(pos);
            if (point != check) point = dicOfPoints.Values.ToArray()[(index + 2) % 3];

            Line2D line1 = new Line2D(new Point2D(StartPoint.X, StartPoint.Y), new Point2D(pos.X, pos.Y));
            Point2D draggedPoint = new Point2D(pos.X + offset.X, pos.Y + offset.Y);
            Point newPos = new Point(line1.ClosestPointTo(draggedPoint, false).X, line1.ClosestPointTo(draggedPoint, false).Y);
            Point newPoint = trans.Transform(newPos);
            double length = (newPos - StartPoint).Length;
            Radius = length / 3;
            ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius);
            ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius);
            if ((ArcEndPoint - ArcStartPoint).Length > (ThirdPoint - EndPoint).Length || double.IsNaN(ArcStartPoint.X) || double.IsNaN(ArcStartPoint.Y) || double.IsNaN(ArcEndPoint.X) || double.IsNaN(ArcEndPoint.Y))
            {
                ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius, true);
                ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius, true);
            }

            if (pos == ThirdPoint)
            {
                EndPoint = newPoint;
                ThirdPoint = newPos;
            }
            else
            {
                ThirdPoint = newPoint;
                EndPoint = newPos;
            }
        }

        /// <summary>
        /// 移動夾角
        /// </summary>
        /// <param name="offset"></param>
        private void DragToMove(Point offset)
        {
            InvalidateVisual();
            StartPoint = new Point(StartPoint.X + offset.X, StartPoint.Y + offset.Y);
            EndPoint = new Point(EndPoint.X + offset.X, EndPoint.Y + offset.Y);
            ThirdPoint = new Point(ThirdPoint.X + offset.X, ThirdPoint.Y + offset.Y);
            ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius);
            ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius);
            if ((ArcEndPoint - ArcStartPoint).Length > (ThirdPoint - EndPoint).Length || double.IsNaN(ArcStartPoint.X) || double.IsNaN(ArcStartPoint.Y) || double.IsNaN(ArcEndPoint.X) || double.IsNaN(ArcEndPoint.Y))
            {
                ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius, true);
                ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius, true);
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            var pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(null, pen, IncludedAngleGrometry);
            dc.DrawGeometry(null, pen, ArcGeomerty);
            dc.DrawGeometry(null, pen, AngleText);
        }

        /// <summary>
        /// 取得圓與線段的交點
        /// 帶入兩點以及半徑，第二個點同時是圓心
        /// </summary>
        /// <param name="pointOnLine">點1</param>
        /// <param name="center">點2(圓心)</param>
        /// <param name="radius">圓半徑</param>
        /// <param name="multiplier"></param>
        /// <returns></returns>
        private Point GetIntersectionOnLine(Point pointOnLine, Point center, double radius, bool multiplier = false)
        {
            var line = Fit.Line(
                new[] { pointOnLine.X, center.X },
                new[] { pointOnLine.Y, center.Y });
            //item1=b
            double item1 = line.Item1;
            //item2=a
            double item2 = line.Item2;

            double x1, x2, y1, y2, A, B, C, D;

            A = Math.Pow(item2, 2) + 1;
            B = 2 * (item2 * item1 - item2 * center.Y - center.X);
            C = Math.Pow(item1 - center.Y, 2) + Math.Pow(center.X, 2) - Math.Pow(radius, 2);
            D = Math.Pow(B, 2) - 4 * A * C;

            x1 = (-B + Math.Sqrt(D)) / (2 * A); y1 = item2 * x1 + item1;
            x2 = (-B - Math.Sqrt(D)) / (2 * A); y2 = item2 * x2 + item1;

            if (multiplier || D < 0)
            {
                if (pointOnLine.X.ToString() == center.X.ToString())
                {
                    x1 = x2 = pointOnLine.X;
                    double bb4ac = -Math.Pow(x1, 2) + 2 * x1 * center.X + Math.Pow(center.X, 2) + Math.Pow(radius, 2);
                    if (bb4ac < 0) { }
                    //y1 = center.Y + Math.Sqrt(bb4ac);
                    //y2 = center.Y - Math.Sqrt(bb4ac);
                    y1 = center.Y + radius;
                    y2 = center.Y - radius;
                }
                else if (pointOnLine.Y.ToString() == center.Y.ToString() && (double.IsNaN(x1) || double.IsNaN(x2)))
                {
                    item1 = 0;
                    item2 = pointOnLine.Y;
                    A = Math.Pow(item2, 2) + 1;
                    B = 2 * (item2 * item1 - item2 * center.Y - center.X);
                    C = Math.Pow(item1 - center.Y, 2) + Math.Pow(center.X, 2) - Math.Pow(radius, 2);
                    D = Math.Pow(B, 2) - 4 * A * C;

                    x1 = (-B + Math.Sqrt(D)) / (2 * A); y1 = item2 * x1 + item1;
                    x2 = (-B - Math.Sqrt(D)) / (2 * A); y2 = item2 * x2 + item1;
                }
            }
            if ((new Point(x1, y1) - pointOnLine).Length < (new Point(x2, y2) - pointOnLine).Length) return new Point(x1, y1);
            return new Point(x2, y2);
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup gg = new GeometryGroup();
                gg.Children.Add(IncludedAngleGeometry);
                return gg;
            }
        }

        private Geometry IncludedAngleGeometry
        {
            get
            {
                LineGeometry lineGeometry1 = new LineGeometry()
                {
                    StartPoint = StartPoint,
                    EndPoint = EndPoint
                };
                LineGeometry lineGeometry2 = new LineGeometry()
                {
                    StartPoint = ThirdPoint,
                    EndPoint = EndPoint
                };
                GeometryGroup geometryGroup = new GeometryGroup();
                geometryGroup.Children.Add(lineGeometry1);
                geometryGroup.Children.Add(lineGeometry2);

                ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius);
                ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius);
                if ((ArcEndPoint - ArcStartPoint).Length > (ThirdPoint - EndPoint).Length || double.IsNaN(ArcStartPoint.X) || double.IsNaN(ArcStartPoint.Y) || double.IsNaN(ArcEndPoint.X) || double.IsNaN(ArcEndPoint.Y))
                {
                    ArcStartPoint = GetIntersectionOnLine(EndPoint, StartPoint, Radius, true);
                    ArcEndPoint = GetIntersectionOnLine(ThirdPoint, StartPoint, Radius, true);
                }

                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    context.BeginFigure(ArcStartPoint, false, false);
                    context.ArcTo(ArcEndPoint, new Size(Radius, Radius), 0.0, false, SweepDirection.Counterclockwise, true, true);
                }
                geometryGroup.Children.Add(geometry);
                return geometryGroup;
            }
        }

        /// <summary>
        /// Draw Lines
        /// </summary>
        private PathGeometry IncludedAngleGrometry
        {
            get
            {
                PathSegment[] segments = new PathSegment[]
                {
                    new LineSegment(StartPoint, true),
                    new LineSegment(EndPoint, true)
                };
                PathFigure figure = new PathFigure(ThirdPoint, segments, false);
                return new PathGeometry(new PathFigure[] { figure });
            }
        }

        /// <summary>
        /// Draw Arc
        /// </summary>
        private StreamGeometry ArcGeomerty
        {
            get
            {
                StreamGeometry geometry = new StreamGeometry();
                using (StreamGeometryContext context = geometry.Open())
                {
                    context.BeginFigure(ArcStartPoint, false, false);
                    if (IncludedAngle < 180)
                        context.ArcTo(ArcEndPoint, new Size(Radius, Radius), 0.0, false, SweepDirection.Counterclockwise, true, true);
                    else
                        context.ArcTo(ArcEndPoint, new Size(Radius, Radius), 0.0, true, SweepDirection.Counterclockwise, true, true);
                }
                return geometry;
            }
        }

        /// <summary>
        /// Draw Text Of Angle
        /// </summary>
        private Geometry AngleText
        {
            get
            {
                string showenText = IncludedAngle.ToString() + "°";
                Point textDirection = new Point((ArcEndPoint.X + ArcStartPoint.X) / 2, (ArcEndPoint.Y + ArcStartPoint.Y) / 2);
                Vector vector = (Vector)textDirection - (Vector)StartPoint;
                textDirection += vector;
                FormattedText text = new FormattedText(showenText, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 16, Brushes.Black, 1.25);
                Geometry geometry = text.BuildGeometry(textDirection);
                return geometry;
            }
        }


    }
}
