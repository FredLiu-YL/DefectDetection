using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model
{
    public partial class Machine
    {

        private IMotionController motionController;
        private MachineSetting machineSetting;



        private bool isSimulate ;
        public Machine()
        {

            isSimulate = true;
            machineSetting = new MachineSetting();

        }
        public AutoFocusSystem FocusSystem { get; protected set; }
        public ICamera Camera { get; protected set; }
        public Axis[] Axes { get; protected set; }
        public SignalDI[] DIs { get; protected set; }
        public SignalDO[] DOs { get; protected set; }




        public  void Dispose()
        {



        }

    }




}
