﻿using Cognex.VisionPro;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliApplication.Common;
using YuanliApplication.Tool;
using YuanliCore.ImageProcess;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;
using YuanliCore.Views.CanvasShapes;

namespace DefectDetection.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private WriteableBitmap mainImage;
        private ICogRecord lastRecord;
        private ObservableCollection<ROIShape> drawings = new ObservableCollection<ROIShape>();
        private MeansureRecipe mainRecipe = new MeansureRecipe();
        private bool isInspectEnabled = false;
        private ObservableCollection<FinalResult> finalResultCollection = new ObservableCollection<FinalResult>();
        private YuanliVision yuanliVision = new YuanliVision();
        private bool isLocate, isTriggerProtecte = true, isDetectionMode=true, isMeansureMode = false;
        private int finalResultCollectionSelect;
        private string version;
        private bool isMultRun;
        private BitmapSource imageSouce;
        private List<DisplayLable> cogTextLsit = new List<DisplayLable>();
        private double contrastThreshold =7, areaThreshold=5;


        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            ////if (IsInDesignMode)
            ////{
            ////    // Code runs in Blend --> create design time data.
            ////}
            ////else
            ////{
            ////    // Code runs "for real"
            ////}
            ///
            Version = $"Yuanli  DefectDetection  Ver : {System.Windows.Forms.Application.ProductVersion}";
        }

        public string Version { get => version; set => SetValue(ref version, value); }
        public WriteableBitmap MainImage { get => mainImage; set => SetValue(ref mainImage, value); }
        public ObservableCollection<ROIShape> Drawings { get => drawings; set => SetValue(ref drawings, value); }
        public ICogRecord LastRecord { get => lastRecord; set => SetValue(ref lastRecord, value); }
        public List<DisplayLable> CogTextLsit { get => cogTextLsit; set => SetValue(ref cogTextLsit, value); }

       
         
        /// <summary>
        /// 滑鼠在影像內 Pixcel 座標
        /// </summary>
        public System.Windows.Point MousePixcel { get; set; }
        /// <summary>
        /// 新增 Shape
        /// </summary>
        public ICommand AddShapeAction { get; set; }
        /// <summary>
        /// 清除 Shape
        /// </summary>
        public ICommand ClearShapeAction { get; set; }

        public BitmapSource ImageSouce { get => imageSouce; set => SetValue(ref imageSouce, value); }
        public MeansureRecipe MainRecipe { get => mainRecipe; set => SetValue(ref mainRecipe, value); }
        public bool IsInspectEnabled { get => isInspectEnabled; set => SetValue(ref isInspectEnabled, value); }
        public bool IsLocate { get => isLocate; set => SetValue(ref isLocate, value); }
        public double ContrastThreshold { get => contrastThreshold; set => SetValue(ref contrastThreshold, value); }
        public double AreaThreshold { get => areaThreshold; set => SetValue(ref areaThreshold, value); }

        public bool IsMeansureMode
        {
            get => isMeansureMode;
            set
            {
                SetValue(ref isMeansureMode, value);
                MainRecipe.IsDetection = !value;
                MainRecipe.IsMeansure = value;
            }
        }
        public bool IsDetectionMode
        {
            get => isDetectionMode;
            set
            {
                SetValue(ref isDetectionMode, value);
                MainRecipe.IsDetection = value;
                MainRecipe.IsMeansure = !value;
            }
        }
        /// <summary>
        /// 防連點觸發 保護機制  動作尚未完成=False
        /// </summary>
        public bool IsTriggerProtecte { get => isTriggerProtecte; set => SetValue(ref isTriggerProtecte, value); }
        public ObservableCollection<FinalResult> FinalResultCollection { get => finalResultCollection; set => SetValue(ref finalResultCollection, value); }
        public int FinalResultCollectionSelect { get => finalResultCollectionSelect; set => SetValue(ref finalResultCollectionSelect, value); }
        public ICommand TestCommand => new RelayCommand(() =>
       {
           string ipAddress = "127.0.0.1";
           int port = 16650;


           try {
               TcpClient client = new TcpClient(ipAddress, port);
               System.Console.WriteLine("連線成功");
               var stream = client.GetStream();
               //將字串轉 byte 陣列，使用 ASCII 編碼
               Byte[] myBytes = Encoding.UTF8.GetBytes("\\v1\\protocol\\current");
               stream.Write(myBytes, 0, myBytes.Length);


               //從網路資料流讀取資料
               int bufferSize = client.ReceiveBufferSize;
               byte[] myBufferBytes = new byte[bufferSize];
               stream.Read(myBufferBytes, 0, bufferSize);
               //取得資料並且解碼文字
               Console.WriteLine(Encoding.UTF8.GetString(myBufferBytes, 0, bufferSize));


           }
           catch {

               return;
           }
       });


        public ICommand OpenImageCommand => new RelayCommand(() =>
       {
           try {


               IsTriggerProtecte = false;
               Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

               dlg.Filter = "BMP files (*.bmp)|*.bmp|JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png";
               Nullable<bool> result = dlg.ShowDialog();
               if (result == true) {// 載入圖片


                   var bms = CreateBmp(dlg.FileName);
                   MainImage = new WriteableBitmap(bms);

                   //var aaaa=  yuanliVision.ReadImage(dlg.FileName);
                   //  MainImage = new WriteableBitmap(aaaa);
                   //  ImageSouce = aaaa;

                   IsLocate = false;
               }
               Task.Run(() =>
               {
                   Task.Delay(3000).Wait();
                   IsTriggerProtecte = true;
               }); //避掉讀取圖片 會移動到影像 canvas的BUG

               IsInspectEnabled = true;

           }
           catch (Exception ex) {

               MessageBox.Show(ex.Message);
           }
       });
        public ICommand SaveRecipeCommand => new RelayCommand(() =>
       {

           using (var dialog = new FolderBrowserDialog()) {

               dialog.Description = "選擇文件夾";
               dialog.ShowNewFolderButton = false;
               dialog.SelectedPath = "C:\\Users\\fred_liu\\Documents\\Recipe\\123-1";

               if (dialog.ShowDialog() == DialogResult.OK) {
                   // 获取所选文件夹的完整路径
                   string path = dialog.SelectedPath;

                   // 获取所选文件夹的名称
                   string name = new DirectoryInfo(path).Name;

                   //  string dirFullPath = Path.GetDirectoryName(fileFullPath);
                   //  CreateFolder("");
                   MainRecipe.Save(path);

                   MessageBox.Show("Save Complate");
               }
           }

       });
        public ICommand LoadRecipeCommand => new RelayCommand(() =>
       {
           using (var dialog = new FolderBrowserDialog()) {

               dialog.Description = "選擇文件夾";
               dialog.ShowNewFolderButton = false;
               // dialog.RootFolder = Environment.SpecialFolder.MyComputer;
               dialog.SelectedPath = "C:\\Users\\fred_liu\\Documents\\Recipe\\123-1";
               //dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
               if (dialog.ShowDialog() == DialogResult.OK) {
                   // 获取所选文件夹的完整路径
                   string path = dialog.SelectedPath;

                   // 获取所选文件夹的名称
                   string name = new DirectoryInfo(path).Name;

                   //  string dirFullPath = Path.GetDirectoryName(fileFullPath);
                   //  CreateFolder("");
                   MeansureRecipe meansureRecipe = new MeansureRecipe();
                   meansureRecipe.Load(path);

                   MainRecipe = meansureRecipe;// InspectUC  裡面的SetMethodParams()  會把參數洗掉  所以有新增新tool的話要新增

                   IsDetectionMode = MainRecipe.IsDetection;
                   IsDetectionMode = MainRecipe.IsMeansure;
                   MessageBox.Show(" Load Complate");

               }


           }
       });

        public ICommand RunCommand => new RelayCommand(async () =>
        {
            try {

                ClearShapeAction.Execute(Drawings);
                Frame<byte[]> frame = MainImage.ToByteFrame();

                if (frame == null) throw new Exception("Image does not exist ");

                yuanliVision.CreateImage += SaveResultImage;



                string reportPath = CreateReportFolder("D:\\DetectionReport");

                List<FinalResult> finalResult = await SingleRun(frame, IsDetectionMode, ContrastThreshold, AreaThreshold , reportPath, 1);
                /* 
                  if (IsDetectionMode) {
                     DetectionResult detectionResults_ = await yuanliVision.Run(frame, MainRecipe.LocateParams, MainRecipe.MethodParams);
                     //vp 的顯示結果
                     LastRecord = detectionResults_.CogRecord;
                     //得到量測結果後 轉換到FinalResult 以便UI印出結果
                     List<FinalResult> finalResult = CreateResult(detectionResults_, 1);

                     FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);
                 }
                 else {


                     if (yuanliVision.IsRunning) throw new Exception("Process is Running");
                     VisionResult[] results_ = await yuanliVision.Run(frame, MainRecipe.LocateParams, MainRecipe.MethodParams, MainRecipe.CombineOptionOutputs, MainRecipe.PixelSize);
                     if (results_ == null) throw new Exception("Locate Fail");
                     //得到量測結果後 轉換到FinalResult 以便UI印出結果
                     List<FinalResult> finalResult = CreateResult(results_, 1);

                     //畫面畫出結果
                     DrawResult(finalResult);

                     FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);
                 }
                */
                if (finalResult != null) {
                    Report(finalResult.ToArray(), reportPath + "report.xlsx");
                    List<DisplayLable> textLsit = new List<DisplayLable>();
                    FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);
                    foreach (var item in finalResult) {
                        textLsit.Add(new DisplayLable(item.Center.Value, item.Diameter.ToString("0.000")));
                    }
                    CogTextLsit = textLsit;
                }

                MessageBox.Show("Finished");
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }
            finally {
                yuanliVision.CreateImage -= SaveResultImage;
            }
        });


        public ICommand MultRunCommand => new RelayCommand(async () =>
        {
            try {
                if (isMultRun) throw new Exception("Process is Running");
                using (FolderBrowserDialog dialog = new FolderBrowserDialog()) {
                    yuanliVision.CreateImage += SaveResultImage;
                    dialog.Description = "選擇文件夾";
                    dialog.ShowNewFolderButton = false;

                    //  dialog.SelectedPath = "C:\\Users\\fred_liu\\Documents\\Recipe\\123-1";
                    //dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                    if (dialog.ShowDialog() == DialogResult.OK) {
                        // 获取所选文件夹的完整路径
                        string path = dialog.SelectedPath;

                        // 获取所选文件夹的名称
                        string name = new DirectoryInfo(path).Name;
                        List<BitmapSource> bmpSources = new List<BitmapSource>();
                        string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".bmp", ".gif" }; // 支持的图片文件扩展名

                        // 获取指定文件夹中的所有文件
                        string[] files = Directory.GetFiles(path);

                        // 筛选出所有的图片文件
                        List<string> imageFiles = new List<string>();
                        foreach (string file in files) {
                            string extension = Path.GetExtension(file).ToLower();
                            if (imageExtensions.Contains(extension))
                                imageFiles.Add(file);

                        }
                        string reportPath = CreateReportFolder("D:\\DetectionReport");
                        //循環流程開始
                        FinalResultCollection.Clear();
                        isMultRun = true;
                        foreach (var fileName in imageFiles) {
                            if (!isMultRun) break;
                            int round = imageFiles.IndexOf(fileName) + 1; //找出是陣列中第幾張圖片

                            var bitmapSource = CreateBmp(fileName);

                            MainImage = new WriteableBitmap(bitmapSource);
                            Frame<byte[]> frame = bitmapSource.ToByteFrame();
                            if (round == 9) round = 9;
                            var finalResult = await SingleRun(frame, IsDetectionMode, ContrastThreshold, AreaThreshold , reportPath, round);

                            // VisionResult[] results_ = await yuanliVision.Run(frame, MainRecipe.LocateParams, MainRecipe.MethodParams, MainRecipe.CombineOptionOutputs, MainRecipe.PixelSize);

                            if (finalResult == null) continue;

                            //得到量測結果後 轉換到FinalResult 以便UI印出結果
                            //     List<FinalResult> finalResult = CreateResult(results_, round);
                            foreach (var item in finalResult) {
                                FinalResultCollection.Add(item);
                            }


                            await Task.Delay(10);
                        }
                        Report(FinalResultCollection.ToArray(), reportPath + "report.xlsx");
                        isMultRun = false;
                        MessageBox.Show("Finished");

                    }

                }
            }
            catch (Exception ex) {

                isMultRun = false;
                MessageBox.Show(ex.Message);
            }
            finally {

                yuanliVision.CreateImage -= SaveResultImage;
            }

        });

        public ICommand MultRunStopCommand => new RelayCommand(() =>
       {
           isMultRun = false;
       });

        private void SaveResultImage(ICogRecord cogRecord)
        {
            /* int id = Thread.CurrentThread.ManagedThreadId;

             CogRecordDisplay cogDisplayer = new CogRecordDisplay();
             cogDisplayer.Record = cogRecord;*/

        }

        private async Task<List<FinalResult>> SingleRun(Frame<byte[]> frame, bool isDetectionMode, double cthreshold, double areathreshold,string reportPath, int round)
        {


            List<FinalResult> finalResult = new List<FinalResult>();

            if (isDetectionMode) {
                DetectionResult detectionResults_ = await yuanliVision.DetectionRun(frame, MainRecipe.LocateParams, cthreshold, areathreshold, MainRecipe.MethodParams);


                //vp 的顯示結果
                LastRecord = detectionResults_.CogRecord;

                if (detectionResults_.BlobDetectorResults.Length == 0) return null;
                //得到量測結果後 轉換到FinalResult 以便UI印出結果
                finalResult = CreateResult(detectionResults_, round);
                var jugeFail = finalResult.Where(r => !r.Judge);


                if (jugeFail.Count() > 0) {
                    var lables = finalResult.Select(r => new DisplayLable(r.Center.Value, r.Diameter.ToString("0.000")));

                    var image = detectionResults_.CogRecord.CogRecordAddText(lables , frame.Width, frame.Height);

             //       var image = detectionResults_.CogRecord.CreateBmp(frame.Width, frame.Height);
                    //  detectionResults_.RecordImage.ToByteFrame().Save($"{reportPath}Image-{round}");
                    image.ToByteFrame().Save($"{reportPath}Image-{round}");
                }

                //    FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);




            }
            else {


                if (yuanliVision.IsRunning) throw new Exception("Process is Running");
                VisionResult[] results_ = await yuanliVision.Run(frame, MainRecipe.LocateParams, MainRecipe.MethodParams, MainRecipe.CombineOptionOutputs, MainRecipe.PixelSize);
                //    if (results_ == null) throw new Exception("Locate Fail");

                if (results_ == null) return null;
                //得到量測結果後 轉換到FinalResult 以便UI印出結果
                finalResult = CreateResult(results_, round);

                //畫面畫出結果
                DrawResult(finalResult);

                //   FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);
            }

            return finalResult;
        }

        private List<FinalResult> CreateResult(IEnumerable<VisionResult> visionResults, int round)
        {
            List<FinalResult> finalResult = new List<FinalResult>();
            foreach (VisionResult item in visionResults) {

                switch (item.ResultOutput) {
                    case OutputOption.None:

                        if (item.MatchResult != null) {

                            for (int i = 0; i < item.MatchResult.Length; i++) {
                                FinalResult matchfinal = new FinalResult
                                {
                                    Number = $"{round}",
                                    //    Distance = item.Distance,
                                    //    Angle = item.Angle,
                                    Center = item.MatchResult[i].Center,
                                    Score = item.MatchResult[i].Score,
                                    Output = item.ResultOutput,
                                    Judge = item.Judge
                                };

                                finalResult.Add(matchfinal);
                            }
                        }
                        else if (item.CaliperResult != null) {



                            FinalResult finalCaliper = new FinalResult
                            {
                                Number = $"{round}",
                                Distance = item.Distance,
                                //    Angle = item.Angle,
                                BeginPoint = item.CaliperResult.BeginPoint,
                                EndPoint = item.CaliperResult.EndPoint,
                                Output = item.ResultOutput,
                                Judge = item.Judge
                            };
                            finalResult.Add(finalCaliper);


                        }
                        else if (item.LineResult != null) {
                            FinalResult finalCaliper = new FinalResult
                            {
                                Number = $"{round}",
                                Distance = item.Distance,
                                //    Angle = item.Angle,
                                BeginPoint = item.LineResult.BeginPoint,
                                EndPoint = item.LineResult.EndPoint,
                                Output = item.ResultOutput,
                                Judge = item.Judge
                            };
                            finalResult.Add(finalCaliper);
                        }
                        break;
                    case OutputOption.Distance:
                        FinalResult finalDistance = new FinalResult
                        {
                            Number = $"{round}",
                            Distance = item.Distance,
                            //    Angle = item.Angle,
                            BeginPoint = item.BeginPoint,
                            EndPoint = item.EndPoint,
                            Output = item.ResultOutput,
                            Judge = item.Judge
                        };
                        finalResult.Add(finalDistance);

                        break;
                    case OutputOption.Angle:

                        break;
                    default:
                        break;
                }


            }

            return finalResult;
        }


        private List<FinalResult> CreateResult(DetectionResult detectionResults, int round)
        {
            List<FinalResult> finalResult = new List<FinalResult>();
            foreach (var result in detectionResults.BlobDetectorResults) {


                var final = new FinalResult
                {
                    Number = $"{round}",
                    Area = result.Area,
                    Center = result.CenterPoint,
                    Judge = result.Judge,
                    Diameter = result.Diameter,

                };
                finalResult.Add(final);
            }


            return finalResult;
        }
        private string CreateReportFolder(string rootPath)
        {
            DateTime dateTime = DateTime.Now;
            var folder = dateTime.ToString("yyyyMMdd-HHmm");
            string path = $"{rootPath}\\{folder}\\";

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
        }
        private void Report(FinalResult[] finalResults, string fullFileName)
        {
            OutputExcel outputExcel = new OutputExcel();
            string[] titles = new string[] { "SN", "Diameter(um)", "Area", "Judge" };
            outputExcel.CreateTitle(titles);

            for (int i = 0; i < finalResults.Length; i++) {
                outputExcel.WriteData(i + 1, finalResults[i].Number, finalResults[i].Diameter.ToString(), finalResults[i].Area.ToString(), finalResults[i].Judge.ToString(), "");

            }



            outputExcel.Save(fullFileName);
        }

        private void DrawResult(IEnumerable<FinalResult> finalResult)
        {
            foreach (FinalResult result in finalResult) {
                if (OutputOption.None == result.Output && result.Center.HasValue) {
                    var center = new ROICross
                    {
                        X = result.Center.Value.X,
                        Y = result.Center.Value.Y,
                        Size = 35,
                        StrokeThickness = 2,
                        Stroke = Brushes.Red
                    };
                    AddShapeAction.Execute(center);

                }
                else if (OutputOption.None == result.Output && result.Distance.HasValue) {
                    var line = new ROILine
                    {
                        X1 = result.BeginPoint.X,
                        Y1 = result.BeginPoint.Y,
                        X2 = result.EndPoint.X,
                        Y2 = result.EndPoint.Y,
                        Stroke = Brushes.Red,
                        StrokeThickness = 3,
                        IsInteractived = false,
                        CenterCrossLength = 4
                    };
                    AddShapeAction.Execute(line);
                }
                if (OutputOption.Distance == result.Output && result.Distance.HasValue) {

                    var line = new ROILine
                    {
                        X1 = result.BeginPoint.X,
                        Y1 = result.BeginPoint.Y,
                        X2 = result.EndPoint.X,
                        Y2 = result.EndPoint.Y,
                        Stroke = Brushes.Red,
                        StrokeThickness = 3,
                        IsInteractived = false,
                        CenterCrossLength = 4
                    };
                    AddShapeAction.Execute(line);
                }
            }



        }



        private BitmapSource CreateBmp(string path)
        {
            BitmapImage bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(path);
            bitmapImage.EndInit();

            // 將圖片轉換為 BitmapSource
            BitmapSource bitmapSource = bitmapImage;
            //不知道原因  有些圖片資訊會丟失  所以先轉成frame  再轉回 BitmapSource
            var frame = bitmapSource.ToByteFrame();
            var bms = frame.ToBitmapSource();
            return bms;
        }

        /// <summary>
        /// 在我的文件夾裡面 創造Recipe 的資料夾  再以Recipe名稱創建資料夾  將recipe的檔案集中在裡面
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>    
        public string CreateRecipeFolder(string folderName)
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string path = $"{systemPath}\\Recipe\\{folderName}";

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            return path;
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

    }

    public class TableIndexConver : IValueConverter
    {
        //当值从绑定源传播给绑定目标时，调用方法Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return 1;
            else
                return 0;
        }

        //当值从绑定目标传播给绑定源时，调用此方法ConvertBack
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((int)value == 0)
                return true;
            else
                return false;
        }
    }


}