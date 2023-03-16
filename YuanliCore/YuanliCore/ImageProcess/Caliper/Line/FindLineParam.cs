using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using Cognex.VisionPro.Caliper;

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
        public CogFindLine RunParams { get; set; }


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

        protected override void LoadCogRecipe(string directoryPath, int id)
        {
            throw new NotImplementedException();
        }

        protected override void SaveCogRecipe(string recipeName)
        {
            throw new NotImplementedException();
        }
    }

}
