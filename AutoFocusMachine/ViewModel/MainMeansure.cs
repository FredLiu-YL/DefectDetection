using AutoFocusMachine.Model.Recipe;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using YuanliCore.AffineTransform;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;
using YuanliCore.CameraLib;
using Cognex.VisionPro;
using Cognex.VisionPro.ToolGroup;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.QuickBuild.Implementation.Internal;
using Cognex.VisionPro.ToolBlock;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.Caliper;
using YuanliCore.Views.CanvasShapes;
using Cognex.VisionPro.CalibFix;
using YuanliCore.ImageProcess;
using Cognex.VisionPro.Blob;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private CogMatchWindow cogMatchWindow;
        private CogMatcher cogMatcher = new CogMatcher();

        private BitmapSource sampleImage;



        public BitmapSource SampleImage { get => sampleImage; set => SetValue(ref sampleImage, value); }
        public ICommand TestCommand => new RelayCommand(async () =>
        {
            var result = cogMatcher.Find();
        });

        public ICommand TestMeansureCommand => new RelayCommand(() =>
        {

            //讀取job檔
            CogJob myjob = (CogJob)CogSerializer.LoadObjectFromFile("D:\\Fred\\Line_Finder0104.vpp");

            CogToolGroup myTG = myjob.VisionTool as CogToolGroup;


            BitmapSource img = MainImage.FormatConvertTo(System.Windows.Media.PixelFormats.Bgr24);
            Frame<byte[]> frametemp = img.ToByteFrame();

            ICogImage cogImg = frametemp.ColorFrameToCogImage(0.333, 0.333, 0.333);



            CogPMAlignTool pmTool = myTG.Tools["CogPMAlignTool1"] as CogPMAlignTool;
            Bitmap bip = pmTool.Pattern.GetTrainedPatternImage().ToBitmap();
            SampleImage = bip.ToBitmapSource();


            CogFindLineTool findLineA = myTG.Tools["CogFindLineToolA"] as CogFindLineTool;
            CogFindLineTool findLineB = myTG.Tools["CogFindLineToolB"] as CogFindLineTool;
            CogBlobTool cogBlobTool = myTG.Tools["CogBlobTool1"] as CogBlobTool;
            CogProcess cogProcess = new CogProcess(pmTool.Pattern, pmTool.RunParams);
            cogProcess.Run(frametemp);
            var resultLine = cogProcess.RunMeansure(findLineA.RunParams, findLineB.RunParams);
            cogProcess.RunInsp(cogBlobTool.RunParams , cogBlobTool.Region);

            //     CogDistanceSegmentSegmentTool findLine1 = myTG.Tools["CogDistanceSegmentSegmentTool1"] as CogDistanceSegmentSegmentTool;
            var aa = new ROILine { X1 = resultLine.lineA.StartX, Y1 = resultLine.lineA.StartY, X2 = resultLine.lineA.EndX, Y2 = resultLine.lineA.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
            var ab = new ROILine { X1 = resultLine.lineB.StartX, Y1 = resultLine.lineB.StartY, X2 = resultLine.lineB.EndX, Y2 = resultLine.lineB.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
   
            AddShapeAction.Execute(aa);
            AddShapeAction.Execute(ab);


            cogProcess.Dispose();
            myjob.Shutdown();
        });
        public ICommand TestControlCommand => new RelayCommand(() =>
        {
            //創建 Cog 管理員
            CogJobManager myJobManager;
            myJobManager = new CogJobManager();
            //讀取job檔
            CogJob myjob = (CogJob)CogSerializer.LoadObjectFromFile("D:\\Fred\\Line_Finder1230.vpp");
            myJobManager.JobAdd(myjob);
            var myTG = myjob.VisionTool as CogToolGroup;


            BitmapSource img = MainImage.FormatConvertTo(System.Windows.Media.PixelFormats.Bgr24);
            Frame<byte[]> frametemp = img.ToByteFrame();

            ICogImage cogImg = frametemp.ColorFrameToCogImage(0.333, 0.333, 0.333);


            // CogInputImageTool imageTool = myTG.Tools["Image Source"] as CogInputImageTool;         
            // imageTool.InputImage = cogImg;
            //  imageTool.Run();

            var aa1 = myJobManager.StateFlags;
            var ab1 = myJobManager.JobsRunningState;

            CogPMAlignTool pmTool = myTG.Tools["CogPMAlignTool1"] as CogPMAlignTool;
            Bitmap bip = pmTool.Pattern.GetTrainedPatternImage().ToBitmap();


            pmTool.InputImage = cogImg;
            var pose = pmTool.Results[0].GetPose();



            CogFindLineTool findLineA = myTG.Tools["CogFindLineToolA"] as CogFindLineTool;
            CogFindLineTool findLineB = myTG.Tools["CogFindLineToolB"] as CogFindLineTool;

            CogProcess cogProcess = new CogProcess(pmTool.Pattern, pmTool.RunParams);
            cogProcess.Run(frametemp);
            var resultLine = cogProcess.RunMeansure(findLineA.RunParams, findLineB.RunParams);
       //     cogProcess.RunInsp(cogBlobTool.RunParams);

            myJobManager.Run();

            CogLineSegment lineA = findLineA.Results.GetLineSegment();
            CogLineSegment lineB = findLineB.Results.GetLineSegment();



            //     CogDistanceSegmentSegmentTool findLine1 = myTG.Tools["CogDistanceSegmentSegmentTool1"] as CogDistanceSegmentSegmentTool;
            var aa = new ROILine { X1 = resultLine.lineA.StartX, Y1 = resultLine.lineA.StartY, X2 = resultLine.lineA.EndX, Y2 = resultLine.lineA.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
            var ab = new ROILine { X1 = resultLine.lineB.StartX, Y1 = resultLine.lineB.StartY, X2 = resultLine.lineB.EndX, Y2 = resultLine.lineB.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
            //   var aa = new ROILine { X1= lineA.StartX, Y1= lineA.StartY, X2= lineA.EndX, Y2 = lineA.EndY,   IsMoveEnabled = false, CenterCrossLength = 4 };
            //   var ab = new ROILine { X1 = lineB.StartX, Y1 = lineB.StartY, X2 = lineB.EndX, Y2 = lineB.EndY,   IsMoveEnabled = false, CenterCrossLength = 4 };
            AddShapeAction.Execute(aa);
            AddShapeAction.Execute(ab);
            SampleImage = bip.ToBitmapSource();

            cogProcess.Dispose();

        });
        public ICommand EditSampleCommand => new RelayCommand(async () =>
        {
            if (cogMatchWindow == null)
                cogMatchWindow = new CogMatchWindow(MainImage);



            cogMatchWindow.PatmaxParam = new PatmaxParams();



            cogMatchWindow.ShowDialog();


            cogMatcher.Patmaxparams = cogMatchWindow.PatmaxParam;
            mainRecipe.PMParams = cogMatcher.Patmaxparams;

            var aa = (CogRectangle)cogMatchWindow.PatmaxParam.Pattern.TrainRegion;

            var ab = cogMatchWindow.PatmaxParam.Pattern.Origin;

            Bitmap bip = cogMatchWindow.PatmaxParam.Pattern.GetTrainedPatternImage().ToBitmap();


            SampleImage = bip.ToBitmapSource();
            //  cogMatchWindow.Close();


        });


    }

}
