using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PatInspect;
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

namespace YuanliCore.ImageProcess
{
    public class CogPatInspectParams : CogParameter
    {
        public CogPatInspectParams(int id) : base(id)
        {
            CogPatInspectTool tool = new CogPatInspectTool();

     
            RunParams = tool.RunParams;
            Pattern = tool.Pattern;
            tool.Dispose();
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }
 

        [JsonIgnore]
        /// 參數
        public CogPatInspectRunParams RunParams { get; set; }
       
        [JsonIgnore]
        /// <summary>
        /// ROI  Cog用
        /// </summary>
        public ICogRegion Region { get; set; }
        [JsonIgnore]
        public CogPatInspectPattern Pattern { get; set; }


        /// <summary>
        /// 取得 料號設定搜尋 ROI 的範圍以 System.Windows 型別提供
        /// </summary>
        public Rect? SearchROI
        {
            get; set;
        }

        public object Tag { get; set; }

        public static CogPatInspectParams Default(int id = 0)
        {
            CogPatInspectTool tool = new CogPatInspectTool();
            return Default(tool, id);
        }

        internal static CogPatInspectParams Default(CogPatInspectTool tool, int id)
        {
            return new CogPatInspectParams(0)
            {
                
                RunParams = tool.RunParams,
                Pattern = tool.Pattern
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogPatInspectTool tool = new CogPatInspectTool();
            tool.RunParams = RunParams;
            tool.Pattern = Pattern;
        

            CogSerializer.SaveObjectToFile(tool, $"{directoryPath}\\VsTool_{Id}.tool");


            tool.Dispose();
            //   CogSerializer.SaveObjectToFile(CogToolBlock1, @"E:\ToolBlock2.vpp");
            //  string fileName = path + $"\\Commom{Id}.json";
            //  Save(fileName);
        }

        protected override void LoadCogRecipe(string directoryPath, int id)
        {

            CogPatInspectTool tool = (CogPatInspectTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
            Pattern = tool.Pattern;
        
            tool.Dispose();
        }
    }
}
