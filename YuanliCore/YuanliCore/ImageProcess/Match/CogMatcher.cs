using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Match
{
    /// <summary>
    /// Cognex 的樣本搜尋器
    /// </summary>
    public class CogMatcher : CogMethod, IMatcher
    {
        private CogPMAlignTool alignTool;
        private CogMatchWindow cogMatchWindow;

        public CogMatcher()
        {
            alignTool = new CogPMAlignTool();

        }

        public override CogParameter RunParams { get; set; } = new PatmaxParams(0);
        public MatchResult[] MatchResults { get; internal set; }

        public override void Dispose()
        {
            if (cogMatchWindow != null)
                cogMatchWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            //  if (cogMatchWindow == null)
            cogMatchWindow = new CogMatchWindow(image);

          var param=  (PatmaxParams)RunParams;
            cogMatchWindow.PatmaxParam = param;
            cogMatchWindow.ShowDialog();


            PatmaxParams patmaxparams = cogMatchWindow.PatmaxParam;


            var sampleImage = cogMatchWindow.GetPatternImage();

            param = patmaxparams;
            param.PatternImage = sampleImage;


            Dispose();
        }

        public IEnumerable<MatchResult> Find(Frame<byte[]> image)
        {

            ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();

            return Find(cogImg1);


        }
        public LocateResult FindCogLocate(Frame<byte[]> image)
        {

            ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            var param = (PatmaxParams)RunParams;
            alignTool.InputImage = cogImg1;
            alignTool.Pattern = param.Pattern;
            alignTool.RunParams = param.RunParams;
            alignTool.SearchRegion = param.SearchRegion;
            alignTool.Run();
            if (alignTool.Results.Count == 0) throw new Exception("Locate Fail");
            CogTransform2DLinear linear = alignTool.Results[0].GetPose();

            return new LocateResult { CogTransform = linear };
        }

        private IEnumerable<MatchResult> Find(ICogImage cogImage)
        {
            var param = (PatmaxParams)RunParams;
            alignTool.InputImage = cogImage;
            alignTool.Pattern = param.Pattern;
            alignTool.RunParams = param.RunParams;
            alignTool.SearchRegion = param.SearchRegion;
            alignTool.Run();

            List<MatchResult> matchings = new List<MatchResult>();

            for (int i = 0; i < alignTool.Results.Count; i++) {
                var pose = alignTool.Results[i].GetPose();

                double x = pose.TranslationX;
                double y = pose.TranslationY;
                double r = pose.Rotation;
                double s = alignTool.Results[i].Score;

                matchings.Add(new MatchResult(x, y, r, s));
            }

            return matchings;
        }
        public override void Run()
        {
            MatchResults = Find(CogImage).ToArray();
        }
    }

    public class LocateResult
    {
        /// <summary>
        /// VisitionPro 定位轉換Transform
        /// </summary>
        public CogTransform2DLinear CogTransform { get; set; }

    }
}
