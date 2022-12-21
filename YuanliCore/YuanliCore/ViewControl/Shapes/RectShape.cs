using MathNet.Numerics;
using MathNet.Spatial.Euclidean;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace YuanliCore.Views.CanvasShapes
{
    public sealed class RectShape : Shape
    {
        private Rect defaultRect = new Rect(50, 50, 100, 100);
        private EllipseGeometry hitThumb;
        private EllipseGeometry TR, BR, TL, BL;
        private Dictionary<EllipseGeometry, EllipseGeometry> correspondingVertex;
        public double thumbSize = 7;
        private double minThumbDistance = 20;
        private Point corner1, corner2, corner3, corner4;

        public static readonly DependencyProperty SizeWidthProperty = DependencyProperty.Register(nameof(SizeWidth), typeof(double), typeof(RectShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IniSet)));
        public static readonly DependencyProperty SizeHeightProperty = DependencyProperty.Register(nameof(SizeHeight), typeof(double), typeof(RectShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IniSet)));
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(nameof(X), typeof(double), typeof(RectShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IniSet)));
        public static readonly DependencyProperty YProperty = DependencyProperty.Register(nameof(Y), typeof(double), typeof(RectShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IniSet)));
        public static readonly DependencyProperty AngleProperty = DependencyProperty.Register(nameof(Angle), typeof(double), typeof(RectShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(IniSet)));

        /// <summary>
        /// Height of rectangle
        /// </summary>
        public double SizeHeight
        {
            get { return (double)GetValue(SizeHeightProperty); }
            set { SetValue(SizeHeightProperty, value); }
        }

        /// <summary>
        /// Width of rectangle
        /// </summary>
        public double SizeWidth
        {
            get => (double)GetValue(SizeWidthProperty);
            set => SetValue(SizeWidthProperty, value);
        }

        /// <summary>
        /// center location x
        /// </summary>
        public double X
        {
            get { return (double)GetValue(XProperty); }
            set { SetValue(XProperty, value); }
        }

        /// <summary>
        /// center location y
        /// </summary>
        public double Y
        {
            get { return (double)GetValue(YProperty); }
            set { SetValue(YProperty, value); }
        }

        /// <summary>
        /// angle
        /// </summary>
        public double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            set { SetValue(AngleProperty, value); }
        }

        public RectShape()
        {
            corner1 = defaultRect.TopLeft;
            corner2 = defaultRect.TopRight;
            corner3 = defaultRect.BottomRight;
            corner4 = defaultRect.BottomLeft;

            TR = new EllipseGeometry(defaultRect.TopRight, thumbSize, thumbSize);
            BR = new EllipseGeometry(defaultRect.BottomRight, thumbSize, thumbSize);
            TL = new EllipseGeometry(defaultRect.TopLeft, thumbSize, thumbSize);
            BL = new EllipseGeometry(defaultRect.BottomLeft, thumbSize, thumbSize);

            correspondingVertex = new Dictionary<EllipseGeometry, EllipseGeometry>
            {
                [TR] = BL,
                [BR] = TL,
                [BL] = TR,
                [TL] = BR
            };
        }

        private static void IniSet(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var property = d as RectShape;
            var height = property.defaultRect.Height;
            var width = property.defaultRect.Width;
            if (property.SizeHeight != 0) height = property.SizeHeight;
            if (property.SizeWidth != 0) width = property.SizeWidth;

            var locationX = property.defaultRect.X;
            var locationY = property.defaultRect.Y;
            if (property.X != 0) locationX = property.X;
            if (property.Y != 0) locationY = property.Y;

            property.defaultRect = new Rect(locationX - property.SizeWidth / 2, locationY - property.SizeHeight / 2, width, height);

            property.corner1 = property.defaultRect.TopLeft;
            property.corner2 = property.defaultRect.TopRight;
            property.corner3 = property.defaultRect.BottomRight;
            property.corner4 = property.defaultRect.BottomLeft;

            property.TR = new EllipseGeometry(property.defaultRect.TopRight, property.thumbSize, property.thumbSize);
            property.BR = new EllipseGeometry(property.defaultRect.BottomRight, property.thumbSize, property.thumbSize);
            property.TL = new EllipseGeometry(property.defaultRect.TopLeft, property.thumbSize, property.thumbSize);
            property.BL = new EllipseGeometry(property.defaultRect.BottomLeft, property.thumbSize, property.thumbSize);

            if (property.Angle != 0)
            {
                Point center = new Point((property.TL.Center.X + property.BR.Center.X) / 2, (property.TL.Center.Y + property.BR.Center.Y) / 2);
                RotateTransform trans = new RotateTransform(property.Angle, center.X, center.Y);
                property.corner1 = trans.Transform(property.TL.Center);
                property.corner2 = trans.Transform(property.TR.Center);
                property.corner3 = trans.Transform(property.BR.Center);
                property.corner4 = trans.Transform(property.BL.Center);
            }

            property.TL.Center = property.corner1;
            property.TR.Center = property.corner2;
            property.BR.Center = property.corner3;
            property.BL.Center = property.corner4;

            property.correspondingVertex = new Dictionary<EllipseGeometry, EllipseGeometry>
            {
                [property.TR] = property.BL,
                [property.BR] = property.TL,
                [property.BL] = property.TR,
                [property.TL] = property.BR
            };
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                GeometryGroup gg = new GeometryGroup();
                gg.Children.Add(ThumbGeometry);
                gg.Children.Add(RectGeometry);

                return gg;
            }
        }

        /// <summary>
        /// Draw points
        /// </summary>
        private Geometry ThumbGeometry
        {
            get
            {
                GeometryGroup thumbGeometry = new GeometryGroup();
                thumbGeometry.Children.Add(TR);
                thumbGeometry.Children.Add(TL);
                thumbGeometry.Children.Add(BR);
                thumbGeometry.Children.Add(BL);
                return thumbGeometry;
            }
        }

        /// <summary>
        /// Draw lines
        /// </summary>
        private PathGeometry RectGeometry
        {
            get
            {
                PathSegment[] segments = new PathSegment[]
                {
                    new LineSegment(corner2, true),
                    new LineSegment(corner3, true),
                    new LineSegment(corner4, true)
                };

                PathFigure figure = new PathFigure(corner1, segments, true);
                return new PathGeometry(new PathFigure[] { figure });
            }
        }

        public Point[] GetRectPoints()
        {
            return null;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            CaptureMouse();
            Point pos = e.GetPosition(this);
            hitThumb = HitTestThumb(pos);
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            CaptureMouse();
            var mousePoint = e.GetPosition(this);            
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (hitThumb != null)
                {
                    var offset = mousePoint - hitThumb.Center;
                    DragToDeform(hitThumb, offset);
                };
            }
            else if (e.RightButton == MouseButtonState.Pressed)
            {
                if (hitThumb != null)
                {
                    var offset = mousePoint - hitThumb.Center;
                    DragThumbMove(hitThumb, offset);
                };
            }
            else if (e.MiddleButton == MouseButtonState.Pressed)
            {
                if (hitThumb != null)
                {
                    var offset = mousePoint - hitThumb.Center;
                    DragThumbRotate(hitThumb, offset);
                };
            }
            e.Handled = true;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
        }

        protected override void OnRender(DrawingContext dc)
        {
            var pen = new Pen(Stroke, StrokeThickness);
            dc.DrawGeometry(Fill, pen, ThumbGeometry);
            dc.DrawGeometry(null, pen, RectGeometry);
        }

        private EllipseGeometry HitTestThumb(Point mousePoint)
        {
            if (TR.FillContains(mousePoint)) return TR;
            if (TL.FillContains(mousePoint)) return TL;
            if (BR.FillContains(mousePoint)) return BR;
            if (BL.FillContains(mousePoint)) return BL;
            return null;
        }

        /// <summary>
        /// line: y=ax+b    circle: (x-c)^2+(y-d)^2=r^2
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="r"></param>
        /// <param name="Vertex"></param>
        /// <returns></returns>
        private (Vector NewVetex1, Vector NewVetex2) GenerateSolution(double a, double b, double c, double d, double r, Vector Vertex)
        {
            //line: y=ax+b
            //circle: (x-c)^2+(y-d)^2=r^2
            //Ax^2+Bx+C=0;
            //double A = Math.Pow(parameter_a, 2) + 1;
            //double B = 2 * (parameter_a * parameter_b - parameter_a * d - c);
            //double C = Math.Pow(parameter_b - d, 2) + Math.Pow(c, 2) - Math.Pow(r, 2);
            //double D = Math.Pow(B, 2) - 4 * A * C;
            double A = Math.Pow(a, 2) + 1;
            double B = 2 * (a * b - a * d - c);
            double C = Math.Pow(b - d, 2) + Math.Pow(c, 2) - Math.Pow(r, 2);
            double D = Math.Pow(B, 2) - 4 * A * C;

            var x1 = (-B + Math.Sqrt(D)) / (2 * A);
            var y1 = a * x1 + b;
            var x2 = (-B - Math.Sqrt(D)) / (2 * A);
            var y2 = a * x2 + b;

            Vector whichIsa0 = new Vector(x1, y1) - Vertex;
            Vector whichIsa1 = new Vector(x2, y2) - Vertex;
            if (whichIsa0.Length < whichIsa1.Length)
                return (new Vector(x1, y1), new Vector(x2, y2));
            else
                return (new Vector(x2, y2), new Vector(x1, y1));
        }

        private bool IsMinDistance(Vector origin, Vector newVetexA1, Vector newVetexA2, Vector newVetexB1, Vector newVetexB2)
        {
            Vector curOffsetA = (Vector)origin - newVetexA1;
            Vector curOffsetB = new Vector(newVetexB2.X - newVetexB1.X, newVetexB2.Y - newVetexB1.Y);
            Vector curOffsetHor = new Vector(newVetexA1.X - newVetexB1.X, newVetexA1.Y - newVetexB1.Y);
            Vector curOffsetVer = new Vector(newVetexB2.X - newVetexA1.X, newVetexB2.Y - newVetexA1.Y);
            if (Math.Abs(curOffsetA.Length) <= minThumbDistance || Math.Abs(curOffsetHor.Length) <= minThumbDistance || Math.Abs(curOffsetVer.Length) <= minThumbDistance || Math.Abs(curOffsetB.Length) <= minThumbDistance) return true;
            else return false;
        }

        /// <summary>
        /// 等比放大
        /// </summary>
        /// <param name="dragedVertex"></param>
        /// <param name="dragedOffset"></param>
        private void DragToZoom(EllipseGeometry dragedVertex, Vector dragedOffset)
        {
            InvalidateVisual();

            EllipseGeometry dragedVertex1 = dragedVertex;
            EllipseGeometry dragedVertex2 = correspondingVertex[dragedVertex];
            int index = correspondingVertex.Select(_ => _.Value).ToList().IndexOf(dragedVertex1);

            EllipseGeometry slavedVertex1 = correspondingVertex.Values.ToArray()[(index + 1) % 4];
            EllipseGeometry slavedVertex2 = correspondingVertex.Values.ToArray()[(index + 3) % 4];

            Vector vertexCenterA1 = (Vector)dragedVertex1.Center;
            Vector vertexCenterA2 = (Vector)dragedVertex2.Center;
            Vector vertexCenterB1 = (Vector)slavedVertex1.Center;
            Vector vertexCenterB2 = (Vector)slavedVertex2.Center;

            Point origin = new Point((dragedVertex1.Center.X + dragedVertex2.Center.X) / 2, (dragedVertex1.Center.Y + dragedVertex2.Center.Y) / 2);

            vertexCenterA1 += dragedOffset;
            double newDistance = (vertexCenterA1 - (Vector)origin).Length;
            double oldDistance = ((Vector)dragedVertex1.Center - (Vector)origin).Length;
            double times = newDistance / oldDistance;

            var dragedDiagnal = Fit.Line(
                new[] { dragedVertex2.Center.X, dragedVertex1.Center.X },
                new[] { dragedVertex2.Center.Y, dragedVertex1.Center.Y });

            var slavedDiagnal = Fit.Line(
                new[] { slavedVertex2.Center.X, slavedVertex1.Center.X },
                new[] { slavedVertex2.Center.Y, slavedVertex1.Center.Y });

            var newVetexA = GenerateSolution(dragedDiagnal.Item2, dragedDiagnal.Item1, origin.X, origin.Y, newDistance, (Vector)dragedVertex1.Center);
            var newVetexB = GenerateSolution(slavedDiagnal.Item2, slavedDiagnal.Item1, origin.X, origin.Y, newDistance, (Vector)slavedVertex1.Center);

            if (IsMinDistance((Vector)origin, newVetexA.NewVetex1, newVetexA.NewVetex2, newVetexB.NewVetex1, newVetexB.NewVetex2)) return;

            corner1 = slavedVertex1.Center = (Point)newVetexB.NewVetex1;
            corner2 = dragedVertex2.Center = (Point)newVetexA.NewVetex2;
            corner3 = slavedVertex2.Center = (Point)newVetexB.NewVetex2;
            corner4 = dragedVertex1.Center = (Point)newVetexA.NewVetex1;
        }

        /// <summary>
        /// Deform rectangle
        /// </summary>
        /// <param name="dragedVertex"></param>
        /// <param name="dragedOffset"></param>
        private void DragToDeform(EllipseGeometry dragedVertex, Vector dragedOffset)
        {
            InvalidateVisual();
            EllipseGeometry dragedVertex1 = dragedVertex;
            EllipseGeometry dragedVertex2 = correspondingVertex[dragedVertex];
            int index = correspondingVertex.Select(_ => _.Value).ToList().IndexOf(dragedVertex1);

            EllipseGeometry slavedVertex1 = correspondingVertex.Values.ToArray()[(index + 1) % 4];
            EllipseGeometry slavedVertex2 = correspondingVertex.Values.ToArray()[(index + 3) % 4];

            Vector vertexCenterA1 = (Vector)dragedVertex1.Center;
            Vector vertexCenterA2 = (Vector)dragedVertex2.Center;
            Vector vertexCenterB1 = (Vector)slavedVertex1.Center;
            Vector vertexCenterB2 = (Vector)slavedVertex2.Center;

            Line2D l1 = new Line2D(new Point2D(vertexCenterA2.X,vertexCenterA2.Y), new Point2D(vertexCenterB1.X,vertexCenterB1.Y));
            Line2D l2 = new Line2D(new Point2D(vertexCenterA2.X, vertexCenterA2.Y), new Point2D(vertexCenterB2.X, vertexCenterB2.Y));
            Point2D p = new Point2D(dragedVertex.Center.X + dragedOffset.X, dragedVertex.Center.Y + dragedOffset.Y);

            Point p1 = new Point(l1.ClosestPointTo(p, false).X, l1.ClosestPointTo(p, false).Y);
            Point p2 = new Point(l2.ClosestPointTo(p, false).X, l2.ClosestPointTo(p, false).Y);

            Point origin = new Point((dragedVertex1.Center.X + dragedVertex2.Center.X) / 2, (dragedVertex1.Center.Y + dragedVertex2.Center.Y) / 2);
            if (IsMinDistance((Vector)origin, new Vector(p.X,p.Y), new Vector(dragedVertex2.Center.X, dragedVertex2.Center.Y), new Vector(p1.X, p1.Y), new Vector(p2.X, p2.Y))) return;

            corner1 = slavedVertex1.Center = new Point(p1.X, p1.Y);
            corner2 = dragedVertex2.Center = new Point(dragedVertex2.Center.X, dragedVertex2.Center.Y);
            corner3 = slavedVertex2.Center = new Point(p2.X, p2.Y);
            corner4 = dragedVertex1.Center = new Point(p.X,p.Y);
        }         

        /// <summary>
        /// Rotate rectangle
        /// </summary>
        /// <param name="dragedVertex"></param>
        /// <param name="dragedOffset"></param>
        private void DragThumbRotate(EllipseGeometry dragedVertex, Vector dragedOffset)
        {
            InvalidateVisual();

            var dragedVertex1 = dragedVertex;
            var dragedVertex2 = correspondingVertex[dragedVertex1];

            var index = correspondingVertex.Select(_ => _.Value).ToList().IndexOf(dragedVertex1);

            EllipseGeometry slavedVertex1 = correspondingVertex.Values.ToArray()[(index + 1) % 4];
            EllipseGeometry slavedVertex2 = correspondingVertex.Values.ToArray()[(index + 3) % 4];

            Vector centerA1 = (Vector)dragedVertex1.Center;
            Vector centerA2 = (Vector)dragedVertex2.Center;
            Vector centerB1 = (Vector)slavedVertex1.Center;
            Vector centerB2 = (Vector)slavedVertex2.Center;
            Point origin = new Point((centerA1.X + centerA2.X) / 2, (centerA1.Y + centerA2.Y) / 2);

            Vector preOffsetA = (Vector)origin - centerA1;
            centerA1 += dragedOffset;
            Vector curOffsetA = (Vector)origin - centerA1;
            double angle = Vector.AngleBetween(preOffsetA, curOffsetA);
            RotateTransform trans = new RotateTransform(angle, origin.X, origin.Y);

            centerA1 = (Vector)trans.Transform(dragedVertex1.Center);
            centerA2 = (Vector)trans.Transform((Point)centerA2);
            centerB1 = (Vector)trans.Transform((Point)centerB1);
            centerB2 = (Vector)trans.Transform((Point)centerB2);

            if (IsMinDistance((Vector)origin, centerA1, centerA2, centerB1, centerB2)) return;

            corner4 = dragedVertex1.Center = (Point)centerA1;
            corner2 = dragedVertex2.Center = (Point)centerA2;
            corner1 = slavedVertex1.Center = (Point)centerB1;
            corner3 = slavedVertex2.Center = (Point)centerB2;
        }

        /// <summary>
        /// Move rectangle
        /// </summary>
        /// <param name="dragedVertex"></param>
        /// <param name="dragedOffset"></param>
        private void DragThumbMove(EllipseGeometry dragedVertex, Vector dragedOffset)
        {
            InvalidateVisual();

            var dragedVertex1 = dragedVertex;
            var dragedVertex2 = correspondingVertex[dragedVertex1];

            var values = correspondingVertex.Select(_ => _.Value).ToList();
            var index = correspondingVertex.Select(_ => _.Value).ToList().IndexOf(dragedVertex1);

            //counterclockwise first B1
            EllipseGeometry slavedVertex1 = correspondingVertex.Values.ToArray()[(index + 1) % 4];
            EllipseGeometry slavedVertex2 = correspondingVertex.Values.ToArray()[(index + 3) % 4];

            var centerA1 = dragedVertex1.Center;
            var centerA2 = dragedVertex2.Center;
            var centerB1 = slavedVertex1.Center;
            var centerB2 = slavedVertex2.Center;

            centerA1 += dragedOffset;
            centerA2 += dragedOffset;
            centerB1 += dragedOffset;
            centerB2 += dragedOffset;

            corner4 = dragedVertex1.Center = centerA1;
            corner2 = dragedVertex2.Center = centerA2;
            corner1 = slavedVertex1.Center = centerB1;
            corner3 = slavedVertex2.Center = centerB2;
        }
    }
}
