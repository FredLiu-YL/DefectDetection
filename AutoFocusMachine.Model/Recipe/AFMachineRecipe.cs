using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YuanliCore.Interface;

namespace AutoFocusMachine.Model.Recipe
{
    public class AFMachineRecipe : AbstractRecipe
    {

       
        public Point[] FiducialMarkPos { get; set; }


        public Point[] LayoutPos { get; set; }

    }
}
