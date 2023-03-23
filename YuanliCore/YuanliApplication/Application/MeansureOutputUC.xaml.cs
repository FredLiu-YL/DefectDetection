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
using YuanliApplication.Common;

namespace YuanliApplication.Application
{
    /// <summary>
    /// MeansureOutputUC.xaml 的互動邏輯
    /// </summary>
    public partial class MeansureOutputUC : UserControl, INotifyPropertyChanged
    {


       private static readonly DependencyProperty ResultCollectionProperty = DependencyProperty.Register(nameof(ResultCollection), typeof(ObservableCollection<FinalResult>), typeof(MeansureOutputUC), new FrameworkPropertyMetadata(new ObservableCollection<FinalResult>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty SelectIndexProperty = DependencyProperty.Register(nameof(SelectIndex), typeof(int), typeof(MeansureOutputUC), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public MeansureOutputUC()
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
}
