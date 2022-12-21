using AutoFocusMachine.Model;
using AutoFocusMachine.Model.Recipe;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore;
using YuanliCore.CameraLib;
using YuanliCore.CameraLib.IDS;
using YuanliCore.Interface;
using YuanliCore.Motion.Marzhauser;
using YuanliCore.Views.CanvasShapes;

namespace AutoFocusMachine.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public partial class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private Machine atfMachine;
        private int tabControlIndex;
        private ObservableCollection<ROIShape> drawings = new ObservableCollection<ROIShape>();
        private WriteableBitmap image;
        private double tablePosX, tablePosY;

        private AFMachineRecipe mainRecipe = new AFMachineRecipe();
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            atfMachine = new Machine();



        }
        public WriteableBitmap Image { get => image; set => SetValue(ref image, value); }
        /// <summary>
        /// 取得或設定 shape 
        /// </summary>
        public ObservableCollection<ROIShape> Drawings { get => drawings; set => SetValue(ref drawings, value); }



        /// <summary>
        /// 滑鼠在影像內 Pixcel 座標
        /// </summary>
        public System.Windows.Point MousePixcel { get; set; }

        /// <summary>
        /// 影像中心Pixel座標 X
        /// </summary>
        public double ControlCenterX { get; set; }

        /// <summary>
        /// 影像中心Pixel座標 Y
        /// </summary>
        public double ControlCenterY { get; set; }

        public double TablePosX { get => tablePosX; set => SetValue(ref tablePosX, value); }
        public double TablePosY { get => tablePosY; set => SetValue(ref tablePosY, value); }
        /// <summary>
        /// 新增 Shape
        /// </summary>
        public ICommand AddShapeAction { get; set; }
        /// <summary>
        /// 清除 Shape
        /// </summary>
        public ICommand ClearShapeAction { get; set; }


        public int TabControlIndex { get => tabControlIndex; set => SetValue(ref tabControlIndex, value); }


        public ICommand LoadedCommand => new RelayCommand<string>(async key =>
        {

            CameraLive();

            isRefresh = true;
            taskRefresh1 = Task.Run(RefreshAFState);
            taskRefresh2 = Task.Run(RefreshPos);

        });
        public ICommand ClosedCommand => new RelayCommand<string>(async key =>
        {
            isRefresh = false;
            await taskRefresh1;
            await taskRefresh2;
            atfMachine.Dispose();

        });
        public ICommand TabControlChangedCommand => new RelayCommand<string>(async key =>
        {

            int i = 0;
            var c = i++;

        });



        private void CameraLive()
        {

            atfMachine.Table_Module.Camera.Grab();

            Image = new WriteableBitmap(atfMachine.Table_Module.Camera.Width, atfMachine.Table_Module.Camera.Height, 96, 96, atfMachine.Table_Module.Camera.PixelFormat, null);
            camlive = atfMachine.Table_Module.Camera.Frames.ObserveLatestOn(TaskPoolScheduler.Default) //取最新的資料 ；TaskPoolScheduler.Default  表示在另外一個執行緒上執行
                         .ObserveOn(DispatcherScheduler.Current)  //將訂閱資料轉換成柱列順序丟出 ；DispatcherScheduler.Current  表示在主執行緒上執行
                         .Subscribe(frame =>
                         {

                             var a = System.Threading.Thread.CurrentThread.ManagedThreadId;
                             if (frame != null) Image.WritePixels(frame);
                             //  Image = new WriteableBitmap(frame.Width, frame.Height, frame.dP, double dpiY, PixelFormat pixelFormat, BitmapPalette palette);
                         });
        }


        private async Task RefreshPos()
        {

            try
            {

                while (isRefresh)
                {
                    var pos = await atfMachine.Table_Module.GetPostion();
                    TablePosX = pos.X;
                    TablePosY = pos.Y;
                    await Task.Delay(300);
                }

            }
            catch (Exception ex)
            {

                // throw ex;
            }

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