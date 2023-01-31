using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore;
using YuanliCore.CameraLib;
using YuanliCore.CameraLib.IDS;
using YuanliCore.Interface;
using YuanliCore.Interface.Motion;
using YuanliCore.Motion;
using YuanliCore.Motion.Marzhauser;

namespace AutoFocusMachine.Model
{
    public partial class Machine
    {
        public event Action<string> IsInitialMessageEvent;

        private void Initialize()
        {
            camera = InitialCamera(isSimulate);
            axes = InitialMotionController(isSimulate);
         
            focusSystem = InitialAFsystem(isSimulate);



        }

        private void AssisnModule()
        {
            var vX = axes[0].AxisVelocity;
            var vY = axes[1].AxisVelocity;
            axes[0].AxisVelocity = new MotionVelocity(18, 2, 2) ;
            axes[1].AxisVelocity = new MotionVelocity(18, 2, 2);

            AFModule = new AutoFocusModule(focusSystem);

            Table_Module = new TableModule(axes, camera);
        }



        private Axis[] InitialMotionController(bool isSimulate)
        {
            IsInitialMessageEvent?.Invoke(" Initial Motion Start");
            if (isSimulate)
            {
                AxisInfo[] axesInfo = new AxisInfo[]
                {   new AxisInfo{AxisID= 0 } ,
                    new AxisInfo{AxisID= 1 },
                    new AxisInfo{AxisID= 2 },
                    new AxisInfo{AxisID= 3 }
                };
                motionController = new SimulateMotionControllor(axesInfo);
            }
            else
            {
                motionController = new TangoController(machineSetting.TangoComPort);
            }


            motionController.InitializeCommand();
            IsInitialMessageEvent?.Invoke(" Initial Motion End");
            return motionController.Axes.ToArray();


        }

        public async void Home()
        {
            IsInitialMessageEvent?.Invoke("Home Start");
            await Task.Run(() => motionController.HomeCommand(0));
            IsInitialMessageEvent?.Invoke("Home end");
        }


        private ICamera InitialCamera(bool isSimulate)
        {
            ICamera camera;
            IsInitialMessageEvent?.Invoke(" Initial Camera Start");
            if (isSimulate)
            {
                string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                camera = new SimulateCamera($"{systemPath}\\03.bmp");
            }

            else
            {
                var ueyeCamera = new UeyeCamera();

                ueyeCamera.Open();
                ueyeCamera.Load(machineSetting.CameraFilePath);
                camera = ueyeCamera;
            }
            IsInitialMessageEvent?.Invoke(" Initial Camera End");
            return camera;
        }




        private AutoFocusSystem InitialAFsystem(bool isSimulate)
        {
            if (isSimulate) return null;
            AutoFocusSystem focusSystem = new AutoFocusSystem(machineSetting.AutoFocusComPort);

            focusSystem.Open();
            return focusSystem;
        }



    }


}
