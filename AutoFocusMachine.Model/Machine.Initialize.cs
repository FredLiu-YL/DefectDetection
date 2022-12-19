using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore;
using YuanliCore.CameraLib;
using YuanliCore.CameraLib.IDS;
using YuanliCore.Interface;
using YuanliCore.Motion;
using YuanliCore.Motion.Marzhauser;

namespace AutoFocusMachine.Model
{
    public partial class Machine
    {



        public void Initialize()
        {

            InitialMotionController(isSimulate);
            InitialCamera(isSimulate);
            InitialAFsystem(isSimulate);



        }




        private void InitialMotionController(bool isSimulate)
        {

            if (isSimulate)
                motionController = new SimulateMotionControllor();
            else
                motionController = new TangoController(machineSetting.TangoComPort);




            motionController.InitializeCommand();

            Axes = motionController.Axes.ToArray();


        }




        private void InitialCamera(bool isSimulate)
        {
            if (isSimulate)
                Camera = new SimulateCamera();
            else
            {
                var    ueyeCamera = new UeyeCamera();

                ueyeCamera.Open();
                ueyeCamera.Load(machineSetting.CameraFilePath);
                Camera = ueyeCamera;
            }
                
        }




        private void InitialAFsystem(bool isSimulate)
        {

            FocusSystem = new AutoFocusSystem(machineSetting.AutoFocusComPort);


            FocusSystem.Open();

        }

    }


}
