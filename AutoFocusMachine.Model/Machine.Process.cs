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


        public event Action<CogProcessResult> ResultEvent;
        public event Action<ICogRecord> RecordEvent;
        public Die[] SimilateDies;


        public async Task ProcessRun(AFMachineRecipe mainRecipe)
        {
            await   Task.Run( async () =>
            {
                try
                {
                    PorcessInitial();

                    //wafer 整體位置 定位
                    //          Die[] dies = await WaferLocate(MainRecipe.PMParams, MainRecipe.WaferData, MainRecipe.FiducialMarkGrabPos);
                    Die[] dies = SimilateDies;
                    foreach (var die in dies)
                    {

                        BitmapSource bmp = await GetDieImage(die);

                        await DieProcess(bmp, mainRecipe);


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
        private async Task DieProcess(BitmapSource bmp, AFMachineRecipe mainRecipe)
        {

            ICogImage cogImg1 = bmp.ColorFrameToCogImage(0.333, 0.333, 0.333);
            await DieLocate(cogImg1, mainRecipe.PMParams);

            var meansureResult = Meansure(mainRecipe.LineAParam, mainRecipe.LineBParam);

            var inspResult = Inspection( mainRecipe.DefectParam.RunParams, mainRecipe.DefectParam.ROI);

            ResultEvent?.Invoke(new CogProcessResult { LineA = meansureResult.lineA, LineB = meansureResult.lineB,MeansureRecord= meansureResult.record ,
                                                    DefectCenter = inspResult.defectCenter, InspRecord = inspResult.record });
        }
        private async Task<BitmapSource> GetDieImage(Die die)
        {
            await Table_Module.TableMoveTo(die.Position);
            await Task.Delay(200);
            BitmapSource bmp = Table_Module.GrabAsync();
            return bmp;
        }
        private async Task DieLocate(ICogImage cogImg1, PatmaxParams patmaxParams)
        {

            var result = cogProcess.RunPatternMatch(cogImg1, patmaxParams.Pattern, patmaxParams.RunParams);
            if (result.Item1.Count() == 0) throw new Exception("Pattern is not Found ");
            cogProcess.RunFixture(result.Item1[0].CogImg, result.Item1[0].Linear);

            RecordEvent?.Invoke(result.Item2);

        }

        private (CogLineSegment lineA, CogLineSegment lineB, ICogRecord record) Meansure( CogFindLine findLineParamA, CogFindLine findLineParamB)
        {
            return cogProcess.RunMeansure(findLineParamA, findLineParamB);


        }

        private (Point[] defectCenter, ICogRecord record) Inspection( CogBlob cogBlobRunParams, ICogRegion cogRegion)
        {

            return cogProcess.RunInsp(cogBlobRunParams, cogRegion);
        }
    }

    public class CogProcessResult
    {

        public CogLineSegment LineA;
        public CogLineSegment LineB;
        public Point[] DefectCenter;
        public ICogRecord MeansureRecord;
        public ICogRecord InspRecord;
    }
}
