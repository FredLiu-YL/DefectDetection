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
   
    public class CaliperResult : CogResult
    {
        public CaliperResult(Point beginPoint, Point centerPoint, Point endPoint)
        {
            BeginPoint = beginPoint;
            CenterPoint = centerPoint;
            EndPoint = endPoint;
            Vector v = EndPoint - BeginPoint;
            Distance = v.Length;
        }

        /// <summary>
        /// Gap 距離
        /// </summary>
        public double Distance { get; }
    }


    public class LineCaliperResult: CogResult
    {
        public LineCaliperResult(Point beginPoint, Point endPoint, Point centerPoint ,double line)
        {
            BeginPoint = beginPoint;          
            EndPoint = endPoint;
            Distance = line;
            CenterPoint = centerPoint;
        }

      /// <summary>
      /// 線段長度
      /// </summary>
        public double Distance { get; }
    }

}
