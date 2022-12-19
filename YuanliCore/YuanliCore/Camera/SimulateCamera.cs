using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.CameraLib
{
    public class SimulateCamera : ICamera
    {
        public int Width => throw new NotImplementedException();

        public int Height => throw new NotImplementedException();

        public IObservable<Frame<byte[]>> Frames => throw new NotImplementedException();

        public PixelFormat PixelFormat { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public IDisposable Grab()
        {
            throw new NotImplementedException();
        }

        public Task<BitmapSource> GrabAsync()
        {
            throw new NotImplementedException();
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }

}
