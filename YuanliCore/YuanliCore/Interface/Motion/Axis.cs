using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface.Motion;

namespace YuanliCore.Interface
{
    public class Axis
    {
        private IMotionController controller;
        private bool isBusy;
        private bool isStop;
        private readonly object lockObj = new object();

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
        public double LimitN { get => GetLimitN(); set => SetLimitN(value); }
        /// <summary>
        /// 軟體負極限
        /// </summary>
        public double LimitP { get => GetLimitP(); set => SetLimitP(value); }
        /// <summary>
        /// 運動軸方向
        /// </summary>
        public AxisDirection AxisDir { get => GetDirection(); set => SetDirection(value); }

        /// <summary>
        /// 運動軸速度
        /// </summary>
        public MotionVelocity AxisVelocity { get => GetVelocity(); set => SetVelocity(value); }



        public async Task HomeAsync()
        {
            if (isBusy) return;
            isBusy = true;
            await Task.Run(() => { controller.HomeCommand(AxisID); });
            isBusy = false;
        }
        public async Task Stop()
        {
            controller.StopCommand(AxisID);
            isStop = true;
        }

        public async Task MoveAsync(double distance)
        {
            if (isBusy) throw new Exception($"ID{ AxisID}  {AxisName}  is Busy");
            try
            {
                int i = 0;
                isBusy = true;
                await Task.Run(async () =>
                {
                    controller.MoveCommand(AxisID, distance);
                    double postion = Position + distance;
                    while (Math.Abs(Position - postion) > 0.005 && !isStop)
                    {
                        i++;
                        await Task.Delay(50);
                        if (i >= 200) throw new Exception($"ID{ AxisID}  {AxisName}  Time out");

                    }

                });
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                isBusy = false;
                isStop = false;

            }

        }

        public async Task MoveToAsync(double postion)
        {

            if (isBusy) throw new Exception($"ID{ AxisID}  {AxisName}  is Busy");
            try
            {
                isBusy = true;
                int i = 0;
                await Task.Run(async () =>
                {

                    controller.MoveToCommand(AxisID, postion);
                    double nowPosition = Position;
                    while (Math.Abs(nowPosition - postion) > 0.005 && !isStop)
                    {
                        i++;
                        await Task.Delay(50);
                        if (i >= 200) throw new Exception($"ID{ AxisID}  {AxisName}  Time out ,Target:{postion} now:{nowPosition}");
                        nowPosition = Position;
                    }

                });
            }
            catch (Exception ex)
            {

                throw ex;
            }
            finally
            {
                isBusy = false;
                isStop = false;

            }


        }



        private double GetPositon()
        {
            return controller.GetPositionCommand(AxisID);

        }
        private AxisDirection GetDirection()
        {
            return controller.GetAxisDirectionCommand(AxisID);
        }
        private void SetDirection(AxisDirection direction)
        {
            controller.SetAxisDirectionCommand(AxisID, direction);
        }
        private double GetLimitP()
        {
            controller.GetLimitCommand(AxisID, out double limitN, out double limitP);
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
        private MotionVelocity GetVelocity()
        {
            return controller.GetSpeedCommand(AxisID);
        }
        private void SetVelocity(MotionVelocity axisVelocity)
        {

            double velocity = axisVelocity.FainalVelocity;
            double accVelocity = axisVelocity.AccVelocity;
            double decVelocity = axisVelocity.DecVelocity;
            controller.SetSpeedCommand(AxisID, axisVelocity);


        }

    }


    public enum AxisDirection
    {

        Forward,
        Backward
    }
}
