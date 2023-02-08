using AutoFocusMachine.Model.Recipe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore;
using YuanliCore.AffineTransform;
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
        public Machine()
        {
            /*
            System.Windows.Point[] sPoints = new System.Windows.Point[] { new System.Windows.Point(0, 0), new System.Windows.Point(5000, 0), new System.Windows.Point(5000, 5000) };
            System.Windows.Point[] tPoints = new System.Windows.Point[] { new System.Windows.Point(-15, -3), new System.Windows.Point(4977, 20), new System.Windows.Point(5024, 4981) };
         
            CogAffineTransform tes = new CogAffineTransform(sPoints, tPoints);

            System.Windows.Media.Matrix matrix2D = System.Windows.Media.Matrix.Identity;

            matrix2D.Scale(0.998808, 0.998808);
            matrix2D.Rotate(-0.226589);
            matrix2D.Translate(-7.25, 14.5);

            var s1 = tes.TransPoint(new System.Windows.Point(5000, 0));
            var s2 = tes.TransPoint(new System.Windows.Point(1500, 2100));
            var a1 = matrix2D.Transform(new System.Windows.Point());
            var a2 = matrix2D.Transform(new System.Windows.Point(5000, 0));
            var a3 = matrix2D.Transform(new System.Windows.Point(1500, 2100));*/

            isSimulate = true;
            machineSetting = new MachineSetting();






        }
        public void Initial()
        {


            Initialize();

            AssisnModule();

            Home();
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
