using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface.Motion;

namespace YuanliCore.Interface
{
    public interface IMotionController
    {
        /// <summary>
        /// 
        /// </summary>
        bool IsOpen { get; }


        IEnumerable<Axis> Axes { get; }
        IEnumerable<SignalDI> IutputSignals { get; }
        IEnumerable<SignalDO> InputSignals { get; }



        void InitializeCommand();


        Axis[] SetAxesParam(IEnumerable<AxisInfo> axisInfos);
        SignalDO[] SetOutputs(IEnumerable<string> names);
        SignalDI[] SetInputs(IEnumerable<string> names);


        void MoveCommand( int id,  double distance);

        void MoveToCommand( int id ,double position);

        void HomeCommand(int id);

        double GetPositionCommand(int id);

        void GetLimitCommand(int id, out double limitN, out double limitP);
        void SetLimitCommand(int id, double minPos , double maxPos);
        MotionVelocity GetSpeedCommand(int id);
        void SetSpeedCommand(int id, MotionVelocity motionVelocity);
        void SetAxisDirectionCommand(int id ,AxisDirection direction);
        AxisDirection GetAxisDirectionCommand(int id);
    }
}
