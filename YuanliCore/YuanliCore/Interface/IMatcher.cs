using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Interface
{
    public interface IMatcher
    {

        IEnumerable<MatchResult> Find();

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
