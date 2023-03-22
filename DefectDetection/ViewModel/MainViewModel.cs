﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliApplication.Common;
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
        private ObservableCollection<ROIShape> drawings = new ObservableCollection<ROIShape>();
        private MeansureRecipe mainRecipe = new MeansureRecipe();
        private bool isInspectEnabled = false;
        private ObservableCollection<FinalResult> finalResultCollection = new ObservableCollection<FinalResult>();
        private YuanliVision yuanliVision = new YuanliVision();
        private bool isLocate, isTriggerProtecte = true;
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
        }


        public WriteableBitmap MainImage { get => mainImage; set => SetValue(ref mainImage, value); }
        public ObservableCollection<ROIShape> Drawings { get => drawings; set => SetValue(ref drawings, value); }

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


        public MeansureRecipe MainRecipe { get => mainRecipe; set => SetValue(ref mainRecipe, value); }
        public bool IsInspectEnabled { get => isInspectEnabled; set => SetValue(ref isInspectEnabled, value); }
        public bool IsLocate { get => isLocate; set => SetValue(ref isLocate, value); }
        /// <summary>
        /// 防連點觸發 保護機制  動作尚未完成=False
        /// </summary>
        public bool IsTriggerProtecte { get => isTriggerProtecte; set => SetValue(ref isTriggerProtecte, value); }
        public ObservableCollection<FinalResult> FinalResultCollection { get => finalResultCollection; set => SetValue(ref finalResultCollection, value); }

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
       IsTriggerProtecte = false;
       Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
         
       dlg.Filter = "BMP files (*.bmp)|*.bmp|JPG files (*.jpg)|*.jpg|PNG files (*.png)|*.png";
       Nullable<bool> result = dlg.ShowDialog();
       if (result == true) {// 載入圖片

           BitmapImage bitmapImage = new BitmapImage();
           bitmapImage.BeginInit();
           bitmapImage.UriSource = new Uri(dlg.FileName);
           bitmapImage.EndInit();

           // 將圖片轉換為 BitmapSource
           BitmapSource bitmapSource = bitmapImage;
           MainImage = new WriteableBitmap(bitmapSource);
           IsLocate = false;
       }
           Task.Run(() => {
               Task.Delay(3000).Wait();
              IsTriggerProtecte = true; }); //避掉讀取圖片 會移動到影像 canvas的BUG

          IsInspectEnabled = true;
           
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

                   MainRecipe = meansureRecipe;


               }


           }
       });

        public ICommand RunCommand => new RelayCommand(async () =>
        {
            try {

                ClearShapeAction.Execute(Drawings);
                Frame<byte[]> frame = MainImage.ToByteFrame();

                VisionResult[] results_ = await yuanliVision.Run(frame, MainRecipe.LocateParams, MainRecipe.MethodParams, MainRecipe.CombineOptionOutputs);

                //得到量測結果後 轉換到FinalResult 以便UI印出結果
                List<FinalResult> finalResult = new List<FinalResult>();
                foreach (VisionResult item in results_) {

                    switch (item.ResultOutput) {
                        case OutputOption.Result:
                           
                            if (item.MatchResult != null) {

                                for (int i = 0; i < item.MatchResult.Length; i++) {
                                    FinalResult matchfinal = new FinalResult
                                    {
                                        Number = "1",
                                        //    Distance = item.Distance,
                                        //    Angle = item.Angle,
                                        Center = item.MatchResult[i].Center,
                                        Score = item.MatchResult[i].Score,
                                        Output = item.ResultOutput,

                                    };

                                    finalResult.Add(matchfinal);
                                }
                            }
                            else if (item.CaliperResult != null) {

                                foreach (var caliper in item.CaliperResult) {
               
                                    FinalResult finalCaliper = new FinalResult
                                    {
                                        Number = "1",
                                        Distance = caliper.Distance,
                                        //    Angle = item.Angle,
                                        BeginPoint = caliper.BeginPoint,
                                        EndPoint = caliper.EndPoint,
                                        Output = item.ResultOutput,

                                    };
                                    finalResult.Add(finalCaliper);
                                }

                            }
                            else if (item.LineResult != null) 
                            {
                                FinalResult finalCaliper = new FinalResult
                                {
                                    Number = "1",
                                    Distance = item.LineResult.Distance,
                                    //    Angle = item.Angle,
                                    BeginPoint = item.LineResult.BeginPoint,
                                    EndPoint = item.LineResult.EndPoint,
                                    Output = item.ResultOutput,

                                };
                                finalResult.Add(finalCaliper);
                            }
                            break;
                        case OutputOption.Distance:
                            FinalResult finalDistance = new FinalResult
                            {
                                Number = "1",
                                Distance = item.Distance,
                                //    Angle = item.Angle,

                                Output = item.ResultOutput,
                            };
                            finalResult.Add(finalDistance);

                            break;
                        case OutputOption.Angle:

                            break;
                        default:
                            break;
                    }




                }
                //印出結果
                foreach (FinalResult result in finalResult) {
                    if (OutputOption.Result == result.Output && result.Center.HasValue) {
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
                    else if (OutputOption.Result == result.Output && result.Distance.HasValue) {
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


                FinalResultCollection = new ObservableCollection<FinalResult>(finalResult);
            }
            catch (Exception ex) {

                MessageBox.Show(ex.Message);
            }
        });


        public ICommand MultRunCommand => new RelayCommand(() =>
       {

       });






        /// <summary>
        /// 在我的文件夾裡面 創造Recipe 的資料夾  再以Recipe名稱創建資料夾  將recipe的檔案集中在裡面
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>    
        public string CreateFolder(string folderName)
        {
            string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            string path = $"{systemPath}\\Recipe\\{folderName}";
            //    string path = $"{systemPath}\\Recipe\\{folderName}";
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
}