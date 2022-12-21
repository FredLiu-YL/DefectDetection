using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace YuanliCore.Views.CanvasShapes
{
    public class ROILine : ROIShape
    {
        public static DependencyProperty X1Property;
        public static DependencyProperty Y1Property;
        public static DependencyProperty X2Property;
        public static DependencyProperty Y2Property;
        private LineGeometry _lineGeometry = new LineGeometry();
        RectangleGeometry P1Geometry = new RectangleGeometry();
        RectangleGeometry P2Geometry = new RectangleGeometry();
        RectangleGeometry MoveGeometry = new RectangleGeometry(); 
        private Point lastMousePosition;

        /// <summary>
        /// X1座標
        /// </summary>
        public double X1
        {
            get => (double)GetValue(X1Property);
            set => SetValue(X1Property, value);
        }

        /// <summary>
        /// Y1座標
        /// </summary>
        public double Y1
        {
            get => (double)GetValue(Y1Property);
            set => SetValue(Y1Property, value);
        }

        /// <summary>
        /// X2座標
        /// </summary>
        public double X2
        {
            get => (double)GetValue(X2Property);
            set => SetValue(X2Property, value);
        }

        /// <summary>
        /// Y2座標
        /// </summary>
        public double Y2
        {
            get => (double)GetValue(Y2Property);
            set => SetValue(Y2Property, value);
        }

        /// <summary>
        /// 移動 框
        /// </summary>
        private Geometry TranslateGeometry
        {
            get
            {
                Point startPoint = new Point(X1 - 1, Y1 - 1);
                Point endPoint = new Point(X2 - 1, Y2 - 1);

                var Length = RectLen;

                MoveGeometry.Rect = new Rect((startPoint.X + endPoint.X) / 2 - Length / 2, (startPoint.Y + endPoint.Y) / 2 - Length / 2, Length, Length);

                return MoveGeometry;
            }
        }

        /// <summary>
        /// 放大縮小 框1
        /// </summary>
        private Geometry Resize1Geometry
        {
            get
            {
                Point startPoint = new Point(X1  - 1,  Y1 -  1);
                Point endPoint = new Point(X2  - 1,  Y2 -  1);
                var Length = RectLen;

                P1Geometry.Rect = new Rect(startPoint.X - Length / 2, startPoint.Y - Length / 2, Length, Length);
                return P1Geometry;
            }
        }

        /// <summary>
        /// 放大縮小 框2
        /// </summary>
        private Geometry Resize2Geometry
        {
            get
            {
                Point startPoint = new Point(X1 -  1, Y1 -  1);
                Point endPoint = new Point(X2 -  1, Y2 -  1);

                var Length = RectLen; 

                P2Geometry.Rect = new Rect(endPoint.X - Length / 2, endPoint.Y - Length / 2, Length, Length);

                return P2Geometry;
            }
        }


        static ROILine()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            X1Property = DependencyProperty.Register("X1", typeof(double), typeof(ROILine), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            Y1Property = DependencyProperty.Register("Y1", typeof(double), typeof(ROILine), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            X2Property = DependencyProperty.Register("X2", typeof(double), typeof(ROILine), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));
            Y2Property = DependencyProperty.Register("Y2", typeof(double), typeof(ROILine), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));
        }

        /// <summary>
        /// 線條
        /// </summary>
        public ROILine()
        {
            GeometryAction();
        }

        /// <summary>
        /// 圖形動作事件
        /// </summary>
        public void GeometryAction()
        {
            pairs.Add(Resize1Geometry, Pos => {
                X1 = Pos.X ; Y1 = Pos.Y ;
            });

            pairs.Add(Resize2Geometry, Pos => {
                X2 = Pos.X ; Y2 = Pos.Y ;
            });

            pairs.Add(TranslateGeometry, Pos => {
                double width = X2 - X1;
                double height = Y2 - Y1;

                X1 = Pos.X - width / 2;
                Y1 = Pos.Y - height / 2;
                X2 = Pos.X + width / 2;
                Y2 = Pos.Y + height / 2;
            });
        }

        /// <summary>
        /// 更新shape大小位置資訊
        /// </summary>
        protected override void ResetLeftTop()
        {
            var len = StrokeThickness * 2;
            ShapeLeft = Math.Min(X1, X2) - len;
            ShapeTop = Math.Min(Y1, Y2) - len;

            DeltaX = Math.Abs(X1 - X2); 
            DeltaY = Math.Abs(Y2 - Y1); 
            Theta = Math.Atan2(Y2 - Y1, X2 - X1) * 180 / Math.PI * -1;
            Distance = Math.Sqrt(Math.Pow(DeltaX, 2) + Math.Pow(DeltaY, 2));
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                Point startPoint = new Point(X1, Y1);
                Point endPoint = new Point(X2, Y2);
                _lineGeometry.StartPoint = startPoint;
                _lineGeometry.EndPoint = endPoint;
                return _lineGeometry;
            }
        }

        public override string ShapeType => "Line";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
        protected override Geometry ReturnContainGeometry(Point MousePosition)
        {
            if (Resize1Geometry.FillContains(MousePosition) && IsInteractived) return Resize1Geometry;
            if (Resize2Geometry.FillContains(MousePosition) && IsInteractived) return Resize2Geometry;
            if (TranslateGeometry.FillContains(MousePosition) && IsMoveEnabled) return TranslateGeometry;

            return null;
        }

        /// <summary>
        /// 改變pen內容 
        /// </summary>
        protected override void OnRender(DrawingContext dc)
        {
            Pen pen = null;
            Pen transparentPen = new Pen(Brushes.Transparent, 1);

            pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(Fill, pen, DefiningGeometry);

            if (IsInteractived)
            {
                pen = new Pen(Brushes.Green, 1);
                dc.DrawGeometry(Brushes.Transparent, pen, Resize1Geometry);
                dc.DrawGeometry(Brushes.Transparent, pen, Resize2Geometry);
                dc.DrawGeometry(Brushes.Transparent, pen, TranslateGeometry);
            }
            else
            {
                dc.DrawGeometry(Brushes.Transparent, transparentPen, Resize1Geometry);
                dc.DrawGeometry(Brushes.Transparent, transparentPen, Resize2Geometry);
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
