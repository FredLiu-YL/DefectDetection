using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess.Caliper
{
    public class CaliperParams
    {
        public CaliperParams(int id = 0)
        {
            CogCaliperTool tool = new CogCaliperTool();

            Id = id;
       
            RunParams = tool.RunParams;
            Region = tool.Region;
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }

        /// <summary>
        /// 取得或設定Patmax 的Id 預設為 = 0, 若一個料號有兩個以上的Patmax參數屬性, 請明確指定Id後再儲存
        /// </summary>
        public int Id { get; set; }

        public CogRectangleAffine Region { get; set; }
        public CogCaliper RunParams { get; set; }
        public static CaliperParams Default(int id = 0)
        {
            CogCaliperTool tool = new CogCaliperTool();
            return Default(tool, id);
        }

        internal static CaliperParams Default(CogCaliperTool tool, int id)
        {
            return new CaliperParams()
            {
              
                RunParams = tool.RunParams,
                Region = tool.Region
            };
        }
    }

}
