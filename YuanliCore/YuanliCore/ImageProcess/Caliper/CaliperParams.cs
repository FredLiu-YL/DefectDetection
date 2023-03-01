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
    public class CaliperParams : CogParameter
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
        /// 卡尺的位置與範圍
        /// </summary>
        public CogRectangleAffine Region { get; set; }
        /// <summary>
        /// 搜尋參數
        /// </summary>
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
