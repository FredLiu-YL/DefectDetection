using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro.Caliper;
using YuanliCore.Interface;
using YuanliCore.CameraLib;

namespace YuanliCore.ImageProcess
{
    public class CogLineSearch
    {
        private CogFindLineTool cogFindLineTool = new CogFindLineTool();
        private CogFindLineEditV2 cogFindLineEdit;
        public CogLineSearch()
        {




        }


        public async Task Run(Frame<byte[]> frame)
        {
            var cogImg = frame.ColorFrameToCogImage(0.333, 0.333, 0.333);
            cogFindLineTool.InputImage = (CogImage8Grey)cogImg;
          //  cogFindLineTool.RunParams.CaliperRunParams;

        }

        
    }
}
