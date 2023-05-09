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

namespace YuanliCore.ImageProcess
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class CogImageConvertWindow : Window, INotifyPropertyChanged
    {
        //  private Frame<byte[]> frame;
        private ICogImage cogImage;
        private CogImageConvertParams imageConvertParam = new CogImageConvertParams(0);
        private bool isDispose = false;
        private bool isFullSelect = true;
        private bool isCenterSelect;



        public CogImageConvertWindow(BitmapSource bitmap)
        {
            //非WPF程式 執行時會丟失 WPF元件 System.Windows.Interactivity.dll  MaterialDesignColors.dll MaterialDesignThemes.Wpf.dll
            //記得要手動複製到Debug 執行檔位置底下
            InitializeComponent();

            UpdateImage(bitmap);

        }
        /// <summary>
        /// 直接傳入cognex的圖像格式  ，為了符合cog 的 變換矩陣流程
        /// </summary>
        /// <param name="cogImage"></param>
        public CogImageConvertWindow(ICogImage cogImage)
        {

            InitializeComponent();

            CogImage = cogImage;

        }
        //   public Frame<byte[]> Frame { get => frame; set => SetValue(ref frame, value); }
        /// <summary>
        ///  影像 Binding
        /// </summary>
        public ICogImage CogImage { get => cogImage; set => SetValue(ref cogImage, value); }
        public CogImageConvertParams ImageConvertParam { get => imageConvertParam; set => SetValue(ref imageConvertParam, value); }
        public bool IsFullSelect
        {
            get => isFullSelect; set
            {
                SetValue(ref isFullSelect, value);
                SetResultSelect();
            }
        }
        public bool IsCenterSelect { get => isCenterSelect; set { SetValue(ref isCenterSelect, value); SetResultSelect(); } }

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
                CogImage = frame.ColorFrameToColorCogImage();

            }


        }
 
        public ICommand ClosingCommand => new RelayCommand(() =>
       {

       });

        public ICommand OpenCommand => new RelayCommand(() =>
        {
            switch (ImageConvertParam.ResultOutput) {
                case ResultSelect.Full:
                    IsFullSelect = true;
                    break;
                case ResultSelect.Center:
                    IsCenterSelect = true;
                    break;
                default:
                    break;
            }

        });

        private void SetResultSelect()
        {
            if (IsFullSelect)
                ImageConvertParam.ResultOutput = ResultSelect.Full;
            else if (IsCenterSelect)
                ImageConvertParam.ResultOutput = ResultSelect.Center;


        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            if (isDispose) e.Cancel = false;
            this.Hide();

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
