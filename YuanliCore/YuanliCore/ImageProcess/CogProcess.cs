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
using System.Windows.Media.Imaging;

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
        private CogRecordsDisplay cogRecordsDisplay = new CogRecordsDisplay();
        private ICogImage cogImg;
        public CogProcess()
        {

            cogPMAlignTool = new CogPMAlignTool();


            cogFixtureTool = new CogFixtureTool();
            //  cogFixtureTool.RunParams = fixtureParam;

            cogFindLineToolA = new CogFindLineTool();
            cogFindLineToolB = new CogFindLineTool();


            cogDistanceSegmentTool = new CogDistanceSegmentSegmentTool();

            cogBlobTool = new CogBlobTool();

        }

        public CogMatchResult[] RunPatternMatch(Frame<byte[]> frame, CogPMAlignPattern pmAlignPattern, CogPMAlignRunParams pmAlignRunParams)
        {
            cogPMAlignTool.Pattern = pmAlignPattern;
            cogPMAlignTool.RunParams = pmAlignRunParams;

            ICogImage cogImg1 = frame.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            cogRecordsDisplay.Size = new System.Drawing.Size(cogImg1.Width, cogImg1.Height);
            cogPMAlignTool.InputImage = cogImg1;
            cogPMAlignTool.Run();
            List<CogMatchResult> matchings = new List<CogMatchResult>();

            for (int i = 0; i < cogPMAlignTool.Results.Count; i++) {
                var pose = cogPMAlignTool.Results[i].GetPose();

                double x = pose.TranslationX;
                double y = pose.TranslationY;
                double r = pose.Rotation;
                double s = cogPMAlignTool.Results[i].Score;

                matchings.Add(new CogMatchResult { Result = new MatchResult(x, y, r, s), Linear = pose, CogImg = cogImg1 });

            }
            return matchings.ToArray();

        }

        public (CogMatchResult[] , ICogRecord)  RunPatternMatch(ICogImage cogImg1, CogPMAlignPattern pmAlignPattern, CogPMAlignRunParams pmAlignRunParams)
        {
            cogPMAlignTool.Pattern = pmAlignPattern;
            cogPMAlignTool.RunParams = pmAlignRunParams;

            //   ICogImage cogImg1 = bitmapSource.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //         cogRecordsDisplay = new CogRecordsDisplay();
            //          cogRecordsDisplay.Size = new System.Drawing.Size(cogImg1.Width, cogImg1.Height);
            cogPMAlignTool.InputImage = cogImg1;
            cogPMAlignTool.Run();

            ICogRecord record = cogPMAlignTool.CreateLastRunRecord().SubRecords[0];
            List<CogMatchResult> matchings = new List<CogMatchResult>();

            for (int i = 0; i < cogPMAlignTool.Results.Count; i++) {
                var pose = cogPMAlignTool.Results[i].GetPose();

                double x = pose.TranslationX;
                double y = pose.TranslationY;
                double r = pose.Rotation;
                double s = cogPMAlignTool.Results[i].Score;

                matchings.Add(new CogMatchResult { Result = new MatchResult(x, y, r, s), Linear = pose, CogImg = cogImg1 });

            }
            return (matchings.ToArray() , record);
        }

        public void RunFixture(ICogImage cogImg1, CogTransform2DLinear linea)
        {

            cogFixtureTool.InputImage = cogImg1;
            cogFixtureTool.RunParams.UnfixturedFromFixturedTransform = linea;
            cogFixtureTool.Run();
            fixtureImg = cogFixtureTool.OutputImage;
            fixtureImg.SelectedSpaceName = cogImg1.SelectedSpaceName;

        }

        public (CogLineSegment lineA, CogLineSegment lineB, ICogRecord record) RunMeansure(CogFindLine findLineParamA, CogFindLine findLineParamB)
        {
            cogFindLineToolA.RunParams = findLineParamA;
            cogFindLineToolB.RunParams = findLineParamB;

            cogFindLineToolA.InputImage = (CogImage8Grey)fixtureImg;
            cogFindLineToolA.Run();
            var lineA = cogFindLineToolA.Results.GetLineSegment();

            cogFindLineToolB.InputImage = (CogImage8Grey)fixtureImg;
            cogFindLineToolB.Run();
            var lineB = cogFindLineToolB.Results.GetLineSegment();

            if (lineA == null || lineB == null) throw new Exception(" LineSegment not enough ");
            cogDistanceSegmentTool.InputImage = fixtureImg;

            cogDistanceSegmentTool.SegmentA = lineA;
            cogDistanceSegmentTool.SegmentB = lineB;
            cogDistanceSegmentTool.Run();
            var dis = cogDistanceSegmentTool.Distance;
            //    CogRecordDisplay cogRecord  = new CogRecordDisplay();


            var record = cogDistanceSegmentTool.CreateLastRunRecord();
     //       cogRecordsDisplay.Subject = record;
     //       System.Drawing.Image runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
     //       var bs = runImg.ToBitmapSource();
            //   bs.Save("D:\\ mean.bmp ");

            //   return (new Point(cogDistanceSegmentTool.SegmentAX, cogDistanceSegmentTool.SegmentAY), new Point(cogDistanceSegmentTool.SegmentBX, cogDistanceSegmentTool.SegmentBY));

            return (lineA, lineB, record.SubRecords[0]);
        }

        public (Point[] defectCenter, ICogRecord record) RunInsp(CogBlob cogBlobRunParams, ICogRegion cogRegion)
        {

            cogBlobTool.InputImage = fixtureImg;
            cogBlobTool.Region = cogRegion;
            cogBlobTool.RunParams = cogBlobRunParams;
            cogBlobTool.Run();
            var blobs = cogBlobTool.Results.GetBlobs();
     //       cogRecordsDisplay.Subject = cogBlobTool.CreateLastRunRecord();

            List<Point> points = new List<Point>();
            for (int i = 0; i < blobs.Count; i++) {
                var x = blobs[i].CenterOfMassX;
                var y = blobs[i].CenterOfMassY;
                points.Add(new Point(x, y));


            }
     //       cogRecordsDisplay.Subject = cogBlobTool.CreateLastRunRecord().SubRecords[0];
            //    System.Drawing.Image runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
            //     var bs = runImg.ToBitmapSource();

            return (points.ToArray(), cogBlobTool.CreateLastRunRecord().SubRecords[0]);

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
    public class CogMatchResult
    {
        public ICogImage CogImg { get; set; }

        public MatchResult Result { get; set; }
        public CogTransform2DLinear Linear { get; set; }
    }
}
