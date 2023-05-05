using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.ImageProcess;

namespace YuanliCore.Interface
{
    public interface IBlobDetector
    {
        IEnumerable<BlobDetectorResult> Run(Frame<byte[]> image);

        void EditParameter(BitmapSource image);

    }
    public class DetectionResult : INotifyPropertyChanged
    {
        public BlobDetectorResult[] BlobDetectorResults { get; set; }
        public ICogRecord CogRecord { get; set; }
        public BitmapSource RecordImage { get; set; }

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


    public class BlobDetectorResult
    {
        public BlobDetectorResult(Point center, double area, double diameter)
        {
            CenterPoint = center;
            Area = area;
            Diameter = diameter;
        }
        public double Area { get; }

        public Point CenterPoint { get; set; }

        public double Diameter { get; set; }

        public bool Judge { get; set; }
    }
}
