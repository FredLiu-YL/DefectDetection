using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public class ROIPolygon : ROIShape
    {
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty DrawPointsProperty;
        private Geometry thisgeometry;
        private GeometryGroup _ResizeGeometry = new GeometryGroup();
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();

        private List<PolygonRectangleGeometry> resizeGeometries = new List<PolygonRectangleGeometry>();
        private Point lastMousePosition;
        public ObservableCollection<Point> DrawPoints
        {
            get => (ObservableCollection<Point>)GetValue(DrawPointsProperty);
            set => SetValue(DrawPointsProperty, value);
        }

        static ROIPolygon()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(ROIPolygon), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ROIPolygon), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            DrawPointsProperty = DependencyProperty.Register("DrawPoints", typeof(ObservableCollection<Point>), typeof(ROIPolygon), new FrameworkPropertyMetadata(new ObservableCollection<Point>(), options, OnDataChanged));
        }

        public ROIPolygon(Point[] points)
        {
            DrawPoints = new ObservableCollection<Point>(points);
            var Len = RectLen;
            resizeGeometries = DrawPoints.Select((point, i) => new PolygonRectangleGeometry() { rectangleGeometry = new RectangleGeometry() { Rect = new Rect(point.X - Len / 2, point.Y - Len / 2, Len, Len) },index =i }).ToList();

            GeometryAction();
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                if (DrawPoints == null || DrawPoints.Count() == 0) return new LineGeometry();
                X = CenterX;
                Y = CenterY;
                List<PathSegment> segments = new List<PathSegment>();
                var Lists = DrawPoints.ToList();
                Lists.ForEach(p =>
                {
                    //var tmpP = new Point(p.X - ShapeLeft - 1, p.Y - ShapeTop - 1);
                    segments.Add(new LineSegment(p, true));
                });
                List<PathFigure> figures = new List<PathFigure>(1);
                var First = new Point(Lists.First().X - ShapeLeft - 1, Lists.First().Y - ShapeTop - 1);
                PathFigure pf = new PathFigure(Lists.First(), segments, true);
                figures.Add(pf);
                Geometry g = new PathGeometry(figures, FillRule.EvenOdd, null);
                thisgeometry = g;
                return g;
            }
        }

        protected override void ResetLeftTop()
        {
            double ShapeRight = DrawPoints.Select(point => point.X).Max();
            double ShapeButtom = DrawPoints.Select(point => point.Y).Max();

            ShapeLeft = DrawPoints.Select(point => point.X).Min();
            ShapeTop = DrawPoints.Select(point => point.Y).Min();

            DeltaX = Math.Abs(ShapeRight - ShapeLeft);
            DeltaY = Math.Abs(ShapeTop - ShapeButtom);
            Theta = 0.0;
            Distance = 0;

            LeftTop = new Point(ShapeLeft, ShapeTop);
            RightBottom = new Point(ShapeLeft + DeltaX, ShapeTop + DeltaY);
        }

        protected override void OnRender(DrawingContext dc)
        {
            Pen pen = null;
            Pen transparentPen = new Pen(Brushes.Transparent, 1);
            pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(Fill, pen, DefiningGeometry);

            pen = new Pen(CenterCrossBrush, StrokeThickness / 2);
            dc.DrawGeometry(Fill, pen, CenterCrossGeometry);

            if (IsInteractived)
            {
                if (IsResizeEnabled)
                {
                    pen = new Pen(Brushes.Green, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, ResizeGeometry);
                }
                if (IsMoveEnabled)
                    dc.DrawGeometry(Brushes.Transparent, transparentPen, TranslateGeometry);
            }
        }

        /// <summary>
        /// 中心十字
        /// </summary>
        private Geometry CenterCrossGeometry
        {
            get
            {

                GeometryGroup centerCrossGeometry = new GeometryGroup();

                LineGeometry _lineGeometryH = new LineGeometry(new Point(CenterX, CenterY - CenterCrossLength),
                                                               new Point(CenterX, CenterY + CenterCrossLength));
                LineGeometry _lineGeometryV = new LineGeometry(new Point(CenterX - CenterCrossLength, CenterY),
                                                               new Point(CenterX + CenterCrossLength, CenterY));

                centerCrossGeometry.Children.Add(_lineGeometryH);
                centerCrossGeometry.Children.Add(_lineGeometryV);

                return centerCrossGeometry;
            }
        }

        protected override Geometry ReturnContainGeometry(Point Pos)
        {
            if (ResizeGeometry.FillContains(Pos) && IsResizeEnabled)
            {
                foreach (var item in _ResizeGeometry.Children)
                    if (item.FillContains(Pos)) return item;
            }
            if (TranslateGeometry.FillContains(Pos) && IsMoveEnabled) return TranslateGeometry;
            return null;
        }

        /// <summary>
        /// 放大縮小 框
        /// </summary>
        private Geometry ResizeGeometry
        {
            get
            {
                _ResizeGeometry.Children.Clear();

                var Len = RectLen;

                resizeGeometries.Select(resize => { resize.rectangleGeometry.Rect = new Rect(DrawPoints[resize.index].X - Len / 2, DrawPoints[resize.index].Y - Len / 2, Len, Len);  return false; }).ToArray();
                
                resizeGeometries.ForEach(resize => _ResizeGeometry.Children.Add(resize.rectangleGeometry));

                return _ResizeGeometry;
            }
        }

        /// <summary>
        /// 移動 框
        /// </summary>
        private Geometry TranslateGeometry
        {
            get
            {
                var Len = RectLen;
                _TranslateGeometry.Rect = new Rect(new Point(CenterX - Len / 2, CenterY - Len / 2), new Size(Len, Len));
                return _TranslateGeometry;
            }
        }

        public void GeometryAction()
        {
            pairs.Clear();
            pairs.Add(_TranslateGeometry, Pos => {
                var position = Pos - new Point(X , Y);
                for (int i=  0; i < DrawPoints.Count(); i++) DrawPoints[i] = new Point(DrawPoints[i].X + position.X, DrawPoints[i].Y + position.Y);
            });

            resizeGeometries.ForEach(geo =>
            {
                pairs.Add(geo.rectangleGeometry, Pos =>
                 {
                     DrawPoints[geo.index] = Pos;
                 });
            });
        }

        public double X
        {
            get => (double)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public double Y
        {
            get => (double)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public double CenterX
        {
            get { return DrawPoints.Select(point => point.X).Average(); }
        }

        public double CenterY
        {
            get { return DrawPoints.Select(point => point.Y).Average(); }
        }

        public override string ShapeType => "Polygon";

        public override bool ShapeContains(Point point)
        {
            return thisgeometry.FillContains(point);
        }

        //public Point[] ShapeContain(Point[] points)
        //{
        //    return points.Where(point =>
        //    {
        //        return thisgeometry.FillContains(point);
        //    }).ToArray();
        //}

        /// <summary>
        /// 滑鼠移動事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!(Parent is IInputElement parent)) return;
            var mousePoint = e.GetPosition(parent);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                CaptureMouse();
            }
            else ReleaseMouseCapture();

            lastMousePosition = mousePoint;
            e.Handled = true;
        }
    }

    public sealed class PolygonRectangleGeometry
    {
        public int index;
        public RectangleGeometry rectangleGeometry;
    }
}
