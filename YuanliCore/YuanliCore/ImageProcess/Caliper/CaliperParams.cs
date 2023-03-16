using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess.Caliper
{
    public class CaliperParams : CogParameter
    {

        public int TestC { get; set; }
        public CaliperParams(int id) : base(id)
        {
            CogCaliperTool tool = new CogCaliperTool();


            RunParams = tool.RunParams;
            Region = tool.Region;
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
            tool.Dispose();
        }


        /// <summary>
        /// 卡尺的位置與範圍
        /// </summary>
        [JsonIgnore]//Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public CogRectangleAffine Region { get; set; }
        /// <summary>
        /// 搜尋參數
        /// </summary>
        [JsonIgnore]//Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public CogCaliper RunParams { get; set; }


        public static CaliperParams Default(int id = 0)
        {
            CogCaliperTool tool = new CogCaliperTool();
            return Default(tool, id);
        }

        internal static CaliperParams Default(CogCaliperTool tool, int id)
        {
            return new CaliperParams(0)
            {

                RunParams = tool.RunParams,
                Region = tool.Region
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogCaliperTool tool = new CogCaliperTool();
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
           
            CogCaliperTool tool = (CogCaliperTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
            Region = tool.Region;
            tool.Dispose();
        }
    }

}
