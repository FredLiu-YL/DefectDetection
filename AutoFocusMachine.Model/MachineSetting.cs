using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model
{
    public class MachineSetting : AbstractRecipe
    {

        public string TangoComPort { get; set; } = "COM4";

        public string CameraFilePath { get; set; }  = "C:\\Users\\User\\Documents\\IDSsetting.ini";

        public string AutoFocusComPort { get; set; } = "COM1";
    }
}
