using Cognex.VisionPro;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Caliper
{
    /// <summary>
    /// Cognex 的卡尺功能 (查找在尺內的一點 或兩點)
    /// </summary>
    public class CogEllipseCaliper : CogMethod
    {
        private CogFindEllipseTool EllipsecaliperTool;
        private CogEllipseCaliperWindow cogCaliperWindow;

        public CogEllipseCaliper()
        {

            EllipsecaliperTool = new CogFindEllipseTool();
            RunParams = new FindEllipseParam(0);
        }
        public CogEllipseCaliper(CogParameter caliperParams)
        {

            EllipsecaliperTool = new CogFindEllipseTool();
            RunParams = caliperParams;
        }

        public override CogParameter RunParams { get; set; }
        public EllipseCaliperResult CaliperResults { get; private set; }
        public override void Dispose()
        {
            if (cogCaliperWindow != null)
                cogCaliperWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            if (image == null) throw new Exception("Image is null");
            cogCaliperWindow = new CogEllipseCaliperWindow(image);


            cogCaliperWindow.CaliperParam = (FindEllipseParam)RunParams;
            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }
        public void EditParameter(System.Drawing.Bitmap image)
        {
            if (image == null) throw new Exception("Image is null");
            BitmapSource bmp = image.ToBitmapSource();
            EditParameter(bmp);


        }
        public void CogEditParameter()
        {
            if (CogFixtureImage == null) throw new Exception("locate is not yet complete");

            cogCaliperWindow = new CogEllipseCaliperWindow(CogFixtureImage);


            cogCaliperWindow.CaliperParam = (FindEllipseParam)RunParams;
          cogCaliperWindow.CaliperParam.RunParams.ExpectedEllipticalArc.SelectedSpaceName = "@\\Fixture";
         


            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }


        public EllipseCaliperResult Find(Frame<byte[]> image)
        {

            ICogImage cogImg1 = image.ColorFrameToCogImage(out ICogImage inputImage ,0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            return Find(cogImg1);


        }
        private EllipseCaliperResult Find(ICogImage cogImage)
        {
            EllipsecaliperTool.InputImage = (CogImage8Grey)cogImage;
            var param = (FindEllipseParam)RunParams;
            EllipsecaliperTool.RunParams = param.RunParams;
            //   caliperTool.Region = param.Region;
            EllipsecaliperTool.Run();

            List<EllipseCaliperResult> results = new List<EllipseCaliperResult>();
       

            MethodResult = new EllipseCaliperResult( );
            return new EllipseCaliperResult();
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
