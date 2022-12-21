using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace YuanliCore.Views.CanvasShapes
{
    public class ROICircle : ROIShape
    {
        public static readonly DependencyProperty CenterXProperty;
        public static readonly DependencyProperty CenterYProperty;
        public static readonly DependencyProperty RadiusProperty;
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();
        private GeometryGroup _ResizeGeometry = new GeometryGroup();
        private EllipseGeometry _ellipseGeometry = new EllipseGeometry();
        private Geometry thisgeometry;
        private Point lastMousePosition;

        /// <summary>
        /// 半徑 X座標
        /// </summary>
        public double X
        {
            get => (double)GetValue(CenterXProperty);
            set => SetValue(CenterXProperty, value);
        }

        /// <summary>
        /// 半徑 Y座標
        /// </summary>
        public double Y
        {
            get => (double)GetValue(CenterYProperty);
            set => SetValue(CenterYProperty, value);
        }

        /// <summary>
        /// 半徑
        /// </summary>
        public double Radius
        {
            get => (double)GetValue(RadiusProperty);
            set => SetValue(RadiusProperty, value);
        }

        /// <summary>
        /// 中心十字
        /// </summary>
        private Geometry CenterCrossGeometry
        {
            get
            {
                GeometryGroup centerCrossGeometry = new GeometryGroup();

                LineGeometry _lineGeometryH = new LineGeometry(new Point(_ellipseGeometry.Center.X, _ellipseGeometry.Center.Y - CenterCrossLength), new Point(_ellipseGeometry.Center.X, _ellipseGeometry.Center.Y + CenterCrossLength));
                LineGeometry _lineGeometryV = new LineGeometry(new Point(_ellipseGeometry.Center.X - CenterCrossLength, _ellipseGeometry.Center.Y), new Point(_ellipseGeometry.Center.X + CenterCrossLength, _ellipseGeometry.Center.Y));

                centerCrossGeometry.Children.Add(_lineGeometryH);
                centerCrossGeometry.Children.Add(_lineGeometryV);

                return centerCrossGeometry;
            }
        }

        /// <summary>
        /// 移動 框
        /// </summary>
        private Geometry TranslateGeometry
        {
            get
            {
                double DrawX = X - ShapeLeft;
                double DrawY = Y - ShapeTop;
                _TranslateGeometry.Rect = new Rect(DrawX - Radius * 0.9, DrawY - Radius * 0.9, Radius * 1.8, Radius * 1.8);
                _TranslateGeometry.RadiusX = _TranslateGeometry.RadiusY = X;
                return _TranslateGeometry;
            }
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
                double DrawX = X - ShapeLeft;
                double DrawY = Y - ShapeTop;
                var tmpRects = new List<RectangleGeometry>();
                var range = Enumerable.Range(-1, 3).Where(i => i % 2 != 0).Select(i => i * Radius).ToList();
                range.ForEach(i => _ResizeGeometry.Children.Add(new RectangleGeometry(
                                                                new Rect(DrawX - i - Len / 2, DrawY - Len / 2, Len, Len))));
                range.ForEach(i => _ResizeGeometry.Children.Add(new RectangleGeometry(
                                                                new Rect(DrawX - Len / 2, DrawY - i - Len / 2, Len, Len))));
                return _ResizeGeometry;
            }
        }

        static ROICircle()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            RadiusProperty = DependencyProperty.Register("Radius", typeof(double), typeof(ROICircle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            CenterXProperty = DependencyProperty.Register("CenterX", typeof(double), typeof(ROICircle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            CenterYProperty = DependencyProperty.Register("CenterY", typeof(double), typeof(ROICircle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
        }

        /// <summary>
        /// 圓
        /// </summary>
        public ROICircle()
        {
            GeometryAction();
        }

        /// <summary>
        /// 圖形動作事件
        /// </summary>
        public void GeometryAction()
        {
            pairs.Add(_TranslateGeometry, Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                X += position.X;
                Y += position.Y;
            });

            pairs.Add(_ResizeGeometry, Pos =>
            {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                Radius = Math.Abs(position.X) > Math.Abs(position.Y) ? Math.Abs(position.X) : Math.Abs(position.Y);
            });
        }

        /// <summary>
        /// 更新shape大小位置資訊
        /// </summary>
        protected override void ResetLeftTop()
        {
            if (Radius <= 0) return;
            ShapeTop = Y - Radius;
            ShapeLeft = X - Radius;
            DeltaX = 2 * Radius;
            DeltaY = 2 * Radius;
            Theta = 0.0;
            Distance = 0;

            Canvas.SetLeft(this, ShapeLeft);
            Canvas.SetTop(this, ShapeTop);

            LeftTop = new Point(X - Radius, Y - Radius);
            RightBottom = new Point(X + Radius, Y + Radius);
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup geometry = new GeometryGroup();
                _ellipseGeometry = new EllipseGeometry(new Rect(0, 0, 2 * Radius, 2 * Radius));
                
                geometry.Children.Add(_ellipseGeometry);
                thisgeometry = geometry;
                return geometry;
            }
        }

        public override string ShapeType => "Circle";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
        protected override Geometry ReturnContainGeometry(Point Pos)
        {
            if (ResizeGeometry.FillContains(Pos) && IsResizeEnabled) return ResizeGeometry;
            if (TranslateGeometry.FillContains(Pos) && IsMoveEnabled) return TranslateGeometry;
            return null;
        }

        /// <summary>
        /// 改變pen內容 
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            //因要修改pen內容，故不需再base
            //base.OnRender(dc);
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

                if(IsMoveEnabled)
                    dc.DrawGeometry(Brushes.Transparent, transparentPen, TranslateGeometry);
            }
        }

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

        public override bool ShapeContains(Point point)
        {
            return thisgeometry.FillContains(Point.Subtract(point,(Vector)LeftTop));
        }

        public override Point[] GetEdgePoints(Size ImageSize)
        {
            Point[] detectPoints = Enumerable.Range(1, (int)ImageSize.Width).Select(x =>
            {
                return Enumerable.Range(1, (int)ImageSize.Height).Select(y =>
                {
                    return new Point(x, y);
                });
            }).SelectMany(_ => _).ToArray();

            double anglestep = 0.01;
            double currentAngle = 0;
            Point[] EdgePoints = Enumerable.Range(0, (int)(360 / anglestep)).Select(index=> 
            {
                double radians = (Math.PI / 180) * currentAngle + anglestep * index;
                double x = X + Radius * Math.Sin(radians);
                double y = Y + Radius * Math.Cos(radians);
                return new Point(Math.Round(x), Math.Round(y));
            }).Distinct().ToArray();

            return EdgePoints;
 
        }
    }
}
