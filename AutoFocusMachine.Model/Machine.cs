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
        private Axis[] axes;
        private ICamera camera;
        private AutoFocusSystem focusSystem;


        private bool isSimulate;
        public  Machine()
        {

            isSimulate = false;
            machineSetting = new MachineSetting();

            Initialize();
            AssisnModule();
            Home() ; 

        }
        //   public AutoFocusSystem FocusSystem { get; protected set; }
        public AutoFocusModule AFModule { get; protected set; }
        public TableModule Table_Module { get; protected set; }


        //    public ICamera Camera { get; protected set; }
        //   public Axis[] Axes { get; protected set; }
        //    public SignalDI[] DIs { get; protected set; }
        //   public SignalDO[] DOs { get; protected set; }




        public void Dispose()
        {
            camera.Close();
            if (focusSystem != null)
                focusSystem.Close();

        }

    }




}
