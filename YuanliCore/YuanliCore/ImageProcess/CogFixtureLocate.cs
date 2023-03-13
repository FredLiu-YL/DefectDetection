using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{

    public class CogFixtureLocate
    {
    //    private CogFixtureTool cogFixtureTool =  new CogFixtureTool();


        public ICogImage RunFixture(Frame<byte[]> image, CogTransform2DLinear linea)
        {
           
        

            ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
             ICogImage fixtureImg;
            CogFixtureTool cogFixtureTool = new CogFixtureTool();

            cogFixtureTool.InputImage = cogImg1;
            cogFixtureTool.RunParams.UnfixturedFromFixturedTransform = linea;
            cogFixtureTool.Run();
            fixtureImg = cogFixtureTool.OutputImage;
            fixtureImg.SelectedSpaceName = cogImg1.SelectedSpaceName;

            cogFixtureTool.Dispose();
            return fixtureImg;


        }
    }


   
}
