using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YuanliCore.Interface;
using YuanliCore.CameraLib;
using GalaSoft.MvvmLight.Command;

namespace YuanliCore.ImageProcess.Blob
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class CogBlobWindow : Window, INotifyPropertyChanged
    {
        //  private Frame<byte[]> frame;
        private ICogImage cogImage;
        private BlobParams blobParam = new BlobParams();
        private bool isDispose = false;
        private bool isFullSelect = true;
        private double judgeMin=10;
        private double judgeMax=99999999;


        public CogBlobWindow(BitmapSource bitmap)
        {

            InitializeComponent();

            UpdateImage(bitmap);

        }
        /// <summary>
        /// 直接傳入cognex的圖像格式  ，為了符合cog 的 變換矩陣流程
        /// </summary>
        /// <param name="cogImage"></param>
        public CogBlobWindow(ICogImage cogImage)
        {

            InitializeComponent();

            CogImage = cogImage;

        }
        //   public Frame<byte[]> Frame { get => frame; set => SetValue(ref frame, value); }
        public ICogImage CogImage { get => cogImage; set => SetValue(ref cogImage, value); }
        public BlobParams BlobParam { get => blobParam; set => SetValue(ref blobParam, value); }
        public bool IsFullSelect { get => isFullSelect; set { SetValue(ref isFullSelect, value); SetResultSelect(); } }


        public double JudgeMin { get => judgeMin; set { SetValue(ref judgeMin, value); SetResultSelect(); } }

        public double JudgeMax { get => judgeMax; set { SetValue(ref judgeMax, value); SetResultSelect(); } }


        public ICommand ClosingCommand => new RelayCommand(() =>
        {

        });

        public ICommand OpenCommand => new RelayCommand(() =>
        {
            JudgeMin = BlobParam.JudgeMin;
      //      JudgeMax = BlobParam.JudgeMax;

        });

        public void UpdateImage(BitmapSource bitmap)
        {
            if (bitmap == null) throw new Exception("Image is null");
            if (bitmap.Format == PixelFormats.Indexed8 || bitmap.Format == PixelFormats.Gray8) {
                var frameGray = bitmap.ToByteFrame();
                CogImage = frameGray.GrayFrameToCogImage();
            }
            else {
                var b = bitmap.FormatConvertTo(PixelFormats.Bgr24);
                var frame = b.ToByteFrame();

                CogImage = frame.ColorFrameToCogImage(out ICogImage inputImage);
            }


        }

        protected override void OnClosing(CancelEventArgs e)
        {

            e.Cancel = true;

            if (isDispose) e.Cancel = false;
            this.Hide();


        }
        private void SetResultSelect()
        {

            BlobParam.JudgeMin = JudgeMin;
         //   BlobParam.JudgeMax = JudgeMax;



        }
        public void Dispose()
        {
            isDispose = true;
            Close();


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
