using goonØ.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace goonØ
{

    public interface IAreaCamera : IAreaCamera<byte[]>
    {
    }

    public interface IAreaCamera<T> : INotifyPropertyChanged
    {
        IObservable<Frame<T>> Frames { get; }

        IDisposable Grab();

        Task<Frame<T>> GrabAsync();

        void Stop();
    }

    public abstract class AreaCamera : IAreaCamera, IAreaCamera<byte[]>, IDevice
    {
        public AreaCamera(int id)
        {
            goonØ.ThrowIfNoAuthorization();
            ID = id;
        }

        private void AreaCamera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("Range")) return;
            PropertyChanged(sender, new PropertyChangedEventArgs(nameof(WidthRange)));
            // 當有任何屬性變更，刷新所有範圍屬性。    
            Console.WriteLine($"{e.PropertyName} changed.");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public abstract IObservable<Frame<byte[]>> Frames { get; }

        public abstract bool IsGrabbing { get; }

        #region Device Property

        public int ID { get; protected set; }

        public string Name { get; set; } = DeviceNames.AreaCamera;

        public abstract string Manufacturer { get; }

        public abstract string Model { get; }

        public string SerialNumber { get; protected set; }

        #endregion

        public Size Resolution { get; set; } = new Size(1.0, 1.0);

        #region Camera Property

        public int Channel => (PixelFormat.BitsPerPixel + 7) / 8;

        public virtual int Width
        {
            get => GetWidth();
            set
            {
                // 為了調整 Width 後畫面仍維持中心點不變動，所以必須改動 Offset 來移動畫面的位置，
                // 而 Offset 變化量 (offsetΔ) 必須是 Width 變化量 (widthΔ) 的一半。
                // 例如 Width 少 4 則 Offset 要加 2，offsetΔ 必須是 widthΔ 的 1/2 倍，才能維持畫面中心點不變動。

                // 另外因 Width 的變動必須增加量 (Increment) 倍數，所以必須使用數學捨入至最接近 Increment 的倍數值，
                // 例如 Width 變化量是 17 而增加量是 8，RoundDown 後變化量變成 16；以此類推變化量是 27，捨入後變成 24 (8 的 3 倍)，
                // 因為 offsetΔ 必須是 widthΔ 的 1/2 倍，所以用來做捨入的增加量會 x2，以上述例子就是增加量由 8 變成 16。

                // 以下情況是預設在 Width 與 Offset 的增加量是相同的情況下發生，一般而言是要相同的，
                // 如果不同則需要算出兩者的最小公倍數，當成 Width 改變量的捨入目標值。

                if (WidthRange.IsEmpty())
                {
                    SetWidth(value); return;
                }

                if (!WidthRange.Contains(value))
                    throw new ArgumentOutOfRangeException($"{nameof(Width)} range is {WidthRange}.");

                value = value.RoundDown(WidthRange.Inc * 2);
                if (!WidthRange.Contains(value)) return;

                var offset = (WidthRange.Max - value) / 2;

                // 寬變大要先調整 Offset 在調整寬才不會跑出最大範圍；寬變小則先調整寬再調整 Offset。
                if (value > Width)
                { OffsetX = offset; SetWidth(value); }
                else
                { SetWidth(value); OffsetX = offset; }
            }
        }

        public virtual int Height
        {
            get => GetHeight();
            set
            {
                if (HeightRange.IsEmpty())
                {
                    SetHeight(value); return;
                }

                if (!HeightRange.Contains(value))
                    throw new ArgumentOutOfRangeException($"{nameof(Height)} range is {HeightRange}.");

                value = value.RoundDown(HeightRange.Inc * 2);
                if (!HeightRange.Contains(value)) return;

                var offset = (HeightRange.Max - value) / 2;

                // 高變大要先調整 Offset 在調整高才不會跑出最大範圍；反之高變小則先調整高再調整 Offset。
                if (value > Height)
                { OffsetY = offset; SetHeight(value); }
                else
                { SetHeight(value); OffsetY = offset; }
            }
        }

        public abstract PixelFormat PixelFormat { get; set; }

        public virtual bool ReverseX { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual bool ReverseY { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual double ExposureTime { get; set; }

        public virtual double FrameRate { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual double Gain { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual bool TriggerMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual ValueRange<double> ExposureTimeRange => ValueRange<double>.Empty;

        public virtual ValueRange<double> FrameRateRange => ValueRange<double>.Empty;

        public virtual ValueRange<double> GainRange => ValueRange<double>.Empty;

        public virtual ValueRange<int> WidthRange => ValueRange<int>.Empty;

        public virtual ValueRange<int> HeightRange => ValueRange<int>.Empty;

        protected virtual int OffsetX { get; set; }

        protected virtual int OffsetY { get; set; }

        #endregion

        public abstract void Open();

        public abstract void Close();

        public abstract void Dispose();

        #region Grab Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public IDisposable Grab()
        {
            if (IsGrabbing) return null;//throw new InvalidOperationException($"Camera({ID}) 重複呼叫 Grab() 方法。");

            try
            {
                goonØ.ThrowIfNoAuthorization();
                AcquisitionStart();
                return Disposable.Create(Stop);
            }
            catch (Exception ex)
            {
                Stop();
                throw new DeviceException(this, 206, ex);
            }
        }

        public async Task<Frame<byte[]>> GrabAsync()
        {
            using (var grab = IsGrabbing ? Disposable.Empty : Grab())
                return await Frames.Take(1).Timeout(TimeSpan.FromSeconds(3));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Stop()
        {
            if (IsGrabbing)
            {
                AcquisitionStop();
            }
        }

        /// <summary>
        /// 開始擷取影像。
        /// </summary>
        protected abstract void AcquisitionStart();

        /// <summary>
        /// 停止擷取影像。
        /// </summary>
        protected abstract void AcquisitionStop();

        #endregion       

        protected abstract int GetHeight();

        protected abstract void SetHeight(int value);

        protected abstract int GetWidth();

        protected abstract void SetWidth(int value);
    }

    /// <summary>
    /// 表示使用者設定，名稱規則依循 GenICam 的名稱，請勿更動。
    /// </summary>
    public enum UserSetSelectors
    {
        Default = 0,

        UserSet0 = 1,

        UserSet1 = 2,

        UserSet2 = 3,

        UserSet3 = 4
    }
}
