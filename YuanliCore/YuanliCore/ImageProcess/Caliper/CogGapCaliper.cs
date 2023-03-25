using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Caliper
{
    /// <summary>
    /// Cognex 的卡尺功能 (查找在尺內的一點 或兩點)
    /// </summary>
    public class CogGapCaliper : CogMethod
    {
        private CogCaliperTool caliperTool;
        private CogCaliperWindow cogCaliperWindow;
        private CogRecordsDisplay cogRecordsDisplay;
        public CogGapCaliper()
        {

            caliperTool = new CogCaliperTool();
            RunParams = new CaliperParams(0);
        }
        public CogGapCaliper(CogParameter caliperParams)
        {

            caliperTool = new CogCaliperTool();
            RunParams = caliperParams;
        }

        public override CogParameter RunParams { get; set; }
        public CaliperResult CaliperResults { get; private set; }
        public override void Dispose()
        {
            if (cogCaliperWindow != null)
                cogCaliperWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            if (image == null) throw new Exception("Image is null");
            cogCaliperWindow = new CogCaliperWindow(image);


            cogCaliperWindow.CaliperParam = (CaliperParams)RunParams;
            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }
        public void CogEditParameter()
        {
            if (CogFixtureImage == null) throw new Exception("locate is not yet complete");

            cogCaliperWindow = new CogCaliperWindow(CogFixtureImage);


            cogCaliperWindow.CaliperParam = (CaliperParams)RunParams;
            cogCaliperWindow.CaliperParam.Region.SelectedSpaceName = "@\\Fixture";


            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }


        public CaliperResult Find(Frame<byte[]> image)
        {
            ICogImage cogImg1 = null;

            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            return Find(cogImg1);


        }
        private CaliperResult Find(ICogImage cogImage)
        {
            caliperTool.InputImage = cogImage;
            var param = (CaliperParams)RunParams;
            caliperTool.RunParams = param.RunParams;
            caliperTool.Region = param.Region;
            caliperTool.Run();

            List<CaliperResult> results = new List<CaliperResult>();

            for (int i = 0; i < caliperTool.Results.Count; i++) {
                CogCaliperEdge edge0 = caliperTool.Results[i].Edge0;
                CogCaliperEdge edge1 = caliperTool.Results[i].Edge1;

                double x1 = edge0.PositionX;
                double y1 = edge0.PositionY;
                double cX = caliperTool.Results[i].PositionX;
                double cY = caliperTool.Results[i].PositionY;
                double x2 = edge1.PositionX;
                double y2 = edge1.PositionY;

                results.Add(new CaliperResult(new Point(x1, y1), new Point(cX, cY), new Point(x2, y2)));
            }
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
           {
               /*
               cogRecordsDisplay = new CogRecordsDisplay();
               cogRecordsDisplay.Size = new System.Drawing.Size(cogImage.Width, cogImage.Height);
               Record = caliperTool.CreateLastRunRecord();
               cogRecordsDisplay.Subject = Record;
               System.Drawing.Image runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
               var bs = runImg.ToBitmapSource();
               bs.Save("D:\\CaliperResult");
               cogRecordsDisplay.Dispose();
               */

           }));

            MethodResult = results.FirstOrDefault();
            //複製貼上錯誤 懶得改 ， 其實只會有一筆資料  ，直接改後段拿第一筆資料
            return results.FirstOrDefault();
        }

        //public  void Run(Frame<byte[]> image)
        //{
        //    CaliperResults = Find(image).ToArray();
        //}

        public override void Run()
        {
            if (CogFixtureImage == null) throw new Exception("Image does not exist");
            CaliperResults = Find(CogFixtureImage);
        }
    }

}
