using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace YuanliCore.ImageProcess.Match
{
    public class PatmaxParams : CogParameter
    {
        public PatmaxParams(int id ):base(id)
        {
            CogPMAlignTool tool = new CogPMAlignTool();

            Pattern = tool.Pattern;
            RunParams = tool.RunParams;
            SearchRegion = tool.SearchRegion;
            tool.Dispose();
          //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }

        
        /// <summary>
        /// 樣本圖片 
        /// </summary>
        public BitmapSource PatternImage { get; set; }

        [JsonIgnore]
        ///搜尋的參數
        public CogPMAlignRunParams RunParams { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 樣本的參數
        /// </summary>
        public CogPMAlignPattern Pattern { get; set; }
        [JsonIgnore]
        /// <summary>
        /// ROI  Cog用
        /// </summary>
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
            return new PatmaxParams(0)
            {
                Pattern = tool.Pattern,
                RunParams = tool.RunParams,
                SearchRegion = tool.SearchRegion
            };
        }

        protected override void LoadRecipe(string directoryPath, int id)
        {
            throw new NotImplementedException();
        }

        protected override void SaveCogRecipe(string recipeName)
        {
            throw new NotImplementedException();
        }
    }
}
