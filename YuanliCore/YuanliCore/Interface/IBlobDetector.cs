using System;
using System.Collections.Generic;
using System.Linq;
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

    public class BlobDetectorResult 
    {
        public BlobDetectorResult(Point center, double area)
        {
            CenterPoint = center;
            Area = area;
        }
        public double Area { get; }

        public Point CenterPoint { get;  set; }
    }
}
