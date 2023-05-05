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
        private PatmaxParams matchParam = new PatmaxParams(0);
        private int methodSelectIndex, methodCollectIndex, combineCollectionIndex;
        private OutputOption combineOptionSelected;
        private double pixelSize = 1;
        private YuanliVision yuanliVision = new YuanliVision();

        //定位用樣本
        private CogMatcher matcher = new CogMatcher(); //使用Vision pro 實體
        private ObservableCollection<CombineOptionOutput> combineCollection = new ObservableCollection<CombineOptionOutput>();
        private List<CogMethod> methodList = new List<CogMethod>();

        private static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(WriteableBitmap), typeof(InspectUC), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        //    private static readonly DependencyProperty MatchParamProperty = DependencyProperty.Register(nameof(MatchParam), typeof(PatmaxParams), typeof(InspectUC), new FrameworkPropertyMetadata(new PatmaxParams(0), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        //   private static readonly DependencyProperty MethodListProperty = DependencyProperty.Register(nameof(MethodList), typeof(List<CogMethod>), typeof(InspectUC), new FrameworkPropertyMetadata(new List<CogMethod>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        //    private static readonly DependencyProperty CombineCollectionProperty = DependencyProperty.Register(nameof(CombineCollection), typeof(ObservableCollection<CombineOptionOutput>), typeof(InspectUC), new FrameworkPropertyMetadata(new ObservableCollection<CombineOptionOutput>(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty RecipeProperty = DependencyProperty.Register(nameof(Recipe), typeof(MeansureRecipe), typeof(InspectUC), new FrameworkPropertyMetadata(new MeansureRecipe(), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnRecipeChanged)));
        private static readonly DependencyProperty IsLocatedProperty = DependencyProperty.Register(nameof(IsLocated), typeof(bool), typeof(InspectUC), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private static readonly DependencyProperty IsDetectionProperty = DependencyProperty.Register(nameof(IsDetection), typeof(bool), typeof(InspectUC), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        private static readonly DependencyProperty IsMeansureProperty = DependencyProperty.Register(nameof(IsMeansure), typeof(bool), typeof(InspectUC), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        private bool isCombineSecEnabled;
        private string combineOption;
        private int cB_CmbineSelectedIndexSN1, cB_CmbineSelectedIndexSN2 = -1;
        private double thresholdMin, thresholdMax;
        private bool isLocated;
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
        /// <summary>
        /// 定位參數
        /// </summary>
        public PatmaxParams MatchParam { get => matchParam; set => SetValue(ref matchParam, value); }
        //{
        //    get => (PatmaxParams)GetValue(MatchParamProperty);
        //    set => SetValue(MatchParamProperty, value);
        //}
        /// <summary>
        /// 演算法集合 
        /// </summary>
        //    public List<CogMethod> MethodList { get => methodList; set => SetValue(ref methodList, value); }
        //    {
        //        get => (List<CogMethod>) GetValue(MethodListProperty);
        //    set => SetValue(MethodListProperty, value);
        //}


        public MeansureRecipe Recipe
        {
            get => (MeansureRecipe)GetValue(RecipeProperty);
            set => SetValue(RecipeProperty, value);

        }
        public bool IsLocated// { get => isLocated; set => SetValue(ref isLocated, value); }
        {
            get => (bool)GetValue(IsLocatedProperty);
            set => SetValue(IsLocatedProperty, value);
        }

        public bool IsDetection// { get => isLocated; set => SetValue(ref isLocated, value); }
        {
            get => (bool)GetValue(IsDetectionProperty);
            set => SetValue(IsDetectionProperty, value);
        }
        public bool IsMeansure// { get => isLocated; set => SetValue(ref isLocated, value); }
        {
            get => (bool)GetValue(IsMeansureProperty);
            set => SetValue(IsMeansureProperty, value);
        }
        /// <summary>
        /// 最終使用者需求的結果輸出
        /// </summary>
        public ObservableCollection<CombineOptionOutput> CombineCollection { get => combineCollection; set => SetValue(ref combineCollection, value); }
        //{
        //    get => (ObservableCollection<CombineOptionOutput>)GetValue(CombineCollectionProperty);
        //    set => SetValue(CombineCollectionProperty, value);
        //}
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
        public double ThresholdMin { get => thresholdMin; set => SetValue(ref thresholdMin, value); }
        public double ThresholdMax { get => thresholdMax; set => SetValue(ref thresholdMax, value); }
        public double PixelSize
        {
            get => pixelSize;
            set
            {
                SetValue(ref pixelSize, value);
                UpdateRecipe();
            }
        }
        public bool IsCombineSecEnabled { get => isCombineSecEnabled; set => SetValue(ref isCombineSecEnabled, value); }


        /// <summary>
        /// 使用演算法的集合 ， 提供給UI做資料顯示 
        /// </summary>
        public ObservableCollection<DisplayMethod> MethodDispCollection { get => methodDispCollection; set => SetValue(ref methodDispCollection, value); }




        public ICommand UnloadedCommand => new RelayCommand(() =>
       {
           foreach (var item in yuanliVision.CogMethods) {
               item.Dispose();
           }
       });


        public ICommand MouseDoubleClickCommand => new RelayCommand(() =>
       {
           try {
               if (!IsLocated) throw new Exception(" locate is not yet complete");

               MethodName methodName = MethodDispCollection[MethodCollectIndex].Name;

               CogMethod method = yuanliVision.GetLocatedMethod(MethodCollectIndex);
               switch (methodName) {
                   case MethodName.PatternMatch:
                       CogMatcher matcher = method as CogMatcher;
                       //  var matcher = MethodList[MethodCollectIndex] as CogMatcher;
                       matcher.CogEditParameter();
                       MethodDispCollection[MethodCollectIndex].ResultName = matcher.RunParams.ResultOutput.ToString();

                       break;
                   case MethodName.GapMeansure:
                       CogGapCaliper gapCaliper = method as CogGapCaliper;
                       gapCaliper.CogEditParameter();
                       MethodDispCollection[MethodCollectIndex].ResultName = gapCaliper.RunParams.ResultOutput.ToString();

                       break;


                   case MethodName.LineMeansure:
                       CogLineCaliper lineCaliper = method as CogLineCaliper;
                       lineCaliper.CogEditParameter();
                       MethodDispCollection[MethodCollectIndex].ResultName = lineCaliper.RunParams.ResultOutput.ToString();

                       break;
                   case MethodName.CircleMeansure:

                       break;

                   case MethodName.BlobDetector:
                       CogBlobDetector blobDetector = method as CogBlobDetector;
                       blobDetector.CogEditParameter();
                       MethodDispCollection[MethodCollectIndex].ResultName = blobDetector.RunParams.ResultOutput.ToString();
                       break;
                   default:
                       break;
               }



           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }


       });
        public ICommand MouseCombineClickCommand => new RelayCommand(() =>
       {
           CombineOptionOutput selectData = CombineCollection[CombineCollectionIndex];

           CombineOptionSelected = selectData.Option;

           //   CombineOptionSelectedIndex = CombineOptionList.ToList().IndexOf(selectData.Option);

           switch (selectData.Option) {
               case OutputOption.None:
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

           ThresholdMax = selectData.ThresholdMax;
           ThresholdMin = selectData.ThresholdMin;

       });
        public ICommand ResultSelectionChangedCommand => new RelayCommand(() =>
       {
           switch (CombineOptionSelected) {

               case OutputOption.None:
                   CB_CmbineSelectedIndexSN2 = -1;
                   IsCombineSecEnabled = false;
                   break;


               case OutputOption.Distance:
                   if (CB_CmbineSelectedIndexSN2 == -1) CB_CmbineSelectedIndexSN2 = 0;
                   IsCombineSecEnabled = true;
                   break;
               case OutputOption.Angle:
                   if (CB_CmbineSelectedIndexSN2 == -1) CB_CmbineSelectedIndexSN2 = 0;
                   IsCombineSecEnabled = true;
                   break;
               default:
                   break;
           }

       });

        public ICommand AddCombineCommand => new RelayCommand(() =>
       {
           try {

               string sn1 = MethodDispCollection[CB_CmbineSelectedIndexSN1].SN;
               AddCombine(sn1);


           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }

           UpdateRecipe();

       });

        public ICommand EditCombineCommand => new RelayCommand(() =>
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
           CombineCollection.Insert(i, new CombineOptionOutput { Option = CombineOptionSelected, SN1 = sn1, SN2 = sn2, ThresholdMax = ThresholdMax, ThresholdMin = ThresholdMin });
           UpdateRecipe();
       });
        public ICommand DeleteCombineCommand => new RelayCommand(() =>
        {
            int i = CombineCollectionIndex;
            CombineCollection.RemoveAt(i);
            UpdateRecipe();
        });



        public ICommand AddMethodCommand => new RelayCommand(() =>
       {


           int sn = yuanliVision.CogMethods.Count + 1;

           //Combobox 要跟 MethodName 列舉的順序對上 比如 LineMeansure 列舉在第二個 ，combobox就放第二個
           switch ((MethodName)MethodSelectIndex) {
               case MethodName.PatternMatch:

                   MethodDispCollection.Add(new DisplayMethod { SN = $"{sn}", Name = MethodName.PatternMatch, ResultName = $"{ResultSelect.Full}" });
                   var cogM = new CogMatcher { MethodName = MethodName.PatternMatch, };
                   cogM.RunParams.Id = sn;
                   yuanliVision.CogMethods.Add(cogM);

                   break;
               case MethodName.GapMeansure:
                   MethodDispCollection.Add(new DisplayMethod { SN = $"{sn}", Name = MethodName.GapMeansure, ResultName = $"{ResultSelect.Full}" });
                   var cogGM = new CogGapCaliper { MethodName = MethodName.GapMeansure };
                   cogGM.RunParams.Id = sn;
                   yuanliVision.CogMethods.Add(cogGM);

                   break;




               case MethodName.LineMeansure:
                   MethodDispCollection.Add(new DisplayMethod { SN = $"{yuanliVision.CogMethods.Count + 1}", Name = MethodName.LineMeansure, ResultName = $"{ResultSelect.Full}" });
                   var cogLM = new CogLineCaliper { MethodName = MethodName.LineMeansure };
                   cogLM.RunParams.Id = sn;
                   yuanliVision.CogMethods.Add(cogLM);

                   break;

               case MethodName.CircleMeansure:
                   MethodDispCollection.Add(new DisplayMethod { SN = $"{yuanliVision.CogMethods.Count + 1}", Name = MethodName.CircleMeansure, ResultName = $"{ResultSelect.Full}" });
                   //    MethodCollection.Add(new CogGapCaliper { MethodName = $"{MethodName.CircleMeansure}" });

                   break;
               case MethodName.BlobDetector:
                   MethodDispCollection.Add(new DisplayMethod { SN = $"{yuanliVision.CogMethods.Count + 1}", Name = MethodName.BlobDetector, ResultName = $"{ResultSelect.Full}" });
                   var cogBlob = new CogBlobDetector { MethodName = MethodName.BlobDetector };
                   cogBlob.RunParams.Id = sn;
                   yuanliVision.CogMethods.Add(cogBlob);
                   break;
               default:
                   break;
           }

           AddCombine(Convert.ToString(yuanliVision.CogMethods.Count));
           UpdateRecipe();


       });
        public ICommand DeleteMethodCommand => new RelayCommand(() =>
       {
           try {
               if (MethodCollectIndex < 0) return;
               int index = MethodCollectIndex;
               MethodDispCollection.RemoveAt(index);
               yuanliVision.CogMethods.RemoveAt(index);

               for (int i = 0; i < MethodDispCollection.Count; i++) {
                   MethodDispCollection[i].SN = $"{i + 1}";
                   yuanliVision.CogMethods[i].RunParams.Id = i + 1;
               }
               UpdateRecipe();
           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }

       });
        public ICommand ClearMethodCommand => new RelayCommand(() =>
       {
           MethodDispCollection.Clear();
           CombineCollection.Clear();
           yuanliVision.CogMethods.Clear();
           UpdateRecipe();
       });
        public ICommand TestMethodCommand => new RelayCommand(() =>
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
        public ICommand EditSampleCommand => new RelayCommand(() =>
       {
           try {

               matcher.RunParams = MatchParam;
               matcher.EditParameter(Image);

               MatchParam = (PatmaxParams)matcher.RunParams;
               if (MatchParam.PatternImage != null)
                   SampleImage = MatchParam.PatternImage.ToBitmapSource();

               UpdateRecipe();

           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }



       });

        public ICommand LocateSampleCommand => new RelayCommand(() =>
       {
           try {

               yuanliVision.ImportGoldenImage(Image, matcher);
               IsLocated = true;
           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }

       });



        public ICommand LOADCommand => new RelayCommand(() =>
       {
           try {
               foreach (CogParameter item in Recipe.MethodParams) {
                   //        item = CogParameter.Load("123-1", 0);

                   //         MethodList.Add(new CogGapCaliper(temp));
                   //         MethodDispCollection.Add(new DisplayMethod { Name = temp.Methodname, ResultName = temp.ResultOutput.ToString() });
               }


           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }


       });
        public ICommand SAVECommand => new RelayCommand(() =>
      {
          try {
              foreach (var item in yuanliVision.CogMethods) {
                  item.RunParams.Save("123-1");
              }
              yuanliVision.CogMethods[0].RunParams.Save("123-1");


          }
          catch (Exception ex) {

              MessageBox.Show(ex.Message);
          }


      });
        private void UpdateRecipe()
        {

            Recipe.LocateParams = MatchParam;
            Recipe.MethodParams = yuanliVision.CogMethods.Select(param => param.RunParams).ToList();
            Recipe.CombineOptionOutputs = CombineCollection.ToList();
            Recipe.PixelSize = PixelSize;

        }


        private static void OnRecipeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as InspectUC;
            dp.SetMethodParams();
        }
        private void SetMethodParams()
        {
            yuanliVision.CogMethods.Clear();
            MethodDispCollection.Clear();
            CombineCollection.Clear();
            MatchParam = Recipe.LocateParams;


            if (MatchParam.PatternImage != null)
                SampleImage = MatchParam.PatternImage.ToBitmapSource();
            CombineCollection = new ObservableCollection<CombineOptionOutput>(Recipe.CombineOptionOutputs);

            matcher = new CogMatcher(MatchParam);
            int i = 0;
            foreach (CogParameter item in Recipe.MethodParams) {
                //        item = CogParameter.Load("123-1", 0);
                switch (item.Methodname) {
                    case MethodName.PatternMatch:
                        yuanliVision.CogMethods.Add(new CogMatcher(item));
                        break;
                    case MethodName.GapMeansure:
                        yuanliVision.CogMethods.Add(new CogGapCaliper(item));
                        break;
                    case MethodName.LineMeansure:
                        yuanliVision.CogMethods.Add(new CogLineCaliper(item));
                        break;
                    case MethodName.CircleMeansure:
                        break;
                    case MethodName.BlobDetector
                    :
                        yuanliVision.CogMethods.Add(new CogBlobDetector(item));
                        break;
                    default:
                        break;
                }
                i++;
                MethodDispCollection.Add(new DisplayMethod { SN = i.ToString(), Name = item.Methodname, ResultName = item.ResultOutput.ToString() });
            }


            PixelSize = Recipe.PixelSize; //因為有參數更新  避免其他資料還沒寫入就被刷掉  所以放到最後面執行
        }

        private void AddCombine(string sn1)
        {
            if (MethodDispCollection.Count == 0) return;
            //選出要組合的結果 0:直接輸出  1: 計算距離
            string option = $"{CombineOptionSelected}";


            string sn2 = "";
            if (CombineOptionSelected == OutputOption.None)
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

            CombineCollection.Add(new CombineOptionOutput { Option = CombineOptionSelected, SN1 = sn1, SN2 = sn2, ThresholdMax = ThresholdMax, ThresholdMin = ThresholdMin });

        }

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
