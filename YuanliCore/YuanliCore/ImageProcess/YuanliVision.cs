using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

        private CogFixtureLocate cogFixtureLocate = new CogFixtureLocate();

        private CombineOptionOutput[] combineOptionOutputs;
        public YuanliVision()
        {


        }

        public CogMatcher LocateMatcher = new CogMatcher();
        /// <summary>
        /// 演算法列表
        /// </summary>
        public List<CogMethod> CogMethods { get; set; } = new List<CogMethod>();

        public async Task< VisionResult[]> Run(Frame<byte[]> frame, PatmaxParams locateParams, IEnumerable<CogParameter> cogParameters, IEnumerable<CombineOptionOutput> combineOutputs)
        {
            //釋放資源 Cog元件實體化以後  不釋放會無法正常關閉程式
            foreach (var method in CogMethods) {
                method.Dispose();
            }
            CogMethods.Clear();

            LocateMatcher.RunParams = locateParams;
            CogMethods = SetMethodParams(cogParameters).ToList();

            int tid1 = System.Threading.Thread.CurrentThread.ManagedThreadId;
 
            List<VisionResult> visionResultList = new List<VisionResult>();

            await Task.Run(() =>
             {
                 LocateResult cogLocateResult = LocateMatcher.FindCogLocate(frame);

                 Cognex.VisionPro.ICogImage cogImg = cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
                 int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;

                 //將所有演算法跑過一遍 得出結果
                 foreach (var item in CogMethods) {
                     item.CogFixtureImage = cogImg;
                     item.Run();
                 }


                 //依照 選擇輸出模式  回傳結果
                 foreach (var option in combineOutputs) {
                     VisionResult visionResult = new VisionResult();
                     switch (option.Option) {
                         case OutputOption.Result:
                             CogMethod resultMethod = CogMethods[Convert.ToInt32(option.SN1) - 1];
                             visionResult = GetMethodFull(resultMethod);
                             break;
                         case OutputOption.Distance:
                             CogMethod distanceMethod1 = CogMethods[Convert.ToInt32(option.SN1) - 1];
                             CogMethod distanceMethod2 = CogMethods[Convert.ToInt32(option.SN2) - 1];

                             visionResult = GetDistance(distanceMethod1, distanceMethod2);

                             break;
                         case OutputOption.Angle:

                             break;
                         default:
                             break;
                     }


                     visionResultList.Add(visionResult);
                 }


             });

            return visionResultList.ToArray();
        }

        private VisionResult GetDistance(CogMethod cogMethod1, CogMethod cogMethod2)
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
                var gapbegin = cogGapCaliper.CaliperResults.First().BeginPoint;
                var gapend = cogGapCaliper.CaliperResults.First().BeginPoint;
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
                return cogGapCaliper.CaliperResults.First().CenterPoint;

            }
            else if (cogMethod is CogLineCaliper) {
                CogLineCaliper cogLineCaliper = cogMethod as CogLineCaliper;
                throw new Exception("Get Center Fail");
            }
            else {
                throw new Exception("Get Center Fail");
            }

        }
        private VisionResult GetMethodFull(CogMethod cogMethod)
        {
            VisionResult visionResults = new VisionResult();
            visionResults.ResultOutput = OutputOption.Result;
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

            }
            else if (cogMethod is CogLineCaliper) {
                CogLineCaliper cogLineCaliper = cogMethod as CogLineCaliper;

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
                var b = bitmapSource.FormatConvertTo(PixelFormats.Bgr24);
                Frame<byte[]> frame = b.ToByteFrame();
                LocateResult cogLocateResult = locateMatcher.FindCogLocate(frame);
                //定位後資訊都在圖片裡 ，  直接拿去後面方法使用就自動帶入affine transform
                cogLocatedImg = cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
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
                        break;
                    case MethodName.CircleMeansure:
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
