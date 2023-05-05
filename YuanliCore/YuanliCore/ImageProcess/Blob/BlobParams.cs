using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess.Blob
{
    public class BlobParams : CogParameter
    {

        public int TestC { get; set; }
        public BlobParams(int id = 0):base(id)
        {
            CogBlobTool tool = new CogBlobTool();

         
       
            RunParams = tool.RunParams;
            ROI = tool.Region;
            //  (CogPMAlignRunParams)CogSerializer.LoadObjectFromFile("");
            tool.Dispose();
        }
        /// <summary>
        ///
        /// </summary>
        [JsonIgnore]//Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public ICogRegion ROI { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore] //Vision pro 不能序列化  所以要忽略  不然就要用到JsonConvert
        public CogBlob RunParams { get; set; }


        public double JudgeMin { get; set; } = 10;
      //  public double JudgeMax { get; set; }

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

        protected override void SaveCogRecipe(string directoryPath)
        {

            //還需要補上 cognex 序列化方法
            CogBlobTool tool = new CogBlobTool();
            tool.RunParams = RunParams;
            tool.Region = ROI;

            CogSerializer.SaveObjectToFile(tool, $"{directoryPath}\\VsTool_{Id}.tool");


            tool.Dispose();
        }

        protected override void LoadCogRecipe(string directoryPath, int id)
        {
            CogBlobTool tool = (CogBlobTool)CogSerializer.LoadObjectFromFile($"{directoryPath}\\VsTool_{id}.tool");
            RunParams = tool.RunParams;
            ROI = tool.Region;
            tool.Dispose();
        }
    }

}
