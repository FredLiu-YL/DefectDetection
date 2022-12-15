﻿using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private bool isRefresh;
        private int positionZ = 13000, movePosZ, distanceZ = 500;
        private double lSPPos, nSPPos;
        private AutoFocusSystem focusSystem;
        private double signalA = 2.012, signalB = 0.012, aFsignalA = 1.234, aFsignalB = 3.111;
        private int patternZ = 600, distancePatternZ = 20, nSP, fSP;
        private WriteableBitmap image ;
        private BitmapImage bitmapImage;
        private BitmapSource mainImage;

        private Task taskRefresh = Task.CompletedTask;
        private Brush aFBackColor;
        private IDisposable subscribeState;
        public int PatternZ { get => patternZ; set => SetValue(ref patternZ, value); }
        public int DistancePatternZ { get => distancePatternZ; set => SetValue(ref distancePatternZ, value); }

        public int PositionZ { get => positionZ; set => SetValue(ref positionZ, value); }
        public int DistanceZ { get => distanceZ; set => SetValue(ref distanceZ, value); }

        public int NSP { get => nSP; set => SetValue(ref nSP, value); }
        public int FSP { get => fSP; set => SetValue(ref fSP, value); }

        public int MovePosZ { get => movePosZ; set => SetValue(ref movePosZ, value); }

        public double SignalA { get => signalA; set => SetValue(ref signalA, value); }
        public double SignalB { get => signalB; set => SetValue(ref signalB, value); }
        public double AFSignalA { get => aFsignalA; set => SetValue(ref aFsignalA, value); }
        public double AFSignalB { get => aFsignalB; set => SetValue(ref aFsignalB, value); }

        public double LSPPos { get => lSPPos; set => SetValue(ref lSPPos, value); }
        public double NSPPos { get => nSPPos; set => SetValue(ref nSPPos, value); }
        public Brush AFBackColor { get => aFBackColor; set => SetValue(ref aFBackColor, value); }

        public WriteableBitmap Image { get => image; set => SetValue(ref image, value); }

        public ICommand OpenCommand => new RelayCommand(() =>
        {
            if (focusSystem == null)
            {
                focusSystem = new AutoFocusSystem("COM1", 19200);
                subscribeState = focusSystem.AfStates.ObserveLatestOn(TaskPoolScheduler.Default) //取最新的資料 ；TaskPoolScheduler.Default  表示在另外一個執行緒上執行
                        .ObserveOn(DispatcherScheduler.Current)  //將訂閱資料轉換成柱列順序丟出 ；DispatcherScheduler.Current  表示在主執行緒上執行
                        .Subscribe(t => {
                            if (t)
                                AFBackColor = Brushes.Green;
                            else
                                AFBackColor = Brushes.Red;

                        });
            }
                
          
            isRefresh = true;
            focusSystem.Open();
   
            taskRefresh = Task.Run(RefreshState);
        });
        public ICommand CloseCommand => new RelayCommand(async () =>
        {
            isRefresh = false;
            await taskRefresh;
            focusSystem.Close();

        });

        public ICommand AFONCommand => new RelayCommand(() =>
        {
            
             focusSystem.Run();

        });
        public ICommand AFOFFCommand => new RelayCommand(() =>
        {

            focusSystem.Stop();

        });

        public ICommand MoveCommand => new RelayCommand<string>(async key =>
        {

            switch (key)
            {
                case "+":
                    var tempPos = PositionZ + DistanceZ;
                    focusSystem.Move(DistanceZ);
                    focusSystem.FSP = tempPos - 1500;
                    focusSystem.NSP = tempPos + 1500;
                    break;

                case "-":
                    var tempPos1 = PositionZ - DistanceZ;
                    focusSystem.Move(-DistanceZ);
                    focusSystem.FSP = tempPos1 - 1500;
                    focusSystem.NSP = tempPos1 + 1500;
                    break;

            }


        });

        public ICommand MoveToCommand => new RelayCommand(() =>
       {
           focusSystem.MoveTo(MovePosZ);


       });


        public ICommand MovePTCommand => new RelayCommand<string>(async key =>
        {


            switch (key)
            {
                case "+":

                    focusSystem.PatternMove(DistancePatternZ);
                    break;

                case "-":
                    focusSystem.PatternMove(-DistancePatternZ);
                    break;

            }

        });


        private async Task RefreshState()
        {
            try
            {
                while (isRefresh)
                {
                    if (focusSystem.IsRunning != true)
                    {

                        SignalA = focusSystem.SensorA;
                        SignalB = focusSystem.SensorB;
                        AFSignalA = focusSystem.AFSignalA;
                        AFSignalB = focusSystem.AFSignalB;

                       
                        PatternZ = (int)focusSystem.Pattern;
                        FSP = focusSystem.FSP;
                        NSP = focusSystem.NSP;
                    }
                    PositionZ = (int)focusSystem.AxisZPosition;
                    await Task.Delay(400);
                }

            }
            catch (Exception ex)
            {

               // throw ex;
            }
           
            


        }
    }
}