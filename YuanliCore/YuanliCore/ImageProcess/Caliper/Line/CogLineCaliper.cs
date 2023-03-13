using Cognex.VisionPro.Caliper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess.Caliper
{
    /// <summary>
    /// Cognex 的卡尺功能 (查找在尺內的一點 或兩點)
    /// </summary>
    public class CogLineCaliper : CogMethod 
    {
        private CogFindLineTool caliperTool;
     //   private CogCaliperWindow cogCaliperWindow;

        public CogLineCaliper()
        {

            caliperTool = new CogFindLineTool();

        }
        public override CogParameter RunParams { get; set; } = new FindLineParam();

        public override void Dispose()
        {
   //         if (cogCaliperWindow != null)
    //            cogCaliperWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
           // if (cogCaliperWindow == null)
 //               cogCaliperWindow = new CogCaliperWindow(image);


 //           cogCaliperWindow.CaliperParam = CaliperParam;
  //          cogCaliperWindow.ShowDialog();


  //          CaliperParam = cogCaliperWindow.CaliperParam;


            Dispose();
        }

        public IEnumerable<CaliperResult> Find(Frame<byte[]> image)
        {
            throw new NotImplementedException();
        }

       

        public override void Run()
        {
            throw new NotImplementedException();
        }
    }

}
