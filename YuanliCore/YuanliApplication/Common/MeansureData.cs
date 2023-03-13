using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.ImageProcess;

namespace YuanliApplication.Common
{


    public class FinalResult
    {
        public string Number { get; set; }
        public OutputOption Output { get; set; }
        public double? Distance { get; set; }
        public double? Angle { get; set; }

        public Point? Center { get; set; }
        public double? Score { get; set; }


    }

}
