using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public abstract class ROIShape : Shape
    {

        protected Point _LastMousePosition;
        protected Dictionary<Geometry, Action<Point>> pairs = new Dictionary<Geometry, Action<Point>>();
        protected Geometry HitGeometry = null;
        protected bool _IsDragable = false;

        private static readonly DependencyProperty DeltaXProperty;
        private static readonly DependencyProperty DeltaYProperty;
        private static readonly DependencyProperty ThetaProperty;
        private static readonly DependencyProperty DistanceProperty;
        private static readonly DependencyProperty IsMoveEnabledProperty;
        private static readonly DependencyProperty IsResizeEnabledProperty;
        private static readonly DependencyProperty IsRotateEnabledProperty;

        public static readonly DependencyProperty ShapeLeftProperty;
        public static readonly DependencyProperty ShapeTopProperty;
        public static readonly DependencyProperty IsInteractivedProperty;

        public static readonly DependencyProperty RectLenProperty;

        public static readonly DependencyProperty CenterCrossLengthProperty;
        public static readonly DependencyProperty CenterCrossBrushProperty;

        public Action<ROIShape> ShapeChangeAction;

        /// <summary>
        /// ROI左上角
        /// </summary>
        public Point LeftTop { get; protected set; } = new Point();

        /// <summary>
        /// ROI右上角
        /// </summary>
        public Point RightTop { get; protected set; } = new Point();

        /// <summary>
        /// ROI左下角
        /// </summary>
        public Point LeftBottom { get; protected set; } = new Point();

        /// <summary>
        /// ROI右下角
        /// </summary>
        public Point RightBottom { get; protected set; } = new Point();

        /// <summary>
        /// 取得或設定 shape 左邊
        /// </summary>
        public double ShapeLeft
        {
            get => (double)GetValue(ShapeLeftProperty);
            protected set => SetValue(ShapeLeftProperty, value);
        }

        /// <summary>
        /// 取得或設定 shape 頂端
        /// </summary>
        public double ShapeTop
        {
            get => (double)GetValue(ShapeTopProperty);
            protected set => SetValue(ShapeTopProperty, value);
        }

        /// <summary>
        /// 取得或設定  是否可以互動
        /// </summary>
        public bool IsInteractived
        {
            get => (bool)GetValue(IsInteractivedProperty);
            set => SetValue(IsInteractivedProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否可以移動
        /// </summary>
        public bool IsMoveEnabled
        {
            get => (bool)GetValue(IsMoveEnabledProperty);
            set => SetValue(IsMoveEnabledProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否可以調整大小
        /// </summary>
        public bool IsResizeEnabled
        {
            get => (bool)GetValue(IsResizeEnabledProperty);
            set => SetValue(IsResizeEnabledProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否可以旋轉
        /// </summary>
        public bool IsRotateEnabled
        {
            get => (bool)GetValue(IsRotateEnabledProperty);
            set => SetValue(IsRotateEnabledProperty, value);
        }

        /// <summary>
        /// 取得 是否可以拖動
        /// </summary>
        public bool IsDragable { get => _IsDragable; }

        /// <summary>
        /// 取得或設定 ΔX
        /// </summary>
        public double DeltaX
        {
            get => (double)GetValue(DeltaXProperty);
            set => SetValue(DeltaXProperty, value);
        }
        /// <summary>
        /// 取得或設定 ΔY
        /// </summary>
        public double DeltaY
        {
            get => (double)GetValue(DeltaYProperty);
            set => SetValue(DeltaYProperty, value);
        }
        /// <summary>
        /// 取得或設定 θ
        /// </summary>
        public double Theta
        {
            get => (double)GetValue(ThetaProperty);
            set => SetValue(ThetaProperty, value);
        }
        /// <summary>
        /// 取得或設定 距離
        /// </summary>
        public double Distance
        {
            get => (double)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        /// <summary>
        /// 取得或設定 移動旋轉框大小
        /// </summary>
        public double RectLen
        {
            get => (double)GetValue(RectLenProperty);
            set => SetValue(RectLenProperty, value);
        }

        /// <summary>
        /// 中心十字長度
        /// </summary>
        public double CenterCrossLength
        {
            get => (double)GetValue(CenterCrossLengthProperty);
            set => SetValue(CenterCrossLengthProperty, value);
        }

        /// <summary>
        /// 中心十字顏色
        /// </summary>
        public Brush CenterCrossBrush
        {
            get => (Brush)GetValue(CenterCrossBrushProperty);
            set => SetValue(CenterCrossBrushProperty, value);
        }

        static ROIShape()
        {
            var options = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault;
            ShapeLeftProperty = DependencyProperty.Register("ShapeLeft", typeof(double), typeof(ROIShape), new FrameworkPropertyMetadata(100.0, options));
            ShapeTopProperty = DependencyProperty.Register("ShapeTop", typeof(double), typeof(ROIShape), new FrameworkPropertyMetadata(100.0, options));

            DeltaXProperty = DependencyProperty.Register("DeltaX", typeof(double), typeof(ROIShape),
                                                          new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            DeltaYProperty = DependencyProperty.Register("DeltaY", typeof(double), typeof(ROIShape),
                                                          new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            ThetaProperty = DependencyProperty.Register("Theta", typeof(double), typeof(ROIShape),
                                                         new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
            DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(ROIShape),
                                                         new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

            IsInteractivedProperty = DependencyProperty.Register("IsInteractived", typeof(bool), typeof(ROIShape), new FrameworkPropertyMetadata(true, options));

            IsMoveEnabledProperty = DependencyProperty.Register("IsMoveEnabled", typeof(bool), typeof(ROIShape), new FrameworkPropertyMetadata(true, options));
            IsResizeEnabledProperty = DependencyProperty.Register("IsResizeEnabled", typeof(bool), typeof(ROIShape), new FrameworkPropertyMetadata(true, options));
            IsRotateEnabledProperty = DependencyProperty.Register("IsRotateEnabled", typeof(bool), typeof(ROIShape), new FrameworkPropertyMetadata(true, options));

            RectLenProperty = DependencyProperty.Register("RectLen", typeof(double), typeof(ROIShape), new FrameworkPropertyMetadata(15.0, options));

            CenterCrossLengthProperty = DependencyProperty.Register("CenterCrossLength", typeof(double), typeof(ROIShape), new FrameworkPropertyMetadata(10.0, options));
            CenterCrossBrushProperty = DependencyProperty.Register("CenterCrossBrush", typeof(Brush), typeof(ROIShape), new FrameworkPropertyMetadata(Brushes.Red, options));

            Shape.StrokeThicknessProperty.OverrideMetadata(typeof(ROIShape), new FrameworkPropertyMetadata(1.0, OnDataChanged));
            Shape.StretchProperty.OverrideMetadata(typeof(ROIShape), new FrameworkPropertyMetadata(Stretch.None));
            Shape.FillProperty.OverrideMetadata(typeof(ROIShape), new FrameworkPropertyMetadata(Brushes.Transparent));
            Shape.StrokeProperty.OverrideMetadata(typeof(ROIShape), new FrameworkPropertyMetadata(Brushes.LightGreen, OnDataChanged));
            Shape.FocusableProperty.OverrideMetadata(typeof(ROIShape), new FrameworkPropertyMetadata(true));
        }

        /// <summary>
        /// DataChange 變更時動作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        internal static void OnDataChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var DependObj = sender as ROIShape;
            DependObj.UpdateChange();
        }

        /// <summary>
        /// 游標離開圖示
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// 滑鼠按下
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            if (IsInteractived)
            {
                CaptureMouse();
                _IsDragable = true;
                HitGeometry = ReturnContainGeometry(e.GetPosition(this));
            }
            e.Handled = true;
        }

        /// <summary>
        /// 滑鼠放開
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            if (IsInteractived)
            {
                HitGeometry = null;
                _IsDragable = false;
                Mouse.OverrideCursor = Cursors.Arrow;
                ReleaseMouseCapture();
            }
        }

        /// <summary>
        /// 變更滑鼠游標形狀
        /// </summary>
        /// <param name="Pos"></param>
        protected void DoCursorOverride(Point Pos)
        {
            if (IsInteractived)
                if (ReturnContainGeometry(Pos) != null)
                    Mouse.OverrideCursor = Cursors.Cross;
                else
                    Mouse.OverrideCursor = Cursors.Arrow;
        }

        /// <summary>
        /// 按下移動
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (IsInteractived)
            {
                var Pos = e.GetPosition(this);
                DoCursorOverride(Pos);
                if (HitGeometry == null) return;

                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    var DragAction = pairs[HitGeometry];
                    DragAction.Invoke(Pos);
                    InvalidateVisual();
                }
            }
        }

        /// <summary>
        /// 給予shape大小位置資訊
        /// </summary>
        protected abstract void ResetLeftTop();

        /// <summary>
        /// DataChange 變更時動作
        /// </summary>
        public void UpdateChange()
        {
            ResetLeftTop();
            ShapeChangeAction?.Invoke(this);
        }

        /// <summary>
        /// 需通過返回定義圖形基元的 Geometry 類型的對象來實現 ReturnContainGeometry
        /// </summary>
        /// <param name="Pos"></param>
        /// <returns></returns>
        protected abstract Geometry ReturnContainGeometry(Point Pos);

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);
            if (!(Parent is IInputElement parent)) return;
            var mousePoint = e.GetPosition(parent);
            DoCursorOverride(mousePoint);
        }

        public virtual bool ShapeContains(Point point) => throw new NotImplementedException();

        public virtual Point[] GetEdgePoints(Size ImageSize) => throw new NotFiniteNumberException();

        public abstract string ShapeType { get; }
    }
}
