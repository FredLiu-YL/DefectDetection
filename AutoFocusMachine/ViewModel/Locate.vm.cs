using AutoFocusMachine.Model.Recipe;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using YuanliCore.AffineTransform;
using YuanliCore.Interface;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private ObservableCollection<Point> targetDieList = new ObservableCollection<Point>();
        private ObservableCollection<Point> sourceDieList = new ObservableCollection<Point>();
        private int targetDieIndex, sourcetDieIndex;
        private double seachPosX1, seachPosY1, seachPosX2, seachPosY2, seachPosX3, seachPosY3;
        private double targetPosX1, targetPosY1, targetPosX2, targetPosY2, targetPosX3, targetPosY3;



        public ObservableCollection<Point> TargetDieList { get => targetDieList; set => SetValue(ref targetDieList, value); }
        public ObservableCollection<Point> SourceDieList { get => sourceDieList; set => SetValue(ref sourceDieList, value); }
        public int SourceDieIndex { get => sourcetDieIndex; set => SetValue(ref sourcetDieIndex, value); }
        public int TargetDieIndex { get => targetDieIndex; set => SetValue(ref targetDieIndex, value); }



        public double SeachPosX1 { get => seachPosX1; set => SetValue(ref seachPosX1, value); }
        public double SeachPosY1 { get => seachPosY1; set => SetValue(ref seachPosY1, value); }

        public double SeachPosX2 { get => seachPosX2; set => SetValue(ref seachPosX2, value); }
        public double SeachPosY2 { get => seachPosY2; set => SetValue(ref seachPosY2, value); }

        public double SeachPosX3 { get => seachPosX3; set => SetValue(ref seachPosX3, value); }
        public double SeachPosY3 { get => seachPosY3; set => SetValue(ref seachPosY3, value); }

        public double TargetPosX1 { get => targetPosX1; set => SetValue(ref targetPosX1, value); }
        public double TargetPosY1 { get => targetPosY1; set => SetValue(ref targetPosY1, value); }

        public double TargetPosX2 { get => targetPosX2; set => SetValue(ref targetPosX2, value); }
        public double TargetPosY2 { get => targetPosY2; set => SetValue(ref targetPosY2, value); }

        public double TargetPosX3 { get => targetPosX3; set => SetValue(ref targetPosX3, value); }
        public double TargetPosY3 { get => targetPosY3; set => SetValue(ref targetPosY3, value); }


        public ICommand AddDielistCommand => new RelayCommand(async () =>
       {
           Point pos = await atfMachine.Table_Module.GetPostion();
           SourceDieList.Add(pos);

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
            try
            {
               Point pos = await atfMachine.Table_Module.GetPostion();
                switch (key)
                {
                    case "1":
                        SeachPosX1 = pos.X;
                        SeachPosY1 = pos.Y;
                        break;
                    case "2":
                        SeachPosX2 = pos.X;
                        SeachPosY2 = pos.Y;
                        break;
                    case "3":
                        SeachPosX3 = pos.X;
                        SeachPosY3 = pos.Y;
                        break;


                }
            }
            catch (Exception ex)
            {

               MessageBox.Show(ex.Message);
            }
            


        });
        public ICommand LocateTransCommand => new RelayCommand(() =>
        {
            Point[] sources = new Point[] { new Point(SeachPosX1, SeachPosY1) ,
                                          new Point(SeachPosX2, SeachPosY2) ,
                                          new Point(SeachPosX3, SeachPosY3) };

            Point[] targets = new Point[] { new Point(TargetPosX1, TargetPosY1) ,
                                          new Point(TargetPosX2, TargetPosY2) ,
                                          new Point(TargetPosX3, TargetPosY3) };

            HAffineTransform hAffineTransform = new HAffineTransform(sources, targets);
            var sps =  SourceDieList.ToArray();
            TargetDieList = new ObservableCollection<Point> (sps.Select(p => hAffineTransform.TransPoint(p)));
      

        });

        public ICommand MoveToDieCommand => new RelayCommand(() =>
        {



        });
        public ICommand NextDieCommand => new RelayCommand(() =>
        {



        });


        public ICommand SaveLocateDataCommand => new RelayCommand(() =>
        {
            List<Point> fPoints = new List<Point>();
            fPoints.Add(new Point(SeachPosX1, SeachPosY1));
            fPoints.Add(new Point(SeachPosX2, SeachPosY2));
            fPoints.Add(new Point(SeachPosX3, SeachPosY3));


            mainRecipe.FiducialMarkPos = fPoints.ToArray();
            mainRecipe.LayoutPos = SourceDieList.ToArray();


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Param Documents|*.json";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                mainRecipe.Save(dlg.FileName);
            }

        });
        public ICommand LoadLocateDataCommand => new RelayCommand(() =>
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Param Documents|*.json";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                mainRecipe = AbstractRecipe.Load<AFMachineRecipe>(dlg.FileName);


            }

        });
    }
}
