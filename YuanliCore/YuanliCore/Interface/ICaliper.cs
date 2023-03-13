using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.ImageProcess;

namespace YuanliCore.Interface
{
    public interface ICaliper
    {

        /// <summary>
        /// 執行
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        IEnumerable<CaliperResult> Run(Frame<byte[]> image);
        /// <summary>
        /// 編輯參數 (會彈出視窗)
        /// </summary>
        /// <param name="image"></param>
        void EditParameter(BitmapSource image);


    }

    public class CaliperResult  
    {
        public CaliperResult(Point beginPoint , Point centerPoint, Point endPoint)
        {
            BeginPoint = beginPoint;
            CenterPoint = centerPoint;
            EndPoint = endPoint;
        }

        public Point EndPoint { get; }
        public Point BeginPoint { get; }
        public Point CenterPoint { get;  set; }
    }
     

}
