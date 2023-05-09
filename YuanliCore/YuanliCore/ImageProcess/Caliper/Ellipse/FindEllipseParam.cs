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
    public class FindEllipseParam : CogParameter
    {
        public FindEllipseParam(int id = 0) : base(0)
        {
            CogFindEllipseTool tool = new CogFindEllipseTool();

            Id = id;

            RunParams = tool.RunParams;

            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
            tool.Dispose();
        }


        /// <summary>
        /// 搜尋參數
        /// </summary>
        [JsonIgnore]//Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public CogFindEllipse RunParams { get; set; }


        public static FindEllipseParam Default(int id = 0)
        {
            CogFindEllipseTool tool = new CogFindEllipseTool();
            return Default(tool, id);
        }

        internal static FindEllipseParam Default(CogFindEllipseTool tool, int id)
        {
                                    
            return new FindEllipseParam(0)
            {

                RunParams = tool.RunParams,
               // Region = tool.Region
            };
        }

        protected override void SaveCogRecipe(string directoryPath)
        {

            // var path = CreateFolder(recipeName);

            //還需要補上 cognex 序列化方法
            CogFindEllipseTool tool = new CogFindEllipseTool();
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

            CogFindEllipseTool tool = (CogFindEllipseTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
          //  Region = tool.Region;
            tool.Dispose();
        }
    }

}
