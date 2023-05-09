using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore.Interface;

namespace YuanliCore.CameraLib
{
    public static class FrameEX
    {

        /// <summary>
        /// 從像素陣列建立新的 BitmapSource。
        /// </summary>
        /// <param name="buffer">影像來源陣列。</param>
        /// <param name="width">影像寬度。</param>
        /// <param name="height">影像高度。</param>
        /// <param name="format">影像像素格式。</param>
        /// <param name="dpiX">螢幕水平解析度，預設為 96。</param>
        /// <param name="dpiY">螢幕垂直解析度，預設為 96。</param>
        /// <returns>BitmapSource影像。</returns>
        public static BitmapSource ToBitmapSource(this byte[] buffer, int width, int height, PixelFormat format)
        {
            int dpiX = 96;
            int dpiY = 96;

            int bytesPerPixel = format.GetBytesPerPixel();
            int stride = bytesPerPixel * width;
            return BitmapSource.Create(width, height, dpiX, dpiY, format, format.DefaultPalette(), buffer, stride);
        }

        /// <summary>
        /// 從儲存在 Unmanaged 記憶體內的像素陣列，建立新的 BitmapSource。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static BitmapSource ToBitmapSource(this IntPtr buffer, int width, int height, PixelFormat format)
        {
            ThrowIfFormatIsIndexed(format);

            int dpiX = 96;
            int dpiY = 96;

            int bytesPerPixel = format.GetBytesPerPixel();
            int stride = bytesPerPixel * width;
            return BitmapSource.Create(width, height, dpiX, dpiY, format, null, buffer, stride * height, stride);
        }
        /// <summary>
        /// 從像素陣列建立新的 Bitmap。。
        /// </summary>
        /// <param name="buffer">影像來源陣列。</param>
        /// <param name="width">影像寬度。</param>
        /// <param name="height">影像高度。</param>
        /// <param name="format">影像像素格式。</param>
        /// <returns>Bitmap影像。</returns>
        public static System.Drawing.Bitmap ToBitmap(this byte[] buffer, int width, int height, System.Drawing.Imaging.PixelFormat format)
        {
            int bytesPerPixel = format.GetBytesPerPixel();
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(width, height, format);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
            IntPtr scan0 = bmpData.Scan0;

            // 因 BitmapData 的 Stride 在某些寬度時會比目前要轉換的影像還多，故需要每列複製，避免 Stride 長度不同的問題。
            int stride = width * bytesPerPixel;
            for (int h = 0; h < height; h++) {
                Marshal.Copy(buffer, h * stride, scan0 + h * bmpData.Stride, stride);
            }
            bitmap.UnlockBits(bmpData);
            return bitmap;
        }
        /// <summary>
        /// 從 BitmapSource 建立新的像素陣列。
        /// </summary>
        /// <param name="source">來源影像。</param>
        /// <returns>影像陣列資料。</returns>
        public static byte[] ToBytes(this BitmapSource source)
        {
            int bytesPerPixel = (source.Format.BitsPerPixel + 7) / 8;
            int stride = source.PixelWidth * bytesPerPixel;

            byte[] buffer = new byte[source.PixelHeight * stride];
            source.CopyPixels(buffer, stride, 0);

            return buffer;
        }
        /// <summary>
        /// 黑白BitmapSource 轉黑白的CogImage
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static ICogImage GrayFrameToCogImage(this Frame<byte[]> frame)
        {
            // Create Cognex Root thing.
            var cogRoot = new CogImage8Root();
            CogImage8Grey cogImage = new CogImage8Grey();
            var rawSize = frame.Width * frame.Height;
            SafeMalloc buf = null;
            try {
                cogImage = new CogImage8Grey();

                buf = new SafeMalloc(rawSize);

                // Copy from the byte array into the
                // previously allocated. memory
                Marshal.Copy(frame.Data, 0, buf, rawSize);

                // Initialise the image root, the stride is the
                // same as the widthas the input image is byte alligned and
                // has no padding etc.
                cogRoot.Initialize(frame.Width, frame.Height, buf, frame.Width, buf);

                // And set the image roor
                cogImage.SetRoot(cogRoot);
            }
            catch (Exception ex) {
                throw ex;
            }

            return cogImage;
        }
        /// <summary>
        /// 彩色BitmapSource 轉彩色的CogImage
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns></returns>
        public static ICogImage ColorFrameToColorCogImage(this BitmapSource bitmapSource)
        {
            try {

                using (System.Drawing.Bitmap bmp = bitmapSource.ToByteFrame().ToBitmap()) 
                 {
                    return new CogImage24PlanarColor(bmp);

                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        /// <summary>
        /// frame 轉彩色的CogImage
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <returns></returns>
        public static ICogImage ColorFrameToColorCogImage(this Frame<byte[]> frame)
        {
            try {

                using (System.Drawing.Bitmap bmp = frame.ToBitmap()) {
                    var cogImg = new CogImage24PlanarColor(bmp);
                    return cogImg;
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        /// <summary>
        /// 彩色Frame 轉黑白的CogImage
        /// </summary>
        /// <param name="frame"></param>
        /// <param name="bayerRedScale"></param>
        /// <param name="bayerGreenScale"></param>
        /// <param name="bayerBlueScale"></param>
        /// <returns></returns>
        public static ICogImage ColorFrameToCogImage(this Frame<byte[]> frame, out ICogImage inputImage, double bayerRedScale = 0.333, double bayerGreenScale = 0.333, double bayerBlueScale = 0.333)
        {
            try {

                using (System.Drawing.Bitmap bmp = frame.ToBitmap()) {

                    CogImage24PlanarColor cogImage = new CogImage24PlanarColor(bmp);

                    using (CogImageConvertTool tool = new CogImageConvertTool()) {
                        tool.InputImage = cogImage;
                        tool.RunParams.RunMode = CogImageConvertRunModeConstants.IntensityFromWeightedRGB;

                        tool.RunParams.IntensityFromWeightedRGBRedWeight = bayerRedScale;
                        tool.RunParams.IntensityFromWeightedRGBGreenWeight = bayerGreenScale;
                        tool.RunParams.IntensityFromWeightedRGBBlueWeight = bayerBlueScale;

                        tool.Run();

                        inputImage = cogImage;
                        return (CogImage8Grey)tool.OutputImage;
                    }
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        /// <summary>
        /// 彩色BitmapSource 轉黑白的CogImage
        /// </summary>
        /// <param name="bitmapSource"></param>
        /// <param name="bayerRedScale"></param>
        /// <param name="bayerGreenScale"></param>
        /// <param name="bayerBlueScale"></param>
        /// <returns></returns>
        public static ICogImage ColorFrameToCogImage(this BitmapSource bitmapSource, double bayerRedScale = 0.333, double bayerGreenScale = 0.333, double bayerBlueScale = 0.333)
        {
            try {

                using (System.Drawing.Bitmap bmp = bitmapSource.ToByteFrame().ToBitmap()) {
                    CogImage24PlanarColor cogImage = new CogImage24PlanarColor(bmp);

                    using (CogImageConvertTool tool = new CogImageConvertTool()) {
                        tool.InputImage = cogImage;
                        tool.RunParams.RunMode = CogImageConvertRunModeConstants.IntensityFromWeightedRGB;

                        tool.RunParams.IntensityFromWeightedRGBRedWeight = bayerRedScale;
                        tool.RunParams.IntensityFromWeightedRGBGreenWeight = bayerGreenScale;
                        tool.RunParams.IntensityFromWeightedRGBBlueWeight = bayerBlueScale;

                        tool.Run();
                        return (CogImage8Grey)tool.OutputImage;
                    }
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            System.Windows.Media.Imaging.BitmapSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            //release resource
            DeleteObject(ptr);

            return result;
        }
        public static System.Windows.Media.Imaging.BitmapSource ToBitmapSource(this System.Drawing.Image image)
        {

            Bitmap bitmap = new Bitmap(image);
            return bitmap.ToBitmapSource();
        }
        //方法有問題 讀硬碟沒事 ，取相機的影像會有事 待確認問題20230115 
        public static System.Drawing.Bitmap ToBitmap(this BitmapSource bitmapSource)
        {
            //  BitmapSource m = (BitmapSource)image1.Source;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, bitmapSource.Format.ConvertPixelFormat());

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            bitmapSource.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }
        public static System.Drawing.Bitmap ToBitmap(this Frame<byte[]> frame)
        {
            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

            if (frame.Format == System.Windows.Media.PixelFormats.Bgr24)
                format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            else if (frame.Format == System.Windows.Media.PixelFormats.Pbgra32)
                format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            else if (frame.Format == System.Windows.Media.PixelFormats.Indexed8 || frame.Format == System.Windows.Media.PixelFormats.Gray8)
                format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            else if (frame.Format == System.Windows.Media.PixelFormats.Bgr32)
                format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            else
                throw new NotSupportedException($"ToBitmap extension function not pxielformat value [{frame.Format}] support");

            return ToBitmap(frame.Data, frame.Width, frame.Height, format);
        }

        public static void CopyPixels(this BitmapSource source, byte[] buffer)
        {
            int stride = source.Format.GetBytesPerPixel() * source.PixelWidth;
            int bufferSize = source.PixelHeight * stride;
            if (buffer.Length != bufferSize) throw new ArgumentException("buffer's length is wrong.");
            source.CopyPixels(buffer, stride, 0);
        }

        public static BitmapSource FormatConvertTo(this BitmapSource image, PixelFormat format)
        {
            if (image == null) return null;
            ThrowIfFormatIsIndexed(format);

            FormatConvertedBitmap fcbmp = new FormatConvertedBitmap();
            fcbmp.BeginInit();
            fcbmp.Source = image;
            fcbmp.DestinationFormat = format;
            fcbmp.EndInit();

            return fcbmp;
        }
        /// <summary>
        /// 取得指定像素格式的預設調色盤。
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        private static BitmapPalette DefaultPalette(this PixelFormat format)
        {
            if (format == PixelFormats.Indexed1) return BitmapPalettes.BlackAndWhite;
            if (format == PixelFormats.Indexed2) return BitmapPalettes.Gray4;
            if (format == PixelFormats.Indexed4) return BitmapPalettes.Gray16;
            if (format == PixelFormats.Indexed8) return BitmapPalettes.Gray256;
            return null;
        }

        // 若指定的像素格式為 Indexed 格式則丟出例外狀況。
        private static void ThrowIfFormatIsIndexed(PixelFormat format)
        {
            if (format == PixelFormats.Indexed1 ||
               format == PixelFormats.Indexed2 ||
               format == PixelFormats.Indexed4 ||
               format == PixelFormats.Indexed8)
                throw new NotImplementedException("Not support indexed format.");
        }
        private static System.Drawing.Imaging.PixelFormat ConvertPixelFormat(this PixelFormat sourceFormat)
        {

            if (sourceFormat == PixelFormats.Bgr24)
                return System.Drawing.Imaging.PixelFormat.Format24bppRgb;

            if (sourceFormat == PixelFormats.Bgra32)
                return System.Drawing.Imaging.PixelFormat.Format32bppArgb;

            if (sourceFormat == PixelFormats.Bgr32)
                return System.Drawing.Imaging.PixelFormat.Format32bppRgb;

            // .. as many as you need...

            return new System.Drawing.Imaging.PixelFormat();
        }
    }
}
