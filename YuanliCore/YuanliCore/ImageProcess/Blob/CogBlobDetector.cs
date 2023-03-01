using Cognex.VisionPro.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Blob
{
    public  class CogBlobDetector: CogMethod ,IBlobDetector
    {
        private CogBlobTool caliperTool;
        private CogBlobWindow cogBlobWindow;

        public CogBlobDetector()
        {

            caliperTool = new CogBlobTool();

        }
        public BlobParams CaliperParam { get; set; } = new BlobParams();

        public override void Dispose()
        {
            if (cogBlobWindow != null)
                cogBlobWindow.Dispose();
        }

        public void EditParameter(BitmapSource image)
        {
            // if (cogCaliperWindow == null)
            cogBlobWindow = new CogBlobWindow(image);


            cogBlobWindow.BlobParam = CaliperParam;
            cogBlobWindow.ShowDialog();


            CaliperParam = cogBlobWindow.BlobParam;


            Dispose();
        }

        public IEnumerable<BlobDetectorResult> Run(Frame<byte[]> image)
        {
            throw new NotImplementedException();
        }
    }
}
