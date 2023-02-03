using AutoFocusMachine.Model;
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
using YuanliCore.Views.CanvasShapes;

namespace AutoFocusMachine.ViewModel
{
    public partial class MainViewModel
    {
        private ObservableCollection<(Point index, Point pos)> targetDieList = new ObservableCollection<(Point index, Point pos)>();
        private ObservableCollection<(Point index, Point pos)> sourceDieList = new ObservableCollection<(Point index, Point pos)>();
        private int targetDieIndex, sourcetDieIndex;
        private double seachPosX1, seachPosY1, seachPosX2, seachPosY2, seachPosX3, seachPosY3;
        private double targetPosX1, targetPosY1, targetPosX2, targetPosY2, targetPosX3, targetPosY3;
        private ROIShape tempselectShape;
        private System.Windows.Point mappingMousePixel;
        private LocateData sourceLocate = new LocateData();
        private LocateData targetLocate = new LocateData();

        public ObservableCollection<(Point index, Point pos)> TargetDieList { get => targetDieList; set => SetValue(ref targetDieList, value); }
        public ObservableCollection<(Point index, Point pos)> SourceDieList { get => sourceDieList; set => SetValue(ref sourceDieList, value); }
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

        public ICommand AddShapeMappingAction { get; set; }


        public ICommand ClearShapeMappingAction { get; set; }

        public System.Windows.Point MappingMousePixel { get => mappingMousePixel; set => SetValue(ref mappingMousePixel, value); }
        public ICommand RemoveShapeMappingAction { get; set; }

        public ICommand AddDielistCommand => new RelayCommand(async () =>
       {
           Point pos =  atfMachine.Table_Module.GetPostion();
           SourceDieList.Add((new Point(SourceDieList.Count+1,0), pos));

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
                Point pos =  atfMachine.Table_Module.GetPostion();
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
        public ICommand SampleMovePosCommand => new RelayCommand<string>(async key =>
        {
            try
            {

                switch (key)
                {
                    case "1":

                        await atfMachine.Table_Module.TableMoveTo(new Point(SeachPosX1, SeachPosY1));
                        break;
                    case "2":

                        await atfMachine.Table_Module.TableMoveTo(new Point(SeachPosX2, SeachPosY2));
                        break;
                    case "3":
                        await atfMachine.Table_Module.TableMoveTo(new Point(SeachPosX3, SeachPosY3));
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
                                       //   new Point(SeachPosX3, SeachPosY3) 
            };

            Point[] targets = new Point[] { new Point(TargetPosX1, TargetPosY1) ,
                                          new Point(TargetPosX2, TargetPosY2) ,
                                       //   new Point(TargetPosX3, TargetPosY3) 
            };

            //ITransform hAffineTransform = new HAffineTransform(sources, targets);
            ITransform hAffineTransform = new CogAffineTransform(sources, targets);

            var sps = SourceDieList.ToArray();
            TargetDieList = new ObservableCollection<(Point  index, Point pos)>(sps.Select(p => (p.index, hAffineTransform.TransPoint(p.pos))));


        });

        public ICommand MoveToDieCommand => new RelayCommand(async () =>
        {
            if (TargetDieIndex == -1) return;
            var pos = TargetDieList[TargetDieIndex];
            await atfMachine.Table_Module.TableMoveTo(pos.pos);
        });
        public ICommand NextDieCommand => new RelayCommand(async () =>
        {
            try
            {
                IsBtnEnable = false;
                TargetDieIndex++;
                var pos = TargetDieList[TargetDieIndex];
                await atfMachine.Table_Module.TableMoveTo(pos.pos);
                IsBtnEnable = true;
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
            }
          
        });


        public ICommand SaveSourceLocateDataCommand => new RelayCommand(() =>
        {
            List<Point> fPoints = new List<Point>();
            fPoints.Add(new Point(SeachPosX1, SeachPosY1));
            fPoints.Add(new Point(SeachPosX2, SeachPosY2));
            fPoints.Add(new Point(SeachPosX3, SeachPosY3));


            sourceLocate.FiducialMarkPos = fPoints.ToArray();
            sourceLocate.LayoutPos = SourceDieList.Select(p => p.pos).ToArray();
            sourceLocate.LayoutIndex = SourceDieList.Select(p => p.index).ToArray();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Param Documents|*.json";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                sourceLocate.Save(dlg.FileName);
            }

        });
        public ICommand LoadSourceLocateDataCommand => new RelayCommand(() =>
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Param Documents|*.json";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                sourceLocate = LocateData.Load<LocateData>(dlg.FileName);

                SeachPosX1 = sourceLocate.FiducialMarkPos[0].X;
                SeachPosY1 = sourceLocate.FiducialMarkPos[0].Y;
                SeachPosX2 = sourceLocate.FiducialMarkPos[1].X;
                SeachPosY2 = sourceLocate.FiducialMarkPos[1].Y;
                SeachPosX3 = sourceLocate.FiducialMarkPos[2].X;
                SeachPosY3 = sourceLocate.FiducialMarkPos[2].Y;


                /* var test1 = mainRecipe.LayoutPos[0];
                 var test2 = mainRecipe.LayoutPos[1];
                 Point[] s1 = new Point[] { new Point(68, 0), new Point(1, 0) };
                 HAffineTransform hAffine = new HAffineTransform(s1, mainRecipe.LayoutPos);
                 for (int i = 1; i <= 68; i++)
                 {
                     Point point = hAffine.TransPoint(new Point(i, 0));
                     SourceDieList.Add(new Point(point.X.Round(3), point.Y.Round(3)));
                 }*/
            
                var test = sourceLocate.LayoutIndex.Zip(sourceLocate.LayoutPos, (i, p) => (i, p));
                SourceDieList = new ObservableCollection<(Point, Point)>(test);
                // SourceDieList = new ObservableCollection<(System.Drawing.Point, Point)>((test, sourceLocate.LayoutPos));


            }

        });

        public ICommand SaveTargetLocateDataCommand => new RelayCommand(() =>
        {
            List<Point> fPoints = new List<Point>();
            fPoints.Add(new Point(TargetPosX1, TargetPosY1));
            fPoints.Add(new Point(TargetPosX2, TargetPosY2));
            fPoints.Add(new Point(TargetPosX3, TargetPosY3));


            targetLocate.FiducialMarkPos = fPoints.ToArray();
            targetLocate.LayoutPos = TargetDieList.Select(p=> p.pos).ToArray();
            targetLocate.LayoutIndex = TargetDieList.Select(p => p.index).ToArray();

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Filter = "Param Documents|*.json";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                targetLocate.Save(dlg.FileName);
            }

        });
        public ICommand LoadTargetLocateDataCommand => new RelayCommand(() =>
        {

        Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
        dlg.Filter = "Param Documents|*.json";
        Nullable<bool> result = dlg.ShowDialog();
        if (result == true)
        {
            targetLocate = LocateData.Load<LocateData>(dlg.FileName);

            TargetPosX1 = targetLocate.FiducialMarkPos[0].X;
            TargetPosY1 = targetLocate.FiducialMarkPos[0].Y;
            TargetPosX2 = targetLocate.FiducialMarkPos[1].X;
            TargetPosY2 = targetLocate.FiducialMarkPos[1].Y;
            TargetPosX3 = targetLocate.FiducialMarkPos[2].X;
            TargetPosY3 = targetLocate.FiducialMarkPos[2].Y;


            /* var test1 = mainRecipe.LayoutPos[0];
             var test2 = mainRecipe.LayoutPos[1];
             Point[] s1 = new Point[] { new Point(68, 0), new Point(1, 0) };
             HAffineTransform hAffine = new HAffineTransform(s1, mainRecipe.LayoutPos); 
             for (int i = 1; i <= 68; i++)
             {
                 Point point = hAffine.TransPoint(new Point(i, 0));
                 SourceDieList.Add(new Point(point.X.Round(3), point.Y.Round(3)));
             }*/
            var test = targetLocate.LayoutIndex.Zip(targetLocate.LayoutPos, (i,p)=> (i ,p ) );

                TargetDieList = new ObservableCollection<(Point ,Point)>(test);



            }

        });


        public ICommand CreateWaferDataCommand => new RelayCommand(() =>
        {


            var waferSize = new Size(195000, 195000);
            //   var dieSize = new Size(3429.183, 3163.755);
            //   Vector diepitch = new Vector(0, 0);
            var dieSize = new Size(3360.0, 3076.1);
            Vector diepitch = new Vector(75, 75);

            Wafer wafer = new Wafer(waferSize, dieSize, diepitch);


            var circleCenter = new Point(waferSize.Width / 2, waferSize.Height / 2);

            var circleDies = wafer.Dies.Select(die =>
            {
                var disRT = new Point(die.Position.X + dieSize.Width / 2, die.Position.Y - dieSize.Height / 2) - circleCenter;
                var disRB = new Point(die.Position.X + dieSize.Width / 2, die.Position.Y + dieSize.Height / 2) - circleCenter;
                var disLB = new Point(die.Position.X - dieSize.Width / 2, die.Position.Y + dieSize.Height / 2) - circleCenter;
                var disLT = new Point(die.Position.X - dieSize.Width / 2, die.Position.Y - dieSize.Height / 2) - circleCenter;

                if (disRT.Length < waferSize.Width / 2 && disRB.Length < waferSize.Width / 2 &&
                    disLB.Length < waferSize.Width / 2 && disLT.Length < waferSize.Width / 2)
                    return die;
                else
                    return null;
            }).Where(d => d != null).ToArray();

            SourceDieList = new ObservableCollection<( Point, Point)>(circleDies.Select(d => ( new Point( d.Index.X, d.Index.Y), d.Position)));
            //顯示用
            foreach (var die in circleDies)
            {
                AddShapeMappingAction.Execute(new ROIRotatedRect
                {
                    X = die.Position.X / 50,
                    Y = die.Position.Y / 50,
                    LengthX = (dieSize.Width / 2) / 50,
                    LengthY = (dieSize.Height / 2) / 50,
                    IsInteractived = true,
                    IsMoveEnabled = false,
                    IsResizeEnabled = false,
                    IsRotateEnabled = false,
                    CenterCrossLength = 0,
                    ToolTip = die.Name
                });

            }

        });
        public ICommand MappingPreviewMouseUpCommand => new RelayCommand(() =>
        {
            ROIShape selectShape = MappingDrawings.Select(shape =>
             {
                 var rectBegin = shape.LeftTop;
                 var rectEnd = shape.RightBottom;
                 var rect = new Rect(rectBegin, rectEnd);
                 if (rect.Contains(MappingMousePixel))
                     return shape;
                 else
                     return null;
             }).Where(s => s != null).FirstOrDefault();


            if (selectShape != null)
            {
                if (tempselectShape != null)
                {
                    tempselectShape.Stroke = System.Windows.Media.Brushes.LightGreen;

                }

                selectShape.Stroke = System.Windows.Media.Brushes.Red;
                tempselectShape = selectShape;

                //        AddShapeMappingAction.Execute(selectShape);

                //         RemoveShapeMappingAction.Execute(selectShape);

            }




        });


    }
}
