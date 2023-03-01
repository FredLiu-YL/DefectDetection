using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess
{
    public abstract class CogMethod
    {

        public string MethodName { get; set; }

        public abstract void Dispose();

    }
}
