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



        public void Initialize()
        {

            axes = InitialMotionController(isSimulate);
            camera = InitialCamera(isSimulate);
            focusSystem = InitialAFsystem(isSimulate);



        }

        public void AssisnModule()
        {
            AFModule = new AutoFocusModule(focusSystem);

            Table_Module=  new TableModule(axes , camera);
        }

        public  void Home()
        {
             Task.Run(()=> motionController.HomeCommand(0) ).Wait() ;
        }

        private Axis[] InitialMotionController(bool isSimulate)
        {

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
                motionController = new TangoController(machineSetting.TangoComPort);




            motionController.InitializeCommand();

            return motionController.Axes.ToArray();


        }




        private ICamera InitialCamera(bool isSimulate)
        {
            ICamera camera;
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
