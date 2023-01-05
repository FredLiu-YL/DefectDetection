using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YuanliCore.ImageProcess.Match
{
    public class PatmaxParams
    {
        public PatmaxParams(int id = 0)
        {
            CogPMAlignTool tool = new CogPMAlignTool();

            Id = id;
            Pattern = tool.Pattern;
            RunParams = tool.RunParams;
            SearchRegion = tool.SearchRegion;
          //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }

        /// <summary>
        /// 取得或設定Patmax 的Id 預設為 = 0, 若一個料號有兩個以上的Patmax參數屬性, 請明確指定Id後再儲存
        /// </summary>
        public int Id { get; set; }

        [JsonIgnore]
        public CogPMAlignRunParams RunParams { get; set; }
        [JsonIgnore]
        public CogPMAlignPattern Pattern { get; set; }
        [JsonIgnore]
        public ICogRegion SearchRegion { get; set; }

        /// <summary>
        /// 取得 料號設定搜尋 ROI 的範圍以 System.Windows 型別提供
        /// </summary>
        public Rect? SearchROI
        {
            get;set;
        }

        public object Tag { get; set; }

        public static PatmaxParams Default(int id = 0)
        {
            CogPMAlignTool tool = new CogPMAlignTool();
            return Default(tool, id);
        }

        internal static PatmaxParams Default(CogPMAlignTool tool, int id)
        {
            return new PatmaxParams()
            {
                Pattern = tool.Pattern,
                RunParams = tool.RunParams,
                SearchRegion = tool.SearchRegion
            };
        }
    }
}
