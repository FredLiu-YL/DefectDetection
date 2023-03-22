using AutoFocusMachine.Model.Recipe;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using AutoFocusMachine.Model;
using YuanliCore.ImageProcess.Blob;
using System.Windows.Threading;
using Cognex.VisionPro.Display;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Threading;
using YuanliCore.ImageProcess.Caliper;
using YuanliApplication.Common;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private CogMatchWindow cogMatchWindow;
        private CogBlobWindow cogBlobWindow;
        private CogCaliperWindow cogCaliperWindow;
        private PatmaxParams matchParam =  new PatmaxParams(0);
        private CogMatcher cogMatcher = new CogMatcher();
        private double lineGap, inspArea;
        private BitmapSource sampleImage;
        private CogRecordsDisplay cogRecordsDisplay = new CogRecordsDisplay();
        private ICogRecord meansurelastRecord;
        private ICogRecord inspLastRecord;
        private List<TestResult> testResults = new List<TestResult>();
        private double inspIndexX, inspIndexY;
        private ObservableCollection<TestResult> methodCollection = new ObservableCollection<TestResult>();
        private ObservableCollection<FinalResult> finalResultCollection = new ObservableCollection<FinalResult>();
        private List<CogMethod> methodList = new List<CogMethod>();
        private ObservableCollection<CombineOptionOutput> combineCollection = new ObservableCollection<CombineOptionOutput>();


        public BitmapSource SampleImage { get => sampleImage; set => SetValue(ref sampleImage, value); }
        public ICogRecord MeansureLastRecord { get => meansurelastRecord; set => SetValue(ref meansurelastRecord, value); }
        public ICogRecord InspLastRecord { get => inspLastRecord; set => SetValue(ref inspLastRecord, value); }

        public double LineGap { get => lineGap; set => SetValue(ref lineGap, value); }
        public double InspArea { get => inspArea; set => SetValue(ref inspArea, value); }
        public double InspIndexX { get => inspIndexX; set => SetValue(ref inspIndexX, value); }
        public double InspIndexY { get => inspIndexY; set => SetValue(ref inspIndexY, value); }
        public ObservableCollection<TestResult> MethodCollection { get => methodCollection; set => SetValue(ref methodCollection, value); }
        public ObservableCollection<FinalResult> FinalResultCollection { get => finalResultCollection; set => SetValue(ref finalResultCollection, value); }


        public PatmaxParams MatchParam { get => matchParam; set => SetValue(ref matchParam, value); }
        public List<CogMethod> MethodList { get => methodList; set => SetValue(ref methodList, value); }

        public ObservableCollection<CombineOptionOutput> CombineCollection { get => combineCollection; set => SetValue(ref combineCollection, value); }




        public ICommand TestCommand => new RelayCommand(async () =>
        {
            try {
/*
                var frame = MainImage.ToByteFrame();

                YuanliVision yuanliVision = new YuanliVision();
                FinalResultCollection.Clear();
                cogMatcher.RunParams = MatchParam;
                var results = await yuanliVision.Run(frame, cogMatcher , MethodList, CombineCollection);

                for (int i = 0; i < results.Length; i++) 
                {

                    switch (results[i].ResultOutput) {
                        case OutputOption.Result:

                            FinalResult[] vR = OptionResult(results[i]);
                            vR[0].Number =$"{i + 1}";// 將第一筆資料 寫入SN
                            foreach (var item in vR) {
                                FinalResultCollection.Add(item);
                            }
                         
                            break;
                        case OutputOption.Distance:
                            FinalResult result = new FinalResult();
                            result.Number = $" { i + 1}";// 將第一筆資料 寫入SN
                            result.Distance= results[i].Distance;
                            FinalResultCollection.Add(result);
                            break;
                        case OutputOption.Angle:

                            break;

                        default:
                            break;
                    }


                   

                }

                */

            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }
        });
        private FinalResult[] OptionResult(VisionResult visionResult)
        {
            List<FinalResult> finalResults = new List<FinalResult>();
            if (visionResult.MatchResult != null) {

                foreach (var item in visionResult.MatchResult) {
                    FinalResult result = new FinalResult();

                    result.Center = item.Center;
                    result.Score = item.Score;

                    finalResults.Add(result);
                }

            }
            else if (visionResult.CaliperResult != null) {
                foreach (var item in visionResult.CaliperResult) {
                    FinalResult result = new FinalResult();

                    result.Center = item.CenterPoint;


                    finalResults.Add(result);
                }
            }
            else if (visionResult.BlobResult != null) {
                foreach (var item in visionResult.BlobResult) {
                    FinalResult result = new FinalResult();

                    result.Center = item.CenterPoint;
                  //  result.Area = item.Area;

                    finalResults.Add(result);
                }

            }

            return finalResults.ToArray();

        }

        public ICommand TestMeansureCommand => new RelayCommand(() =>
        {
            try {
                //讀取job檔
                CogJob myjob = (CogJob)CogSerializer.LoadObjectFromFile("D:\\Fred\\Line_Finder0114.vpp");

                CogToolGroup myTG = myjob.VisionTool as CogToolGroup;


                BitmapSource img = MainImage.FormatConvertTo(System.Windows.Media.PixelFormats.Bgr24);
                Frame<byte[]> frametemp = img.ToByteFrame();

                // ICogImage cogImg = img.ColorFrameToCogImage(0.333, 0.333, 0.333);



                CogPMAlignTool pmTool = myTG.Tools["CogPMAlignTool1"] as CogPMAlignTool;
                System.Drawing.Bitmap bip = pmTool.Pattern.GetTrainedPatternImage().ToBitmap();
                SampleImage = bip.ToBitmapSource();


                CogFindLineTool findLineA = myTG.Tools["CogFindLineToolA"] as CogFindLineTool;
                CogFindLineTool findLineB = myTG.Tools["CogFindLineToolB"] as CogFindLineTool;
                CogBlobTool cogBlobTool = myTG.Tools["CogBlobTool1"] as CogBlobTool;
                CogProcess cogProcess = new CogProcess();
                var result = cogProcess.RunPatternMatch(frametemp, pmTool.Pattern, pmTool.RunParams);
                cogProcess.RunFixture(result[0].CogImg, result[0].Linear);
                var resultLine = cogProcess.RunMeansure(findLineA.RunParams, findLineB.RunParams);
                var tes = cogProcess.RunInsp(cogBlobTool.RunParams, cogBlobTool.Region);

                MeansureLastRecord = resultLine.record;
                InspLastRecord = tes.record;
                //     CogDistanceSegmentSegmentTool findLine1 = myTG.Tools["CogDistanceSegmentSegmentTool1"] as CogDistanceSegmentSegmentTool;
                var aa = new ROILine { X1 = resultLine.lineA.StartX, Y1 = resultLine.lineA.StartY, X2 = resultLine.lineA.EndX, Y2 = resultLine.lineA.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
                var ab = new ROILine { X1 = resultLine.lineB.StartX, Y1 = resultLine.lineB.StartY, X2 = resultLine.lineB.EndX, Y2 = resultLine.lineB.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };

                AddShapeAction.Execute(aa);
                AddShapeAction.Execute(ab);

                cogProcess.Dispose();
                myjob.Shutdown();

            }
            catch (Exception ex) {

                MessageBox.Show(ex.ToString()); ;
            }


        });
        public ICommand TestControlCommand => new RelayCommand(() =>
        {/*
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

            CogProcess cogProcess = new CogProcess();
            cogProcess.RunPatternMatch(frametemp, pmTool.Pattern, pmTool.RunParams);
            cogProcess.RunFixture();
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
            */
        });
        public ICommand EditSampleCommand => new RelayCommand(async () =>
        {
            try {
                if (cogMatchWindow == null)
                    cogMatchWindow = new CogMatchWindow(MainImage);


                cogMatchWindow.PatmaxParam = new PatmaxParams(0);


                cogMatchWindow.ShowDialog();


                cogMatcher.RunParams = cogMatchWindow.PatmaxParam;
                mainRecipe.PMParams = (PatmaxParams)cogMatcher.RunParams;

                //    var aa = (CogRectangle)cogMatchWindow.PatmaxParam.Pattern.TrainRegion;
                //    var ab = cogMatchWindow.PatmaxParam.Pattern.Origin;

                ICogImage cogbip = cogMatchWindow.PatmaxParam.Pattern.GetTrainedPatternImage();
                if (cogbip == null) return;
                System.Drawing.Bitmap bip = cogbip.ToBitmap();


                SampleImage = bip.ToBitmapSource();
                //  cogMatchWindow.Close();
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }



        });


        public ICommand EditCommand => new RelayCommand(async () =>
        {

            if (cogCaliperWindow == null) {
                cogCaliperWindow = new CogCaliperWindow(MainImage);
                cogCaliperWindow.CaliperParam = new CaliperParams(0);
            }


            cogCaliperWindow.ShowDialog();


            CaliperParams ATest = cogCaliperWindow.CaliperParam;


            //    mainRecipe.PMParams = cogMatcher.Patmaxparams;
        });

        public ICommand ReadTestCommand => new RelayCommand(async () =>
        {
            CogGapCaliper cogGapCaliper = new CogGapCaliper();
            cogGapCaliper.EditParameter(MainImage);
            MethodCollection.Add(new TestResult { SN = $"{MethodCollection.Count + 1}", Name = $"T{MethodCollection.Count + 1}", ResultName = $"R{MethodCollection.Count + 1}" });


        });
        public ICommand TestingFlowStopCommand => new RelayCommand(async () =>
        {

        });

        private void DisplayResult(CogProcessResult cogResult)
        {
            MeansureLastRecord = cogResult.MeansureRecord;
            InspLastRecord = cogResult.InspRecord;
            InspIndexX = cogResult.Index.X;
            InspIndexY = cogResult.Index.Y;
            if (cogResult.DefectArea.Length > 0)
                InspArea = cogResult.DefectArea[0];
            else
                InspArea = 0;
            LineGap = cogResult.Distance;

            for (int i = 0; i < cogResult.DefectArea.Length; i++) {

                testResults.Add(new TestResult { Index = cogResult.Index, Area = cogResult.DefectArea[i], Center = cogResult.DefectCenter[i] });

            }

            /* Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
             {

                 //     CogDistanceSegmentSegmentTool findLine1 = myTG.Tools["CogDistanceSegmentSegmentTool1"] as CogDistanceSegmentSegmentTool;
                 //            var aa = new ROILine { X1 = cogResult.LineA.StartX, Y1 = cogResult.LineA.StartY, X2 = cogResult.LineA.EndX, Y2 = cogResult.LineA.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };
                 //            var ab = new ROILine { X1 = cogResult.LineB.StartX, Y1 = cogResult.LineB.StartY, X2 = cogResult.LineB.EndX, Y2 = cogResult.LineB.EndY, Stroke = System.Windows.Media.Brushes.Red, StrokeThickness = 3, IsInteractived = false, CenterCrossLength = 4 };

                 //            AddShapeAction.Execute(aa);
                 //           AddShapeAction.Execute(ab);

             }));*/




        }



        private void SaveCogResult(ICogRecord cogRecord)
        {

            System.Drawing.Image runImg = null;
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                MeansureLastRecord = cogRecord;

                cogRecordsDisplay.Width = 2456;
                cogRecordsDisplay.Height = 2054;
                cogRecordsDisplay.Subject = cogRecord;

            }));
            runImg = cogRecordsDisplay.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);


            var bs = runImg.ToBitmapSource();
            bs.Save("E:\\ mean.bmp ");
        }
    }
    public class TestResult
    {
        public System.Windows.Point Index { get; set; }
        public double Distance { get; set; }
        public double Area { get; set; }
        public System.Windows.Point Center { get; set; }

        public string SN { get; set; }
        public string Name { get; set; }
        public string ResultName { get; set; }
    }


   
}
