using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using Cognex.VisionPro.ImageFile;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using YuanliCore.CameraLib;
using YuanliCore.ImageProcess.Blob;
using YuanliCore.ImageProcess.Caliper;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public class YuanliVision
    {
        private ICogImage cogLocatedImg;
        private CogTransform2DLinear cogTransform;
        private CogFixtureLocate cogFixtureLocate = new CogFixtureLocate();

        private CombineOptionOutput[] combineOptionOutputs;



        public YuanliVision()
        {


        }
        public bool IsRunning { get; set; }
        public event Action<ICogRecord> CreateImage;
        public CogMatcher LocateMatcher = new CogMatcher();
        /// <summary>
        /// 演算法列表
        /// </summary>
        public List<CogMethod> CogMethods { get; set; } = new List<CogMethod>();

        public BitmapSource ReadImage(string fileName)
        {
            CogImageFileTool cog;
            CogImageFile cogImage = new CogImageFile();
            cogImage.Open(fileName, CogImageFileModeConstants.Read);
            CogImage8Grey img = (CogImage8Grey)cogImage[0];

            ICogImage8PixelMemory m = img.Get8GreyPixelMemory(CogImageDataModeConstants.ReadWrite, 0, 0, img.Width, img.Height);
            var bmps = BitmapSource.Create(img.Width, img.Height, 8, 8, PixelFormats.Gray8, BitmapPalettes.Gray256, m.Scan0, (img.Width * img.Height), m.Stride);

            return bmps;
        }


        public async Task<VisionResult[]> Run(Frame<byte[]> frame, PatmaxParams locateParams, IEnumerable<CogParameter> cogParameters, IEnumerable<CombineOptionOutput> combineOutputs, double pixelSize)
        {

            try {
                if (IsRunning) throw new Exception("Process is Running");

                IsRunning = true;
                List<VisionResult> visionResultList = new List<VisionResult>();
                //釋放資源 Cog元件實體化以後  不釋放會無法正常關閉程式
                foreach (var method in CogMethods) {
                    method.Dispose();
                }
                CogMethods.Clear();

                LocateMatcher.RunParams = locateParams; //創建 定位功能
                CogMethods = SetMethodParams(cogParameters).ToList(); //創建演算法列表

                int tid1 = System.Threading.Thread.CurrentThread.ManagedThreadId;

                await Task.Run(() =>
                 {
                     LocateResult cogLocateResult = LocateMatcher.FindCogLocate(frame);
                     if (cogLocateResult == null) return;
                     Cognex.VisionPro.ICogImage cogImg = cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
                     int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;

                     //將所有演算法跑過一遍 得出結果
                     foreach (var item in CogMethods) {
                         item.CogFixtureImage = cogImg;
                         item.Run();
                     }


                     //依照 選擇輸出模式  回傳結果
                     foreach (CombineOptionOutput option in combineOutputs) {
                         VisionResult visionResult = new VisionResult();
                         switch (option.Option) {
                             case OutputOption.None:
                                 CogMethod resultMethod = CogMethods[Convert.ToInt32(option.SN1) - 1];
                                 visionResult = GetMethodFull(resultMethod, pixelSize, option.ThresholdMin, option.ThresholdMax);
                                 break;
                             case OutputOption.Distance:
                                 //  CogMethod distanceMethod1 = CogMethods[Convert.ToInt32(option.SN1) - 1];
                                 //  CogMethod distanceMethod2 = CogMethods[Convert.ToInt32(option.SN2) - 1];
                                 var distanceMethod1 = CogMethods[Convert.ToInt32(option.SN1) - 1].MethodResult;
                                 var distanceMethod2 = CogMethods[Convert.ToInt32(option.SN2) - 1].MethodResult;
                                 //  visionResult = GetDistance(distanceMethod1, distanceMethod2, option.ThresholdMin, option.ThresholdMax);
                                 Vector disVec = distanceMethod1.CenterPoint - distanceMethod2.CenterPoint;
                                 double dis = disVec.Length * pixelSize;
                                 bool judge = (dis >= option.ThresholdMin && dis <= option.ThresholdMax) ? true : false;

                                 visionResult = new VisionResult
                                 {
                                     BeginPoint = distanceMethod1.CenterPoint,
                                     EndPoint = distanceMethod2.CenterPoint,
                                     ResultOutput = option.Option,
                                     Distance = dis,
                                     Judge = judge
                                 };
                                 break;
                             case OutputOption.Angle:
                                 var angleMethod1 = CogMethods[Convert.ToInt32(option.SN1) - 1].MethodResult;
                                 var angleMethod2 = CogMethods[Convert.ToInt32(option.SN2) - 1].MethodResult;
                                 Vector v1 = angleMethod1.EndPoint - angleMethod1.BeginPoint;
                                 Vector v2 = angleMethod2.EndPoint - angleMethod2.BeginPoint;
                                 double angle = Vector.AngleBetween(v1, v2);
                                 bool judge1 = (angle >= option.ThresholdMin && angle <= option.ThresholdMax) ? true : false;
                                 visionResult = new VisionResult
                                 {
                                     ResultOutput = option.Option,
                                     Angle = angle,
                                     Judge = judge1
                                 };
                                 break;
                             default:
                                 break;
                         }


                         visionResultList.Add(visionResult);
                     }


                 });
                return visionResultList.ToArray();
            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

                IsRunning = false; ;
            }




        }
        public async Task<DetectionResult> DetectionRun(Frame<byte[]> frame, PatmaxParams locateParams, IEnumerable<CogParameter> cogParameters)
        {

            try {
                if (IsRunning) throw new Exception("Process is Running");

                IsRunning = true;
                List<VisionResult> visionResultList = new List<VisionResult>();
                //釋放資源 Cog元件實體化以後  不釋放會無法正常關閉程式
                foreach (var method in CogMethods) {
                    method.Dispose();
                }
                CogMethods.Clear();

                LocateMatcher.RunParams = locateParams; //創建 定位功能
                CogMethods = SetMethodParams(cogParameters).ToList(); //創建演算法列表

                int tid1 = System.Threading.Thread.CurrentThread.ManagedThreadId;
                List<BlobDetectorResult> detectorResults = new List<BlobDetectorResult>();
                DetectionResult detectionResult = new DetectionResult();
                await Task.Run(() =>
                {
                    LocateResult cogLocateResult = LocateMatcher.FindCogLocate(frame);
                    if (cogLocateResult == null) {
                        //對位失敗 另外處理  圖片與 結果                      
                        detectionResult.CogRecord = LocateMatcher.Record;
                        detectorResults.Add(new BlobDetectorResult(new Point(999,999),9999,9999));
                        return;
                    } 
            //        Cognex.VisionPro.ICogImage cogImg = cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
                    int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;

                    //將所有演算法跑過一遍 得出結果
                    foreach (CogMethod item in CogMethods) {
                        CogPatInspect detector = item as CogPatInspect;  //現階段  沒有其他檢測方法  暫時先都轉型成CogPatInspect，方便傳入數值
                        detector.CogFixtureImage = cogLocateResult.LocateCogImg;
                        detector.CogTransform = cogLocateResult.CogTransform;
                        detector.Run();
                

                        // var judgeDetect =  detector.DetectorResults.Where(r=>r.Diameter > blobparm.JudgeMin);
                        foreach (var Detect in detector.DetectorResults) {
                            if (Detect.Diameter >= detector.RunParams.JudgeMin)
                                Detect.Judge = false; 
                            else
                                Detect.Judge = true;
                        }

                        detectorResults.AddRange(detector.DetectorResults);

                        if (detectionResult.CogRecord == null)//如果是第一次演算  新增record
                            detectionResult.CogRecord = detector.Record;
                        else if (detectionResult.CogRecord != null && detector.Record != null)
                            detectionResult.CogRecord.SubRecords.Add(detector.Record);  //第二次以後 將record相加
                    }

                 
                 
                    // Testc(detectionResult.CogRecord);
                    CreateImage?.Invoke(detectionResult.CogRecord);
                });

                var image = CreateBmp(detectionResult.CogRecord, frame.Width, frame.Height);
                detectionResult.RecordImage = image;
                detectionResult.BlobDetectorResults = detectorResults.ToArray();

                return detectionResult;
            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

                IsRunning = false; ;
            }


        }




        private BitmapSource CreateBmp(ICogRecord cogRecord, int width, int height)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            BitmapSource bmps = null;
      //      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
      //      {

                CogRecordsDisplay cogDisplayers = new CogRecordsDisplay();
                cogDisplayers.Size = new System.Drawing.Size(width, height);
                cogDisplayers.Subject = cogRecord;

                var bmp = cogDisplayers.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);

                bmps = bmp.ToBitmapSource();
                // bmp.ToBitmapSource().Save("D:\\qaswed.bmp");

                // CogRecordDisplay cogDisplayer = new CogRecordDisplay();
                // cogDisplayer.Record = cogRecord;
    //        }));

            return bmps;

        }


        private VisionResult GetDistance(CogMethod cogMethod1, CogMethod cogMethod2, double thresholdMin, double thresholdMax)
        {
            VisionResult visionResults = new VisionResult();

            Point center1 = GetMethodCenter(cogMethod1);
            Point center2 = GetMethodCenter(cogMethod2);

            Vector vector = center1 - center2;
            visionResults.ResultOutput = OutputOption.Distance;
            visionResults.Distance = vector.Length;
            return visionResults;
        }

        private (Point begin, Point end) GetMethodLine(CogMethod cogMethod)
        {

            if (cogMethod is CogGapCaliper) {
                CogGapCaliper cogGapCaliper = cogMethod as CogGapCaliper;
                var gapbegin = cogGapCaliper.CaliperResults.BeginPoint;
                var gapend = cogGapCaliper.CaliperResults.BeginPoint;
                return (gapbegin, gapend);

            }
            else if (cogMethod is CogLineCaliper) {
                CogLineCaliper cogLineCaliper = cogMethod as CogLineCaliper;
                throw new Exception("Get Center Fail");
            }
            else {
                throw new Exception("Get Center Fail");
            }

        }
        private Point GetMethodCenter(CogMethod cogMethod)
        {


            if (cogMethod is CogMatcher) {
                CogMatcher cogMatcher = cogMethod as CogMatcher;
                return cogMatcher.MatchResults.First().Center;
            }
            else if (cogMethod is CogBlobDetector) {
                CogBlobDetector cogBlobDetector = cogMethod as CogBlobDetector;
                return cogBlobDetector.DetectorResults.First().CenterPoint;
            }
            else if (cogMethod is CogGapCaliper) {
                CogGapCaliper cogGapCaliper = cogMethod as CogGapCaliper;
                return cogGapCaliper.CaliperResults.CenterPoint;

            }
            else if (cogMethod is CogLineCaliper) {
                CogLineCaliper cogLineCaliper = cogMethod as CogLineCaliper;
                throw new Exception("Get Center Fail");
            }
            else {
                throw new Exception("Get Center Fail");
            }

        }

        /// <summary>
        /// 將方法實體化 存到對應結果
        /// </summary>
        /// <param name="cogMethod"></param>
        /// <returns></returns>
        private VisionResult GetMethodFull(CogMethod cogMethod, double pixelSize, double thresholdMin, double thresholdMax)
        {
            VisionResult visionResults = new VisionResult();
            visionResults.ResultOutput = OutputOption.None;
            if (cogMethod is CogMatcher) {
                CogMatcher cogMatcher = cogMethod as CogMatcher;
                visionResults.MatchResult = cogMatcher.MatchResults;
            }
            else if (cogMethod is CogBlobDetector) {
                CogBlobDetector cogBlobDetector = cogMethod as CogBlobDetector;
                visionResults.BlobResult = cogBlobDetector.DetectorResults;
            }
            else if (cogMethod is CogGapCaliper) {
                CogGapCaliper cogGapCaliper = cogMethod as CogGapCaliper;

                visionResults.CaliperResult = cogGapCaliper.CaliperResults;

                double dis = cogGapCaliper.CaliperResults.Distance * pixelSize;// 乘以 比例
                visionResults.Distance = dis;
                if (dis >= thresholdMin && dis <= thresholdMax)
                    visionResults.Judge = true;
            }
            else if (cogMethod is CogLineCaliper) {
                CogLineCaliper cogLineCaliper = cogMethod as CogLineCaliper;
                visionResults.LineResult = cogLineCaliper.CaliperResults;
                double dis = cogLineCaliper.CaliperResults.Distance * pixelSize;// 乘以 比例
                visionResults.Distance = dis;
                if (dis >= thresholdMin && dis <= thresholdMax)
                    visionResults.Judge = true;
            }
            return visionResults;
        }

        /// <summary>
        /// 對圖像定位後生成Visition 座標系統
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <param name="locateMatcher"></param>
        public void ImportGoldenImage(BitmapSource bitmapSource, CogMatcher locateMatcher)
        {
            try {
                Frame<byte[]> frame;

                if (bitmapSource.Format == PixelFormats.Indexed8 || bitmapSource.Format == PixelFormats.Gray8) {
                    frame = bitmapSource.ToByteFrame();
                }
                else {
                    var b = bitmapSource.FormatConvertTo(PixelFormats.Bgr24);
                    frame = b.ToByteFrame();
                }

                LocateResult cogLocateResult = locateMatcher.FindCogLocate(frame);
                if (cogLocateResult == null) throw new Exception("Locate Fail");
                //定位後資訊都在圖片裡 ，  直接拿去後面方法使用就自動帶入affine transform
                cogLocatedImg = cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
                cogTransform = cogLocateResult.CogTransform;
            }
            catch (Exception ex) {

                throw ex;
            }
        }

        /// <summary>
        /// 經過定位過的演算法
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public CogMethod GetLocatedMethod(int index)
        {
            try {
                CogMethods[index].CogFixtureImage = cogLocatedImg;
                CogMethods[index].CogTransform = cogTransform;
                return CogMethods[index];
            }
            catch (Exception ex) {

                throw ex;
            }

        }




        private IEnumerable<CogMethod> SetMethodParams(IEnumerable<CogParameter> cogParameters)
        {

            List<CogMethod> cogMethods = new List<CogMethod> { };

            int i = 0;
            foreach (CogParameter item in cogParameters) {
                //        item = CogParameter.Load("123-1", 0);
                switch (item.Methodname) {
                    case MethodName.PatternMatch:
                        cogMethods.Add(new CogMatcher(item));
                        break;
                    case MethodName.GapMeansure:
                        cogMethods.Add(new CogGapCaliper(item));
                        break;
                    case MethodName.LineMeansure:
                        cogMethods.Add(new CogLineCaliper(item));
                        break;
                    case MethodName.CircleMeansure:
                        break;
                    case MethodName.BlobDetector:
                        cogMethods.Add(new CogBlobDetector(item));
                        break;
                    case MethodName.PatternComparison:
                        cogMethods.Add(new CogPatInspect(item));
                        break;

                        
                    default:
                        break;
                }
                i++;

            }


            return cogMethods;
        }
    }
}
