using AutoFocusMachine.Model;
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
        private int tabControlIndex ;
        private ObservableCollection<ROIShape> drawings = new ObservableCollection<ROIShape>();
        private WriteableBitmap image;
        private double seachPosX1, seachPosY1, seachPosX2, seachPosY2, seachPosX3, seachPosY3;

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {

            atfMachine =   new Machine();
           

          
        }
        public WriteableBitmap Image { get => image; set => SetValue(ref image, value); }
        /// <summary>
        /// ���o�γ]�w shape 
        /// </summary>
        public ObservableCollection<ROIShape> Drawings { get => drawings; set => SetValue(ref drawings, value); }

        public double SeachPosX1 { get => seachPosX1; set => SetValue(ref seachPosX1, value); }
        public double SeachPosY1 { get => seachPosY1; set => SetValue(ref seachPosY1, value); }

        public double SeachPosX2 { get => seachPosX2; set => SetValue(ref seachPosX2, value); }
        public double SeachPosY2 { get => seachPosY2; set => SetValue(ref seachPosY2, value); }

        public double SeachPosX3 { get => seachPosX3; set => SetValue(ref seachPosX3, value); }
        public double SeachPosY3 { get => seachPosY3; set => SetValue(ref seachPosY3, value); }


        /// <summary>
        /// �ƹ��b�v���� Pixcel �y��
        /// </summary>
        public System.Windows.Point MousePixcel { get; set; }

        /// <summary>
        /// �v������Pixel�y�� X
        /// </summary>
        public double ControlCenterX { get; set; }

        /// <summary>
        /// �v������Pixel�y�� Y
        /// </summary>
        public double ControlCenterY { get; set; }


        /// <summary>
        /// �s�W Shape
        /// </summary>
        public ICommand AddShapeAction { get; set; }
        /// <summary>
        /// �M�� Shape
        /// </summary>
        public ICommand ClearShapeAction { get; set; }


        public int  TabControlIndex { get => tabControlIndex; set => SetValue(ref tabControlIndex, value); }


        public ICommand LoadedCommand => new RelayCommand<string>(async key =>
        {

            CameraLive();

        });
        public ICommand ClosedCommand => new RelayCommand<string>(async key =>
        {



        });
        public ICommand TabControlChangedCommand => new RelayCommand<string>(async key =>
        {

            int i=0;
            var c =  i++;

        });



        private void CameraLive()
        {

            atfMachine.Table_Module.Camera.Grab();

            Image = new WriteableBitmap(atfMachine.Table_Module.Camera.Width, atfMachine.Table_Module.Camera.Height, 96, 96, atfMachine.Table_Module.Camera.PixelFormat, null);
            camlive = atfMachine.Table_Module.Camera.Frames.ObserveLatestOn(TaskPoolScheduler.Default) //���̷s����� �FTaskPoolScheduler.Default  ��ܦb�t�~�@�Ӱ�����W����
                         .ObserveOn(DispatcherScheduler.Current)  //�N�q�\����ഫ���W�C���ǥ�X �FDispatcherScheduler.Current  ��ܦb�D������W����
                         .Subscribe(frame =>
                         {

                             var a = System.Threading.Thread.CurrentThread.ManagedThreadId;
                             if (frame != null) Image.WritePixels(frame);
                             //  Image = new WriteableBitmap(frame.Width, frame.Height, frame.dP, double dpiY, PixelFormat pixelFormat, BitmapPalette palette);
                         });
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
            // oldValue �M newValue �ثe�S���Ψ�A�N����ݭn�A��@�C
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        }


    }
}