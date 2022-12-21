using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Interface.Motion
{
    public class AxisInfo
    {
        /// <summary>
        /// 運動軸在卡的號碼
        /// </summary>
        public int AxisID { get; set; }
        /// <summary>
        /// 運動軸名稱
        /// </summary>
        public string AxisName { get; set; }
        /// <summary>
        /// 運動軸最高速度
        /// </summary>
        public double AxisVelocity { get; set; }
        /// <summary>
        /// 運動軸加速度
        /// </summary>
        public double AxisAccVelocity { get; set; }
        /// <summary>
        /// 運動軸減速度
        /// </summary>
        public double AxisDecVelocity { get; set; }
        /// <summary>
        /// 運動軸方向
        /// </summary>
        public AxisDirection AxisDir { get; set; }

    }


}
