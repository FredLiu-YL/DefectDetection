using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess
{
    /// <summary>
    ///  相似變換  vision pro內的 影像座標系
    /// </summary>
    public class CogFixture
    {
        private CogFixtureTool cogFixtureTool;
        public CogFixture()
        {

            cogFixtureTool = new CogFixtureTool();
        }
        /// <summary>
        /// 因此方法已耦合康奈視系統 故不做多餘處理 只對其做封裝
        /// </summary>
        /// <param name="image"></param>
        /// <param name="unfixturedFromFixturedTransform"></param>
        /// <returns></returns>
        ICogImage Run(ICogImage image ,  ICogTransform2D unfixturedFromFixturedTransform)
        {
            cogFixtureTool.InputImage = image;
            cogFixtureTool.RunParams.UnfixturedFromFixturedTransform = unfixturedFromFixturedTransform;
            cogFixtureTool.Run();
            return cogFixtureTool.OutputImage;
        }
    }
}
