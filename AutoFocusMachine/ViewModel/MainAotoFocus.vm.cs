using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YuanliCore.Views.CanvasShapes;

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
        private Task taskRefresh1 = Task.CompletedTask;
        private Task taskRefresh2 = Task.CompletedTask;
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

        });
        public ICommand CloseCommand => new RelayCommand(async () =>
        {



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

            atfMachine.Table_Module.TableX.AxisDir = YuanliCore.Interface.AxisDirection.Backward;


        });
        public ICommand TEST2Command => new RelayCommand(() =>
        {
            atfMachine.Table_Module.TableX.AxisDir = YuanliCore.Interface.AxisDirection.Forward;


        });
        public ICommand TEST3Command => new RelayCommand(() =>
        {
            //  Drawings.Add( new ROICross { X=200 , Y=200 ,Size=100 });
            int t;
            int count = 80;
            int radus = 2000;
            int pitch = radus * 2 / count;
            //半徑假設350
            for (int x = 1; x <= count + 1; x++)
            {
                for (int y = 1; y <= count + 1; y++)
                {

                    System.Windows.Point point = new System.Windows.Point(radus, radus);

                    System.Windows.Point drawPoint = new System.Windows.Point(x * pitch, y * pitch);
                    System.Windows.Vector v = point - drawPoint;

                    if (v.Length < radus)
                        AddShapeMappingAction.Execute(new ROIRotatedRect { X = 500 + drawPoint.X, Y = 500 + drawPoint.Y, LengthX = pitch / 2.2, LengthY = pitch / 2.2, IsInteractived = false, IsMoveEnabled = false, CenterCrossLength = 2 });
                    else
                        t = count + 2;

                }
            }



        });
        public ICommand TEST4Command => new RelayCommand(async () =>
        {
            var dis = Convert.ToDouble(TableDistance);

            await atfMachine.Table_Module.TableX.MoveAsync(dis);



        });
        public ICommand TableContinueMoveCommand => new RelayCommand<string>(async key =>
        {
            var dis = Convert.ToDouble(TableDistance);
            if (dis == 0)
            {
                switch (key)
                {
                    case "X+":
                        await atfMachine.Table_Module.TableX.MoveToAsync(atfMachine.Table_Module.TableX.LimitN);
                        break;
                    case "X-":
                        await atfMachine.Table_Module.TableX.MoveToAsync(atfMachine.Table_Module.TableX.LimitP);
                        break;
                    case "Y+":
                        await atfMachine.Table_Module.TableY.MoveToAsync(-dis);
                        break;
                    case "Y-":
                        await atfMachine.Table_Module.TableY.MoveToAsync(dis);
                        break;

                }

            }



        });

        public ICommand TableMoveCommand => new RelayCommand<string>(async key =>
        {
            var dis = Convert.ToDouble(TableDistance);
            if (dis == 0)
            {
                await atfMachine.Table_Module.TableX.Stop();
                await atfMachine.Table_Module.TableY.Stop();
            }

            switch (key)
            {
                case "X+":
                    await atfMachine.Table_Module.TableX.MoveAsync(-dis);
                    break;
                case "X-":
                    await atfMachine.Table_Module.TableX.MoveAsync(dis);
                    break;
                case "Y+":
                    await atfMachine.Table_Module.TableY.MoveAsync(-dis);
                    break;
                case "Y-":
                    await atfMachine.Table_Module.TableY.MoveAsync(dis);
                    break;

            }


        });

        public ICommand AFONCommand => new RelayCommand(() =>
        {

            atfMachine.AFModule.AFSystem.Run();

        });
        public ICommand AFOFFCommand => new RelayCommand(() =>
        {

            atfMachine.AFModule.AFSystem.Stop();

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
                        await atfMachine.AFModule.AFSystem.Move(DistanceZ);
                        atfMachine.AFModule.AFSystem.FSP = tempPos - 1500;
                        atfMachine.AFModule.AFSystem.NSP = tempPos + 1500;
                        break;

                    case "-":
                        var tempPos1 = PositionZ - DistanceZ;
                        await atfMachine.AFModule.AFSystem.Move(-DistanceZ);
                        atfMachine.AFModule.AFSystem.FSP = tempPos1 - 1500;
                        atfMachine.AFModule.AFSystem.NSP = tempPos1 + 1500;
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
            atfMachine.AFModule.AFSystem.FSP = MovePosZ - 1500;
            atfMachine.AFModule.AFSystem.NSP = MovePosZ + 1500;
            atfMachine.AFModule.AFSystem.MoveTo(MovePosZ);


        });


        public ICommand MovePTCommand => new RelayCommand<string>(async key =>
        {


            switch (key)
            {
                case "+":

                    atfMachine.AFModule.AFSystem.PatternMove(DistancePatternZ);
                    break;

                case "-":
                    atfMachine.AFModule.AFSystem.PatternMove(-DistancePatternZ);
                    break;

            }

        });


        private async Task RefreshAFState()
        {
            try
            {
                if (atfMachine.AFModule.AFSystem == null) return;
                while (isRefresh)
                {
                    if (!atfMachine.AFModule.AFSystem.IsRunning)
                    {
                        var sign = atfMachine.AFModule.AFSystem.Signals;
                        SignalA = sign.SensorA;
                        SignalB = sign.SensorB;
                        AFSignalA = sign.AFSignalA;
                        AFSignalB = sign.AFSignalB;

                        PatternZ = (int)atfMachine.AFModule.AFSystem.Pattern;
                        FSP = atfMachine.AFModule.AFSystem.FSP;
                        NSP = atfMachine.AFModule.AFSystem.NSP;
                    }


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
