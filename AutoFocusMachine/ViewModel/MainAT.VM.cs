using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private int positionZ = 13000, movePosZ, distanceZ = 500;
        private double lSPPos, nSPPos;

        private double signalA = 2.012, signalB = 0.012, aFsignalA = 1.234, aFsignalB = 3.111;
        private int patternZ = 600, distancePatternZ = 20;

        private WriteableBitmap image = new WriteableBitmap((int) 900, (int) 600, 96, 96, PixelFormats.Bgra32, null);

        public int PatternZ { get => patternZ; set => SetValue(ref patternZ, value); }
        public int DistancePatternZ { get => distancePatternZ; set => SetValue(ref distancePatternZ, value); }

        public int PositionZ { get => positionZ; set => SetValue(ref positionZ, value); }
        public int DistanceZ { get => distanceZ; set => SetValue(ref distanceZ, value); }
        public int MovePosZ { get => movePosZ; set => SetValue(ref movePosZ, value); }

        public double SignalA { get => signalA; set => SetValue(ref signalA, value); }
        public double SignalB { get => signalB; set => SetValue(ref signalB, value); }
        public double AFSignalA { get => aFsignalA; set => SetValue(ref aFsignalA, value); }
        public double AFSignalB { get => aFsignalB; set => SetValue(ref aFsignalB, value); }

        public double LSPPos { get => lSPPos; set => SetValue(ref lSPPos, value); }
        public double NSPPos { get => nSPPos; set => SetValue(ref nSPPos, value); }


        public WriteableBitmap Image { get => image; set => SetValue(ref image, value); }


        public ICommand MoveCommand => new RelayCommand<string>(async key =>
        {
            switch (key)
            {
                case "+":
                    PositionZ += DistanceZ;
                    break;

                case "-":
                    PositionZ -= DistanceZ;
                    break;

            }


        });

        public ICommand MoveToCommand => new RelayCommand(() =>
       {

           PositionZ = MovePosZ;

       });


        public ICommand MovePTCommand => new RelayCommand<string>(async key =>
        {


            switch (key)
            {
                case "+":
                    PatternZ += DistancePatternZ;
                    break;

                case "-":
                    PatternZ -= DistancePatternZ;
                    break;

            }

        });

    }
}
