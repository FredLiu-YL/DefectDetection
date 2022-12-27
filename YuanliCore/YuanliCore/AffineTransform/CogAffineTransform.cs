using Cognex.VisionPro;
using Cognex.VisionPro.CalibFix;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.Interface;

namespace YuanliCore.AffineTransform
{
    public class CogAffineTransform : ITransform
    {
        private System.Windows.Media.Matrix matrix2D = System.Windows.Media.Matrix.Identity;
        private CogCalibNPointToNPointTool calibNPointTool;

        public CogAffineTransform(IEnumerable<Point> source, IEnumerable<Point> target)
        {
           
            calibNPointTool = new CogCalibNPointToNPointTool();
            calibNPointTool.Calibration.DOFsToCompute = CogNPointToNPointDOFConstants.ScalingRotationAndTranslation;

            matrix2D = CreateMatriX(source.ToArray(), target.ToArray());

        }
        private System.Windows.Media.Matrix CreateMatriX(Point[] source, Point[] target)
        {
            System.Windows.Media.Matrix mat = System.Windows.Media.Matrix.Identity;
            if (source.Length != target.Length) throw new Exception("poins are incorrect");

            for (int i = 0; i < source.Length; i++)
                calibNPointTool.Calibration.AddPointPair(target[i].X, target[i].Y, source[i].X, source[i].Y);


            // CalibNPointTool.Calibration.AddPointPair(-15, -3, 0, 0);
            //  CalibNPointTool.Calibration.AddPointPair(4977, 20, 5000, 0);
            //  CalibNPointTool.Calibration.AddPointPair(5024, 4981, 5000, 5000);

            calibNPointTool.Calibration.Calibrate();

            ICogTransform2D cogMat = calibNPointTool.Calibration.GetComputedUncalibratedFromCalibratedTransform();
            CogTransform2DLinear linear = cogMat.LinearTransform(0, 0);
            var arc = linear.Rotation / Math.PI * 180;


            //將比例 旋轉  位移量 加入矩陣   ，順序必須正確
            mat.Scale(linear.Scaling, linear.Scaling);
            //跟Halcon的象限有差 ，所以角度可能差一個負號 。 但計算結果方向一致 ， 待觀察
            mat.Rotate(arc);
            mat.Translate(linear.TranslationX, linear.TranslationY);

            //釋放康奈視資源
            calibNPointTool.Dispose();
            linear.Dispose();

            return mat;
        }


        public Point TransPoint(Point point)
        {
            return matrix2D.Transform(point);
        }
    }
}
