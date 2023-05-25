using Cognex.VisionPro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    public abstract class CogMethod
    {
        /// <summary>
        /// 計算後的結果
        /// </summary>
        public CogResult MethodResult { get; set; }
        /// <summary>
        /// 計算後的結果 圖片(Vision pro 用)
        /// </summary>
        public ICogRecord Record { get; set; }
        /// <summary>
        /// 對位後的圖片(使用Vision pro 方法校正過 )
        /// </summary>
        public ICogImage CogFixtureImage { get; set; }
        public CogTransform2DLinear CogTransform { get; set; }
        public MethodName MethodName { get => RunParams.Methodname; set { RunParams.Methodname = value; } }

        public abstract void Dispose();
      
        public abstract void SetCogToolParameter(ICogTool cogTool);
        public abstract ICogTool GetCogTool();
        /// <summary>
        /// 開啟編輯參數視窗
        /// </summary>
        /// <param name="image"></param>
        public abstract void EditParameter(BitmapSource image);
        /// <summary>
        /// 開啟編輯參數視窗
        /// </summary>
        /// <param name="image"></param>
        public void EditParameter(System.Drawing.Bitmap image)
        {
            if (image == null) throw new Exception("Image is null");
            BitmapSource bmp = image.ToBitmapSource();
            EditParameter(bmp);


        }
        public abstract CogParameter RunParams { get; set; }


        public abstract void Run();
    }
}
