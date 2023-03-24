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

        /// <summary>
        /// VisionPro 座標定位 資訊都會存在ICogImage內
        /// </summary>
        /// <param name="image"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ICogImage RunFixture(Frame<byte[]> image, CogTransform2DLinear linea)
        {

            ICogImage cogImg1 = null;
            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);

        //    ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
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

        /// <summary>
        /// VisionPro 座標定位 資訊都會存在ICogImage內
        /// </summary>
        /// <param name="cogImg"></param>
        /// <param name="linea"></param>
        /// <returns></returns>
        public ICogImage RunFixture(ICogImage cogImg, CogTransform2DLinear linea)
        {



           // ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);
            ICogImage fixtureImg;
            CogFixtureTool cogFixtureTool = new CogFixtureTool();

            cogFixtureTool.InputImage = cogImg;
            cogFixtureTool.RunParams.UnfixturedFromFixturedTransform = linea;
            cogFixtureTool.Run();
            fixtureImg = cogFixtureTool.OutputImage;
    //        fixtureImg.SelectedSpaceName = cogImg.SelectedSpaceName;

            cogFixtureTool.Dispose();
            return fixtureImg;


        }
    }


   
}
