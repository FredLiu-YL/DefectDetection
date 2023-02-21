using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.ImageProcess.Match;

namespace YuanliCore.Interface
{
    public interface IMatcher
    {
        PatmaxParams Patmaxparams { get; set; }
        IEnumerable<MatchResult> Find();

         void EditParameter(BitmapSource image);


    }



    public struct MatchResult
    {
        public MatchResult(double x, double y, double angle, double score)
        {
            X = x;
            Y = y;
            Angle = angle;
            Score = score;


        }


        public double X { get;  }
        public double Y { get;  }
        public double Angle { get; }
        public double Score { get;  }


    
    }
}
