using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;

namespace YuanliCore.Motion
{
    public class SimulateMotionControllor : IMotionController
    {
        public bool IsOpen => throw new NotImplementedException();

        public IEnumerable<Axis> Axes => throw new NotImplementedException();

        public IEnumerable<SignalDI> IutputSignals => throw new NotImplementedException();

        public IEnumerable<SignalDO> InputSignals => throw new NotImplementedException();

        public void GetLimitCommand(int id, out double limitN, out double limitP)
        {
            throw new NotImplementedException();
        }

        public double GetPositionCommand(int id)
        {
            throw new NotImplementedException();
        }

        public void HomeCommand(int id)
        {
            throw new NotImplementedException();
        }

        public void InitializeCommand()
        {
            throw new NotImplementedException();
        }

        public void MoveCommand(int id, double distance)
        {
            throw new NotImplementedException();
        }

        public void MoveToCommand(int id, double position)
        {
            throw new NotImplementedException();
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
    }

}
