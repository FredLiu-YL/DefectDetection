using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YuanliCore.Interface
{
    public interface IBlobDetector
    {
        IEnumerable<BlobDetectorResult> Run(Frame<byte[]> image);

        void EditParameter(BitmapSource image);

    }

    public struct BlobDetectorResult
    {

        public Point[] Points { get; set; }

    }
}
