using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
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
    /// <summary>
    /// Cognex 的樣本搜尋器
    /// </summary>
    public class CogImageConverter : CogMethod
    {
        private CogImageConvertTool convertTool;
        private CogImageConvertWindow cogImageConvertWindow;

        public CogImageConverter()
        {
            convertTool = new CogImageConvertTool();

        }
        public CogImageConverter(CogParameter matcherParams)
        {

            convertTool = new CogImageConvertTool();
            RunParams = matcherParams;
        }
        public override CogParameter RunParams { get; set; } = new CogImageConvertParams(0);
        public ICogImage OutputImage { get; internal set; }

        public override void Dispose()
        {
            if (cogImageConvertWindow != null)
                cogImageConvertWindow.Dispose();
        }

        public override void EditParameter(BitmapSource image)
        {
            try {

                if (image == null) throw new Exception("Image is null");

                cogImageConvertWindow = new CogImageConvertWindow(image);

                CogImageConvertParams param = (CogImageConvertParams)RunParams;
                cogImageConvertWindow.ImageConvertParam = param;
                cogImageConvertWindow.ShowDialog();


                CogImageConvertParams patmaxparams = cogImageConvertWindow.ImageConvertParam;



                param = patmaxparams;

                Dispose();

            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

            }

        }
        public void EditParameter(System.Drawing.Bitmap image)
        {
            if (image == null) throw new Exception("Image is null");
            BitmapSource bmp = image.ToBitmapSource();
            EditParameter(bmp);


        }
        /// <summary>
        /// 已經定位過的影像作編輯
        /// </summary>
        public void CogEditParameter()
        {
            try {

                if (CogFixtureImage == null) throw new Exception("locate is not yet complete");

                cogImageConvertWindow = new CogImageConvertWindow(CogFixtureImage);

                CogImageConvertParams param = (CogImageConvertParams)RunParams;

                cogImageConvertWindow.ImageConvertParam = param;

                cogImageConvertWindow.ShowDialog();


                CogImageConvertParams patmaxparams = cogImageConvertWindow.ImageConvertParam;


                param = patmaxparams;

                Dispose();

            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

            }

        }
        public BitmapSource Convert(Frame<byte[]> image)
        {
            ICogImage cogImg1 = null;
            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(out ICogImage inputImage ,0.333, 0.333, 0.333);
            //   ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);

            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            var img = Convert(cogImg1).ToBitmap();

            return img.ToBitmapSource();


        }


        private ICogImage Convert(ICogImage cogImage)
        {
            var param = (CogImageConvertParams)RunParams;
            convertTool.InputImage = cogImage;

            convertTool.RunParams = param.RunParams;
            convertTool.Region = param.Region;
            convertTool.Run();
            Record = convertTool.CreateLastRunRecord().SubRecords[0];


            return convertTool.OutputImage;
        }
        public override void Run()
        {
            OutputImage = Convert(CogFixtureImage);
        }
    }

}
