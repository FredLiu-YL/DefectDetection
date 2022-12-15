using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Reactive.Linq
{
    public static class RxLinq
    {


        /// <summary>
        /// Cancel all subscriptions on observable sequence and throw a OperationCanceledException to observers.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="token">Propagates notification that sequence should be canceled.</param>
        /// <returns></returns>
        public static IObservable<TSource> CancelBy<TSource>(this IObservable<TSource> source, CancellationToken token)
        {
            return Observable.Create<TSource>(observer =>
            {
                token.Register(async () =>
                {
                        // Task.Yield() 是為了將底下的 OnError 換到另一條 Thread，
                        // 避免 Dead Lock (尤其在 Console 下特別容易發生)。
                        await Task.Yield();
                    observer.OnError(new OperationCanceledException());
                });
                return source.Subscribe(observer);
            });
        }

        public static IObservable<TSource> ObserveLatestOn<TSource>(this IObservable<TSource> source)
            => source.ObserveLatestOn(CurrentThreadScheduler.Instance);

        /// <summary>
        /// Asynchronously lastest notify observers on the specified scheduler.
        /// </summary>
        /// <typeparam name="TSource">The type of source.</typeparam>
        /// <param name="source">The source sequence.</param>
        /// <param name="scheduler">The scheduler to notify observers on.</param>
        /// <returns>The source sequence whose observations happen on the specified scheduler.</returns>
        public static IObservable<TSource> ObserveLatestOn<TSource>(this IObservable<TSource> source, IScheduler scheduler)
        {
            return Observable.Create<TSource>(observer =>
            {
                Notification<TSource> curNotification = null;
                var cancelable = new MultipleAssignmentDisposable();

                var subscription = source
                .Materialize()
                .Subscribe(notification =>
                {
                        // 將 curNotification 設定為 notification，並傳回 curNotification 的值成為 preNotification，
                        // 若 preNotification 不等於 null 則代表前一次的 OnNext() 尚在執行，直接跳過此次 OnNext()。
                        var preNotification = Interlocked.Exchange(ref curNotification, notification);
                    if (preNotification != null) return;

                    cancelable.Disposable = scheduler.Schedule(() =>
                    {
                        curNotification.Accept(observer);
                        Interlocked.Exchange(ref curNotification, null);
                    });
                });

                return new CompositeDisposable(subscription, cancelable);
            });
        }

        /// <summary>
        /// Pause the observable sequence.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="token">Propagates notification that sequence should be paused.</param>
        /// <returns></returns>
        public static IObservable<TSource> PauseBy<TSource>(this IObservable<TSource> source, PauseToken token)
        {
            return source.Do(_ => token.WaitWhilePausedAsync().Wait());
        }

        /// <summary>
        /// 將可訂閱序列的每一元素與前一元素綁定成一對後形成新的序列。
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(this IObservable<TSource> source)
            => source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current));


        /// <summary>
        /// Add TimeInterval to each onNext().
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="onNext">Action to invoke for each element in observable sequence.</param>
        /// <returns></returns>
        public static IObservable<TimeInterval<TSource>> TimeInterval<TSource>(this IObservable<TSource> source, Action<TSource> onNext)
        {
            return source
                .Timestamp()
                .Do(e => onNext(e.Value))
                .Timestamp()
                .Select(e =>
                {
                    var endTime = e.Timestamp;
                    var startTime = e.Value.Timestamp;

                    var value = e.Value.Value;
                    var interval = endTime - startTime;

                    return new TimeInterval<TSource>(value, interval);
                });
        }
    }

    public interface IRxValue<T> : IObservable<T>, INotifyPropertyChanged, IDisposable
    {
        T Value { get; set; }
    }

    public interface IReadOnlyRxValue<T> : IObservable<T>, INotifyPropertyChanged, IDisposable
    {
        T Value { get; }
    }

    /// <summary>
    /// 表示可被訂閱其改變的值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RxValue<T> : IRxValue<T>
        where T : struct, IComparable, IComparable<T>, IEquatable<T>
    {
        //private static readonly IScheduler eventScheduler;

        //static RxValue()
        //{
        //    if (SynchronizationContext.Current == null)
        //        eventScheduler = ImmediateScheduler.Instance;
        //    else
        //        eventScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
        //}

        //public static class PropertyChangedNotificationInterceptor
        //{
        //    public static void Intercept(object target, Action onPropertyChangedAction, string propertyName)
        //    {

        //    }
        //}

        private readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        private readonly Subject<T> source = new Subject<T>();
        private readonly CompositeDisposable subscriptions = new CompositeDisposable();

        private T latestValue = default;
        private readonly Func<T> getValue;
        private readonly Action<T> setValue;
        private readonly Func<ValueRange<T>> getRangel = () => null;

        public RxValue(T initialValue = default)
        {
            latestValue = initialValue;
            getValue = () => latestValue;
            setValue = v => latestValue = v;
        }

        public RxValue(Func<T> getValue, Action<T> setValue, Func<ValueRange<T>> getRange)
        {
            this.getValue = getValue;
            this.setValue = setValue;
            this.getRangel = getRange;
            ReadOnly = false;
        }

        public string Name { get; set; }

        public bool ReadOnly { get; }

        public T Value
        {
            get => getValue();
            set
            {
                if (ReadOnly) throw new NotSupportedException($"{Name ?? "Value"} is readonly.");
                if (comparer.Equals(latestValue, value)) return;
                if (Range != null && !Range.Contains(value))
                    throw new ArgumentOutOfRangeException($"{Name ?? "Value"} range is {Range}");

                setValue(value);
                source.OnNext(value);

                //eventScheduler.Schedule(() =>
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))));
            }
        }

        public ValueRange<T> Range => getRangel();

        public event PropertyChangedEventHandler PropertyChanged;

        public static implicit operator T(RxValue<T> property) => property.Value;

        //public static implicit operator RxValue<T>(T value) => new RxValue<T>(value);

        public override string ToString() => $"{latestValue}{(Range != null ? $" ({Range})" : "")}";

        public void Dispose()
        {
            if (subscriptions.IsDisposed) return;

            PropertyChanged = null;
            source.OnCompleted();
            subscriptions.Dispose();
        }

        /// <summary>
        /// 訂閱後當值改變時會發出通知，並在訂閱的同時時發出通知當前的值。
        /// </summary>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (subscriptions.IsDisposed)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            var subscription = source.Subscribe(observer);
            subscriptions.Add(subscription);
            return subscription;
        }
    }


    public class ReadOnlyRxValue<T> : IReadOnlyRxValue<T>
    {
        // eventScheduler 是為了再發出 PropertyChanged 的事件時可以自動抓到當時的主執行緒，
        // 例如 Dispather 執行緒，避免 Bonding 時產生 Cross Thread 的錯誤，
        // 但這必須建立在使用 ReadOnlyRxValue 當屬性的類別在主執行緒上被建立，通常都是會這樣的情況，
        // 等哪天真的必須再副執行緒上物件被建立且這個屬性又要被使用來 Binding 時，在開放一個靜態屬性讓使用者可以自行設定 Scheduler。
        private static readonly IScheduler eventScheduler;

        private readonly IEqualityComparer<T> comparer = EqualityComparer<T>.Default;
        private readonly CompositeDisposable subscriptions = new CompositeDisposable();
        private readonly Subject<T> innerSource = new Subject<T>();

        static ReadOnlyRxValue()
        {
            if (SynchronizationContext.Current == null)
                eventScheduler = ImmediateScheduler.Instance;
            else
                eventScheduler = new SynchronizationContextScheduler(SynchronizationContext.Current);
        }

        public ReadOnlyRxValue(IObservable<T> source, T initialValue = default)
        {
            Value = initialValue;

            var sub =
                source
                .DistinctUntilChanged(comparer)
                .Do(value =>
                {
                    Value = value;
                    innerSource.OnNext(value);
                })
                .ObserveOn(eventScheduler)
                .Subscribe(value => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value))));

            subscriptions.Add(sub);
        }

        public static implicit operator T(ReadOnlyRxValue<T> property) => property.Value;

        public event PropertyChangedEventHandler PropertyChanged;

        public T Value { get; private set; }

        public void Dispose()
        {
            if (subscriptions.IsDisposed) return;

            PropertyChanged = null;

            innerSource.OnCompleted();
            subscriptions.Dispose();
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (subscriptions.IsDisposed)
            {
                observer.OnCompleted();
                return Disposable.Empty;
            }

            var sub = innerSource.Subscribe(observer);
            subscriptions.Add(sub);

            return sub;
        }

    }

    /// <summary>
    /// 表示值範圍。
    /// </summary>
    public class ValueRange<T> : INotifyPropertyChanged
        where T : struct, IComparable, IComparable<T>, IEquatable<T>
    {
        public ValueRange(T max, T min, T inc)
        {
            if (min.CompareTo(max) > 0) throw new ArgumentException($"{nameof(max)} must be greater than {nameof(min)}.");

            Min = min;
            Max = max;
            Inc = inc;
        }

        public static ValueRange<T> Empty => new ValueRange<T>(default, default, default);


        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 取得範圍的最小值。
        /// </summary>
        public T Min
        {
            get; set;
        }

        /// <summary>
        /// 取得範圍的最大值。
        /// </summary>
        public T Max
        {
            get; set;
        }

        /// <summary>
        /// 取得值變動的最小增加(減少)量。
        /// </summary>
        public T Inc { get; set; }

        public static implicit operator (T Min, T Max)(ValueRange<T> range) => (range.Min, range.Max);

        public static bool operator <(ValueRange<T> range, T value) => value.CompareTo(range.Min) < 0;

        public static bool operator <(T value, ValueRange<T> range) => value.CompareTo(range.Min) < 0;

        public static bool operator >(ValueRange<T> range, T value) => value.CompareTo(range.Max) > 0;

        public static bool operator >(T value, ValueRange<T> range) => value.CompareTo(range.Max) > 0;

        //public static bool operator ==(ValueRange<T> range1, ValueRange<T> range2) =>
        //    range1.Min.CompareTo(range2.Min) == 0 &&
        //    range1.Max.CompareTo(range2.Max) == 0 &&
        //    range1.Inc.CompareTo(range2.Inc) == 0;

        //public static bool operator !=(ValueRange<T> range1, ValueRange<T> range2) =>
        //    range1.Min.CompareTo(range2.Min) != 0 ||
        //    range1.Max.CompareTo(range2.Max) != 0 ||
        //    range1.Inc.CompareTo(range2.Inc) != 0;

      //  public override string ToString() => $"{Min} ~ {Max}";

        /// <summary>
        /// 取得指定值是否包含於當前的範圍中。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(T value) => (Min.CompareTo(value) <= 0 && value.CompareTo(Max) <= 0);

        public bool IsEmpty() =>
            Min.CompareTo(default) == 0 &&
            Max.CompareTo(default) == 0 &&
            Inc.CompareTo(default) == 0;
    }

}
