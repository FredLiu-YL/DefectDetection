using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using uEye.Defines;
using uEye.Types;
using uEye;
using YuanliCore.Interface;


namespace YuanliCore.CameraLib.IDS
{
    public class UeyeCamera: ICamera
    {
        private readonly uEye.Camera cam = new uEye.Camera();
        private readonly int bufferCount = 3;
        private byte[] buffer;
        private IObservable<Frame<byte[]>> frames  ;

        private readonly Dictionary<ColorMode, System.Windows.Media.PixelFormat> formatMap = new Dictionary<ColorMode, System.Windows.Media.PixelFormat>
        {
            [ColorMode.BGR8Packed] = PixelFormats.Bgr24,
            [ColorMode.BGRA8Packed] = PixelFormats.Bgr32,
            [ColorMode.Mono8] = PixelFormats.Gray8,
            [ColorMode.BGR10Packed] = PixelFormats.Bgr101010,
            [ColorMode.BGR12Unpacked] = PixelFormats.Rgba64
        };


        /// <summary>
        /// 建構 uEyeAreaCamera 相機，當有多支相機時 cameraId 並非每次啟動都是固定的，需使用 iDS 的官方軟體設定才可固定其 Id。
        /// </summary>
        /// <param name="cameraId">若將 cameraId 指定為 0 則自動抓取第一支可用相機，CameraId 可於 IDS Vision Cockpit 中修改。</param>
        public UeyeCamera(int cameraId = 0)
        {
            uEye.Info.Camera.GetCameraList(out CameraInformation[] camInfoList);
            if (camInfoList.Length == 0) throw new Exception("");

            CameraInformation camInfo;
            if (cameraId == 0) camInfo = camInfoList.First();
            else
            {
                var matches = camInfoList.Where(info => info.CameraID == cameraId);
                if (!matches.Any()) throw new Exception($"CameraID = {cameraId}");
                camInfo = matches.First();
            }

            ID = camInfo.CameraID;
            SerialNumber = camInfo.SerialNumber;
            Model = camInfo.Model;
        }


        public IObservable<Frame<byte[]>> Frames => frames;
        public System.Windows.Size Resolution { get; set; } = new System.Windows.Size(1.0, 1.0);


        public bool IsGrabbing { get; set; }

        #region Device Property

        public int ID { get; protected set; }

        public string Name { get; set; }

        public string Manufacturer { get; }


        public string SerialNumber { get; protected set; }

        #endregion



        #region Device Infomation

        // public string Manufacturer => "iDS";
        /// <summary>
        /// 當看到 PixelFormat為 Rgba64 ，等於相機是使用12bit(BGR12Unpacked)
        /// </summary>
        public  System.Windows.Media.PixelFormat PixelFormat
        {
            get
            {
                cam.PixelFormat.Get(out ColorMode mode);
                if (formatMap.TryGetValue(mode, out System.Windows.Media.PixelFormat format)) return format;
                throw new InvalidCastException($"Can not converter native pixelformat '{mode}'.");
            }
            set
            {
                // 將記錄對應格式的 Dictionary 內的 Key / Value 倒置。
                if (!formatMap.ReverseKeyValue().TryGetValue(value, out ColorMode mode))
                    throw new NotSupportedException($"{value} format is not supported.");

                cam.PixelFormat.Set(mode);

                ReallocateMemory(bufferCount);
            }
        }

        public  int Width
        {
           
            get => GetWidth();
            //set
            //{
            //    // 為了調整 Width 後畫面仍維持中心點不變動，所以必須改動 Offset 來移動畫面的位置，
            //    // 而 Offset 變化量 (offsetΔ) 必須是 Width 變化量 (widthΔ) 的一半。
            //    // 例如 Width 少 4 則 Offset 要加 2，offsetΔ 必須是 widthΔ 的 1/2 倍，才能維持畫面中心點不變動。

            //    // 另外因 Width 的變動必須增加量 (Increment) 倍數，所以必須使用數學捨入至最接近 Increment 的倍數值，
            //    // 例如 Width 變化量是 17 而增加量是 8，RoundDown 後變化量變成 16；以此類推變化量是 27，捨入後變成 24 (8 的 3 倍)，
            //    // 因為 offsetΔ 必須是 widthΔ 的 1/2 倍，所以用來做捨入的增加量會 x2，以上述例子就是增加量由 8 變成 16。

            //    // 以下情況是預設在 Width 與 Offset 的增加量是相同的情況下發生，一般而言是要相同的，
            //    // 如果不同則需要算出兩者的最小公倍數，當成 Width 改變量的捨入目標值。

            //    if (WidthRange.IsEmpty())
            //    {
            //        SetWidth(value); return;
            //    }

            //    if (!WidthRange.Contains(value))
            //        throw new ArgumentOutOfRangeException($"{nameof(Width)} range is {WidthRange}.");

            //    value = value.RoundDown(WidthRange.Inc * 2);
            //    if (!WidthRange.Contains(value)) return;

            //    var offset = (WidthRange.Max - value) / 2;

            //    // 寬變大要先調整 Offset 在調整寬才不會跑出最大範圍；寬變小則先調整寬再調整 Offset。
            //    if (value > Width)
            //    { OffsetX = offset; SetWidth(value); }
            //    else
            //    { SetWidth(value); OffsetX = offset; }
            //}
        }

       

        public  int Height
        {
            get => GetHeight();
            //set
            //{
            //    if (HeightRange.IsEmpty())
            //    {
            //        SetHeight(value); return;
            //    }

            //    if (!HeightRange.Contains(value))
            //        throw new ArgumentOutOfRangeException($"{nameof(Height)} range is {HeightRange}.");

            //    value = value.RoundDown(HeightRange.Inc * 2);
            //    if (!HeightRange.Contains(value)) return;

            //    var offset = (HeightRange.Max - value) / 2;

            //    // 高變大要先調整 Offset 在調整高才不會跑出最大範圍；反之高變小則先調整高再調整 Offset。
            //    if (value > Height)
            //    { OffsetY = offset; SetHeight(value); }
            //    else
            //    { SetHeight(value); OffsetY = offset; }
            //}
        }

        private int GetHeight()
        {
            cam.Size.AOI.Get(out int s32PosX, out int s32PosY, out int s32Width, out int s32Height);
            return s32Height;
        }
        private int GetWidth()
        {
            cam.Size.AOI.Get(out int s32PosX, out int s32PosY, out int s32Width, out int s32Height);
            return s32Width;
        }
        public string Model { get; }

        /// <summary>
        /// 取得 相機開啟狀態
        /// </summary>
        public bool IsOpen => cam.IsOpened;

        #endregion
        public double Gain
        {
            get
            {
                cam.Gain.Hardware.Scaled.GetMaster(out int gain);
                return gain;
            }
            set
            {
                if (!GainRange.Contains(value))
                    throw new ArgumentOutOfRangeException($"{nameof(Gain)} range is {GainRange}.");

                cam.Gain.Hardware.Scaled.SetMaster((int)value);
            }
        }

        public double ExposureTime
        {
            get
            {
                cam.Timing.Exposure.Get(out double exposureTime);
                return exposureTime;
            }
            set
            {
                if (!ExposureTimeRange.Contains(value))
                    throw new ArgumentOutOfRangeException($"{nameof(ExposureTime)} range is {ExposureTimeRange}.");

                cam.Timing.Exposure.Set(value);
            }
        }

        public double FrameRate
        {
            get
            {
                cam.Timing.Framerate.Get(out double rate);
                return rate;
            }
            set
            {
                if (!FrameRateRange.Contains(value))
                    throw new ArgumentOutOfRangeException($"{nameof(FrameRate)} range is {FrameRateRange}.");

                cam.Timing.Framerate.Set(value);
            }
        }

        public bool TriggerMode
        {
            get
            {
                cam.Trigger.Get(out TriggerMode mode);
                return mode != uEye.Defines.TriggerMode.Off;
            }
            set => cam.Trigger.Set(value ? uEye.Defines.TriggerMode.Hi_Lo : uEye.Defines.TriggerMode.Off);
        }


        public  ValueRange<double> ExposureTimeRange
        {
            get
            {
               
                 cam.Timing.Exposure.GetRange(out Range<double> range);
                return new ValueRange<double>(range.Maximum, range.Minimum, range.Increment);
            }
        }

        public  ValueRange<double> FrameRateRange
        {
            get
            {
                cam.Timing.Framerate.GetFrameRateRange(out Range<double> range);
                return new ValueRange<double>(range.Maximum, range.Minimum, range.Increment);
            }
        }

        public  ValueRange<double> GainRange => new ValueRange<double>(100, 0, 1);

        public  ValueRange<int> WidthRange
        {
            get
            {
                cam.Size.AOI.GetSizeRange(out Range<int> widthRange, out _);
                return new ValueRange<int>(widthRange.Maximum, widthRange.Minimum, widthRange.Increment);
            }
        }

        public  ValueRange<int> HeightRange
        {
            get
            {
                cam.Size.AOI.GetSizeRange(out _, out Range<int> heightRange);
                return new ValueRange<int>(heightRange.Maximum, heightRange.Minimum, heightRange.Increment);
            }
        }

        protected  int OffsetX
        {
            get
            {
                cam.Size.AOI.Get(out Rectangle rect);
                return rect.X;
            }

            set
            {
                cam.Size.AOI.Get(out Rectangle aoi);
                aoi.X = value;
                cam.Size.AOI.Set(aoi);

                cam.Size.AOI.Get(out aoi);
                if (aoi.X != value) throw new InvalidOperationException();
            }
        }

        protected  int OffsetY
        {
            get
            {
                cam.Size.AOI.Get(out Rectangle rect);
                return rect.Y;
            }

            set
            {
                cam.Size.AOI.Get(out Rectangle aoi);
                aoi.Y = value;
                cam.Size.AOI.Set(aoi);
            }
        }

        public void Open()
        {
            if (IsOpen) return;

            // 初始化時使用 DeviceId 需要與 DeviceEnumeration.UseDeviceID 做 or 運算；使用 CameraId 則不需要。
            // PS:使用 serialNumber 建構時得到的 ID 為 DeviceId。

            Status result = Status.Success;

            result = cam.Init(ID);
            if (result != Status.Success) throw new InvalidOperationException($"Failed to initilize camera. message = '{result}'.");

            cam.Exit(); // 初始化後再次關閉是為了防止前一次相機不正常關閉導致產生修改相機的屬性卻無法取得已修改的屬性值，如 Width 或 Height 等...

            result = cam.Init(ID);
            if (result != Status.Success) throw new InvalidOperationException($"Failed to initilize camera. message = '{result}'.");

            cam.ThrowIfNoAuthorization();

            cam.AllocImageMems(bufferCount);
            cam.InitSequence();

            frames = Observable.FromEventPattern(
                h => cam.EventFrame += h,
                h => cam.EventFrame -= h)
                .Select(OnFrameReceived)
                .Where(frame => frame != null)
                .ObserveOn(TaskPoolScheduler.Default);
                 


            // 載入儲存於相機內的設定。
            Load();
        }

        public void Close()
        {
            if (!IsOpen) return;
            if (IsGrabbing) Stop();
            Status result = cam.Exit();
            if (result != Status.Success) throw new InvalidOperationException($"Failed to close camera. message = '{result}'.");
        }

        public void Dispose()
        {
            Close();
        }
        public void Load(string config = null)
        {
            // 暫存取像狀態
            bool tempIsGrabbing = IsGrabbing;
            if (tempIsGrabbing) Stop();

            cam.ClearSequence();
            cam.FreeImageMems();

            if (!string.IsNullOrEmpty(config))
            {
                if (!File.Exists(config)) throw new FileNotFoundException("Camera config not found.", config);
                cam.Parameter.Load(config);
            }
            else cam.Parameter.Load();

            cam.AllocImageMems(bufferCount);
            cam.InitSequence();

            if (tempIsGrabbing) Grab();
        }

        public void Save(string config)
        {
            string path = System.Environment.CurrentDirectory; // 取得當前目錄
            string fullPath = Path.Combine(path, config);
            cam.Parameter.Save(fullPath);
        }

        #region Acquisition Methods

        protected  void AcquisitionStart()
        {
        
            // 開始取像前重新配置 Buffer，避免寬與高有被變動過。
            //AllocateMemoryAndBuffer();
            cam.Acquisition.Capture();
            IsGrabbing = true;
        }

        protected  void AcquisitionStop()
        {
            cam.Acquisition.Stop();
            IsGrabbing = false;
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        #endregion


        #region Grab Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDisposable Grab()
        {
            if (IsGrabbing) return null;//throw new InvalidOperationException($"Camera({ID}) 重複呼叫 Grab() 方法。");

            try
            {
          
                AcquisitionStart();
                return Disposable.Create(Stop);
            }
            catch (Exception ex)
            {
                Stop();
                throw ex;
            }
        }

        public async Task<BitmapSource> GrabAsync()
        {
            //  using (var grab = IsGrabbing ? Disposable.Empty : Grab())
            //      return await Frames.Take(1).Timeout(TimeSpan.FromSeconds(3));
            return null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (IsGrabbing)
            {
                AcquisitionStop();
            }
        }



        #endregion


        // 相機擷取 Frame 後的 callback 處理方法。
        private Frame<byte[]> OnFrameReceived(EventPattern<object> ev)
        {
            try
            {
                // 記憶體的 GetLast 與 Lock 有機率失敗，尤其在改變寬或高屬性的時候，
                // 導致後續的動作失敗造成錯誤，所以必須卡控並在轉換為 Rx 的時候避掉傳出 null。
                if (cam.Memory.GetLast(out int memId) != Status.Success) return null;
                if (cam.Memory.Lock(memId) != Status.Success) return null;

                cam.Memory.Inquire(memId, out int width, out int height, out int bitsPerPixel, out int stride);
                cam.Memory.GetBitsPerPixel(memId, out int bpp);
                int bytesPerPixel = (bpp + 7) / 8;

                int bufferSize = width * height * bytesPerPixel;
                if (buffer == null || buffer.Length != bufferSize) buffer = new byte[bufferSize];

                // 使用 iDS 複製到陣列的 API 會飆記憶體，故自行使用 Marshal.Copy 取代。
                //cam.Memory.CopyToArray(memId, out buffer); 

                cam.Memory.ToIntPtr(out IntPtr source);
                Marshal.Copy(source, buffer, 0, buffer.Length);
                cam.Memory.Unlock(memId);
            //    var bmp = buffer.ToBitmapSource(width, height, PixelFormat);
                
               
                return new Frame<byte[]>(buffer, width, height, PixelFormat, Resolution);
            }
            catch (Exception ex)
            {
              // Logger.Default.Error(ex, "");
                return null;
            }
        }

        //變動寬高或像素格式屬性後需重新分配記憶體。
        private void ReallocateMemory(int count)
        {
            cam.Acquisition.HasStarted(out bool isLive);
            if (isLive) cam.Acquisition.Stop();

            cam.ClearSequence();
            cam.FreeImageMems();
            cam.AllocImageMems(count);
            cam.InitSequence();

            if (isLive) cam.Acquisition.Capture();
        }

        /// <summary>
        /// 取得插在電腦上 IDS 可用相機訊息
        /// </summary>
        /// <returns>回傳 Camera Id 對應的相機序號</returns>
        public static Dictionary<int, string> GetAvailableCamerasInfo()
        {
            CameraInformation[] cameraList = new CameraInformation[0];
            uEye.Info.Camera.GetCameraList(out cameraList);
            if (!cameraList.Any()) return new Dictionary<int, string>();

            return cameraList.Select(info => info)
                             .ToDictionary(info => info.CameraID, info => info.SerialNumber);
        }

        //public Rectangle SetROI(Rectangle roi)
        //{
        //    Rectangle currROI = new Rectangle(OffsetX, OffsetY, Width, Height);

        //    try
        //    {
        //        cam.Size.AOI.GetSizeRange(out Range<int> rangeWidth, out Range<int> rangeHeight);

        //        int adjWidth = (roi.Width / rangeWidth.Increment) * rangeWidth.Increment;
        //        int adjHeight = (roi.Height / rangeHeight.Increment) * rangeHeight.Increment;

        //        cam.Size.AOI.GetPosRange(out Range<int> rangePosX, out Range<int> rangePosY);

        //        int adjX = (roi.X / rangePosX.Increment) * rangePosX.Increment;
        //        int adjY = (roi.Y / rangePosY.Increment) * rangePosY.Increment;

        //        cam.Size.AOI.Set(adjX, adjY, adjWidth, adjHeight);
        //        cam.Timing.Framerate.GetFrameRateRange(out Range<double> fpsRange);
        //        cam.Timing.Framerate.Set(fpsRange.Maximum);
        //        return new Rectangle(OffsetX, OffsetY, Width, Height);
        //    }
        //    catch (Exception ex)
        //    {
        //        Width = currROI.Width;
        //        Height = currROI.Height;
        //        OffsetX = currROI.X;
        //        OffsetY = currROI.Y;
        //        throw ex;
        //    }
        //    finally
        //    {
        //        int[] mems;
        //        cam.Memory.GetList(out mems);
        //        cam.Memory.Free(mems);

        //        // 重新分配載入設定檔後的記憶體空間。
        //        cam.Memory.Allocate();
        //    }
        //}

        public void GetRoiIncrement(out int xIncrement, out int yIncrement, out int widthIncrement, out int heightIncrement)
        {
            cam.Size.AOI.GetSizeRange(out Range<int> rangeWidth, out Range<int> rangeHeight);
            cam.Size.AOI.GetPosRange(out Range<int> rangePosX, out Range<int> rangePosY);
            xIncrement = rangePosX.Increment;
            yIncrement = rangePosY.Increment;
            widthIncrement = rangeWidth.Increment;
            heightIncrement = rangeHeight.Increment;
        }


    }



    /// <summary>
    /// 參考 iDS 範例中的 uEye_DotNet_Cockpit 專案
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名樣式", Justification = "<暫止>")]
    static class uEyeEx
    {
        public static Status AllocImageMems(this uEye.Camera Camera, int nCount)
        {
            Status statusRet = Status.SUCCESS;

            for (int i = 0; i < nCount; i++)
            {
                statusRet = Camera.Memory.Allocate();

                if (statusRet != Status.SUCCESS)
                {
                    FreeImageMems(Camera);
                }
            }
            return statusRet;
        }

        public static Status FreeImageMems(this uEye.Camera Camera)
        {
            Status statusRet = Camera.Memory.GetList(out int[] idList);

            if (Status.SUCCESS == statusRet)
            {
                foreach (int nMemID in idList)
                {
                    do
                    {
                        statusRet = Camera.Memory.Free(nMemID);

                        if (Status.SEQ_BUFFER_IS_LOCKED == statusRet)
                        {
                            Thread.Sleep(1);
                            continue;
                        }

                        break;
                    }
                    while (true);
                }
            }
            return statusRet;
        }

        public static Status InitSequence(this uEye.Camera Camera)
        {
            Status statusRet = Camera.Memory.GetList(out int[] idList);

            if (Status.SUCCESS == statusRet)
            {
                statusRet = Camera.Memory.Sequence.Add(idList);

                if (Status.SUCCESS != statusRet)
                {
                    ClearSequence(Camera);
                }
            }
            return statusRet;
        }

        public static Status ClearSequence(this uEye.Camera Camera)
        {
            return Camera.Memory.Sequence.Clear();
        }

        [Conditional("L201")]
        public static void ThrowIfNoAuthorization(this uEye.Camera cam)
        {
            cam.Information.GetCameraInfo(out CameraInfo camInfo);
            if (camInfo.ID != "StrokePae") throw new UnauthorizedAccessException("Unauthorized Access. Code:201");
        }
    }

}
