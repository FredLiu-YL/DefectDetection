using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private ObservableCollection<Point> targetDieList = new ObservableCollection<Point>();
        private ObservableCollection<Point> sourceDieList = new ObservableCollection<Point>();
        private int targetDieIndex, sourcetDieIndex;

        public ObservableCollection<Point> TargetDieList { get => targetDieList; set => SetValue(ref targetDieList, value); }
        public ObservableCollection<Point> SourceDieList { get => sourceDieList; set => SetValue(ref sourceDieList, value); }
        public int SourceDieIndex { get => sourcetDieIndex; set => SetValue(ref sourcetDieIndex, value); }
        public int TargetDieIndex { get => targetDieIndex; set => SetValue(ref targetDieIndex, value); }


        public ICommand AddDielistCommand => new RelayCommand( () =>
        {

            SourceDieList.Add(new Point(SourceDieIndex, SourceDieIndex));

        });
        public ICommand DelDielistCommand => new RelayCommand(() =>
        {

            SourceDieList.RemoveAt(SourceDieIndex);

        });
        public ICommand ClearDielistCommand => new RelayCommand(() =>
        {

            SourceDieList.Clear();

        });
        public ICommand TableGetPosCommand => new RelayCommand<string>(async key =>
        {

            switch (key)
            {
                case "1":
                //    await atfMachine.Axes[0].MoveAsync();
                    break;
                case "2":
                //    await atfMachine.Axes[0].MoveAsync();
                    break;
                case "3":
               //     await atfMachine.Axes[1].MoveAsync();
                    break;
                

            }


        });
        public ICommand LocateTransCommand => new RelayCommand(() =>
        {



        });

        public ICommand MoveToDieCommand => new RelayCommand(() =>
        {



        });
        public ICommand NextDieCommand => new RelayCommand(() =>
        {



        });
        

        public ICommand SaveLocateDataCommand => new RelayCommand(() =>
        {

            

        });
        public ICommand LoadLocateDataCommand => new RelayCommand(() =>
        {



        });
    }
}
