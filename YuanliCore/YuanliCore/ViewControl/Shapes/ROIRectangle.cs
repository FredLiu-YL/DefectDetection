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
    public class ROIRectangle : ROIShape
    {
        public static readonly DependencyProperty RadiusXProperty;
        public static readonly DependencyProperty RadiusYProperty;
        public static readonly DependencyProperty Row1Property;
        public static readonly DependencyProperty Row2Property;
        public static readonly DependencyProperty Column1Property;
        public static readonly DependencyProperty Column2Property;
        private Geometry thisgeometry;
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();
        private RectangleGeometry _LTGeometry = new RectangleGeometry();
        private RectangleGeometry _LBGeometry = new RectangleGeometry();
        private RectangleGeometry _RTGeometry = new RectangleGeometry();
        private RectangleGeometry _RBGeometry = new RectangleGeometry();
        private GeometryGroup _ResizeGeometry = new GeometryGroup();
        private RectangleGeometry _rectangleGeometry = new RectangleGeometry();
        private Point lastMousePosition;

        /// <summary>
        /// X座標
        /// </summary>
        public double X
        {
            get => (double)GetValue(RadiusXProperty);
            set => SetValue(RadiusXProperty, value);
        }

        /// <summary>
        /// Y座標
        /// </summary>
        public double Y
        {
            get => (double)GetValue(RadiusYProperty);
            set => SetValue(RadiusYProperty, value);
        }

        /// <summary>
        /// 取得或設定 左上角位置 Row
        /// </summary>
        public double Row1
        {
            get => (double)GetValue(Row1Property);
            set => SetValue(Row1Property, value);
        }

        /// <summary>
        /// 取得或設定 右下角位置 Row
        /// </summary>
        public double Row2
        {
            get => (double)GetValue(Row2Property);
            set => SetValue(Row2Property, value);
        }

        /// <summary>
        /// 取得或設定 左上角位置 Col
        /// </summary>
        public double Column1
        {
            get => (double)GetValue(Column1Property);
            set => SetValue(Column1Property, value);
        }

        /// <summary>
        /// 取得或設定 右下角位置 Col
        /// </summary>
        public double Column2
        {
            get => (double)GetValue(Column2Property);
            set => SetValue(Column2Property, value);
        }

        /// <summary>
        /// 中心十字
        /// </summary>
        private Geometry CenterCrossGeometry
        {
            get
            {
                var centerX = Math.Min(Column1, Column2) + Math.Abs(Column2 - Column1) / 2;
                var centerY = Math.Min(Row1, Row2) + Math.Abs(Row2 - Row1) / 2;

                GeometryGroup centerCrossGeometry = new GeometryGroup();

                LineGeometry _lineGeometryH = new LineGeometry(new Point(centerX, centerY - CenterCrossLength),
                                                               new Point(centerX, centerY + CenterCrossLength));
                LineGeometry _lineGeometryV = new LineGeometry(new Point(centerX - CenterCrossLength, centerY),
                                                               new Point(centerX + CenterCrossLength, centerY));

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
                var col = Math.Min(Column1, Column2);
                var row = Math.Min(Row1, Row2);

                _TranslateGeometry.Rect = new Rect(col, row, Math.Abs(Column2 - Column1), Math.Abs(Row2 - Row1));
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
                _LTGeometry.Rect = new Rect(Column1 - Len / 2, Row1 - Len / 2, Len, Len);
                _RTGeometry.Rect = new Rect(Column2 - Len / 2, Row1 - Len / 2, Len, Len);
                _LBGeometry.Rect = new Rect(Column1 - Len / 2, Row2 - Len / 2, Len, Len);
                _RBGeometry.Rect = new Rect(Column2 - Len / 2, Row2 - Len / 2, Len, Len);
                _ResizeGeometry.Children.Add(_LTGeometry);
                _ResizeGeometry.Children.Add(_LBGeometry);
                _ResizeGeometry.Children.Add(_RTGeometry);
                _ResizeGeometry.Children.Add(_RBGeometry);
                return _ResizeGeometry;
            }
        }

        static ROIRectangle()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            RadiusXProperty = DependencyProperty.Register("RadiusX", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            RadiusYProperty = DependencyProperty.Register("RadiusY", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            Row1Property = DependencyProperty.Register("Row1", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            Row2Property = DependencyProperty.Register("Row2", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));
            Column1Property = DependencyProperty.Register("Column1", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            Column2Property = DependencyProperty.Register("Column2", typeof(double), typeof(ROIRectangle), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));
        }

        /// <summary>
        /// 矩形
        /// </summary>
        public ROIRectangle()
        {
            GeometryAction();
        }

        /// <summary>
        /// 圖形動作事件
        /// </summary>
        public void GeometryAction()
        {
            pairs.Add(_TranslateGeometry, Pos => {
                var lengthX = Math.Abs(Column2 - Column1) / 2;
                var lengthY = Math.Abs(Row2 - Row1) / 2;

                Column1 = Pos.X - lengthX; 
                Column2 = Pos.X + lengthX;
                Row1 = Pos.Y - lengthY; 
                Row2 = Pos.Y + lengthY;

                X = (Column1 + Column2) / 2;
                Y = (Row1 + Row2) / 2;
            });
            pairs.Add(_LTGeometry, Pos => {
                Column1 = Pos.X; 
                Row1 = Pos.Y;
                if (Column1 >= Column2) Column1 = Column2;
                if (Row1 >= Row2) Row1 = Row2;
            });
            pairs.Add(_LBGeometry, Pos => {
                Column1 = Pos.X; 
                Row2 = Pos.Y;
                if (Column1 >= Column2) Column1 = Column2;
                if (Row2 <= Row1) Row2 = Row1;
            });
            pairs.Add(_RTGeometry, Pos => {
                Column2 = Pos.X; 
                Row1 = Pos.Y;
                if (Column2 <= Column1) Column2 = Column1;
                if (Row1 >= Row2) Row1 = Row2;
            });
            pairs.Add(_RBGeometry, Pos => {
                Column2 = Pos.X; 
                Row2 = Pos.Y;
                if (Column2 <= Column1) Column2 = Column1;
                if (Row2 <= Row1) Row2 = Row1;
            });
        }

        /// <summary>
        /// 更新shape大小位置資訊
        /// </summary>
        protected override void ResetLeftTop()
        {
            ShapeLeft = Math.Min(Column1, Column2);
            ShapeTop = Math.Min(Row1, Row2);
            DeltaX = Math.Abs(Column2 - Column1); 
            DeltaY = Math.Abs(Row2 - Row1); 
            Theta = 0.0; 
            Distance = 0;
            
            LeftTop = new Point(ShapeLeft, ShapeTop);
            RightBottom = new Point(ShapeLeft + DeltaX, ShapeTop + DeltaY);
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected override Geometry DefiningGeometry
        {
            get
            {
                var col = Math.Min(Column1, Column2);
                var row = Math.Min(Row1, Row2);
                GeometryGroup geometry = new GeometryGroup();
                _rectangleGeometry = new RectangleGeometry(new Rect(col, row, Math.Abs(Column2 - Column1), Math.Abs(Row2 - Row1)));
                geometry.Children.Add(_rectangleGeometry);
                thisgeometry = geometry;
                return geometry;
            }
        }

        public override string ShapeType => "Rectangle";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
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
        /// 改變pen內容 
        /// </summary>
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
            return thisgeometry.FillContains(point);
        }

        public override Point[] GetEdgePoints(Size ImageSize)
        {
            int width = (int)Math.Round(RightBottom.X - LeftTop.X + 1);
            int height = (int)Math.Round(RightBottom.Y - LeftTop.Y + 1);
            int[] xPoints = Enumerable.Range((int)Math.Round(LeftTop.X), width).ToArray();
            int[] yPoints = Enumerable.Range((int)Math.Round(LeftTop.Y), height).ToArray();

            Point[] LeftLineY = yPoints.Select(y => { return new Point((int)Math.Round(LeftTop.X), y); }).ToArray();
            Point[] RightLineY = yPoints.Select(y => { return new Point((int)Math.Round(RightBottom.X), y); }).ToArray();

            Point[] TopLineX = xPoints.Select(x => { return new Point(x, (int)Math.Round(LeftTop.Y)); }).ToArray();
            Point[] ButtomLineX = xPoints.Select(x => { return new Point(x, (int)Math.Round(RightBottom.Y)); }).ToArray();
            Point[] EdgePoints = new Point[0];
            EdgePoints = EdgePoints.Concat(LeftLineY).ToArray();
            EdgePoints = EdgePoints.Concat(RightLineY).ToArray();
            EdgePoints = EdgePoints.Concat(TopLineX).ToArray();
            EdgePoints = EdgePoints.Concat(ButtomLineX).ToArray();
            return EdgePoints;

        }
    }
}
