using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.ImageProcess;
using YuanliCore.ImageProcess.Match;

namespace YuanliCore.Interface
{
    public interface IMatcher
    {
        CogParameter RunParams { get; set; }
        IEnumerable<MatchResult> Find(Frame<byte[]> image);

        void EditParameter(BitmapSource image);


    }

    

    public class MatchResult
    {
        public MatchResult(double x, double y, double angle, double score)
        {

            Center = new Point(x, y);
            Angle = angle;
            Score = score;


        }


        public Point Center { get; }

        public double Angle { get; }
        public double Score { get; }



    }
}
