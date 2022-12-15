using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Motion
{
    public class Tango
    {
        [DllImport("User32.dll", SetLastError = true)]
        static extern Boolean MessageBeep(UInt32 beepType);

        [DllImport("Tango_dll.dll", SetLastError = true)]
        static extern Int32 LS_ConnectSimple(Int32 lAnInterfaceType, String pcAComName, Int32 lABaudRate, Int32 bAShowProt);

        [DllImport("Tango_dll.dll", SetLastError = true)]
        static extern Int32 LS_SetShowProt(Int32 bAShowProt);

        [DllImport("Tango_dll.dll", SetLastError = true)]
        static extern Int32 LS_GetPos(out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

        [DllImport("Tango_dll.dll", SetLastError = true)]
        static extern Int32 LS_MoveRelSingleAxis(Int32 lAAxis, Double dDelta, Int32 bAWait);

        // Here you may add all other required Tango_dll.dll functions
        // For more details how to use standard DLL with C# source please read
        // http://msdn.microsoft.com/en-us/magazine/cc164123.aspx  

        private string comPort;

        public Tango(string comPort)
        {

            this.comPort = comPort;

        }

        public double AxisXPos;
        public double AxisYPos;
        public double AxisZPos;
        public double AxisRPos;

        public bool IsOpen { get; set; }

        private void Initial()
        {



        }

        private void Open()
        {

            try
            {
                Int32 ShowProt = 0;

                Int32 loc_err = LS_ConnectSimple(1, comPort, 57600, 0);
                if (loc_err == 0)
                    IsOpen = true;
                else
                    throw new Exception("");

            }
            catch
            {
                MessageBeep(0);

            }

        }


        private void UpdatePosition()
        {
            Double xx, yy, zz, aa;
            try
            {
                LS_GetPos(out xx, out yy, out zz, out aa);
                AxisXPos = Math.Round(xx,4); 
                AxisYPos = Math.Round(yy,4);
                AxisZPos = Math.Round(zz,4);
                AxisRPos = Math.Round(aa,4);
            }
            catch
            {
            }
        }
    }
}
