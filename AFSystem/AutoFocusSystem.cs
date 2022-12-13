using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore
{
    public class AutoFocusSystem
    {
        private SerialPort serialPort;
        private readonly object lockObj = new object();
        private Task task = Task.CompletedTask;
        private bool isRefresh = false;

        public AutoFocusSystem(string comPort, int baudrate)
        {
            serialPort = new SerialPort(comPort, baudrate, Parity.None, 8, StopBits.Two);
            serialPort.WriteTimeout = 2000;
            serialPort.ReadTimeout = 2000;
        
        }


        public double AxisZPosition { get => ReadZPosition(); }
        public double Pattern { get => ReadPattern(); }
        public double SensorA { get; private set; }
        public double SensorB { get; private set; }
        public double AFSignalA { get; private set; }
        public double AFSignalB { get; private set; }

        public double BPF { get => ReadBPF(); }
        public double Balance { get => ReadBalance(); }
        public void Open()
        {
            isRefresh = true;               
            serialPort.Open();
            task = Task.Run(Refresh);
        }
        public void Close()
        {
            isRefresh = false;
            task.Wait();
            serialPort.Close();
        }
        /// <summary>
        /// 送訊息給裝置
        /// </summary>
        /// <param name="message"></param>
        /// <returns>回傳字串</returns>
        public string SendMessage(string message)
        {
            lock (lockObj)
            {
                serialPort.Write(message + "\r\n");
                Task.Delay(100).Wait();
                return serialPort.ReadExisting();

            }

        }
        public void MoveTo(double position)
        {

            string response = SendMessage($"AB:{position}");
           
            if (response != "K")
                throw new Exception("moving fail");

        }

        public void Move(double distance)
        {
            if (distance == 0)
                return;
            string response = "";
            if (distance < 0)
                response = SendMessage($"F:{-distance}");
      
            else if (distance > 0)
                response = SendMessage($"N:{distance}");

            if (response != "G")
                throw new Exception("moving fail");
        }
        public void PatternMove(double distance)
        {
            if (distance == 0)
                return;

            if (distance < 0)
              
                SendMessage($"SF:{-distance}");
            else if (distance > 0)
                SendMessage($"SN:{distance}");
          
            string response = serialPort.ReadExisting();
            if (response != "G")
                throw new Exception("moving fail");
        }
        public void Stop()
        {
            string response = SendMessage("Q");
            // serialPort.Write("Q" + "\r\n");
            //  string response = serialPort.ReadExisting();
            if (response != "K")
                throw new Exception("moving fail");

        }
        public double Home()
        {
            string response = SendMessage($"FL");
            response = SendMessage("RST");
            //   serialPort.Write("FL" + "\r\n");
            //   serialPort.Write("RST" + "\r\n");

            //    response = serialPort.ReadExisting();
            return 0;
        }
        private double ReadZLimitN()
        {
            string response = SendMessage("ASPD");
            //  serialPort.Write("ASPD" + "\r\n");
            //    string response = serialPort.ReadExisting();
            return 0;
        }
        private double ReadZLimitP()
        {
            string response = SendMessage("ASPD");
            return Convert.ToDouble(response);
        }

        /// <summary>
        ///讀取 INT  AGC
        /// </summary>
        /// <returns></returns>
        private double ReadAT()
        {
            string response = SendMessage("AT");
            //  serialPort.Write("AT" + "\r\n");
            // string response = serialPort.ReadExisting();
            return Convert.ToDouble(response);
        }
        private double ReadZPosition()
        {
            string response = SendMessage("DP");
            //     serialPort.Write("DP" + "\r\n");
            //    Task.Delay(100).Wait();
            //    string response = serialPort.ReadExisting();

            return Convert.ToDouble(response);
        }
        private double ReadPattern()
        {
            string response = SendMessage("SDP");
            //  serialPort.Write("SDP" + "\r\n");
            //    Task.Delay(100).Wait();
            //   string response = serialPort.ReadExisting();

            return Convert.ToDouble(response);
        }
        private async Task ReadSensor()
        {
            string response = SendMessage("SIGD");
            var strSplit = response.Split(new char[] { ',', '\r' });

            SensorA = Convert.ToDouble(strSplit[2].Insert(1, "."));
            SensorB = Convert.ToDouble(strSplit[3].Insert(1, "."));
            AFSignalA = Convert.ToDouble(strSplit[0].Insert(1, "."));
            AFSignalB = Convert.ToDouble(strSplit[1].Insert(1, "."));
        }


        private double ReadBPF()
        {
            string response = SendMessage("VR2D");
            //   serialPort.Write("VR2D" + "\r\n");
            //   Task.Delay(100).Wait();
            //   string response = serialPort.ReadExisting();

            return Convert.ToDouble(response);
        }
        private double ReadBalance()
        {
            string response = SendMessage("VR3D");
            //   serialPort.Write("VR3D" + "\r\n");
            //  Task.Delay(100).Wait();
            //  string response = serialPort.ReadExisting();

            return Convert.ToDouble(response);
        }



        private async Task Refresh()
        {


            while (isRefresh)
            {
                await ReadSensor();
                await Task.Delay(100);
            }

        }

    }


}
