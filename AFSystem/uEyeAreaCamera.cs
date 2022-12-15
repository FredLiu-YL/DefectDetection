using goonØ;
using goonØ.Resources;
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
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using uEye;
using uEye.Defines;
using uEye.Types;

namespace goonα.iDS
{
    /// <summary>
    /// iDS 系列相機的的控制物件。
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:命名樣式", Justification = "<暫止>")]
    public class uEyeAreaCamera : AreaCamera, IAreaCamera<byte[]>, IDevice
    {
        private readonly Dictionary<ColorMode, System.Windows.Media.PixelFormat> formatMap = new Dictionary<ColorMode, System.Windows.Media.PixelFormat>
        {
            [ColorMode.BGR8Packed] = PixelFormats.Bgr24,
            [ColorMode.BGRA8Packed] = PixelFormats.Bgr32,
            [ColorMode.Mono8] = PixelFormats.Gray8,
            [ColorMode.BGR10Packed] = PixelFormats.Bgr101010,
            [ColorMode.BGR12Unpacked] = PixelFormats.Rgba64
        };
        private readonly Camera cam = new Camera();
        private readonly int bufferCount = 3;
        private byte[] buffer;
        private IObservable<Frame<byte[]>> frames;

        /// <summary>
        /// 建構 uEyeAreaCamera 相機，當有多支相機時 cameraId 並非每次啟動都是固定的，需使用 iDS 的官方軟體設定才可固定其 Id。
        /// </summary>
        /// <param name="cameraId">若將 cameraId 指定為 0 則自動抓取第一支可用相機，CameraId 可於 IDS Vision Cockpit 中修改。</param>
        public uEyeAreaCamera(int cameraId = 0)
            : base(cameraId)
        {
            uEye.Info.Camera.GetCameraList(out CameraInformation[] camInfoList);
            if (camInfoList.Length == 0) throw new DeviceException(201);

            CameraInformation camInfo;
            if (cameraId == 0) camInfo = camInfoList.First();
            else
            {
                var matches = camInfoList.Where(info => info.CameraID == cameraId);
                if (!matches.Any()) throw new DeviceException(202, $"CameraID = {cameraId}");
                camInfo = matches.First();
            }

            ID = camInfo.CameraID;
            SerialNumber = camInfo.SerialNumber;
            Model = camInfo.Model;
        }

        public override IObservable<Frame<byte[]>> Frames => frames;

        #region Device Infomation

        public override string Manufacturer => "iDS";

        public override string Model { get; }

        /// <summary>
        /// 取得 相機開啟狀態
        /// </summary>
        public bool IsOpen => cam.IsOpened;

        #endregion

        #region Frame Properties

        public override bool IsGrabbing
        {
            get
            {
                cam.Acquisition.HasStarted(out bool started);
                return started;
            }
        }

        /// <summary>
        /// 當看到 PixelFormat為 Rgba64 ，等於相機是使用12bit(BGR12Unpacked)
        /// </summary>
        public override System.Windows.Media.PixelFormat PixelFormat
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

        public override bool ReverseX
        {
            get
            {
                cam.RopEffect.Get(out RopEffectMode mode);
                return mode.HasFlag(RopEffectMode.LeftRight);
            }
            set => cam.RopEffect.Set(RopEffectMode.LeftRight, value);
        }

        public override bool ReverseY
        {
            get
            {
                cam.RopEffect.Get(out RopEffectMode mode);
                return mode.HasFlag(RopEffectMode.UpDown);
            }
            set => cam.RopEffect.Set(RopEffectMode.UpDown, value);
        }

        public override double Gain
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

        public override double ExposureTime
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

        public override double FrameRate
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

        public override bool TriggerMode
        {
            get
            {
                cam.Trigger.Get(out TriggerMode mode);
                return mode != uEye.Defines.TriggerMode.Off;
            }
            set => cam.Trigger.Set(value ? uEye.Defines.TriggerMode.Hi_Lo : uEye.Defines.TriggerMode.Off);
        }

        public override ValueRange<double> ExposureTimeRange
        {
            get
            {
                cam.Timing.Exposure.GetRange(out Range<double> range);
                return new ValueRange<double>(range.Maximum, range.Minimum, range.Increment);
            }
        }

        public override ValueRange<double> FrameRateRange
        {
            get
            {
                cam.Timing.Framerate.GetFrameRateRange(out Range<double> range);
                return new ValueRange<double>(range.Maximum, range.Minimum, range.Increment);
            }
        }

        public override ValueRange<double> GainRange => new ValueRange<double>(100, 0, 1);

        public override ValueRange<int> WidthRange
        {
            get
            {
                cam.Size.AOI.GetSizeRange(out Range<int> widthRange, out _);
                return new ValueRange<int>(widthRange.Maximum, widthRange.Minimum, widthRange.Increment);
            }
        }

        public override ValueRange<int> HeightRange
        {
            get
            {
                cam.Size.AOI.GetSizeRange(out _, out Range<int> heightRange);
                return new ValueRange<int>(heightRange.Maximum, heightRange.Minimum, heightRange.Increment);
            }
        }

        protected override int OffsetX
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

        protected override int OffsetY
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


        #endregion

        #region Initialize Methods

        public override void Open()
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

        public override void Close()
        {
            if (!IsOpen) return;
            if (IsGrabbing) Stop();
            Status result = cam.Exit();
            if (result != Status.Success) throw new InvalidOperationException($"Failed to close camera. message = '{result}'.");
        }

        public override void Dispose()
        {
            Close();
        }

        #endregion

        #region Acquisition Methods

        protected override void AcquisitionStart()
        {
            // 開始取像前重新配置 Buffer，避免寬與高有被變動過。
            //AllocateMemoryAndBuffer();
            cam.Acquisition.Capture();
        }

        protected override void AcquisitionStop()
        {
            cam.Acquisition.Stop();
        }

        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        public static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);

        #endregion

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

        protected override int GetWidth()
        {
            cam.Size.AOI.Get(out Rectangle rect);
            return rect.Width;
        }

        protected override void SetWidth(int value)
        {
            cam.Size.AOI.Get(out Rectangle aoi);
            aoi.Width = value;
            cam.Size.AOI.Set(aoi);

            ReallocateMemory(bufferCount);
        }

        protected override int GetHeight()
        {
            cam.Size.AOI.Get(out Rectangle rect);
            return rect.Height;
        }

        protected override void SetHeight(int value)
        {
            cam.Size.AOI.Get(out Rectangle aoi);
            aoi.Height = value;
            cam.Size.AOI.Set(aoi);
            ReallocateMemory(bufferCount);
        }

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
                return new Frame<byte[]>(buffer, width, height, PixelFormat, Resolution);
            }
            catch (Exception ex)
            {
                Logger.Default.Error(ex, "");
                return null;
            }
        }

        // 變動寬高或像素格式屬性後需重新分配記憶體。
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

        public Rectangle SetROI(Rectangle roi)
        {
            Rectangle currROI = new Rectangle(OffsetX, OffsetY, Width, Height);

            try
            {
                cam.Size.AOI.GetSizeRange(out Range<int> rangeWidth, out Range<int> rangeHeight);

                int adjWidth = (roi.Width / rangeWidth.Increment) * rangeWidth.Increment;
                int adjHeight = (roi.Height / rangeHeight.Increment) * rangeHeight.Increment;

                cam.Size.AOI.GetPosRange(out Range<int> rangePosX, out Range<int> rangePosY);

                int adjX = (roi.X / rangePosX.Increment) * rangePosX.Increment;
                int adjY = (roi.Y / rangePosY.Increment) * rangePosY.Increment;

                cam.Size.AOI.Set(adjX, adjY, adjWidth, adjHeight);
                cam.Timing.Framerate.GetFrameRateRange(out Range<double> fpsRange);
                cam.Timing.Framerate.Set(fpsRange.Maximum);
                return new Rectangle(OffsetX, OffsetY, Width, Height);
            }
            catch (Exception ex)
            {
                Width = currROI.Width;
                Height = currROI.Height;
                OffsetX = currROI.X;
                OffsetY = currROI.Y;
                throw ex;
            }
            finally
            {
                int[] mems;
                cam.Memory.GetList(out mems);
                cam.Memory.Free(mems);

                // 重新分配載入設定檔後的記憶體空間。
                cam.Memory.Allocate();
            }
        }

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
        public static Status AllocImageMems(this Camera Camera, int nCount)
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

        public static Status FreeImageMems(this Camera Camera)
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

        public static Status InitSequence(this Camera Camera)
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

        public static Status ClearSequence(this Camera Camera)
        {
            return Camera.Memory.Sequence.Clear();
        }

        [Conditional("L201")]
        public static void ThrowIfNoAuthorization(this Camera cam)
        {
            cam.Information.GetCameraInfo(out CameraInfo camInfo);
            if (camInfo.ID != "StrokePae") throw new UnauthorizedAccessException("Unauthorized Access. Code:201");
        }
    }
}
