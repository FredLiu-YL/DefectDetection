using AutoFocusMachine.Model;
using GalaSoft.MvvmLight.Command;
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
using YuanliCore.CameraLib;
using YuanliCore.CameraLib.IDS;
using YuanliCore.Interface;
using YuanliCore.Motion.Marzhauser;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private bool isRefresh;
        private int positionZ = 13000, movePosZ, distanceZ = 500;
        private double lSPPos, nSPPos;
      //  private AutoFocusSystem focusSystem;
        private double signalA = 2.012, signalB = 0.012, aFsignalA = 1.234, aFsignalB = 3.111;
        private int patternZ = 600, distancePatternZ = 20, nSP, fSP;

        private BitmapImage bitmapImage;
        private BitmapSource mainImage;
    //    private UeyeCamera ueyeCamera;
        private Task taskRefresh = Task.CompletedTask;
        private Brush aFBackColor;

        private IDisposable camlive;
        private bool isBtnEnable = true;
        private string tableDistance;



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

        public bool IsBtnEnable { get => isBtnEnable; set => SetValue(ref isBtnEnable, value); }

        public Brush AFBackColor { get => aFBackColor; set => SetValue(ref aFBackColor, value); }


        public string TableDistance { get => tableDistance; set => SetValue(ref tableDistance, value); }

        public ICommand OpenCommand => new RelayCommand(() =>
        {
          /*  if (focusSystem == null)
            {
                focusSystem = new AutoFocusSystem("COM1", 19200);

            }


            isRefresh = true;
            focusSystem.Open();
            focusSystem.JustFocus += AFRunningStates;
            taskRefresh = Task.Run(RefreshState);*/
        });
        public ICommand CloseCommand => new RelayCommand(async () =>
        {
            isRefresh = false;
            await taskRefresh;
           // focusSystem.Close();

        });
        public ICommand CamOpenCommand => new RelayCommand(() =>
        {
          /* if (ueyeCamera == null)
            {
                ueyeCamera = new UeyeCamera();

            }
            ueyeCamera.Open();
            ueyeCamera.Load("C:\\Users\\User\\Documents\\IDSsetting.ini");

            CameraLive();
            ueyeCamera.Grab();*/
        });
        public ICommand CamCloseCommand => new RelayCommand(async () =>
        {
           // ueyeCamera.Stop(); ;
           // ueyeCamera.Close();
        });
        public ICommand TEST1Command => new RelayCommand(() =>
        {




        });
        public ICommand TEST2Command => new RelayCommand(() =>
        {



        });
        public ICommand TEST3Command => new RelayCommand(() =>
        {


        });
        public ICommand TEST4Command => new RelayCommand(async () =>
        {
            var dis = Convert.ToDouble(TableDistance);

            await atfMachine.Axes[0].MoveAsync(dis);



        });
        public ICommand TableMoveCommand => new RelayCommand(() =>
        {



        });
        public ICommand AFONCommand => new RelayCommand(() =>
        {

          atfMachine.FocusSystem.Run();

        });
        public ICommand AFOFFCommand => new RelayCommand(() =>
        {

            atfMachine.FocusSystem.Stop();

        });

        public ICommand MoveCommand => new RelayCommand<string>(async key =>
        {
            if (!IsBtnEnable) return;
            IsBtnEnable = false;
            try
            {
                switch (key)
                {
                    case "+":
                        var tempPos = PositionZ + DistanceZ;
                        atfMachine.FocusSystem.Move(DistanceZ);
                        atfMachine.FocusSystem.FSP = tempPos - 1500;
                        atfMachine.FocusSystem.NSP = tempPos + 1500;
                        break;

                    case "-":
                        var tempPos1 = PositionZ - DistanceZ;
                        atfMachine.FocusSystem.Move(-DistanceZ);
                        atfMachine.FocusSystem.FSP = tempPos1 - 1500;
                        atfMachine.FocusSystem.NSP = tempPos1 + 1500;
                        break;

                }
            }
            catch (Exception ex)
            {


            }
            finally
            {
                IsBtnEnable = true;

            }


        });

        public ICommand MoveToCommand => new RelayCommand(() =>
       {
           atfMachine.FocusSystem.MoveTo(MovePosZ);


       });


        public ICommand MovePTCommand => new RelayCommand<string>(async key =>
        {


            switch (key)
            {
                case "+":

                    atfMachine.FocusSystem.PatternMove(DistancePatternZ);
                    break;

                case "-":
                    atfMachine.FocusSystem.PatternMove(-DistancePatternZ);
                    break;

            }

        });


        private async Task RefreshState()
        {
            try
            {
                while (isRefresh)
                {
                    if (atfMachine.FocusSystem.IsRunning != true)
                    {
                        var sign = atfMachine.FocusSystem.Signals;
                        SignalA = sign.SensorA;
                        SignalB = sign.SensorB;
                        AFSignalA = sign.AFSignalA;
                        AFSignalB = sign.AFSignalB;


                        PatternZ = (int)atfMachine.FocusSystem.Pattern;
                        FSP = atfMachine.FocusSystem.FSP;
                        NSP = atfMachine.FocusSystem.NSP;
                    }
                    PositionZ = (int)atfMachine.FocusSystem.AxisZPosition;
                    await Task.Delay(300);
                }

            }
            catch (Exception ex)
            {

                // throw ex;
            }

           


        }

        private void AFRunningStates(bool t)
        {


            if (t)
                AFBackColor = Brushes.Green;
            else
                AFBackColor = Brushes.Red;


        }

       

    }
}
