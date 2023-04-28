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
        public CogMatcher(CogParameter matcherParams)
        {

            alignTool = new CogPMAlignTool();
            RunParams = matcherParams;
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
            try {

                if (image == null) throw new Exception("Image is null");

                cogMatchWindow = new CogMatchWindow(image);

                var param = (PatmaxParams)RunParams;
                cogMatchWindow.PatmaxParam = param;
                cogMatchWindow.ShowDialog();


                PatmaxParams patmaxparams = cogMatchWindow.PatmaxParam;


                var sampleImage = cogMatchWindow.GetPatternImage();

                param = patmaxparams;
                if (sampleImage != null)
                    param.PatternImage = sampleImage.ToByteFrame();
                Dispose();

            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

            }

        }
        /// <summary>
        /// 已經定位過的影像作編輯
        /// </summary>
        public void CogEditParameter()
        {
            try {

                if (CogFixtureImage == null) throw new Exception("locate is not yet complete");

                cogMatchWindow = new CogMatchWindow(CogFixtureImage);

                PatmaxParams param = (PatmaxParams)RunParams;
                param.Pattern.TrainRegion.SelectedSpaceName = "@\\Fixture";
                cogMatchWindow.PatmaxParam = param;

                cogMatchWindow.ShowDialog();


                PatmaxParams patmaxparams = cogMatchWindow.PatmaxParam;


                var sampleImage = cogMatchWindow.GetPatternImage();

                param = patmaxparams;
                param.PatternImage = sampleImage.ToByteFrame();
                Dispose();

            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

            }

        }
        public IEnumerable<MatchResult> Find(Frame<byte[]> image)
        {
            ICogImage cogImg1 = null;
            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(out ICogImage inputImage,0.333, 0.333, 0.333);
            //   ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);

            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();

            return Find(cogImg1);


        }
        public LocateResult FindCogLocate(Frame<byte[]> image)
        {
            ICogImage cogImg1 = null;
            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(out ICogImage inputImage, 0.333, 0.333, 0.333);
            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            var param = (PatmaxParams)RunParams;
            alignTool.InputImage = cogImg1;
            alignTool.Pattern = param.Pattern;
            alignTool.RunParams = param.RunParams;
            alignTool.SearchRegion = param.SearchRegion;
            alignTool.Run();

            if (alignTool.Results.Count == 0) return null;
            CogTransform2DLinear linear = alignTool.Results[0].GetPose();

            Record = alignTool.CreateLastRunRecord().SubRecords[0];
            return new LocateResult { LocateCogImg = cogImg1, CogTransform = linear };


        }

        private IEnumerable<MatchResult> Find(ICogImage cogImage)
        {
            var param = (PatmaxParams)RunParams;
            alignTool.InputImage = cogImage;
            alignTool.Pattern = param.Pattern;
            alignTool.RunParams = param.RunParams;
            alignTool.SearchRegion = param.SearchRegion;
            alignTool.Run();
            Record = alignTool.CreateLastRunRecord().SubRecords[0];
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
            MatchResults = Find(CogFixtureImage).ToArray();
        }
    }

    public class LocateResult
    {
        /// <summary>
        /// VisitionPro 定位轉換Transform
        /// </summary>
        public CogTransform2DLinear CogTransform { get; set; }


        public ICogImage LocateCogImg { get; set; }

    }


    public class LocateException : Exception
    {

    }
}
