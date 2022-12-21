using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public class ROICrossBox : ROIShape
    {
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty SizeProperty;
        public static readonly DependencyProperty BreadthProperty;
        public static readonly DependencyProperty OrientationProperty;

        private Transform RotateTrans = null;
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();
        private RectangleGeometry _ResizeGeometry = new RectangleGeometry();
        private RectangleGeometry _Resize2Geometry = new RectangleGeometry();
        private EllipseGeometry _RotateGeometry = new EllipseGeometry();

        //private CombinedGeometry _crossBox = new CombinedGeometry();
        private Point lastMousePosition;

        /// <summary>
        /// X座標
        /// </summary>
        public double X
        {
            get => (double)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y
        {
            get => (double)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        /// <summary>
        /// 大小(長邊)
        /// </summary>
        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// 寬度(短邊)
        /// </summary>
        public double LengthWidth
        {
            get => (double)GetValue(BreadthProperty);
            set => SetValue(BreadthProperty, value);
        }

        /// <summary>
        /// 方向角度
        /// </summary>
        public double Orientation
        {
            get => (double)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// 中心十字
        /// </summary>
        private Geometry CenterCrossGeometry
        {
            get
            {
                Point x1 = new Point(X - ShapeLeft - CenterCrossLength, Y - ShapeTop);
                Point x2 = new Point(X - ShapeLeft + CenterCrossLength, Y - ShapeTop);

                Point y1 = new Point(X - ShapeLeft, Y - ShapeTop - CenterCrossLength);
                Point y2 = new Point(X - ShapeLeft, Y - ShapeTop + CenterCrossLength);

                GeometryGroup centerCrossGeometry = new GeometryGroup();
                centerCrossGeometry.Children.Add(new LineGeometry(y1, y2));
                centerCrossGeometry.Children.Add(new LineGeometry(x1, x2));

                centerCrossGeometry.Transform = RotateTrans;
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
                var DrawCenterX = X - ShapeLeft;
                var DrawCenterY = Y - ShapeTop;
                var Len = Math.Max(RectLen, 2 * StrokeThickness);

                Rect rect1 = new Rect(DrawCenterX - LengthWidth / 2, DrawCenterY - Size, LengthWidth, Math.Abs(DrawCenterX + Size));
                Rect rect2 = new Rect(DrawCenterX - Size, DrawCenterY - LengthWidth / 2, Math.Abs(DrawCenterY + Size), LengthWidth);
                Rect rect = Rect.Union(rect1, rect2);
                _TranslateGeometry.Rect = rect;
                _TranslateGeometry.Transform = RotateTrans;

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
                var DrawX = X - ShapeLeft + LengthWidth / 2;
                var DrawY = Y - ShapeTop + Size;
                var Len = RectLen;
                _ResizeGeometry.Rect = new Rect(DrawX - Len / 2, DrawY - Len / 2, Len, Len);
                _ResizeGeometry.Transform = RotateTrans;
                return _ResizeGeometry;
            }
        }

        /// <summary>
        /// 放大縮小2 框
        /// </summary>
        private Geometry Resize2Geometry
        {
            get
            {
                var DrawX = X - ShapeLeft - LengthWidth / 2;
                var DrawY = Y - ShapeTop - Size - (LengthWidth / 2 - Size);
                var Len = RectLen;
                _Resize2Geometry.Rect = new Rect(DrawX - Len / 2, DrawY - Len / 2, Len, Len);
                _Resize2Geometry.Transform = RotateTrans;
                return _Resize2Geometry;
            }
        }

        /// <summary>
        /// 旋轉 框
        /// </summary>
        private Geometry RotateGeometry
        {
            get
            {
                var Len = RectLen;
                var DrawX = X - ShapeLeft + Size;
                var DrawY = Y - ShapeTop;
                _RotateGeometry.Center = new Point(DrawX, DrawY);
                _RotateGeometry.RadiusX = _RotateGeometry.RadiusY = Len / 2;
                _RotateGeometry.Transform = RotateTrans;
                return _RotateGeometry;
            }
        }

        static ROICrossBox()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(ROICrossBox), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ROICrossBox), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(ROICrossBox), new FrameworkPropertyMetadata(12.0, options, OnDataChanged));
            BreadthProperty = DependencyProperty.Register("Breadth", typeof(double), typeof(ROICrossBox), new FrameworkPropertyMetadata(20.0, options, OnDataChanged));
            OrientationProperty = DependencyProperty.Register("Orientation", typeof(double), typeof(ROICrossBox), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
        }

        /// <summary>
        /// 十字
        /// </summary>
        public ROICrossBox()
        {
            GeometryAction();
        }

        /// <summary>
        /// 圖形動作事件
        /// </summary>
        public void GeometryAction()
        {
            pairs.Add(TranslateGeometry, (Pos) => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                X += position.X;
                Y += position.Y;
            });

            pairs.Add(ResizeGeometry, (Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                Size = Math.Abs(position.X) > Math.Abs(position.Y) ? Math.Abs(position.X) : Math.Abs(position.Y);
                if (LengthWidth / 2 > Size) Size = LengthWidth / 2;
            }));

            pairs.Add(Resize2Geometry, (Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                LengthWidth = Math.Abs(position.X) > Math.Abs(position.Y) ? Math.Abs(position.X * 2) : Math.Abs(position.Y * 2);
                if (LengthWidth / 2 > Size) LengthWidth = Size * 2;
            }));

            pairs.Add(RotateGeometry, (Pos) => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                var angle = -Math.Atan2(position.Y, position.X) * 180 / Math.PI;
                Orientation = angle < 0 ? angle + 360 : angle;
            });
        }

        /// <summary>
        /// 更新shape大小位置資訊
        /// </summary>
        protected override void ResetLeftTop()
        {
            ShapeLeft = X - Size;
            ShapeTop = Y - Size;

            RotateTrans = new RotateTransform(Orientation * -1, X - ShapeLeft - 1, Y - ShapeTop - 1);
            DeltaX = DeltaY = 2 * Size;
            Theta = Orientation;
            Distance = 0;

            Canvas.SetLeft(this, ShapeLeft);
            Canvas.SetTop(this, ShapeTop);

            LeftTop = new Point(ShapeLeft, ShapeTop);
            RightBottom = new Point(X + Size, Y + Size);
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                RectangleGeometry _rectangleGeometry1 = new RectangleGeometry(new Rect(X - ShapeLeft - LengthWidth / 2, Y - Size - ShapeTop, LengthWidth, X + Size - ShapeLeft));
                RectangleGeometry _rectangleGeometry2 = new RectangleGeometry(new Rect(X - Size - ShapeLeft, Y - ShapeTop - LengthWidth / 2, Y + Size - ShapeTop, LengthWidth));
                CombinedGeometry _crossBox = new CombinedGeometry(_rectangleGeometry1, _rectangleGeometry2);

                GeometryGroup myGeometryGroup = new GeometryGroup();
                myGeometryGroup.Children.Add(_crossBox);

                myGeometryGroup.Transform = RotateTrans;
                return myGeometryGroup;
            }
        }

        public override string ShapeType => "CrossBox";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
        protected override Geometry ReturnContainGeometry(Point Pos)
        {
            if (Resize2Geometry.FillContains(Pos) && IsResizeEnabled) return Resize2Geometry;
            if (ResizeGeometry.FillContains(Pos) && IsResizeEnabled) return ResizeGeometry;
            if (RotateGeometry.FillContains(Pos) && IsRotateEnabled) return RotateGeometry;
            if (TranslateGeometry.FillContains(Pos) && IsMoveEnabled) return TranslateGeometry;
            return null;
        }

        /// <summary>
        /// 改變pen內容 
        /// </summary>
        /// <param name="dc"></param>
        protected override void OnRender(DrawingContext dc)
        {
            Pen pen = null;
            Pen transparentPen = new Pen(Brushes.Transparent, 1);

            //十字框
            pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(Fill, pen, DefiningGeometry);

            pen = new Pen(CenterCrossBrush, StrokeThickness / 2);
            dc.DrawGeometry(Fill, pen, CenterCrossGeometry);

            if (IsInteractived)
            {
                if (IsResizeEnabled)
                {
                    pen = new Pen(Brushes.Green, 1);
                    //放大縮小框
                    dc.DrawGeometry(Brushes.Transparent, pen, ResizeGeometry);
                    //變化寬度框
                    dc.DrawGeometry(Brushes.Transparent, pen, Resize2Geometry);
                }

                //旋轉框
                if (IsRotateEnabled)
                {
                    pen = new Pen(Brushes.Orange, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, RotateGeometry);
                }

                //移動框
                if (IsMoveEnabled)
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
    }
}
