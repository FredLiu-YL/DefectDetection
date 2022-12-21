using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.CameraLib
{
    public class SimulateCamera : ICamera
    {
        private BitmapSource simpleImage;
        private Frame<byte[]> tempFrames;
        private Subject<Frame<byte[]>> frames = new Subject<Frame<byte[]>>();
        private bool freshImage;
        public SimulateCamera(string path)
        {
            BitmapImage bi = new BitmapImage();
            // BitmapImage.UriSource must be in a BeginInit/EndInit block.
            bi.BeginInit();
            bi.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
            bi.EndInit();
            // Set the image source.
            tempFrames = bi.ToByteFrame();



        }
        public int Width => tempFrames.Width;

        public int Height => tempFrames.Height;

        public IObservable<Frame<byte[]>> Frames => frames;

        public PixelFormat PixelFormat { get => tempFrames.Format; set => throw new NotImplementedException(); }

        public void Close()
        {

        }

        public IDisposable Grab()
        {
            freshImage = true;
            Task.Run(ShowImage);
            return null;
        }

        public Task<BitmapSource> GrabAsync()
        {
            return Task.Run(() => tempFrames.ToBitmapSource());
        }

        public void Open()
        {

        }

        public void Stop()
        {
            freshImage = false;
        }



        private async Task ShowImage()
        {
            while (freshImage)
            {
                frames.OnNext(tempFrames);

                await Task.Delay(300);
            }


        }
    }

}
