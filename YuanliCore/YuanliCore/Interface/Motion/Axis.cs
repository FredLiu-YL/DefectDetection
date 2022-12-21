using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.Interface
{
    public class Axis
    {
        private IMotionController controller;
        private bool isBusy;

        public Axis(IMotionController motioncontroller, int axisNum)
        {
            controller = motioncontroller;
            this.AxisID = axisNum;
        }

        /// <summary>
        /// 運動軸編號
        /// </summary>
        public int AxisID { get; }
        /// <summary>
        /// 運動軸名稱
        /// </summary>
        public string AxisName { get; }
        /// <summary>
        /// 當前位置
        /// </summary>
        public double Position { get => GetPositon(); }

        /// <summary>
        /// 軟體正極限
        /// </summary>
        public double LimitN { get => GetLimitN(); set=> SetLimitN(value); }
        /// <summary>
        /// 軟體負極限
        /// </summary>
        public double LimitP { get => GetLimitP(); set => SetLimitP(value); }
        /// <summary>
        /// 運動軸方向
        /// </summary>
        public AxisDirection AxisDir { get; set; }

        public async Task HomeAsync()
        {
            if (isBusy) return;
            isBusy = true;
            await Task.Run(() => { controller.HomeCommand(AxisID); });
            isBusy = false;
        }


        public async Task MoveAsync(double distance)
        {
            if (isBusy) return;
            isBusy = true;
            await Task.Run(() =>
            {
                controller.MoveCommand(AxisID , distance);
            });
            isBusy = false;
        }

        public async Task MoveToAsync(double postion)
        {
            if (isBusy) return;
            await Task.Run(() =>
            {
                controller.MoveToCommand(AxisID ,postion);
            });
            isBusy = false;
        }

        private double GetPositon()
        {
            return controller.GetPositionCommand(AxisID);

        }

        private void SetDirection(AxisDirection direction)
        {
            controller.SetAxisDirectionCommand(AxisID , direction);
        }
        private double GetLimitP()
        {
            controller.GetLimitCommand(AxisID,out double limitN,  out double limitP);
            return limitP;
        }
        private double GetLimitN()
        {
            controller.GetLimitCommand(AxisID, out double limitN, out double limitP);
            return limitN;
        }


        private void SetLimitN(double limit)
        {
            controller.SetLimitCommand(AxisID, limit, LimitP);
        }
        private void SetLimitP(double limit)
        {
            controller.SetLimitCommand(AxisID, LimitN, limit);
        }

    }

  
    public enum AxisDirection
    {

        Forward,
        Backward
    }
}
