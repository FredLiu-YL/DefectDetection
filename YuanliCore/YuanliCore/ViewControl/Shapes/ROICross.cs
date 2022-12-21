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
    public class ROICross : ROIShape
    {
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty SizeProperty;
        public static readonly DependencyProperty OrientationProperty;

        private Transform RotateTrans = null;
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();
        private RectangleGeometry _ResizeGeometry = new RectangleGeometry();
        private EllipseGeometry _RotateGeometry = new EllipseGeometry();

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
        /// 大小
        /// </summary>
        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
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
        /// 移動 框
        /// </summary>
        private Geometry TranslateGeometry
        {
            get
            {
                var DrawCenterX = X - ShapeLeft;
                var DrawCenterY = Y - ShapeTop;
                var Len = Math.Max(RectLen, 2 * StrokeThickness);
                //_TranslateGeometry.Rect = new Rect(DrawCenterX - Len / 2, DrawCenterY - Len / 2, Len, Len);
                _TranslateGeometry.Rect = new Rect(DrawCenterX / 2, DrawCenterY / 2, Math.Abs(DrawCenterX), Math.Abs(DrawCenterY));
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
                var DrawX = X - ShapeLeft;
                var DrawY = Y - ShapeTop - Size;
                var Len = RectLen;
                _ResizeGeometry.Rect = new Rect(DrawX - Len / 2, DrawY - Len / 2, Len, Len);
                _ResizeGeometry.Transform = RotateTrans;
                return _ResizeGeometry;
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

        static ROICross()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(ROICross), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ROICross), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(ROICross), new FrameworkPropertyMetadata(1.0, options, OnDataChanged));
            OrientationProperty = DependencyProperty.Register("Orientation", typeof(double), typeof(ROICross), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
        }

        /// <summary>
        /// 十字
        /// </summary>
        public ROICross()
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
            ShapeTop = Y - Size ;
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
                Point x1 = new Point(X - Size - ShapeLeft, Y - ShapeTop);
                Point x2 = new Point(X + Size - ShapeLeft, Y - ShapeTop);

                Point y1 = new Point(X - ShapeLeft, Y - Size - ShapeTop);
                Point y2 = new Point(X - ShapeLeft, Y + Size - ShapeTop);

                GeometryGroup myGeometryGroup = new GeometryGroup();
                myGeometryGroup.Children.Add(new LineGeometry(x1, x2));
                myGeometryGroup.Children.Add(new LineGeometry(y1, y2));

                myGeometryGroup.Transform = RotateTrans;
                return myGeometryGroup;
            }
        }

        public override string ShapeType => "Cross";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
        protected override Geometry ReturnContainGeometry(Point Pos)
        {
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
            pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(Fill, pen, DefiningGeometry);

            if (IsInteractived)
            {
                if (IsResizeEnabled)
                {
                    pen = new Pen(Brushes.Green, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, ResizeGeometry);
                }
                if (IsRotateEnabled)
                {
                    pen = new Pen(Brushes.Orange, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, RotateGeometry);
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
    }
}
