using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;

namespace YuanliCore.CameraLib
{
    public static class SystemDrawingEx
    {
        /// <summary>
        /// 取得指定像素格式中像素的 byte 數。
        /// </summary>
        /// <param name="format">像素格式。</param>
        /// <returns>位元組(Bytes)大小。</returns>
        public static int GetBytesPerPixel(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Format64bppArgb:
                case PixelFormat.Format64bppPArgb:
                    return 8;
                case PixelFormat.Format48bppRgb:
                    return 6;
                case PixelFormat.Format32bppArgb:
                case PixelFormat.Format32bppPArgb:
                case PixelFormat.Format32bppRgb:
                case PixelFormat.Canonical:
                    return 4;
                case PixelFormat.Format24bppRgb:
                    return 3;
                case PixelFormat.Format16bppArgb1555:
                case PixelFormat.Format16bppGrayScale:
                case PixelFormat.Format16bppRgb555:
                case PixelFormat.Format16bppRgb565:
                    return 2;
                case PixelFormat.Format8bppIndexed:
                    return 1;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 設定 Bitmap 影像的調色盤為灰階調色盤。
        /// </summary>
        /// <param name="bitmap">表示Bitmap影像。</param>
        public static void SetGrayPalette(this Bitmap bitmap)
        {
            ColorPalette gray = bitmap.Palette;

            for (int i = 0; i < 256; i++)
                gray.Entries[i] = Color.FromArgb(255, i, i, i);

            bitmap.Palette = gray;
        }

        /// <summary>
        /// 從像素陣列建立新的 Bitmap。。
        /// </summary>
        /// <param name="buffer">影像來源陣列。</param>
        /// <param name="width">影像寬度。</param>
        /// <param name="height">影像高度。</param>
        /// <param name="format">影像像素格式。</param>
        /// <returns>Bitmap影像。</returns>
        public static Bitmap ToBitmap(this byte[] buffer, int width, int height, PixelFormat format)
        {
            int bytesPerPixel = format.GetBytesPerPixel();
            Bitmap bitmap = new Bitmap(width, height, format);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
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
       
        public static System.Drawing.Bitmap ToBitmap(this Frame<byte[]> frame)
        {
            System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

            if (frame.Format == System.Windows.Media.PixelFormats.Bgr24)
                format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
            else if (frame.Format == System.Windows.Media.PixelFormats.Pbgra32)
                format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            else if (frame.Format == System.Windows.Media.PixelFormats.Indexed8 || frame.Format == System.Windows.Media.PixelFormats.Gray8)
                format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
            else
                throw new NotSupportedException($"ToBitmap extension function not pxielformat value [{frame.Format}] support");

            return ToBitmap(frame.Data, frame.Width, frame.Height, format);
        }

        /// <summary>
        /// 從 Bitmap 建立新的像素陣列。
        /// </summary>
        /// <param name="bitmap">來源影像。</param>
        /// <returns>影像陣列資料。</returns>
        public static byte[] ToBytes(this Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             ImageLockMode.ReadOnly,
                                             bitmap.PixelFormat);

            var length = bitmapData.Stride * bitmapData.Height;

            byte[] bytes = new byte[length];
            Marshal.Copy(bitmapData.Scan0, bytes, 0, length);
            bitmap.UnlockBits(bitmapData);

            return bytes;
        }

        public static void CloneTo(this Bitmap bitmap, ref byte[] array)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                             ImageLockMode.ReadOnly,
                                             bitmap.PixelFormat);

            try
            {
                var bmpLength = bitmapData.Stride * bitmapData.Height;
                if (array.Length != bmpLength) throw new ArgumentException($"{nameof(array)}.Length is wrong, it must be {bmpLength}.");

                Marshal.Copy(bitmapData.Scan0, array, 0, bmpLength);
            }
            finally
            {
                bitmap.UnlockBits(bitmapData);
            }
        }



    }
}
