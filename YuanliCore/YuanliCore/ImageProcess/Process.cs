using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.Caliper;
using YuanliCore.Views.CanvasShapes;
using Cognex.VisionPro.CalibFix;
using YuanliCore.Interface;
using Cognex.VisionPro.Dimensioning;
using YuanliCore.CameraLib;
using System.Windows;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Display;

namespace YuanliCore.ImageProcess
{
    public class CogProcess
    {

        private CogPMAlignTool cogPMAlignTool;
        private CogFixtureTool cogFixtureTool;
        private CogFindLineTool cogFindLineToolA;
        private CogFindLineTool cogFindLineToolB;
        private CogDistanceSegmentSegmentTool cogDistanceSegmentTool;
        private CogBlobTool cogBlobTool;
        private ICogImage fixtureImg;
        private CogRecordsDisplay cogRecordsDisplay;

        public CogProcess(CogPMAlignPattern pmAlignPattern, CogPMAlignRunParams pmAlignRunParams)
        {

            cogPMAlignTool = new CogPMAlignTool();
            cogPMAlignTool.Pattern = pmAlignPattern;
            cogPMAlignTool.RunParams = pmAlignRunParams;

            cogFixtureTool = new CogFixtureTool();
            //  cogFixtureTool.RunParams = fixtureParam;

            cogFindLineToolA = new CogFindLineTool();
            cogFindLineToolB = new CogFindLineTool();


            cogDistanceSegmentTool = new CogDistanceSegmentSegmentTool();

            cogBlobTool = new CogBlobTool();

        }

        public void Run(Frame<byte[]> frame)
        {
            ICogImage cogImg = frame.ColorFrameToCogImage(0.333, 0.333, 0.333);
            cogRecordsDisplay = new CogRecordsDisplay();
            cogRecordsDisplay.Size = new System.Drawing.Size(cogImg.Width, cogImg.Height);
            cogPMAlignTool.InputImage = cogImg;
            cogPMAlignTool.Run();
            CogTransform2DLinear linea = cogPMAlignTool.Results[0].GetPose();

            cogFixtureTool.InputImage = cogImg;
            cogFixtureTool.RunParams.UnfixturedFromFixturedTransform = linea;
            cogFixtureTool.Run();
            fixtureImg = cogFixtureTool.OutputImage;
            fixtureImg.SelectedSpaceName = cogImg.SelectedSpaceName;
        }

        public (CogLineSegment lineA, CogLineSegment lineB) RunMeansure( CogFindLine findLineParamA, CogFindLine findLineParamB)
        {
            cogFindLineToolA.RunParams = findLineParamA;
            cogFindLineToolB.RunParams = findLineParamB;

            cogFindLineToolA.InputImage = (CogImage8Grey)fixtureImg;
            cogFindLineToolA.Run();
            var lineA = cogFindLineToolA.Results.GetLineSegment();

            cogFindLineToolB.InputImage = (CogImage8Grey)fixtureImg;
            cogFindLineToolB.Run();
            var lineB = cogFindLineToolB.Results.GetLineSegment();
          

            cogDistanceSegmentTool.InputImage = fixtureImg;

            cogDistanceSegmentTool.SegmentA = lineA;
            cogDistanceSegmentTool.SegmentB = lineB;
            cogDistanceSegmentTool.Run();
            var dis = cogDistanceSegmentTool.Distance;
        //    CogRecordDisplay cogRecord  = new CogRecordDisplay();
          
       
            cogRecordsDisplay.Subject = cogDistanceSegmentTool.CreateLastRunRecord();
 
            System.Drawing.Image runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
           var bs = runImg.ToBitmapSource();
            bs.Save("D:\\ mean.bmp ");

            //   return (new Point(cogDistanceSegmentTool.SegmentAX, cogDistanceSegmentTool.SegmentAY), new Point(cogDistanceSegmentTool.SegmentBX, cogDistanceSegmentTool.SegmentBY));
       
            return (lineA, lineB);
        }

        public void RunInsp(CogBlob cogBlobRunParams , ICogRegion cogRegion)
        {

            cogBlobTool.InputImage = fixtureImg;
            cogBlobTool.Region = cogRegion;
            cogBlobTool.RunParams = cogBlobRunParams;
            cogBlobTool.Run();
            var test = cogBlobTool.Results.GetBlobs();
            cogRecordsDisplay.Subject = cogBlobTool.CreateLastRunRecord();
            System.Drawing.Image runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
            var bs = runImg.ToBitmapSource();
            bs.Save("D:\\ blob.bmp ");
        }

        public void Dispose()
        {

            cogPMAlignTool.Dispose();
            cogFixtureTool.Dispose();
            cogFindLineToolA.Dispose();
            cogFindLineToolB.Dispose();
            cogDistanceSegmentTool.Dispose();
            cogBlobTool.Dispose();
            cogRecordsDisplay.Dispose();
        }

    }
}
