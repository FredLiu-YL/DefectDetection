using GalaSoft.MvvmLight.Command;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YuanliCore.CameraLib;
using YuanliCore.Views.CanvasShapes;

namespace YuanliCore.Views
{
    /// <summary>
    /// VirtualCanvas.xaml 的互動邏輯
    /// </summary>
    public partial class VirtualCanvas : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(BitmapSource), typeof(VirtualCanvas),
                                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<ROIShape>), typeof(VirtualCanvas),
                                                                                                    new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ShapesProperty = DependencyProperty.Register(nameof(Shapes), typeof(ObservableCollection<Shape>), typeof(VirtualCanvas),
                                                                                            new FrameworkPropertyMetadata(new ObservableCollection<Shape>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback((obj, e) =>
                                                                                             {
                                                                                                 var canvas = obj as VirtualCanvas;
                                                                                                 canvas.Shapes.CollectionChanged += (sender, j) =>
                                                                                                 {
                                                                                                     ShapesItemsChanged(obj, sender, j);
                                                                                                 };
                                                                                             })));

        public static readonly DependencyProperty ShapeTypeProperty = DependencyProperty.Register(nameof(ShapeType), typeof(ShapeTypes), typeof(VirtualCanvas),
                                                                                               new FrameworkPropertyMetadata(ShapeTypes.Null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsShowClearProperty = DependencyProperty.Register(nameof(IsShowClear), typeof(bool), typeof(VirtualCanvas),
                                                                                                      new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsShowBarProperty = DependencyProperty.Register(nameof(IsShowBar), typeof(bool), typeof(VirtualCanvas),
                                                                                                               new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ControlCenterXProperty = DependencyProperty.Register(nameof(ControlCenterX), typeof(double), typeof(VirtualCanvas),
                                                                                                 new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ControlCenterYProperty = DependencyProperty.Register(nameof(ControlCenterY), typeof(double), typeof(VirtualCanvas),
                                                                                                 new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsRederHorizontalMirrorProperty = DependencyProperty.Register(nameof(IsRederHorizontalMirror), typeof(bool), typeof(VirtualCanvas),
                                                                                                 new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty IsRederVerticalMirrorProperty = DependencyProperty.Register(nameof(IsRederVerticalMirror), typeof(bool), typeof(VirtualCanvas),
                                                                                                 new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty MousePixcelProperty = DependencyProperty.Register(nameof(MousePixcel), typeof(Point), typeof(VirtualCanvas),
                                                                                                    new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AddShapeActionProperty = DependencyProperty.Register(nameof(AddShapeAction), typeof(ICommand), typeof(VirtualCanvas),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RemoveShapeActionProperty = DependencyProperty.Register(nameof(RemoveShapeAction), typeof(ICommand), typeof(VirtualCanvas),
                                                                                      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ClearShapeActionProperty = DependencyProperty.Register(nameof(ClearShapeAction), typeof(ICommand), typeof(VirtualCanvas),
                                                                                              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty ZoomFitActionProperty = DependencyProperty.Register(nameof(ZoomFitAction), typeof(ICommand), typeof(VirtualCanvas),
                                                                                              new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty RValueProperty = DependencyProperty.Register(nameof(RValue), typeof(byte), typeof(VirtualCanvas),
                                                                                      new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty GValueProperty = DependencyProperty.Register(nameof(GValue), typeof(byte), typeof(VirtualCanvas),
                                                                                       new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BValueProperty = DependencyProperty.Register(nameof(BValue), typeof(byte), typeof(VirtualCanvas),
                                                                                       new FrameworkPropertyMetadata(default(byte), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AutoFitParentProperty = DependencyProperty.Register(nameof(AutoFitParent), typeof(bool), typeof(VirtualCanvas), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public VirtualCanvas()
        {
            InitializeComponent();
        }

        private void MainGrid_Loaded(object sender, RoutedEventArgs e)
        {
            AddShapeAction = new RelayCommand<ROIShape>(key => MainCanvas.AddShpae(key));
            RemoveShapeAction = new RelayCommand<ROIShape>(key => MainCanvas.RemoveShape(key));
            ClearShapeAction = new RelayCommand(() => MainCanvas.ClearShape());
            ZoomFitAction = new RelayCommand(() => MainCanvas.ZoomFitParent());

        }

        private static void ShapesItemsChanged(DependencyObject obj, object sender, NotifyCollectionChangedEventArgs changedItemPack)
        {
            var canvas = obj as VirtualCanvas;
            var shapeCollection = sender as ObservableCollection<Shape>;
            int newIndex = changedItemPack.NewStartingIndex;
            var oldIndex = changedItemPack.OldStartingIndex;

            switch (changedItemPack.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in changedItemPack.NewItems)
                    {
                        canvas.MainCanvas.Children.Add((Shape)item);
                        canvas.MainCanvas.ShapesItems.Add((Shape)item);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in changedItemPack.OldItems)
                    {
                        canvas.MainCanvas.Children.Remove((Shape)item);
                        canvas.MainCanvas.ShapesItems.Remove((Shape)item);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:

                    if (newIndex == oldIndex) return;
                    var temp = shapeCollection[newIndex];
                    shapeCollection[newIndex] = shapeCollection[oldIndex];
                    shapeCollection[oldIndex] = temp;
                    break;

                case NotifyCollectionChangedAction.Reset:
                    List<UIElement> waitForDelectingShapes = new List<UIElement>();
                    int countOfArray = 0;
                    UIElement[] elements = new UIElement[canvas.MainCanvas.Children.Count];

                    foreach (var item in canvas.MainCanvas.Children)
                    {
                        elements[countOfArray] = (UIElement)item;
                        countOfArray++;
                        canvas.MainCanvas.ShapesItems.Select(shape =>
                        {
                            if (item == shape) waitForDelectingShapes.Add((UIElement)item);
                            return shape;
                        }).ToList();
                    }
                    for (int i = 0; i < countOfArray; i++)
                    {
                        waitForDelectingShapes.Select(toBeDelected =>
                        {
                            if (elements[i] == toBeDelected) elements[i] = null;
                            return toBeDelected;
                        }).ToList();
                    }
                    canvas.MainCanvas.Children.Clear();
                    canvas.MainCanvas.ShapesItems.Clear();
                    for (int i = 0; i < countOfArray; i++)
                    {
                        if (elements[i] != null) canvas.MainCanvas.Children.Add(elements[i]);
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    int oldItemIndex = -1, newItemIndex = -1, count = 0;

                    foreach (var item in canvas.MainCanvas.Children)
                    {
                        foreach (var shapes in shapeCollection)
                        {
                            if (item == shapeCollection[newIndex]) oldItemIndex = count;
                            else if (item == shapeCollection[oldIndex]) newItemIndex = count;
                        }
                        count = count + 1;
                    }
                    canvas.MainCanvas.ShapesItems.Move(newIndex, oldIndex);
                    int countt = canvas.MainCanvas.Children.Count;
                    UIElement[] newSequenceOfShapes = new UIElement[countt];
                    canvas.MainCanvas.Children.CopyTo(newSequenceOfShapes, 0);
                    canvas.MainCanvas.Children.Clear();
                    for (int child = 0; child < countt; child++)
                    {
                        if (child == newItemIndex) canvas.MainCanvas.Children.Insert(newItemIndex, shapeCollection[newIndex]);
                        else if (child == oldItemIndex) canvas.MainCanvas.Children.Insert(oldItemIndex, shapeCollection[oldIndex]);
                        else canvas.MainCanvas.Children.Add(newSequenceOfShapes[child]);
                    }
                    break;
            }
        }

        /// <summary>
        /// 取得或設定 影像
        /// </summary>
        public BitmapSource ImageSource
        {
            get => GetValue(ImageSourceProperty) as BitmapSource;
            set => SetValue(ImageSourceProperty, value);
        }

        /// <summary>
        /// 取得或設定 ROIShape
        /// </summary>
        public ObservableCollection<ROIShape> ItemsSource
        {
            get => (ObservableCollection<ROIShape>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public ObservableCollection<Shape> Shapes
        {
            get => (ObservableCollection<Shape>)GetValue(ShapesProperty);
            set => SetValue(ShapesProperty, value);
        }

        /// <summary>
        /// 取得或設定 清除shape按鈕顯示
        /// </summary>
        public bool IsShowClear
        {
            get => (bool)GetValue(IsShowClearProperty);
            set => SetValue(IsShowClearProperty, value);
        }

        /// <summary>
        /// 取得或設定 狀態欄顯示
        /// </summary>
        public bool IsShowBar
        {
            get => (bool)GetValue(IsShowBarProperty);
            set => SetValue(IsShowBarProperty, value);
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
        /// 取得或設定 滑鼠在影像上座標
        /// </summary>
        public Point MousePixcel
        {
            get => (Point)GetValue(MousePixcelProperty);
            set => SetValue(MousePixcelProperty, value);
        }

        /// <summary>
        /// 取得或設定 長度
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// 取得或設定 dx
        /// </summary>
        public double DeltaX { get; set; }

        /// <summary>
        /// 取得或設定 dy
        /// </summary>
        public double DeltaY { get; set; }

        /// <summary>
        /// 取得或設定 角度
        /// </summary>
        public double Theta { get; set; }

        /// <summary>
        /// 取得或設定 是否選擇量測
        /// </summary>
        public bool RulerEnabled { get; set; }

        /// <summary>
        /// ZoomFit按鈕
        /// </summary>
        public ICommand ZoomFitCommand => new RelayCommand(() => MainCanvas.ZoomFitParent());

        /// <summary>
        /// 中心十字按鈕
        /// </summary>
        public ICommand CenterlineCommand => new RelayCommand(() => MainCanvas.ShowCross());

        /// <summary>
        /// 量測按鈕
        /// </summary>
        public ICommand RulerCommand => new RelayCommand(() => { MainCanvas.Ruler(); });

        /// <summary>
        /// 新增Shape
        /// </summary>
        public ICommand AddShapeAction
        {
            get => (ICommand)GetValue(AddShapeActionProperty);
            set => SetValue(AddShapeActionProperty, value);
        }

        /// <summary>
        /// 清除指定Shape
        /// </summary>
        public ICommand RemoveShapeAction
        {
            get => (ICommand)GetValue(RemoveShapeActionProperty);
            set => SetValue(RemoveShapeActionProperty, value);
        }

        /// <summary>
        /// 清除所有Shape
        /// </summary>
        public ICommand ClearShapeAction
        {
            get => (ICommand)GetValue(ClearShapeActionProperty);
            set => SetValue(ClearShapeActionProperty, value);
        }

        public ICommand ZoomFitAction
        {
            get => (ICommand)GetValue(ZoomFitActionProperty);
            set => SetValue(ZoomFitActionProperty, value);
        }

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
        /// 取得或設定Button產生的Shape
        /// </summary>
        public ShapeTypes ShapeType
        {
            get => (ShapeTypes)GetValue(ShapeTypeProperty);
            set => SetValue(ShapeTypeProperty, value);
        }

        /// <summary>
        /// 取得或設定 是否要自動縮放自動大小
        /// </summary>
        public bool AutoFitParent
        {
            get => (bool)GetValue(AutoFitParentProperty);
            set => SetValue(AutoFitParentProperty, value);
        }

        public event EventHandler<MouseMoveArgs> MouseCanvasDoubleClick;

        public enum ShapeTypes
        {
            Null,
            AngleShape,
            ROICircle,
            ROICross,
            ROICrossBox,
            ROILine,
            ROIPolygon,
            ROIRectangle,
            ROIRotateRect,
            RectShape
        }

        /// <summary>
        /// 存圖按鈕
        /// </summary>
        public ICommand SaveCommand => new RelayCommand(() =>
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Title = "存檔路徑";
            dialog.Filter = "bmp(*.bmp)|*.bmp";
            dialog.DefaultExt = "";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file = dialog.FileName.Split('.').First();
                ImageSource.Save(file);
            }
        });

        public ICommand ClearCommand => new RelayCommand(() =>
          {
              List<UIElement> waitForDelectingShapes = new List<UIElement>();
              int countOfArray = 0;
              UIElement[] elements = new UIElement[MainCanvas.Children.Count];

              foreach (var item in MainCanvas.Children)
              {
                  elements[countOfArray] = (UIElement)item;
                  countOfArray++;
                  MainCanvas.ShapesItems.Select(shape =>
                  {
                      if (item == shape) waitForDelectingShapes.Add((UIElement)item);
                      return shape;
                  }).ToList();
              }
              for (int i = 0; i < countOfArray; i++)
              {
                  waitForDelectingShapes.Select(toBeDelected =>
                  {
                      if (elements[i] == toBeDelected) elements[i] = null;
                      return toBeDelected;
                  }).ToList();
              }
              MainCanvas.Children.Clear();
              MainCanvas.ShapesItems.Clear();
              ShapesItemsChanged(this, Shapes, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
              for (int i = 0; i < countOfArray; i++)
              {
                  if (elements[i] != null) MainCanvas.Children.Add(elements[i]);
              }
              if (MainCanvas.ItemsSource != null)
                  MainCanvas.ClearShape();
              RulerEnabled = false;
          });

        private void MainCanvas_MouseCanvasDoubleClick(object sender, MouseMoveArgs e)
        {
            MouseCanvasDoubleClick?.Invoke(sender, new MouseMoveArgs(MousePixcel.X, MousePixcel.Y));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            T oldValue = field;
            field = value;
            OnPropertyChanged(propertyName, oldValue, value);
        }

        protected virtual void OnPropertyChanged<T>(string name, T oldValue, T newValue)
        {
            // oldValue 和 newValue 目前沒有用到，代爾後需要再實作。
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
