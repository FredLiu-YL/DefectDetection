using Cognex.VisionPro;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Match
{
    public class SearchMaxParams : CogParameter
    {
        public SearchMaxParams(int id) : base(id)
        {
            CogSearchMaxTool tool = new CogSearchMaxTool();

            Pattern = tool.Pattern;
            RunParams = tool.RunParams;
            SearchRegion = tool.SearchRegion;
            tool.Dispose();
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }


        /// <summary>
        /// 樣本圖片 
        /// </summary>
        public Frame<byte[]> PatternImage { get; set; }

        [JsonIgnore]
        ///搜尋的參數
        public CogSearchMaxRunParams RunParams { get; set; }
        [JsonIgnore]
        /// <summary>
        /// 樣本的參數
        /// </summary>
        public CogSearchMaxPattern Pattern { get; set; }
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
            get; set;
        }

        public object Tag { get; set; }

        public static SearchMaxParams Default(int id = 0)
        {
            CogSearchMaxTool tool = new CogSearchMaxTool();
            return Default(tool, id);
        }

        internal static SearchMaxParams Default(CogSearchMaxTool tool, int id)
        {
            return new SearchMaxParams(0)
            {
                Pattern = tool.Pattern,
                RunParams = tool.RunParams,
                SearchRegion = tool.SearchRegion
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogSearchMaxTool tool = new CogSearchMaxTool();
            tool.RunParams = RunParams;
            tool.SearchRegion = SearchRegion;
            tool.Pattern = Pattern;

            CogSerializer.SaveObjectToFile(tool, $"{directoryPath}\\VsTool_{Id}.tool");


            tool.Dispose();
            //   CogSerializer.SaveObjectToFile(CogToolBlock1, @"E:\ToolBlock2.vpp");
            //  string fileName = path + $"\\Commom{Id}.json";
            //  Save(fileName);
        }

        protected override void LoadCogRecipe(string directoryPath, int id)
        {

            CogSearchMaxTool tool = (CogSearchMaxTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
            SearchRegion = tool.SearchRegion;
            Pattern = tool.Pattern;
            tool.Dispose();
        }
    }
}
