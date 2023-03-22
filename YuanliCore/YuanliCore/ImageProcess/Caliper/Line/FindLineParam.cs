using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;
using Newtonsoft.Json;

namespace YuanliCore.ImageProcess.Caliper
{
    public class FindLineParam : CogParameter
    {
        public FindLineParam(int id = 0) : base(0)
        {
            CogFindLineTool tool = new CogFindLineTool();

            Id = id;

            RunParams = tool.RunParams;

            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
            tool.Dispose();
        }


        /// <summary>
        /// 搜尋參數
        /// </summary>
        [JsonIgnore]//Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public CogFindLine RunParams { get; set; }


        public static FindLineParam Default(int id = 0)
        {
            CogFindLineTool tool = new CogFindLineTool();
            return Default(tool, id);
        }

        internal static FindLineParam Default(CogFindLineTool tool, int id)
        {
            return new FindLineParam(0)
            {

                RunParams = tool.RunParams,
               // Region = tool.Region
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogFindLineTool tool = new CogFindLineTool();
            tool.RunParams = RunParams;
           // tool.Region = Region;

            CogSerializer.SaveObjectToFile(tool, $"{directoryPath}\\VsTool_{Id}.tool");


            tool.Dispose();
            //   CogSerializer.SaveObjectToFile(CogToolBlock1, @"E:\ToolBlock2.vpp");
            //  string fileName = path + $"\\Commom{Id}.json";
            //  Save(fileName);
        }

        protected override void LoadCogRecipe(string directoryPath, int id)
        {

            CogFindLineTool tool = (CogFindLineTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
          //  Region = tool.Region;
            tool.Dispose();
        }
    }

}
