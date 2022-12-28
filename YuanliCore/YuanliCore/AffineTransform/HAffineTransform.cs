//using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.Interface;

namespace YuanliCore.AffineTransform
{
    public class HAffineTransform : ITransform
    {


        //private HHomMat2D homMat2D = new HHomMat2D();

        //public HAffineTransform(IEnumerable<Point> source, IEnumerable<Point> target)
        //{
        //    homMat2D = CreateTransform( source,  target);
        //}



        //public Point TransPoint(Point point)
        //{

        //    var tY = homMat2D.AffineTransPoint2d(point.Y, point.X, out double tX);
        //    return new  Point(tX, tY) ;
        //}


        //private HHomMat2D CreateTransform(IEnumerable<Point> source, IEnumerable<Point> target)
        //{
        //    HHomMat2D hom2D = new HHomMat2D();
        //    HTuple sX = new HTuple(source.Select(p => p.X).ToArray());
        //    HTuple sY = new HTuple(source.Select(p => p.Y).ToArray());
        //    HTuple tX = new HTuple(target.Select(p => p.X).ToArray());
        //    HTuple tY = new HTuple(target.Select(p => p.Y).ToArray());

        //    hom2D.VectorToSimilarity(sY, sX, tY, tX);
        //    return hom2D;
        //}
        public Point TransPoint(Point point)
        {
            throw new NotImplementedException();
        }
    }


}
