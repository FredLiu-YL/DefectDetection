using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model
{
    public class TableModule
    {
        public TableModule(Axis[] axes, ICamera camera)
        {
            TableX = axes[0];
            TableY = axes[1];
            Camera = camera;

        }

        public Axis TableX { get; set; }
        public Axis TableY { get; set; }

        public ICamera Camera { get; set; }



        public async Task TableMoveTo(Point Pos)
        {
            await Task.WhenAll(TableX.MoveToAsync(Pos.X), TableY.MoveToAsync(Pos.Y));

        }
        public async Task TableMove(Vector Distance)
        {

            await Task.WhenAll(TableX.MoveAsync(Distance.X), TableY.MoveAsync(Distance.Y));
        }

        public async Task<Point> GetPostion()
        {
           return new Point(TableX.Position, TableY.Position);

        }

    }



}
