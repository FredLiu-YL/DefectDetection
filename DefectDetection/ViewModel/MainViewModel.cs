using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using YuanliCore.ImageProcess;
using YuanliCore.Views.CanvasShapes;

namespace DefectDetection.ViewModel
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
    public class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private WriteableBitmap mainImage;
        private ObservableCollection<ROIShape> drawings;
        private MeansureRecipe mainRecipe = new MeansureRecipe();


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
        }


        public WriteableBitmap MainImage { get => mainImage; set => SetValue(ref mainImage, value); }
        public ObservableCollection<ROIShape> Drawings { get => drawings; set => SetValue(ref drawings, value); }
        public MeansureRecipe MainRecipe { get => mainRecipe; set => SetValue(ref mainRecipe, value); }


        public ICommand TestCommand => new RelayCommand(async () =>
        {
            string ipAddress = "127.0.0.1";
            int port = 16650;


            try {
                TcpClient client = new TcpClient(ipAddress, port);
                System.Console.WriteLine("�s�u���\");
                var stream = client.GetStream();
                //�N�r���� byte �}�C�A�ϥ� ASCII �s�X
                Byte[] myBytes = Encoding.UTF8.GetBytes("\\v1\\protocol\\current");
                stream.Write(myBytes, 0, myBytes.Length);


                //�q������ƬyŪ�����
                int bufferSize = client.ReceiveBufferSize;
                byte[] myBufferBytes = new byte[bufferSize];
                stream.Read(myBufferBytes, 0, bufferSize);
                //���o��ƨåB�ѽX��r
                Console.WriteLine(Encoding.UTF8.GetString(myBufferBytes, 0, bufferSize));


            }
            catch {

                return;
            }
        });
        public ICommand OpenImageCommand => new RelayCommand(async () =>
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "BMP files (*.bmp)|*.bmp|JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true) {// ���J�Ϥ�
                
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.UriSource = new Uri(dlg.FileName);
                bitmapImage.EndInit();

                // �N�Ϥ��ഫ�� BitmapSource
                BitmapSource bitmapSource = bitmapImage;
                MainImage = new WriteableBitmap(bitmapSource);

            }

        

        });
        public ICommand SaveRecipeCommand => new RelayCommand(async () =>
        {

        });
        public ICommand LoadRecipeCommand => new RelayCommand(async () =>
        {

        });

        public ICommand RunCommand => new RelayCommand(async () =>
        {

        });


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