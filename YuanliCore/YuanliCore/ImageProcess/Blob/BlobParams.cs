using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess.Blob
{
    public class BlobParams
    {
        public BlobParams(int id = 0)
        {
            CogBlobTool tool = new CogBlobTool();

            Id = id;
       
            RunParams = tool.RunParams;
            ROI = tool.Region;
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }

        /// <summary>
        /// 取得或設定Patmax 的Id 預設為 = 0, 若一個料號有兩個以上的Patmax參數屬性, 請明確指定Id後再儲存
        /// </summary>
        public int Id { get; set; }

        public ICogRegion ROI { get; set; }
        public CogBlob RunParams { get; set; }
        public static BlobParams Default(int id = 0)
        {
            CogBlobTool tool = new CogBlobTool();
            return Default(tool, id);
        }

        internal static BlobParams Default(CogBlobTool tool, int id)
        {
            return new BlobParams()
            {
              
                RunParams = tool.RunParams,
                ROI = tool.Region
            };
        }
    }

}
