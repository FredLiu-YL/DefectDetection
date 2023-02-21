using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliApplication.Application
{
    /// <summary>
    /// InspectUC.xaml 的互動邏輯
    /// </summary>
    public partial class InspectUC : UserControl, INotifyPropertyChanged
    {
        private BitmapSource sampleImage;
        private ObservableCollection<TestResult> methodCollection;
        private IMatcher matcher;
        private static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(InspectUC), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public InspectUC()
        {
            InitializeComponent();
        }

        public WriteableBitmap Image
        {
            get => (WriteableBitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }
        public BitmapSource SampleImage { get => sampleImage; set => SetValue(ref sampleImage, value); }
        public ObservableCollection<TestResult> MethodCollection { get => methodCollection; set => SetValue(ref methodCollection, value); }
        public PatmaxParams PatmaxParam { get; set; }
        

        public ICommand ReadTestCommand => new RelayCommand(async () =>
        {

            MethodCollection.Add(new TestResult { SN = $"{MethodCollection.Count + 1}", Name = $"T{MethodCollection.Count + 1}", ResultName = $"R{MethodCollection.Count + 1}" });


        });

        public ICommand TestMatchCommand => new RelayCommand(() =>
        {
            try {
              
 
            }
            catch (Exception ex) {

                MessageBox.Show(ex.ToString()); ;
            }


        });
        public ICommand EditSampleCommand => new RelayCommand(async () =>
        {
            try {
               

                matcher.EditParameter(Image);


                PatmaxParam = matcher.Patmaxparams;
              

 
 
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }



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
            // oldValue 和 newValue 目前沒有用到，代爾後需要再實作。
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        public class TestResult
        {
 

            public string SN { get; set; }
            public string Name { get; set; }
            public string ResultName { get; set; }
        }
    }
}
