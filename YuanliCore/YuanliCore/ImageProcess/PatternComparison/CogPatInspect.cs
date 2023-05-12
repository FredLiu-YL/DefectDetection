using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.ImageProcessing;
using Cognex.VisionPro.PatInspect;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.SearchMax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;
using YuanliCore.Interface;

namespace YuanliCore.ImageProcess
{
    /// <summary>
    /// Cognex 的樣本搜尋器
    /// </summary>
    public class CogPatInspect : CogMethod
    {
        private CogPatInspectTool patInspectTool;
        private CogBlobTool blobTool;
        private CogPatInspectWindow CogPatInspectWindow;

        public CogPatInspect()
        {
            patInspectTool = new CogPatInspectTool();
            if (blobTool == null)
                blobTool = CreateBlobTool();
            var param = new CogPatInspectParams(0);
            param.Pattern.SobelOffset = 20;
            param.Pattern.SobelScale = 1.5;
            param.Pattern.ThresholdOffset = 20;
            param.Pattern.ThresholdScale = 1.5;
            RunParams = param;
        }
        public CogPatInspect(CogParameter matcherParams)
        {

            patInspectTool = new CogPatInspectTool();
            if (blobTool == null)
                blobTool = CreateBlobTool();
            RunParams = matcherParams;
        }
        public override CogParameter RunParams { get; set; }
        public BlobDetectorResult[] DetectorResults { get; internal set; }

        public override void Dispose()
        {
            if (CogPatInspectWindow != null)
                CogPatInspectWindow.Dispose();

        }

        public override void EditParameter(BitmapSource image)
        {
            try {

                if (image == null) throw new Exception("Image is null");

                CogPatInspectWindow = new CogPatInspectWindow(image);

                CogPatInspectParams param = (CogPatInspectParams)RunParams;
                CogPatInspectWindow.PatInspectParams = param;
                CogPatInspectWindow.ShowDialog();


                CogPatInspectParams inspectparams = CogPatInspectWindow.PatInspectParams;

                param = inspectparams;

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

                CogPatInspectWindow = new CogPatInspectWindow(CogFixtureImage, CogTransform);

                CogPatInspectParams param = (CogPatInspectParams)RunParams;



                CogPatInspectWindow.PatInspectParams = param;

                CogPatInspectWindow.ShowDialog();


                CogPatInspectParams patmaxparams = CogPatInspectWindow.PatInspectParams;


                param = patmaxparams;

                Dispose();

            }
            catch (Exception ex) {

                throw ex;
            }
            finally {

            }

        }
        private CogBlobTool CreateBlobTool()
        {
            blobTool = new CogBlobTool();

            blobTool.RunParams.SegmentationParams.Mode = CogBlobSegmentationModeConstants.HardFixedThreshold;
            blobTool.RunParams.SegmentationParams.Polarity = CogBlobSegmentationPolarityConstants.LightBlobs;
            blobTool.RunParams.SegmentationParams.HardFixedThreshold = 20;
            blobTool.RunParams.ConnectivityMinPixels = 10;

            var operations = new CogBlobMorphologyCollection();
            operations.Add(CogBlobMorphologyConstants.DilateSquare);
            operations.Add(CogBlobMorphologyConstants.DilateSquare);
            operations.Add(CogBlobMorphologyConstants.DilateSquare);
            operations.Add(CogBlobMorphologyConstants.DilateSquare);
            operations.Add(CogBlobMorphologyConstants.ErodeSquare);
            operations.Add(CogBlobMorphologyConstants.ErodeSquare);
            operations.Add(CogBlobMorphologyConstants.ErodeSquare);
            operations.Add(CogBlobMorphologyConstants.ErodeSquare);
            blobTool.RunParams.MorphologyOperations = operations;

            return blobTool;
        }
        private IEnumerable<BlobDetectorResult> FindBlob(ICogImage cogImage, int threshold = 20, int minPixels = 10)
        {
            blobTool.InputImage = cogImage;
            blobTool.RunParams.SegmentationParams.HardFixedThreshold = threshold;
            blobTool.RunParams.ConnectivityMinPixels = minPixels;

            blobTool.Run();

            List<BlobDetectorResult> results = new List<BlobDetectorResult>();
            var blobResults = blobTool.Results.GetBlobs();

            for (int i = 0; i < blobResults.Count; i++) {
                var pose = blobResults[i].CenterOfMassX;

                double x = blobResults[i].CenterOfMassX;
                double y = blobResults[i].CenterOfMassY;
                double area = blobResults[i].Area;
                var radiusH = blobResults[i].GetMeasure(CogBlobMeasureConstants.BoundingBoxPrincipalAxisHeight); //得到最大矩形 高
                var radiusW = blobResults[i].GetMeasure(CogBlobMeasureConstants.BoundingBoxPrincipalAxisWidth);//得到最大矩形 寬
                Vector rect = new Vector(radiusW, radiusH);
                var diameter = rect.Length; //算出最大矩形對角線 當作Blob直徑

                results.Add(new BlobDetectorResult(new Point(x, y), area, diameter));
            }
            var lastRunRecord = blobTool.CreateLastRunRecord().SubRecords[0];
            //  Record = blobTool.CreateLastRunRecord().SubRecords[0];
            Record.SubRecords.Add(lastRunRecord);
            return results;
        }

        public IEnumerable<BlobDetectorResult> DifferenceMatch(Frame<byte[]> image)
        {
            ICogImage cogImg1 = null;
            if (image.Format == System.Windows.Media.PixelFormats.Indexed8 || image.Format == System.Windows.Media.PixelFormats.Gray8)
                cogImg1 = image.GrayFrameToCogImage();
            else
                cogImg1 = image.ColorFrameToCogImage(out ICogImage inputImage, 0.333, 0.333, 0.333);
            //   ICogImage cogImg1 = image.ColorFrameToCogImage(0.333, 0.333, 0.333);

            //  cogImg = cogImg1;
            //     cogRecordsDisplay = new CogRecordsDisplay();
            var result = DifferenceMatch(cogImg1);

            return result;


        }


        private IEnumerable<BlobDetectorResult> DifferenceMatch(ICogImage cogImage)
        {
            var param = (CogPatInspectParams)RunParams;
            patInspectTool.InputImage = (CogImage8Grey)cogImage;
            patInspectTool.Pose = CogTransform;
            patInspectTool.RunParams = param.RunParams;
            patInspectTool.Pattern = param.Pattern;
            if (!patInspectTool.Pattern.Trained) {
                patInspectTool.Pattern.Train();
                patInspectTool.Pattern.EndStatisticalTraining();
            }

            patInspectTool.Run();

            if (patInspectTool.Result == null) return new BlobDetectorResult[] { };

            // var lastRunRecord = patInspectTool.CreateLastRunRecord().SubRecords[0];
            var inputRunRecord = patInspectTool.CreateCurrentRecord().SubRecords[0]; //紀錄傳入圖片
            Record = inputRunRecord;

            var differenceImage = patInspectTool.Result.GetDifferenceImage(CogPatInspectDifferenceImageConstants.Absolute);
            differenceImage.ToBitmap().Save("D:\\11122.bmp");
            var result = FindBlob(differenceImage);
            return result;
        }
        public override void Run()
        {
            DetectorResults = DifferenceMatch(CogFixtureImage).ToArray();
        }
    }

}
