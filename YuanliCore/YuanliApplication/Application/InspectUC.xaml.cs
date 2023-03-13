using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YuanliCore.ImageProcess;
using YuanliCore.ImageProcess.Blob;
using YuanliCore.ImageProcess.Caliper;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace YuanliApplication.Application
{
    /// <summary>
    /// InspectUC.xaml 的互動邏輯
    /// </summary>
    public partial class InspectUC : UserControl, INotifyPropertyChanged
    {
        private BitmapSource sampleImage;
        private ObservableCollection<DisplayMethod> methodDispCollection = new ObservableCollection<DisplayMethod>();

        private int methodSelectIndex, methodCollectIndex, combineCollectionIndex;
        private OutputOption combineOptionSelected;
        //定位用樣本
        private IMatcher matcher = new CogMatcher(); //使用Vision pro 實體

        private static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(InspectUC), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty MatchParamProperty = DependencyProperty.Register(nameof(MatchParam), typeof(PatmaxParams), typeof(InspectUC), new FrameworkPropertyMetadata(new PatmaxParams(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty MethodListProperty = DependencyProperty.Register(nameof(MethodList), typeof(List<CogMethod>), typeof(InspectUC), new FrameworkPropertyMetadata(new List<CogMethod>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty CombineCollectionProperty = DependencyProperty.Register(nameof(CombineCollection), typeof(ObservableCollection<CombineOptionOutput>), typeof(InspectUC), new FrameworkPropertyMetadata(new ObservableCollection<CombineOptionOutput>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty RecipeProperty = DependencyProperty.Register(nameof(Recipe), typeof(MeansureRecipe), typeof(InspectUC), new FrameworkPropertyMetadata(new MeansureRecipe(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        
        private bool isCombineSecEnabled;
        private string combineOption;
        private int cB_CmbineSelectedIndexSN1, cB_CmbineSelectedIndexSN2 = -1;

        public InspectUC()
        {
            InitializeComponent();
        }
        /// <summary>
        /// BitmapSource 影像資料傳入
        /// </summary>
        public WriteableBitmap Image
        {
            get => (WriteableBitmap)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public PatmaxParams MatchParam
        {
            get => (PatmaxParams)GetValue(MatchParamProperty);
            set => SetValue(MatchParamProperty, value);
        }
        /// <summary>
        /// 演算法集合 
        /// </summary>
        public List<CogMethod> MethodList
        {
            get => (List<CogMethod>)GetValue(MethodListProperty);
            set => SetValue(MethodListProperty, value);
        }
        public MeansureRecipe Recipe
        {
            get => (MeansureRecipe)GetValue(RecipeProperty);
            set => SetValue(RecipeProperty, value);

        }
        


        /// <summary>
        /// 最終使用者需求的結果輸出
        /// </summary>
        public ObservableCollection<CombineOptionOutput> CombineCollection
        {
            get => (ObservableCollection<CombineOptionOutput>)GetValue(CombineCollectionProperty);
            set => SetValue(CombineCollectionProperty, value);
        }
        /// <summary>
        /// 顯示定位樣本圖
        /// </summary>
        public BitmapSource SampleImage { get => sampleImage; set => SetValue(ref sampleImage, value); }
        public int MethodSelectIndex { get => methodSelectIndex; set => SetValue(ref methodSelectIndex, value); }
        public int MethodCollectIndex { get => methodCollectIndex; set => SetValue(ref methodCollectIndex, value); }
        public OutputOption CombineOptionSelected { get => combineOptionSelected; set => SetValue(ref combineOptionSelected, value); }
        public int CombineCollectionIndex { get => combineCollectionIndex; set => SetValue(ref combineCollectionIndex, value); }

        public string CombineOption { get => combineOption; set => SetValue(ref combineOption, value); }
        public int CB_CmbineSelectedIndexSN1 { get => cB_CmbineSelectedIndexSN1; set => SetValue(ref cB_CmbineSelectedIndexSN1, value); }
        public int CB_CmbineSelectedIndexSN2 { get => cB_CmbineSelectedIndexSN2; set => SetValue(ref cB_CmbineSelectedIndexSN2, value); }

        public bool IsCombineSecEnabled { get => isCombineSecEnabled; set => SetValue(ref isCombineSecEnabled, value); }
        /// <summary>
        /// 使用演算法的集合 ， 提供給UI做資料顯示 
        /// </summary>
        public ObservableCollection<DisplayMethod> MethodDispCollection { get => methodDispCollection; set => SetValue(ref methodDispCollection, value); }
      



        public ICommand UnloadedCommand => new RelayCommand(async () =>
        {
            foreach (var item in MethodList) {
                item.Dispose();
            }
        });


        public ICommand MouseDoubleClickCommand => new RelayCommand(async () =>
        {
            MethodName methodName = MethodDispCollection[MethodCollectIndex].Name;

            switch (methodName) {
                case MethodName.PatternMatch:
                    var matcher = MethodList[MethodCollectIndex] as CogMatcher;
                    matcher.EditParameter(Image);
                    MethodDispCollection[MethodCollectIndex].ResultName = matcher.RunParams.ResultOutput.ToString();
                    break;
                case MethodName.GapMeansure:
                    var gapCaliper = MethodList[MethodCollectIndex] as CogGapCaliper;
                    gapCaliper.EditParameter(Image);
                    MethodDispCollection[MethodCollectIndex].ResultName = gapCaliper.RunParams.ResultOutput.ToString();
                    break;

                //case MethodName.BlobDetector:
                //    var blobDetector = MethodList[MethodCollectIndex] as CogBlobDetector;
                //    blobDetector.EditParameter(Image);
                //    MethodDispCollection[MethodCollectIndex].ResultName = blobDetector.BlobParam.ResultOutput.ToString();
                //    break;
                case MethodName.LineMeansure:

                    break;
                case MethodName.CircleMeansure:

                    break;
                default:
                    break;
            }



        });
        public ICommand MouseCombineClickCommand => new RelayCommand(async () =>
        {
            CombineOptionOutput selectData = CombineCollection[CombineCollectionIndex];

            CombineOptionSelected = selectData.Option;

            //   CombineOptionSelectedIndex = CombineOptionList.ToList().IndexOf(selectData.Option);

            switch (selectData.Option) {
                case OutputOption.Result:
                    CB_CmbineSelectedIndexSN1 = MethodDispCollection.Select(m => m.SN).ToList().IndexOf(selectData.SN1);
                    CB_CmbineSelectedIndexSN2 = -1;
                    break;

                case OutputOption.Distance:
                    CB_CmbineSelectedIndexSN1 = MethodDispCollection.Select(m => m.SN).ToList().IndexOf(selectData.SN1);
                    CB_CmbineSelectedIndexSN2 = MethodDispCollection.Select(m => m.SN).ToList().IndexOf(selectData.SN2);
                    break;

                default:
                    break;
            }



        });
        public ICommand ResultSelectionChangedCommand => new RelayCommand(async () =>
        {
            switch (CombineOptionSelected) {

                case OutputOption.Result:
                    CB_CmbineSelectedIndexSN2 = -1;
                    IsCombineSecEnabled = false;
                    break;


                case OutputOption.Distance:
                    if (CB_CmbineSelectedIndexSN2 == -1) CB_CmbineSelectedIndexSN2 = 0;
                    IsCombineSecEnabled = true;
                    break;

                default:
                    break;
            }

        });

        public ICommand AddCombineCommand => new RelayCommand(async () =>
        {
            try {
                if (MethodDispCollection.Count == 0) return;

                if (MethodDispCollection.Count == 0) return;
                //選出要組合的結果 0:直接輸出  1: 計算距離
                string option = $"{CombineOptionSelected}";

                string sn1 = MethodDispCollection[CB_CmbineSelectedIndexSN1].SN;

                string sn2 = "";
                if (CombineOptionSelected == OutputOption.Result)
                    sn2 = "null";
                else
                    sn2 = MethodDispCollection[CB_CmbineSelectedIndexSN2].SN;

                //在最後的時候檢查 如果計算距離 或 其他操作?  要判斷兩者的計算是否能成立  例:計算距離  就不能選擇結果直接輸出
                if (CombineOptionSelected > 0) {

                    //找出 對應SN的資料
                    DisplayMethod sn1Data = MethodDispCollection.First(m => m.SN == sn1);
                    DisplayMethod sn2Data = MethodDispCollection.First(m => m.SN == sn2);

                    var temp = ResultSelect.Full.ToString();
                    if (sn1Data.ResultName == temp || sn2Data.ResultName == temp)
                        throw new Exception("This action cannot be selected");
                }

                CombineCollection.Add(new CombineOptionOutput { Option = CombineOptionSelected, SN1 = sn1, SN2 = sn2 });
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }



        });

        public ICommand EditCombineCommand => new RelayCommand(async () =>
        {
            if (CombineCollection.Count == 0 || CombineCollectionIndex == -1) return;
            string option = $"{CombineOptionSelected}";
            string sn1 = MethodDispCollection[CB_CmbineSelectedIndexSN1].SN;
            string sn2 = "";
            if (CombineOptionSelected == 0)
                sn2 = "null";
            else
                sn2 = MethodDispCollection[CB_CmbineSelectedIndexSN2].SN;

            int i = CombineCollectionIndex;
            CombineCollection.RemoveAt(i);
            CombineCollection.Insert(i, new CombineOptionOutput { Option = CombineOptionSelected, SN1 = sn1, SN2 = sn2 });

        });

        public ICommand AddMethodCommand => new RelayCommand(async () =>
        {

            switch ((MethodName)MethodSelectIndex) {
                case MethodName.PatternMatch:

                    MethodDispCollection.Add(new DisplayMethod { SN = $"{MethodList.Count + 1}", Name = MethodName.PatternMatch, ResultName = $"{ResultSelect.Full}" });
                    MethodList.Add(new CogMatcher { MethodName = MethodName.PatternMatch, });

                    break;
                case MethodName.GapMeansure:
                    MethodDispCollection.Add(new DisplayMethod { SN = $"{MethodList.Count + 1}", Name = MethodName.GapMeansure, ResultName = $"{ResultSelect.Full}" });
                    MethodList.Add(new CogGapCaliper { MethodName = MethodName.GapMeansure });

                    break;

                //case MethodName.BlobDetector:
                //    MethodDispCollection.Add(new DisplayMethod { SN = $"{MethodList.Count + 1}", Name = MethodName.BlobDetector, ResultName = $"{ResultSelect.Full}" });
                //    MethodList.Add(new CogBlobDetector { MethodName = $"{MethodName.BlobDetector}" });
                //    break;


                case MethodName.LineMeansure:
                    MethodDispCollection.Add(new DisplayMethod { SN = $"{MethodList.Count + 1}", Name = MethodName.LineMeansure, ResultName = $"{ResultSelect.Full}" });
                    //    MethodCollection.Add(new CogGapCaliper { MethodName = $"{MethodName.LineMeansure}" });

                    break;
                case MethodName.CircleMeansure:
                    MethodDispCollection.Add(new DisplayMethod { SN = $"{MethodList.Count + 1}", Name = MethodName.CircleMeansure, ResultName = $"{ResultSelect.Full}" });
                    //    MethodCollection.Add(new CogGapCaliper { MethodName = $"{MethodName.CircleMeansure}" });

                    break;
                default:
                    break;
            }


        });
        public ICommand DeleteMethodCommand => new RelayCommand(async () =>
        {
            if (MethodCollectIndex < 0) return;

            MethodDispCollection.RemoveAt(MethodCollectIndex);
            MethodList.RemoveAt(MethodCollectIndex);
            for (int i = 0; i < MethodDispCollection.Count; i++) 
            {
                MethodDispCollection[i].SN = $"{i + 1}";
              
            }

        });
        public ICommand ClearMethodCommand => new RelayCommand(async () =>
        {
            MethodDispCollection.Clear();
            CombineCollection.Clear();
            MethodList.Clear();
        });
        public ICommand TestMethodCommand => new RelayCommand(async () =>
        {
            try {
            
                var frame = Image.ToByteFrame();
             
            //    YuanliVision yuanliVision = new YuanliVision();

            //    var results = await yuanliVision.Run(frame, MethodList, CombineCollection);

              
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }
            

        });
        public ICommand EditSampleCommand => new RelayCommand(async () =>
        {
            try {

                matcher.RunParams = MatchParam;
                matcher.EditParameter(Image);

                MatchParam = (PatmaxParams)matcher.RunParams;
                SampleImage = MatchParam.PatternImage;


            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }



        });
        public ICommand LOADCommand => new RelayCommand(async () =>
        {
            try {

                CogParameter temp =  CogParameter.Load("123-1",0);

                MethodList.Add(new CogGapCaliper(temp));
                MethodDispCollection.Add(new DisplayMethod { Name = temp.Methodname, ResultName = temp.ResultOutput.ToString()});

            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }


        });
        public ICommand SAVECommand => new RelayCommand(async () =>
        {
            try 
            {
                foreach (var item in MethodList) 
                {
                    item.RunParams.Save("123-1");
                }
                MethodList[0].RunParams.Save("123-1") ;


            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }


        });



        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            T oldValue = field;
            field = value;
            OnPropertyChanged(propertyName, oldValue, value);
        }
        protected virtual void OnPropertyChanged<T>(string name, T oldValue, T newValue)
        {
            // oldValue 和 newValue 目前沒有用到，代爾後需要再實作。
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }



        



        public class DisplayMethod : INotifyPropertyChanged
        {
            private string sN;
            private MethodName name;
            private string resultName;
            /// <summary>
            /// 序號  (陣列位置+1)
            /// </summary>
            public string SN { get => sN; set => SetValue(ref sN, value); }
            public MethodName Name { get => name; set => SetValue(ref name, value); }

            public string ResultName { get => resultName; set => SetValue(ref resultName, value); }




            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            {
                if (EqualityComparer<T>.Default.Equals(field, value)) return;
                T oldValue = field;
                field = value;
                OnPropertyChanged(propertyName, oldValue, value);
            }
            protected virtual void OnPropertyChanged<T>(string name, T oldValue, T newValue)
            {
                // oldValue 和 newValue 目前沒有用到，代爾後需要再實作。
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }



      
    }
}
