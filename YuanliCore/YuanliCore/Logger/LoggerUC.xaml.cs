using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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


namespace YuanliCore.Logger
{
    /// <summary>
    /// LoggerUC.xaml 的互動邏輯
    /// </summary>
    public partial class LoggerUC : UserControl, INotifyPropertyChanged
    {
        private string mainLog;
        private string message;

        private static readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(Message), typeof(string), typeof(LoggerUC), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(AddMessageChanged)));
        private static readonly DependencyProperty MachineNameProperty = DependencyProperty.Register(nameof(MachineName), typeof(string), typeof(LoggerUC), new FrameworkPropertyMetadata("Machine", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(AddMessageChanged)));

        public LoggerUC()
        {
            InitializeComponent();
        }

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public string MachineName
        {
            get => (string)GetValue(MachineNameProperty);
            set => SetValue(MachineNameProperty, value);
        }

        public string MainLog { get => mainLog; set => SetValue(ref mainLog, value); }


        private static void AddMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as LoggerUC;
            dp.AddMessage();

          
        }


        private void AddMessage()
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DateTime dateTime = DateTime.Now;

            string str = $"{dateTime.ToString("G")} :{  dateTime.Millisecond}   {Message} \r\n";
            string path = $"{systemPath}\\AutoFocusMachine";
            if (!Directory.Exists(path)) ; Directory.CreateDirectory(path);


            File.AppendAllText($"{path}\\Log.txt", str);
            //  File.AppendAllText(path, $"{dateTime.ToString("G")}{message}");
            MainLog += str;

            TextBoxLog.ScrollToEnd();

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
