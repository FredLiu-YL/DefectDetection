using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;
using YuanliCore.Interface.Motion;

namespace YuanliCore.Motion
{
    public class SimulateMotionControllor : IMotionController
    {
        private double[] simulatePosition; //暫存虛擬座標
        private IEnumerable<Axis> axes;
        public SimulateMotionControllor(IEnumerable< AxisInfo> axisInfos)
        {
            List<double> axesPos = new List<double>();

            axes = axisInfos.Select(info => {

                axesPos.Add(0);
                return new Axis(this, info.AxisID);
                
                }).ToArray();
            simulatePosition = axesPos.ToArray();
        }

        public bool IsOpen => throw new NotImplementedException();

        public IEnumerable<Axis> Axes => axes;

        public IEnumerable<SignalDI> IutputSignals => throw new NotImplementedException();

        public IEnumerable<SignalDO> InputSignals => throw new NotImplementedException();

        public AxisDirection GetAxisDirectionCommand(int id)
        {
            throw new NotImplementedException();
        }

        public void GetLimitCommand(int id, out double limitN, out double limitP)
        {
            throw new NotImplementedException();
        }

        public double GetPositionCommand(int id)
        {
            return simulatePosition[id];
        }

        public MotionVelocity GetSpeedCommand(int id)
        {
            throw new NotImplementedException();
        }

        public void HomeCommand(int id)
        {
            simulatePosition[id]=0;
        }

        public void InitializeCommand()
        {
            
        }

        public void MoveCommand(int id, double distance)
        {
            simulatePosition[id]+= distance;
        }

        public void MoveToCommand(int id, double position)
        {
            simulatePosition[id]  = position;
        }

        public Axis[] SetAxesParam(IEnumerable<AxisInfo> axisInfos)
        {
            throw new NotImplementedException();
        }

       

        public void SetAxisDirectionCommand(int id, AxisDirection direction)
        {
            throw new NotImplementedException();
        }

        public SignalDI[] SetInputs(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }

        public void SetLimitCommand(int id, double minPos, double maxPos)
        {
            throw new NotImplementedException();
        }

        public SignalDO[] SetOutputs(IEnumerable<string> names)
        {
            throw new NotImplementedException();
        }

        public void SetSpeedCommand(int id, double velocity, double accVelocity, double decVelocity)
        {
            throw new NotImplementedException();
        }

        public void SetSpeedCommand(int id, MotionVelocity motionVelocity)
        {
            throw new NotImplementedException();
        }
    }

}
