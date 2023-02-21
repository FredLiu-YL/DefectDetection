using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Match
{
    public class CogMatcher : IMatcher
    {
        private CogPMAlignTool alignTool;
        private CogMatchWindow cogMatchWindow;
        public CogMatcher()
        {
            alignTool = new CogPMAlignTool();

        }

        public PatmaxParams Patmaxparams { get; set; } =new PatmaxParams();

        public void EditParameter(BitmapSource image)
        {
            if (cogMatchWindow == null)
                cogMatchWindow = new CogMatchWindow( image);


            cogMatchWindow.PatmaxParam = Patmaxparams;
            cogMatchWindow.ShowDialog();


            PatmaxParams patmaxparams = cogMatchWindow.PatmaxParam;


            var  sampleImage = cogMatchWindow.GetPatternImage();
            if (sampleImage == null) return ;
            cogMatchWindow.Dispose();

            Patmaxparams= patmaxparams;
        }

        public IEnumerable<MatchResult> Find()
        {

            alignTool.Pattern = Patmaxparams.Pattern;
            alignTool.RunParams = Patmaxparams.RunParams;
            alignTool.SearchRegion = Patmaxparams.SearchRegion;
            alignTool.Run();

            List<MatchResult> matchings = new List<MatchResult>();

            for (int i = 0; i < alignTool.Results.Count; i++)
            {
                var pose = alignTool.Results[i].GetPose();

                double x = pose.TranslationX;
                double y = pose.TranslationY;
                double r = pose.Rotation;
                double s = alignTool.Results[i].Score;

                matchings.Add(new MatchResult(x, y, r, s));
            }

            return matchings;
        }
    }
}
