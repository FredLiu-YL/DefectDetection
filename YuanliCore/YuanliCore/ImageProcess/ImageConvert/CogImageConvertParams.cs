using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
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
    public class CogImageConvertParams : CogParameter
    {
        public CogImageConvertParams(int id) : base(id)
        {
            CogImageConvertTool tool = new CogImageConvertTool();

     
            RunParams = tool.RunParams;
            Region = tool.Region;
            tool.Dispose();
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
        }
 

        [JsonIgnore]
        /// 參數
        public CogImageConvertRunParams RunParams { get; set; }
       
        [JsonIgnore]
        /// <summary>
        /// ROI  Cog用
        /// </summary>
        public ICogRegion Region { get; set; }

        /// <summary>
        /// 取得 料號設定搜尋 ROI 的範圍以 System.Windows 型別提供
        /// </summary>
        public Rect? SearchROI
        {
            get; set;
        }

        public object Tag { get; set; }

        public static CogImageConvertParams Default(int id = 0)
        {
            CogImageConvertTool tool = new CogImageConvertTool();
            return Default(tool, id);
        }

        internal static CogImageConvertParams Default(CogImageConvertTool tool, int id)
        {
            return new CogImageConvertParams(0)
            {
                
                RunParams = tool.RunParams,
                Region = tool.Region
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogImageConvertTool tool = new CogImageConvertTool();
            tool.RunParams = RunParams;
            tool.Region = Region;
        

            CogSerializer.SaveObjectToFile(tool, $"{directoryPath}\\VsTool_{Id}.tool");


            tool.Dispose();
            //   CogSerializer.SaveObjectToFile(CogToolBlock1, @"E:\ToolBlock2.vpp");
            //  string fileName = path + $"\\Commom{Id}.json";
            //  Save(fileName);
        }

        protected override void LoadCogRecipe(string directoryPath, int id)
        {

            CogImageConvertTool tool = (CogImageConvertTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
            Region = tool.Region;
        
            tool.Dispose();
        }
    }
}
