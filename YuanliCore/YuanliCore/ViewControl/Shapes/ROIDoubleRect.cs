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
    public class ROIDoubleRect : ROIShape
    {
        public static readonly DependencyProperty XProperty;
        public static readonly DependencyProperty YProperty;
        public static readonly DependencyProperty LengthXProperty;
        public static readonly DependencyProperty LengthYProperty;
        public static readonly DependencyProperty InnerLengthXProperty;
        public static readonly DependencyProperty InnerLengthYProperty;
        public static readonly DependencyProperty OrientationProperty;
        public static readonly DependencyProperty InnerStrokeBrushProperty;

        private Geometry thisgeometry;

        private Transform RotateTrans = null;
        private RectangleGeometry _TranslateGeometry = new RectangleGeometry();
     
        private GeometryGroup _ResizeGeometry = new GeometryGroup();
        private GeometryGroup _InnerResizeGeometry = new GeometryGroup();
        private EllipseGeometry _RotateGeometry = new EllipseGeometry();
        private Point lastMousePosition;
        private RectangleGeometry _rectangleGeometry = new RectangleGeometry();
        private RectangleGeometry _rectangle1Geometry = new RectangleGeometry();
        //public static readonly DependencyProperty Row1Property;
        //public static readonly DependencyProperty Row2Property;
        //public static readonly DependencyProperty Column1Property;
        //public static readonly DependencyProperty Column2Property;

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
        /// 寬度半徑
        /// </summary>
        public double LengthX
        {
            get => (double)GetValue(LengthXProperty);
            set => SetValue(LengthXProperty, value);
        }

        /// <summary>
        /// 長度半徑
        /// </summary>
        public double LengthY
        {
            get => (double)GetValue(LengthYProperty);
            set => SetValue(LengthYProperty, value);
        }
        /// <summary>
        /// 寬度半徑
        /// </summary>
        public double InnerLengthX
        {                                                                                                        
            get => (double)GetValue(InnerLengthXProperty);
            set => SetValue(InnerLengthXProperty, value);
        }
                                                                                                                                                                  
        /// <summary>
        /// 長度半徑
        /// </summary>
        public double InnerLengthY
        {
            get => (double)GetValue(InnerLengthYProperty);
            set => SetValue(InnerLengthYProperty, value);
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
        ///  第二個框顏色
        /// </summary>
        public Brush InnerStrokeBrush
        {
            get => (Brush)GetValue(InnerStrokeBrushProperty);
            set => SetValue(InnerStrokeBrushProperty, value);
        }

        /// <summary>
        /// 取得或設定 左上角位置 Row
        /// </summary>
        public double Row1
        {
            get =>  Y - LengthY;
                
        }

        /// <summary>
        /// 取得或設定 右下角位置 Row
        /// </summary>
        public double Row2
        {
            get => Y + LengthY;
        }

        /// <summary>
        /// 取得或設定 左上角位置 Col
        /// </summary>
        public double Column1
        {
            get => X - LengthX;
        }

        /// <summary>
        /// 取得或設定 右下角位置 Col
        /// </summary>
        public double Column2
        {
            get => X + LengthX;
        }


        /// <summary>
        /// 中心十字
        /// </summary>
        private Geometry CenterCrossGeometry
        {
            get
            {
                GeometryGroup centerCrossGeometry = new GeometryGroup();

                LineGeometry _lineGeometryH = new LineGeometry(new Point(_rectangleGeometry.Rect.Size.Width / 2, _rectangleGeometry.Rect.Size.Height / 2 - CenterCrossLength),
                                                               new Point(_rectangleGeometry.Rect.Size.Width / 2, _rectangleGeometry.Rect.Size.Height / 2 + CenterCrossLength));
                LineGeometry _lineGeometryV = new LineGeometry(new Point(_rectangleGeometry.Rect.Size.Width / 2 - CenterCrossLength, _rectangleGeometry.Rect.Size.Height / 2),
                                                               new Point(_rectangleGeometry.Rect.Size.Width / 2 + CenterCrossLength, _rectangleGeometry.Rect.Size.Height / 2));

                centerCrossGeometry.Children.Add(_lineGeometryH);
                centerCrossGeometry.Children.Add(_lineGeometryV);

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
                double DrawX = X - ShapeLeft;
                double DrawY = Y - ShapeTop;
                _TranslateGeometry.Rect = new Rect((DrawX  / 2), (DrawY  / 2), DrawX, DrawY);
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
                _ResizeGeometry.Children.Clear();
                var Len = RectLen;
                double DrawX = X - ShapeLeft;
                double DrawY = Y - ShapeTop;
                var tmpRects =
                    from i in Enumerable.Range(-1, 3).Where(i => i % 2 != 0).Select(i => i * LengthX)
                    from j in Enumerable.Range(-1, 3).Where(j => j % 2 != 0).Select(j => j * LengthY)
                    select new RectangleGeometry(new Rect(DrawX - i - Len / 2, DrawY - j - Len / 2, Len, Len));
                foreach (var item in tmpRects)
                    _ResizeGeometry.Children.Add(item);
             //   _ResizeGeometry.Transform = RotateTrans;
                return _ResizeGeometry;
            }
        }
        /// <summary>
        /// 放大縮小 框
        /// </summary>
        private Geometry InnerResizeGeometry
        {
            get
            {
                 _InnerResizeGeometry.Children.Clear();
                
               /*    var Len = RectLen;
                 RectangleGeometry _LTGeometry = new RectangleGeometry(new Rect(Column1 - Len / 2, Row1 - Len / 2, Len, Len));
                 RectangleGeometry _RTGeometry = new RectangleGeometry(new Rect(Column2 - Len / 2, Row1 - Len / 2, Len, Len));
                 RectangleGeometry _LBGeometry = new RectangleGeometry(new Rect(Column1 - Len / 2, Row2 - Len / 2, Len, Len));
                 RectangleGeometry _RBGeometry = new RectangleGeometry(new Rect(Column2 - Len / 2, Row2 - Len / 2, Len, Len));
                 _Resize1Geometry.Children.Add(_LTGeometry);
                 _Resize1Geometry.Children.Add(_LBGeometry);
                 _Resize1Geometry.Children.Add(_RTGeometry);
                 _Resize1Geometry.Children.Add(_RBGeometry);
                 return _Resize1Geometry;*/
               var Len = RectLen;
                double DrawX = X - ShapeLeft;
                double DrawY = Y - ShapeTop;
                var tmpRects =
                    from i in Enumerable.Range(-1, 3).Where(i => i % 2 != 0).Select(i => i * InnerLengthX)
                    from j in Enumerable.Range(-1, 3).Where(j => j % 2 != 0).Select(j => j * InnerLengthY)
                    select new RectangleGeometry(new Rect(DrawX - i - Len / 2, DrawY - j - Len / 2, Len, Len));
                foreach (var item in tmpRects)
                    _InnerResizeGeometry.Children.Add(item);
                _InnerResizeGeometry.Transform = RotateTrans;
                return _InnerResizeGeometry;
            }
        }
        /// <summary>
        /// 旋轉 圓
        /// </summary>
        private Geometry RotateGeometry
        {
            get
            {
                var Len = RectLen;
                var DrawX = X - ShapeLeft + InnerLengthX;
                var DrawY = Y - ShapeTop;
                _RotateGeometry.Center = new Point(DrawX, DrawY);
                _RotateGeometry.RadiusX = _RotateGeometry.RadiusY = Len / 2;
                _RotateGeometry.Transform = RotateTrans;
                return _RotateGeometry;
            }
        }

        static ROIDoubleRect()
        {
            var options = FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            LengthXProperty = DependencyProperty.Register("LengthX", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            LengthYProperty = DependencyProperty.Register("LengthY", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            InnerLengthXProperty = DependencyProperty.Register("InnerLengthX", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(50.0, options, OnDataChanged));
            InnerLengthYProperty = DependencyProperty.Register("InnerLengthY", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(50.0, options, OnDataChanged));
            
            XProperty = DependencyProperty.Register("X", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            YProperty = DependencyProperty.Register("Y", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            OrientationProperty = DependencyProperty.Register("Orientation", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
            
            InnerStrokeBrushProperty = DependencyProperty.Register("InnerStrokeBrush", typeof(Brush), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(Brushes.Aqua, options));

            /*   Row1Property = DependencyProperty.Register("Row1", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
               Row2Property = DependencyProperty.Register("Row2", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));
               Column1Property = DependencyProperty.Register("Column1", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(0.0, options, OnDataChanged));
               Column2Property = DependencyProperty.Register("Column2", typeof(double), typeof(ROIDoubleRect), new FrameworkPropertyMetadata(100.0, options, OnDataChanged));*/
        }

        /// <summary>
        /// 旋轉矩形
        /// </summary>
        public ROIDoubleRect()
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

            pairs.Add(_ResizeGeometry, Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                Transform rotTransform = new RotateTransform(Orientation);
                var org = rotTransform.Transform(new Point(position.X, position.Y));
                LengthX = Math.Abs(org.X);
                LengthY = Math.Abs(org.Y);
            });

            pairs.Add(_RotateGeometry, Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                var angle = -Math.Atan2(position.Y, position.X) * 180 / Math.PI;
                angle = angle < 0 ? angle + 360 : angle;
                Orientation = angle;
            });

            pairs.Add(_InnerResizeGeometry, Pos => {
                var position = Pos - new Point(X - ShapeLeft - 1, Y - ShapeTop - 1);
                Transform rotTransform = new RotateTransform(Orientation);
                var org = rotTransform.Transform(new Point(position.X, position.Y));

                InnerLengthX = Math.Abs(org.X);
                InnerLengthY = Math.Abs(org.Y);
              
                /*Column1 = Pos.X - lengthX;
                Column2 = Pos.X + lengthX;
                Row1 = Pos.Y - lengthY;
                Row2 = Pos.Y + lengthY;*/
            });

        }

        /// <summary>
        /// 更新shape大小位置資訊
        /// </summary>
        protected override void ResetLeftTop()
        {
            if (LengthX <= 0 || LengthY <= 0) return;
           
        
            //記錄矩形四角座標
            Point[] Corners = new Point[] {
                new Point(X - LengthX, Y - LengthY),
                new Point(X + LengthX, Y - LengthY),
                new Point(X + LengthX, Y + LengthY),
                new Point(X - LengthX, Y + LengthY)
            };

            var trans = new RotateTransform(Orientation * -1, X, Y);
            var rots = Corners.Select(p => trans.Transform(p));

            ShapeTop = Y - LengthY;
            ShapeLeft = X - LengthX;

            RotateTrans = new RotateTransform(Orientation * -1, X - ShapeLeft, Y - ShapeTop);
            DeltaX = 2 * LengthX;
            DeltaY = 2 * LengthY;
            Theta = Orientation;
            Distance = 0;

            Canvas.SetLeft(this, ShapeLeft);
            Canvas.SetTop(this, ShapeTop);

            LeftTop = new Point(rots.Min(p => p.X), rots.Min(p => p.Y));
            RightBottom = new Point(rots.Max(p => p.X), rots.Max(p => p.Y));
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected override Geometry DefiningGeometry 
        {
            get
            {
                GeometryGroup geometry = new GeometryGroup();
         
      
                _rectangleGeometry = new RectangleGeometry(new Rect(0, 0, 2 * LengthX, 2 * LengthY));

                //  _rectangleGeometry =new RectangleGeometry(new Rect(LengthX/2, LengthY/2,  LengthX,  LengthY));

         //       geometry.Transform = RotateTrans;
                geometry.Children.Add(_rectangleGeometry);
           //     geometry.Children.Add(_rectangle1Geometry);
                thisgeometry = geometry;
                return geometry;
            }
        }
        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 DefiningGeometry
        /// </summary>
        protected Geometry InnerDefiningGeometry
        {
            get
            {
                GeometryGroup geometry = new GeometryGroup();
                int w = 100, h = 100;
            //    _rectangle1Geometry = new RectangleGeometry(new Rect(new Point(Column1,Row1) , new Point(Column2, Row2) ));
                _rectangle1Geometry = new RectangleGeometry(new Rect(LengthX - InnerLengthX    , LengthY - InnerLengthY   , InnerLengthX*2, InnerLengthY*2));
  
         //       _rectangle1Geometry = new RectangleGeometry(new Rect(LengthX / 2, LengthY / 2, LengthX, LengthY));
                geometry.Transform = RotateTrans;      
                geometry.Children.Add(_rectangle1Geometry);
          //      thisgeometry1 = geometry;
                return geometry;
            }
        }
        public override string ShapeType => "RotateRect";

        /// <summary>
        /// 再可移動的框內變更滑鼠形狀
        /// </summary>
        /// <param name="Pos">游標座標</param>
        /// <returns></returns>
        protected override Geometry ReturnContainGeometry(Point Pos)
        {
            if (ResizeGeometry.FillContains(Pos) && IsResizeEnabled) return ResizeGeometry;
            if (InnerResizeGeometry.FillContains(Pos)) return InnerResizeGeometry;
            if (RotateGeometry.FillContains(Pos) && IsRotateEnabled) return RotateGeometry;
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
            
            Pen pen1 = new Pen(InnerStrokeBrush, StrokeThickness);
            dc.DrawGeometry(Fill, pen1, InnerDefiningGeometry);
            pen = new Pen(CenterCrossBrush, StrokeThickness / 2);
            dc.DrawGeometry(Fill, pen, CenterCrossGeometry);

            if (IsInteractived)
            {
                if (IsResizeEnabled)
                {
                    pen = new Pen(Brushes.Green, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, ResizeGeometry);
                    pen = new Pen(Brushes.Aqua, 1);
                    dc.DrawGeometry(Brushes.Transparent, pen, InnerResizeGeometry);
                }

                if (IsRotateEnabled)
                {
                    pen = new Pen(Brushes.DarkMagenta, 1);
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

        public override bool ShapeContains(Point point)
        {
            return thisgeometry.FillContains(Point.Subtract(point, (Vector)LeftTop));
        }
    }
}
