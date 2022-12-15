using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;


namespace YuanliCore.CameraLib
{
   public interface ICamera
    {
    }

    public class Frame : Frame<byte[]>
    {
        public Frame(int width, int height, PixelFormat format)
            : base(new byte[width * height * format.GetBytesPerPixel()], width, height, format, new Size(1, 1))
        { }
    }

    public class Frame<TImage> : IDisposable
    {
        private bool isDisposed;

        public Frame(TImage data, int width, int height, PixelFormat format)
            : this(data, width, height, format, Size.Empty)
        { }

        [JsonConstructor]
        public Frame(TImage data, int width, int height, PixelFormat format, Size resolution)
        {
            Data = data;
            Width = width;
            Height = height;
            Format = format;
            Resolution = resolution;
        }

        ~Frame()
        {
            Dispose(false);
        }

        /// <summary>
        /// 取得 Frame 中心像素位置。
        /// 影像是以左上角為坐標原點 (0, 0)，每個像素是以 row 和 column 表示其坐標，
        /// 坐標值的範圍從 (0, 0) 到 (Height-1, Width-1)。每個像素的尺寸為 1，
        /// 第一個像素的中心坐標為 (0, 0)，因此第 1 個像素的範圍是從(-0.5, -0.5) 到 (0.5, 0.5)。
        /// </summary>
        public Point Center => new Point((Width - 1) / 2, (Height - 1) / 2);

        /// <summary>
        /// 取得影像資料。
        /// </summary>
        public TImage Data { get; internal set; }

        /// <summary>
        /// 取得影像像素寬度。
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// 取得影像像素高度。
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// 取得影像像素格式。
        /// </summary>
        public PixelFormat Format { get; }

        /// <summary>
        /// 取得 Frame 的像素解析度，即每個像素與實際長度單位的換算係數。
        /// </summary>
        public Size Resolution { get; }

        /// <summary>
        /// 取得 Frame 內的 ROI 集合。
        /// </summary>
        public List<(System.Drawing.Point Id, Rect Region)> Rois { get; } = new List<(System.Drawing.Point Id, Rect Region)>();

        /// <summary>
        /// 釋放 Frame 的資源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 取得影像上指定位置與中心點之位移差。
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public Vector OffsetWithCenter(Point point)
        {
            Vector offset = ((Vector)point - (Vector)Center);
            return offset;
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed) return;
            if (isDisposing)
            {
                Rois.Clear();
            }

            if (Data is IDisposable disposable) disposable.Dispose();

            isDisposed = true;
        }
    }

    public static partial class goonØ
    {
        public static Frame<byte[]> Rotate(this Frame<byte[]> frame, RotateAngles angle)
        {
            var rotated = new TransformedBitmap(frame.ToBitmapSource(), new RotateTransform((double)angle));
            return rotated.ToByteFrame();

            #region 使用指標旋轉影像資料 ，速度較 TransformedBitmap 慢，故不使用。
            //int bps = frame.Format.GetBytesPerPixel();
            //int stride = bps * frame.Width; // 旋轉 90 度不管正負，長寬數值會對調。          

            //int newWidth = frame.Height;
            //int newStride = bps * newWidth;

            //byte[] newData = new byte[frame.Data.Length];

            //GCHandle frameHandle = GCHandle.Alloc(frame.Data, GCHandleType.Pinned);
            //GCHandle newHandle = GCHandle.Alloc(newData, GCHandleType.Pinned);

            //unsafe
            //{
            //    var framePointer = (byte*)frameHandle.AddrOfPinnedObject();
            //    var newPointer = (byte*)newHandle.AddrOfPinnedObject();

            //    Parallel.For(0, frame.Height, row =>
            //    Parallel.For(0, frame.Width, col =>
            //    {
            //        int newCol = 0, newRow = 0;

            //        switch (angle)
            //        {
            //            case RotateAngles.Clockwise90: // Col 跟 Row 對調後再反轉每一行。
            //                newCol = (frame.Height - row - 1) * bps;
            //                newRow = col;
            //                break;
            //            case RotateAngles.CounterClockWise90: // Col 跟 Row 對調後在反轉每一列。
            //                newCol = row * bps;
            //                newRow = frame.Width - col - 1;
            //                break;
            //        }

            //        var index = (stride * row) + col * bps;
            //        var newIndex = newStride * newRow + newCol;

            //        for (int k = 0; k < bps; k++)
            //            newPointer[newIndex + k] = framePointer[index + k];
            //    }));
            //}

            //frameHandle.Free();
            //newHandle.Free();

            //return new Frame<byte[]>(newData, frame.Height, frame.Width, frame.Format); 
            #endregion
        }

        public static void Save(this Frame<byte[]> frame, string filename)
        {
            var bmp = frame.ToBitmapSource();
            bmp.Save(filename);
        }

        public static BitmapSource ToBitmapSource(this Frame<byte[]> frame)
        {
            return frame.Data.ToBitmapSource(frame.Width, frame.Height, frame.Format);
        }

        public static void WritePixels(this WriteableBitmap wbmp, Frame<byte[]> frame)
        {
            Int32Rect rect = new Int32Rect(
                (wbmp.PixelWidth - frame.Width) / 2,
                (wbmp.PixelHeight - frame.Height) / 2,
                frame.Width,
                frame.Height);

            int stride = frame.Format.BitsPerPixel / 8;
            wbmp.WritePixels(rect, frame.Data, frame.Width * stride, 0);
        }

        public static void WriteFrame(this WriteableBitmap wbmp, Frame<byte[]> frame)
        {
            try
            {
                IntPtr pointer = IntPtr.Zero;

                wbmp.Dispatcher.Invoke(() =>
                {
                    pointer = wbmp.BackBuffer;
                    wbmp.Lock();
                }, System.Windows.Threading.DispatcherPriority.Render);

                Marshal.Copy(frame.Data, 0, pointer, frame.Data.Length);

                wbmp.Dispatcher.Invoke(() =>
                {
                    wbmp.AddDirtyRect(new Int32Rect(0, 0, frame.Width, frame.Height));
                    wbmp.Unlock();
                }, System.Windows.Threading.DispatcherPriority.Render);
            }
            catch (OperationCanceledException)
            {
                // 若工作取消不須發出錯誤。
            }
        }

        public static Frame<byte[]> ToByteFrame(this BitmapSource bmp)
            => new Frame<byte[]>(bmp.ToBytes(), bmp.PixelWidth, bmp.PixelHeight, bmp.Format);

        /// <summary>
        /// 將 Bitmap 複製到指定的 Frame 中。
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="frame"></param>
        //public static void CloneTo(this System.Drawing.Bitmap bitmap, ref Frame<byte[]> frame)
        //{
        //    var data = frame.Data;
        //    System.Drawing.goonØ.CloneTo(bitmap, ref data);
        //}

        /// <summary>
        /// 將 Bitmap 複製到指定的 Frame 中。
        /// </summary>
        //public static void CloneTo(this System.Drawing.Bitmap bitmap, ref Frame frame)
        //{
        //    var data = frame.Data;
        //    System.Drawing.goonØ.CloneTo(bitmap, ref data);
        //}

        //public static System.Drawing.Bitmap ToBitmap(this Frame<byte[]> frame)
        //{
        //    System.Drawing.Imaging.PixelFormat format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;

        //    if (frame.Format == System.Windows.Media.PixelFormats.Bgr24)
        //        format = System.Drawing.Imaging.PixelFormat.Format24bppRgb;
        //    else if (frame.Format == System.Windows.Media.PixelFormats.Pbgra32)
        //        format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
        //    else if (frame.Format == System.Windows.Media.PixelFormats.Indexed8 || frame.Format == System.Windows.Media.PixelFormats.Gray8)
        //        format = System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        //    else
        //        throw new NotSupportedException($"ToBitmap extension function not pxielformat value [{frame.Format}] support");

        //    return System.Drawing.goonØ.ToBitmap(frame.Data, frame.Width, frame.Height, format);
        //}
    }

    public enum RotateAngles
    {
        /// <summary>
        /// 順時針 90 度。
        /// </summary>
        Clockwise90 = 90,

        /// <summary>
        /// 順時針 180 度。
        /// </summary>
        Clockwise180 = 180,

        /// <summary>
        /// 順時針 270 度。
        /// </summary>
        Clockwise270 = 270,

        /// <summary>
        /// 逆時針 90 度。
        /// </summary>
        CounterClockWise90 = -90,

        /// <summary>
        /// 逆時針 180 度。
        /// </summary>
        CounterClockWise180 = -180,

        /// <summary>
        /// 逆時針 270 度。
        /// </summary>
        CounterClockWise270 = -270,
    }


}
