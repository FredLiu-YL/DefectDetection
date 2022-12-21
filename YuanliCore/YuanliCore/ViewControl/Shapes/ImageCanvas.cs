using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public class ImageCanvas : ZoomableCanvas
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register("ImageSource", typeof(BitmapSource), typeof(ImageCanvas), new PropertyMetadata(null, new PropertyChangedCallback(OnImageChanged)));
        
        public static readonly DependencyProperty AutoFitParentProperty = DependencyProperty.Register(nameof(AutoFitParent), typeof(bool), typeof(ImageCanvas), new PropertyMetadata(true));

        public static readonly DependencyProperty ControlCenterXProperty = DependencyProperty.Register("ControlCenterX", typeof(double), typeof(ImageCanvas),
                                                                                                new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ControlCenterYProperty = DependencyProperty.Register("ControlCenterY", typeof(double), typeof(ImageCanvas),
                                                                                                 new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsRederHorizontalMirrorProperty = DependencyProperty.Register("IsRederHorizontalMirror", typeof(bool), typeof(ImageCanvas), new PropertyMetadata(false, new PropertyChangedCallback(RenderImageChanged)));

        public static readonly DependencyProperty IsRederVerticalMirrorProperty = DependencyProperty.Register("IsRederVerticalMirror", typeof(bool), typeof(ImageCanvas), new PropertyMetadata(false, new PropertyChangedCallback(RenderImageChanged)));

        private Image image = new Image();
        private Line crosslineV; // 垂直線
        private Line crosslineH; // 水平線
        private bool showCross = false;

        public ImageCanvas()
        {
            crosslineV = new Line()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 10,
                Visibility = Visibility.Hidden
            };

            crosslineH = new Line()
            {
                Stroke = Brushes.Red,
                StrokeThickness = 10,
                Visibility = Visibility.Hidden
            };

            Children.Add(image);
            Children.Add(crosslineV);
            Children.Add(crosslineH);
        }

        /// <summary>
        /// 取得或設定 影像
        /// </summary>
        public BitmapSource ImageSource
        {
            get => (BitmapSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否要自動縮放自動大小
        /// </summary>
        public bool AutoFitParent
        {
            get => (bool)GetValue(AutoFitParentProperty);
            set => SetValue(AutoFitParentProperty, value);
        }

        /// <summary>
        /// 取得或設定 目標拉至畫面中心 X座標
        /// </summary>
        public double ControlCenterX
        {
            get => (double)GetValue(ControlCenterXProperty);
            set => SetValue(ControlCenterXProperty, value);
        }

        /// <summary>
        /// 取得或設定 目標拉至畫面中心 Y座標
        /// </summary>
        public double ControlCenterY
        {
            get => (double)GetValue(ControlCenterYProperty);
            set => SetValue(ControlCenterYProperty, value);
        }

        /// <summary>
        /// 取得或設定 影像是否需要水平鏡向
        /// </summary>
        public bool IsRederHorizontalMirror
        {
            get => (bool)GetValue(IsRederHorizontalMirrorProperty);
            set => SetValue(IsRederHorizontalMirrorProperty, value);
        }

        /// <summary>
        /// 取得或設定 影像是否需要垂直鏡向
        /// </summary>
        public bool IsRederVerticalMirror
        {
            get => (bool)GetValue(IsRederVerticalMirrorProperty);
            set => SetValue(IsRederVerticalMirrorProperty, value);
        }

        /// <summary>
        /// 滾輪放大縮小 最大值
        /// </summary>
        private double ScaleMax { get; set; } = 3.0;

        /// <summary>
        /// 滾輪放大縮小 最小值
        /// </summary>
        private double ScaleMin { get; set; } = 0.05;

        /// <summary>
        /// 中心十字線
        /// </summary>
        /// <param name="canvs"></param>
        /// <param name="source"></param>
        private static void FitCrossLine(ImageCanvas canvs, BitmapSource source)
        {
            // 調整十字線的起點及終點符合影像大小。
            canvs.crosslineV.X1 = (source.PixelWidth - 1) / 2;
            canvs.crosslineV.Y1 = 0;
            canvs.crosslineV.X2 = (source.PixelWidth - 1) / 2;
            canvs.crosslineV.Y2 = source.PixelHeight - 1;

            canvs.crosslineH.X1 = 0;
            canvs.crosslineH.Y1 = (source.PixelHeight - 1) / 2;
            canvs.crosslineH.X2 = source.PixelWidth - 1;
            canvs.crosslineH.Y2 = (source.PixelHeight - 1) / 2;
        }

        /// <summary>
        /// 縮放至適當大小
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="source"></param>
        private static void FitParent(ImageCanvas canvas, BitmapSource source)
        {
            FrameworkElement felm = canvas.Parent as FrameworkElement;
            double scaleX;
            double scaleY;
            if (felm.ActualWidth < source.PixelWidth)
            {
                scaleX = felm.ActualWidth / source.PixelWidth;
                scaleY = felm.ActualHeight / source.PixelHeight;
            }
            else
            {
                scaleX = 1.1;
                scaleY = 1.1;
            }

            if (scaleX <= 0 || scaleY <= 0) return;
            canvas.Scale = Math.Min(scaleX, scaleY);
            canvas.Offset = new Point();
            canvas.GetScreenCenter();
        }

        //protected override Size MeasureOverride(Size availableSize)
        //{
        //    return base.MeasureOverride(availableSize);
        //}

        //protected override Size ArrangeOverride(Size finalSize)
        //{
        //    return base.ArrangeOverride(finalSize);
        //}

        /// <summary>
        /// 影像切換
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageCanvas canvas = (ImageCanvas)d;
            BitmapSource imgSrc = e.NewValue as BitmapSource;

            canvas.Width = imgSrc.PixelWidth;
            canvas.Height = imgSrc.PixelHeight;
            canvas.image.Source = imgSrc;

            RenderImageChanged(d, e);

            FitCrossLine(canvas, imgSrc);
            if (canvas.AutoFitParent) FitParent(canvas, imgSrc);
        }

        /// <summary>
        /// 影像切換
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void RenderImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ImageCanvas canvas = (ImageCanvas)d;

            canvas.image.RenderTransformOrigin = new Point(0.5, 0.5);
            canvas.image.RenderTransform = new ScaleTransform(canvas.IsRederHorizontalMirror ? -1 : 1, canvas.IsRederVerticalMirror ? -1 : 1);
        }

        /// <summary>
        /// 滾輪縮放影像大小
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            IInputElement parent = this.Parent as IInputElement;
            if (parent == null) return;

            var x = Math.Pow(2, e.Delta / 3.0 / Mouse.MouseWheelDeltaForOneLine);

            var tempScale = Scale * x;
            if (tempScale > ScaleMax) tempScale = ScaleMax;
            else if (tempScale < ScaleMin) tempScale = ScaleMin;
            else if (Scale == tempScale) return;

            x = tempScale / Scale;
            Scale = tempScale;

            // Adjust the offset to make the point under the mouse stay still.
            var position = (Vector)e.GetPosition(parent);
            Offset = (Point)((Vector)(Offset + position) * x - position);

            //取得目前影像大小中心
            GetScreenCenter();

            e.Handled = true;
        }

        /// <summary>
        /// 滑鼠按鈕事件
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right && e.ClickCount == 2)
            {
                ZoomFitParent();
            }

            if (e.ChangedButton == MouseButton.Middle)
            {
                ShowCross();
            }
        }

        /// <summary>
        /// 縮放至適當大小
        /// </summary>
        /// <param name="d"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public void ZoomFitParent()
        {
            if (ImageSource != null) FitParent(this, ImageSource);

            //取得目前影像大小中心
            GetScreenCenter();
        }

        /// <summary>
        /// 中心顯示十字線顯示
        /// </summary>
        public void ShowCross()
        {
            if (!showCross)
            {
                crosslineV.Visibility = Visibility.Visible;
                crosslineH.Visibility = Visibility.Visible;
            }
            else
            {
                crosslineV.Visibility = Visibility.Hidden;
                crosslineH.Visibility = Visibility.Hidden;
            }
            showCross = !showCross;
        }

        /// <summary>
        /// 獲取目標拉至畫面中心座標
        /// </summary>
        /// <returns></returns>
        public Point GetScreenCenter()
        {
            FrameworkElement felm = this.Parent as FrameworkElement;
            if (felm == null) return new Point(0, 0);

            ControlCenterX = Offset.X / Scale + felm.ActualWidth / Scale / 2;
            ControlCenterY = Offset.Y / Scale + felm.ActualHeight / Scale / 2;

            return new Point(ControlCenterX, ControlCenterY);
        }
    }
}
