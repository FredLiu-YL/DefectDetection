using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.AffineTransform;

namespace AutoFocusMachine.Model
{
    public class Wafer
    {

        public Wafer(Size waferSize, Size dieSize, Vector diesPitch)
        {
            WaferSize = waferSize;
            DiesPitch = diesPitch;
            DieSize = dieSize;



            RowDiameterCount = (int)Math.Floor((WaferSize.Height + diesPitch.Y) / (DieSize.Height + diesPitch.Y));
            ColDiameterCount = (int)Math.Floor((WaferSize.Width + diesPitch.X) / (DieSize.Width + diesPitch.X));

            Dies = GenDies(RowDiameterCount, ColDiameterCount);
        }

        public Point[] FiducialMarkPos { get; set; }

        /// <summary>
        /// 晶粒資訊
        /// </summary>
        public Die[] Dies { get; set; }
        /// <summary>
        ///  晶粒 的尺寸 um
        /// </summary>
        public Size DieSize { get; }

        /// <summary>
        /// Wafer尺寸 (um)
        /// </summary>
        public Size WaferSize { get; }

        /// <summary>
        /// 晶粒之間的間距 um
        /// </summary>
        public Vector DiesPitch { get; }

        /// <summary>
        /// wafer 直徑上直列數量
        /// </summary>
        public int RowDiameterCount { get; }

        /// <summary>
        /// wafer 直徑上橫列數量
        /// </summary>
        public int ColDiameterCount { get; }


        private Die[] GenDies(int rows, int cols)
        {
            //得出WAFER矩陣 Die Index 依照順序  左上  右上  右下
            Point[] sourceIndex = new Point[] { new Point(1, 1), new Point(cols, 1), new Point(cols, rows) };


            double centerStartX = DieSize.Width / 2;
            double centerStartY = DieSize.Height / 2;



            var centerEndX = centerStartX + (DieSize.Width + DiesPitch.X) * (ColDiameterCount - 1);
            var centerEndY = centerStartY + (DieSize.Height + DiesPitch.Y) * (RowDiameterCount - 1);
            //得出WAFER矩陣 die中心position 依照順序  左上  右上  右下
            Point[] targetPos = new Point[] { new Point(centerStartX, centerStartY), new Point(centerEndX, centerStartY), new Point(centerEndX, centerEndY) };

            // 將 Index 與 中心座標關聯起來
            CogAffineTransform cogAffineTransform = new CogAffineTransform(sourceIndex, targetPos);
            List<Die> dieList = new List<Die>();

           
            for (int x = 1; x <= cols; x++)
            {
                for (int y = 1; y <= rows; y++)
                {
                    var posX = centerStartX + (DieSize.Width + DiesPitch.X) * (x - 1);
                    var posY = centerStartY + (DieSize.Height + DiesPitch.Y) * (y - 1);
                    //  Point pos = cogAffineTransform.TransPoint(new Point(x, y));
                    Point pos = new Point(posX, posY);
                    Die die = new Die
                    {
                        Index = new System.Drawing.Point(x, y),
                        Position = pos,
                        Name = $"X={x} , Y={y}  Pos:{pos}"
                    };
                    dieList.Add(die);
                }
            }



            return dieList.ToArray();
        }

        private void CreateTransForm()
        {



        }

    }
}
