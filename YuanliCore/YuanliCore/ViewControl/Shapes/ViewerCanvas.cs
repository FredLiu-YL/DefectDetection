using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
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
    public class ViewerCanvas : ImageCanvas
    {
        private bool isShapeCommand = false;
        private ObservableCollection<ROIShape> itemsSources = new ObservableCollection<ROIShape>();


        public static readonly DependencyProperty MousePixcelProperty = DependencyProperty.Register("MousePixcel", typeof(Point), typeof(ViewerCanvas),
                                                                                     new FrameworkPropertyMetadata(default(Point), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(ObservableCollection<ROIShape>), typeof(ViewerCanvas),
                                                                                             new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback((d, e) =>
                                                                                              {
                                                                                                  var dp = d as ViewerCanvas;
                                                                                                  dp.Load();
                                                                                              })));

        public static readonly DependencyProperty RValueProperty = DependencyProperty.Register("RValue", typeof(byte), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty GValueProperty = DependencyProperty.Register("GValue", typeof(byte), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BValueProperty = DependencyProperty.Register("BValue", typeof(byte), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RulerEnabledProperty = DependencyProperty.Register("RulerEnabled", typeof(bool), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register("Distance", typeof(double), typeof(ViewerCanvas),
                                                                               new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DeltaXProperty = DependencyProperty.Register("DeltaX", typeof(double), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty DeltaYProperty = DependencyProperty.Register("DeltaY", typeof(double), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ThetaProperty = DependencyProperty.Register("Theta", typeof(double), typeof(ViewerCanvas),
                                                                                       new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ViewerCanvas()
        {

        }

        #region
        /// <summary>
        /// 取得或設定 滑鼠在影像上座標
        /// </summary>
        public Point MousePixcel
        {
            get => (Point)GetValue(MousePixcelProperty);
            set => SetValue(MousePixcelProperty, value);
        }

        /// <summary>
        /// 取得或設定 ROIShape
        /// </summary>
        public ObservableCollection<ROIShape> ItemsSource
        {
            get => (ObservableCollection<ROIShape>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ObservableCollection<Shape> ShapesItems = new ObservableCollection<Shape>();


        /// <summary>
        /// 取得或設定 R值
        /// </summary>
        public byte RValue
        {
            get => (byte)GetValue(RValueProperty);
            set => SetValue(RValueProperty, value);
        }

        /// <summary>
        /// 取得或設定 G值
        /// </summary>
        public byte GValue
        {
            get => (byte)GetValue(GValueProperty);
            set => SetValue(GValueProperty, value);
        }

        /// <summary>
        /// 取得或設定 B值
        /// </summary>
        public byte BValue
        {
            get => (byte)GetValue(BValueProperty);
            set => SetValue(BValueProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否選擇量測
        /// </summary>
        public bool RulerEnabled
        {
            get => (bool)GetValue(RulerEnabledProperty);
            set => SetValue(RulerEnabledProperty, value);
        }

        /// <summary>
        /// 取得或設定 長度
        /// </summary>
        public double Distance
        {
            get => (double)GetValue(DistanceProperty);
            set => SetValue(DistanceProperty, value);
        }

        /// <summary>
        /// 取得或設定 dx
        /// </summary>
        public double DeltaX
        {
            get => (double)GetValue(DeltaXProperty);
            set => SetValue(DeltaXProperty, value);
        }

        /// <summary>
        /// 取得或設定 dy
        /// </summary>
        public double DeltaY
        {
            get => (double)GetValue(DeltaYProperty);
            set => SetValue(DeltaYProperty, value);
        }

        /// <summary>
        /// 取得或設定 角度
        /// </summary>
        public double Theta
        {
            get => (byte)GetValue(ThetaProperty);
            set => SetValue(ThetaProperty, value);
        }

        /// <summary>
        /// 取得或設定 滑鼠在影像上最後座標位
        /// </summary>
        private Point LastMousePosition { get; set; }

        #endregion

        /// <summary>
        /// 滑鼠移動
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
                MousePixcel = e.GetPosition(this);

            if (ItemsSource != null && RulerEnabled)
            {
                var _shape = ItemsSource.LastOrDefault() as ROILine;
                if (_shape == null) return;
                UpdateMeasureData(_shape);
            }

            IInputElement parent = this.Parent as IInputElement;
            if (parent == null) return;
            var position = e.GetPosition(parent);

            //RGB值顯示
            Point imgPos = e.GetPosition(this);
            var RGB = GetPixelColor((int)Math.Round(imgPos.X), (int)Math.Round(imgPos.Y));
            RValue = RGB.Item1; GValue = RGB.Item2; BValue = RGB.Item3;

            //移動畫面
            if (e.LeftButton == MouseButtonState.Pressed
              && !(e.OriginalSource is Thumb) && !(e.OriginalSource is Shape)) // Don't block the scrollbars.
            {
                CaptureMouse();
                GetScreenCenter();
                Offset -= position - LastMousePosition;
                e.Handled = true;
            }
            else ReleaseMouseCapture();

            GetScreenCenter();

            LastMousePosition = position;
        }

        public event EventHandler<MouseMoveArgs> CanvasMouseDoubleClick;

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                CanvasMouseDoubleClick?.Invoke(this, new MouseMoveArgs(MousePixcel.X, MousePixcel.Y));
            }
        }

        /// <summary>
        /// 量測
        /// </summary>
        /// <param name="itemsSource"></param>
        public void Ruler()
        {
            if (RulerEnabled)
            {
                //ClearShape();
                if (ItemsSource.Count != 0)
                {
                    var line = ItemsSource.FirstOrDefault(p => p is ROILine);
                    RemoveShape(line);
                }
                RulerEnabled = false;
            }
            else
            {
                if (ItemsSource == null)
                    ItemsSource = new ObservableCollection<ROIShape>();
                ROILine roiLine = new ROILine()
                {
                    RectLen = 50,
                    X1 = ControlCenterX - 100,
                    X2 = ControlCenterX + 200,
                    Y1 = ControlCenterY,
                    Y2 = ControlCenterY,
                    Stroke = Brushes.Red,
                    StrokeThickness = 3,
                    IsInteractived = true,
                    IsMoveEnabled = true
                };
                AddShpae(roiLine);
                RulerEnabled = true;
            }
        }

        //private StudentList studentList = new StudentList();

        private void Load()
        {
            //if (studentList.CountOfHandlers == 0)
            if (ItemsSource != null)
                ItemsSource.CollectionChanged += ShapeCollectionChanged;
        }

        public void CloseCollectionChanged()
        {
            if (ItemsSource != null)
                ItemsSource.CollectionChanged -= ShapeCollectionChanged;
        }

        private void ShapeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (isShapeCommand) return;
            //if (e.Action == NotifyCollectionChangedAction.Add)
            //{
            //    foreach (var item in e.NewItems)
            //    {
            //        itemsSources.Add((ROIShape)item);
            //        Children.Add((ROIShape)item);
            //    }
            //}

            //if (e.Action == NotifyCollectionChangedAction.Remove)
            //{
            //    if (itemsSources.Count == 0) return;
            //    itemsSources.RemoveAt(e.OldStartingIndex);
            //    Children.RemoveAt(e.OldStartingIndex + 3);
            //}

            //if (e.Action == NotifyCollectionChangedAction.Reset)
            //{
            //    foreach (var item in this.itemsSources)
            //    {
            //        Children.Remove(item);
            //        Children.Capacity = Children.Count;
            //    }
            //    itemsSources.Clear();
            //}
        }

        /// <summary>
        /// 新增 Shape
        /// </summary>
        /// <param name="roiShape"></param>
        public void AddShpae(ROIShape roiShape)
        {
            isShapeCommand = true;

            ItemsSource.Add(roiShape);  
            Children.Add(roiShape);

            isShapeCommand = false;
        }

        /// <summary>
        /// 清除指定 Shape
        /// </summary>
        /// <param name="roiShape"></param>
        public void RemoveShape(ROIShape roiShape)
        {
            //isShapeCommand = true;

            int indexItemsSource = ItemsSource.IndexOf(roiShape);
            ItemsSource.RemoveAt(indexItemsSource);
            int indexChildren = Children.IndexOf(roiShape);
            Children.RemoveAt(indexChildren);

            //isShapeCommand = false;
        }

        /// <summary>
        /// 清除所有Shape
        /// </summary>
        public void ClearShape()
        {
            //isShapeCommand = true;

            foreach (var item in this.ItemsSource)
            {
                Children.Remove(item);
                Children.Capacity = Children.Count;
            }
            ItemsSource.Clear();

            //isShapeCommand = false;
        }

        /// <summary>
        /// pixel 顏色占比
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private (byte, byte, byte) GetPixelColor(int x, int y)
        {
            if (x < 0 || y < 0) return (0, 0, 0);
            if (ImageSource == null) return (0, 0, 0);
            if (x > ImageSource.PixelWidth - 1 || y > ImageSource.PixelHeight - 1) return (0, 0, 0);
            var bytesPerPixel = (ImageSource.Format.BitsPerPixel + 7) / 8;
            var bytes = new byte[bytesPerPixel];
            var rect = new Int32Rect(x, y, 1, 1);
            ImageSource.CopyPixels(rect, bytes, bytesPerPixel, 0);

            if (ImageSource.Format == PixelFormats.Bgra32)
            {
                return (bytes[2], bytes[1], bytes[0]);
            }
            else if (ImageSource.Format == PixelFormats.Bgr32)
            {
                return (bytes[2], bytes[1], bytes[0]);
            }
            else if (ImageSource.Format == PixelFormats.Bgr24)
            {
                return (bytes[2], bytes[1], bytes[0]);
            }
            else if (ImageSource.Format == PixelFormats.Gray8)
            {
                return (bytes[0], bytes[0], bytes[0]);
            }
            else if (ImageSource.Format == PixelFormats.Bgr101010)
            {
                return (bytes[2], bytes[1], bytes[0]);
            }
            else if (ImageSource.Format == PixelFormats.Indexed8)
            {
                return (bytes[0], bytes[0], bytes[0]);
            }
            else
            {
                return (0, 0, 0);
            }
        }

        /// <summary>
        /// 更新測量數據
        /// </summary>
        /// <param name="shape"></param>
        private void UpdateMeasureData(ROILine shape = null)
        {
            if (shape == null) return;
            DeltaX = shape.DeltaX.Round(2);
            DeltaY = shape.DeltaY.Round(2);
            Theta = shape.Theta.Round(2);
            Distance = (shape.Distance * 1).Round(2);
        }

        #region 未使用
        /// <summary>
        /// ItemsSource(Drawing Shape)來源更動
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as ViewerCanvas;
            control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }
        private bool isChanged;

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
            }
            if (isChanged == true) return;
            // Add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
                isChanged = true;
            }
        }

        private void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    itemsSources.Add((ROIShape)item);
                    Children.Add((ROIShape)item);
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                itemsSources.RemoveAt(e.OldStartingIndex);
                Children.RemoveAt(e.OldStartingIndex + 3);
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                //Children.Clear();
                foreach (var item in this.itemsSources)
                {
                    Children.Remove(item);
                    Children.Capacity = Children.Count;
                }
                itemsSources.Clear();
            }
        }
        #endregion
    }

    //public class StudentList : ObservableCollection<ROIShape>
    //{
    //    public int CountOfHandlers { get; private set; }

    //    public override event NotifyCollectionChangedEventHandler CollectionChanged
    //    {
    //        add { if (value != null) CountOfHandlers += value.GetInvocationList().Length; }
    //        remove { if (value != null) CountOfHandlers -= value.GetInvocationList().Length; }
    //    }
    //}



    public class MouseMoveArgs : EventArgs
    {
        public MouseMoveArgs(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        public double X { get; private set; }
        public double Y { get; private set; }
    }
}
