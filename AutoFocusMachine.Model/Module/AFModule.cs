using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore;

namespace AutoFocusMachine.Model
{
    public class AutoFocusModule
    {

        public AutoFocusModule(AutoFocusSystem autoFocusSystem)
        {
            AFSystem = autoFocusSystem;

        }



        public AutoFocusSystem AFSystem { get; set; }

        public async Task MoveToZ()
        {


        }
        public async Task MoveZ()
        {


        }
    }
}
