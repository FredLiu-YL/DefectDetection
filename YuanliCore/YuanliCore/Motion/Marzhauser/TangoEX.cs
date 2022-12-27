using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Motion.Marzhauser
{
    public class TangoLib
    {

        #region Marzhauser Function
        // 參考手冊
        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_ConnectSimple(Int32 lAnInterfaceType, String pcAComName, Int32 lABaudRate, Int32 bAShowProt);
       
        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_Disconnect(int lLSID);
        

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetPos(out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_GetPosEx(int lLSID, out Double pdX, out Double pdY, out Double pdZ, out Double pdA, bool bEncoder);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_MoveRelSingleAxis(Int32 lAAxis, Double dDelta, Int32 bAWait);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_MoveAbsSingleAxis(Int32 lAAxis, Double dDelta, Int32 bAWait);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetVel(out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetAccel(out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetStopAccel(out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetVel(Double pdX, Double pdY, Double pdZ, Double pdA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetAccel(int lLSID, double dX, double dY, double dZ, double dA);
        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetStopAccel(int lLSID, double dX, double dY, double dZ, double dA);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetLimit(int lLSID, int lAxis, out double pdMinRange, out double pdMaxRange);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetLimit(int lLSID, Int32 lAxis, Double dMinRange, Double dMaxRange);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_GetLimitControl(int lLSID, Int32 lAxis, out bool pbActive);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetLimitControl(int lLSID, Int32 lAxis, bool pbActive);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_Calibrate();

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_RMeasure();

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_MoveAbs(Double pX, Double pY, Double pZ, Double pA, Int32 bAWait);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_StopAxes();

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetActiveAxes(Int32 lFlags);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetHandWheelOn(bool bPositionCount, bool bEncoder);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetJoystickOff();

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetJoystickOn(bool bPositionCount, bool bEncoder);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LS_SetHandWheelOff();

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_GetAxisDirection(int lLSID, out int plXD, out int plYD, out int plZD, out int plAD);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_SetAxisDirection(int lLSID, int plXD, int plYD, int plZD, int plAD);


        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_GetJoystickDir(int lLSID, out int plXD, out int plYD, out int plZD, out int plAD);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_SetJoystickDir(int lLSID, int plXD, int plYD, int plZD, int plAD);

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_GetEncoder(int lLSID, out Double pdX, out Double pdY, out Double pdZ, out Double pdA);
        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_GetStatusAxis(int lLSID, out char pcStatusAxisStr, int lMaxLen);
       
        

        [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
        public static extern Int32 LSX_ClearEncoder(int lLSID, int lAxis);
        /*
         [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
         static extern Int32 LSX_GetEncoderPeriod(int lLSID, out Double pdX, out Double pdY, out Double pdZ, out Double pdA);

         [DllImport("Tango_DLL_x64.dll", SetLastError = true)]
         static extern Int32 LSX_SetEncoderPeriod(int lLSID, Double pdX, Double pdY, Double pdZ, Double pdA);
         */
        #endregion
        // 1~4對應的接線方法, 看Switch Borad

        // Here you may add all other required Tango_dll.dll functions
        // For more details how to use standard DLL with C# source please read
        // http://msdn.microsoft.com/en-us/magazine/cc164123.aspx  
    }
}
