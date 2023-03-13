using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Blob
{
    public class CogBlobDetector : CogMethod
    {
        private CogBlobTool blobTool;
        private CogBlobWindow cogBlobWindow;

        public CogBlobDetector()
        {

            blobTool = new CogBlobTool();

        }
        public override CogParameter RunParams { get; set; } = new BlobParams();
        public BlobDetectorResult[] DetectorResults { get; set; }
        public override void Dispose()
        {
            if (cogBlobWindow != null)
                cogBlobWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            // if (cogCaliperWindow == null)
            cogBlobWindow = new CogBlobWindow(image);


            cogBlobWindow.BlobParam = (BlobParams)RunParams;
            cogBlobWindow.ShowDialog();


            RunParams = cogBlobWindow.BlobParam;


            Dispose();
        }

        public IEnumerable<BlobDetectorResult> Find(Frame<byte[]> image)
        {
            ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);

            return Find(cogImg1);
        }
        private IEnumerable<BlobDetectorResult> Find(ICogImage cogImage)
        {         

            blobTool.InputImage = cogImage;
           var param= RunParams as BlobParams;
            blobTool.RunParams = param.RunParams;
            blobTool.Region = param.ROI;
            blobTool.Run();

            List<BlobDetectorResult> results = new List<BlobDetectorResult>();
            var blobResults = blobTool.Results.GetBlobs();

            for (int i = 0; i < blobResults.Count; i++) {
                var pose = blobResults[i].CenterOfMassX;

                double x = blobResults[i].CenterOfMassX;
                double y = blobResults[i].CenterOfMassY;
                double area = blobResults[i].Area;


                results.Add(new BlobDetectorResult(new Point(x, y), area));
            }

            return results;
        }
        public override void Run()
        {
            DetectorResults = Find(CogImage).ToArray();
        }
    }
}
