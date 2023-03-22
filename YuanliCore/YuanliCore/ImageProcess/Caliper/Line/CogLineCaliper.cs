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
    public class CogLineCaliper : CogMethod
    {
        private CogFindLineTool linecaliperTool;
        private CogLineCaliperWindow cogCaliperWindow;

        public CogLineCaliper()
        {

            linecaliperTool = new CogFindLineTool();
            RunParams = new FindLineParam(0);
        }
        public CogLineCaliper(CogParameter caliperParams)
        {

            linecaliperTool = new CogFindLineTool();
            RunParams = caliperParams;
        }

        public override CogParameter RunParams { get; set; }
        public CaliperResult[] CaliperResults { get; private set; }
        public override void Dispose()
        {
            if (cogCaliperWindow != null)
                cogCaliperWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            if (image == null) throw new Exception("Image is null");
            cogCaliperWindow = new CogLineCaliperWindow(image);


            cogCaliperWindow.CaliperParam = (FindLineParam)RunParams;
            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }
        public void CogEditParameter()
        {
            if (CogFixtureImage == null) throw new Exception("locate is not yet complete");

            cogCaliperWindow = new CogLineCaliperWindow(CogFixtureImage);


            cogCaliperWindow.CaliperParam = (FindLineParam)RunParams;
            cogCaliperWindow.CaliperParam.RunParams.ExpectedLineSegment.SelectedSpaceName = "@\\Fixture";
          //  cogCaliperWindow.CaliperParam.Region.SelectedSpaceName = "@\\Fixture";


            cogCaliperWindow.ShowDialog();


            RunParams = cogCaliperWindow.CaliperParam;


            Dispose();
        }


        public IEnumerable<CaliperResult> Find(Frame<byte[]> image)
        {

            ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            return Find(cogImg1);


        }
        private IEnumerable<CaliperResult> Find(ICogImage cogImage)
        {
            linecaliperTool.InputImage = (CogImage8Grey)cogImage;
            var param = (FindLineParam)RunParams;
            linecaliperTool.RunParams = param.RunParams;
         //   caliperTool.Region = param.Region;
            linecaliperTool.Run();

            List<CaliperResult> results = new List<CaliperResult>();

            for (int i = 0; i < linecaliperTool.Results.Count; i++) 
                {
            
               /* CogCaliperEdge edge0 = linecaliperTool.Results[i].Edge0;
                CogCaliperEdge edge1 = linecaliperTool.Results[i].Edge1;

                double x1 = edge0.PositionX;
                double y1 = edge0.PositionY;
                double cX = linecaliperTool.Results[i].PositionX;
                double cY = linecaliperTool.Results[i].PositionY;
                double x2 = edge1.PositionX;
                double y2 = edge1.PositionY;
              
                results.Add(new CaliperResult(new Point(x1, y1), new Point(cX, cY), new Point(x2, y2))); 
               */
            }

            return results;
        }

        //public  void Run(Frame<byte[]> image)
        //{
        //    CaliperResults = Find(image).ToArray();
        //}

        public override void Run()
        {
            if (CogFixtureImage == null) throw new Exception("Image does not exist");
            CaliperResults = Find(CogFixtureImage).ToArray();
        }
    }

}
