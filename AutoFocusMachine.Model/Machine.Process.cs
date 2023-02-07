using AutoFocusMachine.Model.Recipe;
using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.Caliper;
using Cognex.VisionPro.PMAlign;
using Cognex.VisionPro.QuickBuild;
using Cognex.VisionPro.ToolGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using YuanliCore.AffineTransform;
using YuanliCore.CameraLib;
using YuanliCore.ImageProcess;
using YuanliCore.ImageProcess.Match;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model
{
    public partial class Machine
    {
        private CogProcess cogProcess = new CogProcess();
        private CogAffineTransform cogAffineTransform;


        public event Action<CogProcessResult> ResultEvent ;
        public event Action<string> processMessage;
        public event Action<ICogRecord> RecordEvent;
        public Die[] SimilateDies;


        public async Task ProcessRun(AFMachineRecipe mainRecipe)
        {
            int id =  Thread.CurrentThread.ManagedThreadId;
            await   Task.Run( async () =>
            {
                try
                {
                    PorcessInitial();
                    int id1 = Thread.CurrentThread.ManagedThreadId;
                    //wafer 整體位置 定位
                    //          Die[] dies = await WaferLocate(MainRecipe.PMParams, MainRecipe.WaferData, MainRecipe.FiducialMarkGrabPos);
                    Die[] dies = SimilateDies;
                    foreach (var die in dies)
                    {
                        processMessage?.Invoke($" Processing Index  X:{die.Index.X } , Index  X:{die.Index.Y }  ");
                        BitmapSource bmp = await GetDieImage(die);
                        await DieProcess(die.Index, bmp, mainRecipe);

                    }
                }
                catch (Exception ex)
                {

                    throw ex;
                }
                finally
                {



                }
            });
        }




        public void PorcessInitial()
        {
            //模擬用


        }


        private async Task<Die[]> WaferLocate(PatmaxParams patmaxParams, Wafer wafer, Point[] grabPos)
        {
            //找出Wafer  3點來與原資料做定位
            foreach (var pos in grabPos)
            {
              
                await Table_Module.TableMoveTo(pos);

                BitmapSource img = Table_Module.GrabAsync();

                //  BitmapSource img = MainImage.FormatConvertTo(System.Windows.Media.PixelFormats.Bgr24);
                Frame<byte[]> frametemp = img.ToByteFrame();
                ICogImage cogImg = frametemp.ColorFrameToCogImage(0.333, 0.333, 0.333);

                //獲得定位點的三個座標
                cogProcess.RunPatternMatch(frametemp, patmaxParams.Pattern, patmaxParams.RunParams);

            }
            //暫時不需要資料 先用同一組
            cogAffineTransform = new CogAffineTransform(wafer.FiducialMarkPos, wafer.FiducialMarkPos);

            return wafer.Dies.Select(die =>
            {
                var pos = cogAffineTransform.TransPoint(die.Position);
                Die d = new Die();
                d = die;
                d.Position = pos;
                return d;
            }).ToArray();




        }
        private async Task DieProcess(System.Drawing.Point index , BitmapSource bmp, AFMachineRecipe mainRecipe)
        {
            int id1 = Thread.CurrentThread.ManagedThreadId;
            ICogImage cogImg1 = bmp.ColorFrameToCogImage(0.333, 0.333, 0.333);
            await DieLocate(cogImg1, mainRecipe.PMParams);
            try {
                processMessage?.Invoke($" Meansure Processing  ");
                var meansureResult = Meansure(mainRecipe.LineAParam, mainRecipe.LineBParam);
                if (meansureResult.lineA == null || meansureResult.lineB == null)
                {
                    bmp.Save($"E:\\ErrorImage {index.X}-{index.Y}.bmp");
                    return;
                }
                processMessage?.Invoke($" Inspection Processing  ");
                var inspResult = Inspection( mainRecipe.DefectParam.RunParams, mainRecipe.DefectParam.ROI);

                ResultEvent?.Invoke(new CogProcessResult {Index=new Point( index.X,index.Y) ,LineA = meansureResult.lineA, LineB = meansureResult.lineB,MeansureRecord= meansureResult.record ,
                                                 Distance= meansureResult.Distance,   DefectCenter = inspResult.defectCenter,   DefectArea= inspResult.defectArea, InspRecord = inspResult.record });

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private async Task<BitmapSource> GetDieImage(Die die)
        {
            int id1 = Thread.CurrentThread.ManagedThreadId;
            await Table_Module.TableMoveTo(die.Position);
            processMessage?.Invoke($" Move To : {die.Position}  ");
            await Task.Delay(500);
            BitmapSource bmp = Table_Module.GrabAsync();
            processMessage?.Invoke($" Grab Image  ");
            return bmp;
        }
        private async Task DieLocate(ICogImage cogImg1, PatmaxParams patmaxParams)
        {

            var result = cogProcess.RunPatternMatch(cogImg1, patmaxParams.Pattern, patmaxParams.RunParams);
            if (result.Item1.Count() == 0) throw new Exception("Pattern is not Found ");
            cogProcess.RunFixture(result.Item1[0].CogImg, result.Item1[0].Linear);

         //   RecordEvent?.Invoke(result.Item2);

        }

        private (CogLineSegment lineA, CogLineSegment lineB, double Distance, ICogRecord record) Meansure( CogFindLine findLineParamA, CogFindLine findLineParamB)
        {
            int id1 = Thread.CurrentThread.ManagedThreadId;
            return cogProcess.RunMeansure(findLineParamA, findLineParamB);


        }

        private (Point[] defectCenter, double[] defectArea, ICogRecord record) Inspection( CogBlob cogBlobRunParams, ICogRegion cogRegion)
        {
            int id1 = Thread.CurrentThread.ManagedThreadId;
            return cogProcess.RunInsp(cogBlobRunParams, cogRegion);
        }
    }

    public class CogProcessResult
    {
        public  Point Index ;
        public CogLineSegment LineA;
        public CogLineSegment LineB;
        public double Distance;
        public Point[] DefectCenter;
        public double[] DefectArea;
        public ICogRecord MeansureRecord;
        public ICogRecord InspRecord;
    }
}
