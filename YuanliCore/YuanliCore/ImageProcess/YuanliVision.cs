using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.ImageProcess.Blob;
using YuanliCore.ImageProcess.Caliper;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public class YuanliVision
    {
        CogFixtureLocate cogFixtureLocate = new CogFixtureLocate();
        private CogMethod[] cogMethods;
        private CombineOptionOutput[] combineOptionOutputs;
        public YuanliVision()
        {


        }



        public async Task<VisionResult[]> Run(Frame<byte[]> frame, CogMatcher cogMatcher, IEnumerable<CogMethod> cogMethods, IEnumerable<CombineOptionOutput> combineOutputs)
        {
            int tid1 = System.Threading.Thread.CurrentThread.ManagedThreadId;
            CogMethod[] methods = cogMethods.ToArray();
            List<VisionResult> visionResultList = new List<VisionResult>();

            await Task.Run(() =>
             {
                 LocateResult cogLocateResult = cogMatcher.FindCogLocate(frame);
                
                 var cogImg =cogFixtureLocate.RunFixture(frame, cogLocateResult.CogTransform);
                 int tid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                 
                 foreach (var item in methods)
                 {
                     item.CogImage = cogImg;
                     item.Run();
                 }
                    


                 foreach (var option in combineOutputs) {
                     VisionResult visionResult = new VisionResult();
                     switch (option.Option) {
                         case OutputOption.Result:
                             CogMethod resultMethod = methods[Convert.ToInt32(option.SN1) - 1];
                             visionResult = GetMethodFull(resultMethod);
                             break;
                         case OutputOption.Distance:
                             CogMethod distanceMethod1 = methods[Convert.ToInt32(option.SN1) - 1];
                             CogMethod distanceMethod2 = methods[Convert.ToInt32(option.SN2) - 1];

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

    }
}
