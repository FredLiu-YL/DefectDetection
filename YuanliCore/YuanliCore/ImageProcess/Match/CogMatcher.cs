using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Match
{
    public class CogMatcher : IMatcher
    {
        private CogPMAlignTool alignTool;

        public CogMatcher()
        {
            alignTool = new CogPMAlignTool();

        }

        public PatmaxParams Patmaxparams { get; set; }

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
