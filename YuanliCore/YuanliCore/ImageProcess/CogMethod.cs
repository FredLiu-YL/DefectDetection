using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public abstract class CogMethod 
    {
        public ICogImage CogImage { get; set; }
        public MethodName MethodName { get=> RunParams.Methodname; set { RunParams.Methodname = value; } }

        public abstract void Dispose();

        public abstract void EditParameter(BitmapSource image);

        public abstract CogParameter RunParams { get; set; }

    public abstract void Run();
    }
}
