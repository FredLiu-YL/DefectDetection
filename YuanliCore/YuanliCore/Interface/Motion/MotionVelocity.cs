using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Interface.Motion
{
    public struct MotionVelocity
    {
    
        public MotionVelocity(double fainalVelocity, double accVelocity, double decVelocity)
        {
            FainalVelocity = fainalVelocity;
            AccVelocity = accVelocity;
            DecVelocity = decVelocity;
        }
        /// <summary>
        /// 運動軸最高速度
        /// </summary>
        public double FainalVelocity { get; set; }
        /// <summary>
        /// 運動軸加速度
        /// </summary>
        public double AccVelocity { get; set; }
        /// <summary>
        /// 運動軸減速度
        /// </summary>
        public double DecVelocity { get; set; }
    }
}
