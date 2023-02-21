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


namespace YuanliCore.ImageProcess.Caliper
{
    /// <summary>
    /// Window1.xaml 的互動邏輯
    /// </summary>
    public partial class CogCaliperWindow : Window, INotifyPropertyChanged
    {
        //  private Frame<byte[]> frame;
        private ICogImage cogImage;
        private CaliperParams caliperParam = new CaliperParams();
        private bool isDispose =false;
        public CogCaliperWindow(BitmapSource bitmap)
        {
        
            InitializeComponent();
          
            UpdateImage(bitmap);
           
        }

        //   public Frame<byte[]> Frame { get => frame; set => SetValue(ref frame, value); }
        public ICogImage CogImage { get => cogImage; set => SetValue(ref cogImage, value); }
        public CaliperParams CaliperParam { get => caliperParam; set => SetValue(ref caliperParam, value); }


        public void UpdateImage(BitmapSource bitmap)
        {
            var b = bitmap.FormatConvertTo(PixelFormats.Bgr24);
            var frame = b.ToByteFrame();
         
            CogImage = frame.ColorFrameToCogImage();

        }

        protected override void OnClosing(CancelEventArgs e)
        {
    
            e.Cancel = true;

            if (isDispose) e.Cancel = false;
            this.Hide();


        }
        public  void Dispose()
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
