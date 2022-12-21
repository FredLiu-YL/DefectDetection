using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace YuanliCore.AffineTransform
{
    public class HAffineTransform
    {
        HHomMat2D homMat2D = new HHomMat2D();


        public void CreateTransform(IEnumerable<Point> source, IEnumerable<Point> target)
        {
            HTuple sX = new HTuple(source.Select(p => p.X));
            HTuple sY = new HTuple(source.Select(p => p.Y));
            HTuple tX = new HTuple(target.Select(p => p.X));
            HTuple tY = new HTuple(target.Select(p => p.Y));

            homMat2D.VectorToSimilarity(sY, sX, tY, tX);

        }

        public Point TransPoint(Point point)
        {

            var tX = homMat2D.AffineTransPoint2d(point.Y, point.X, out double tY);
            return new  Point(tX, tY) ;
        }

    }


}
