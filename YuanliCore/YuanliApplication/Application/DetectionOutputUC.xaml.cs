using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
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
using YuanliApplication.Common;

namespace YuanliApplication.Application
{
    /// <summary>
    /// DetectionOutputUC.xaml 的互動邏輯
    /// </summary>
    public partial class DetectionOutputUC : UserControl, INotifyPropertyChanged
    {


       private static readonly DependencyProperty ResultCollectionProperty = DependencyProperty.Register(nameof(ResultCollection), typeof(ObservableCollection<FinalResult>), typeof(DetectionOutputUC), new FrameworkPropertyMetadata(new ObservableCollection<FinalResult>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty SelectIndexProperty = DependencyProperty.Register(nameof(SelectIndex), typeof(int), typeof(DetectionOutputUC), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public DetectionOutputUC()
        {
            InitializeComponent();
        }

      public int  SelectIndex
        {
            get => (int)GetValue(SelectIndexProperty);
            set => SetValue(SelectIndexProperty, value);
        }

        public ObservableCollection<FinalResult> ResultCollection
        {
            get => (ObservableCollection<FinalResult>)GetValue(ResultCollectionProperty);
            set => SetValue(ResultCollectionProperty, value);
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

    public class JudgeColorConver : IValueConverter
    {
        //当值从绑定源传播给绑定目标时，调用方法Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.Green;
            else
                return Brushes.DarkRed;
        }

        //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
