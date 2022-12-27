using Cognex.VisionPro;
using Cognex.VisionPro.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            for (int h = 0; h < height; h++)
            {
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
        public static ICogImage GrayFrameToCogImage(this Frame<byte[]> frame)
        {
            // Create Cognex Root thing.
            var cogRoot = new CogImage8Root();
            CogImage8Grey cogImage = new CogImage8Grey();
            var rawSize = frame.Width * frame.Height;
            SafeMalloc buf = null;
            try
            {
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
            catch (Exception ex)
            {
                throw ex;
            }

            return cogImage;
        }

        public static ICogImage ColorFrameToCogImage(this Frame<byte[]> frame, double bayerRedScale, double bayerGreenScale, double bayerBlueScale)
        {
            try
            {

                using (System.Drawing.Bitmap bmp = frame.ToBitmap())
                {
                    CogImage24PlanarColor cogImage = new CogImage24PlanarColor(bmp);

                    using (CogImageConvertTool tool = new CogImageConvertTool())
                    {
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
            catch (Exception ex)
            {
                throw ex;
            }
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

    }
}
