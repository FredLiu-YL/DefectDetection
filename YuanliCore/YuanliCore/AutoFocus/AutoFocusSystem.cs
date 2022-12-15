using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Subjects;
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
        private Subject<bool> afStates = new Subject<bool>();
        private int tempPattern, tempPositionZ , tempFSP, tempNSP;

        public AutoFocusSystem(string comPort, int baudrate)
        {
            serialPort = new SerialPort(comPort, baudrate, Parity.None, 8, StopBits.Two);
            serialPort.WriteTimeout = 2000;
            serialPort.ReadTimeout = 2000;
           // serialPort.ReadBufferSize = 40960;
           
        }

        public bool IsOpen { get => isRefresh; }
        public bool IsRunning { get; private set; }

        public IObservable<bool> AfStates => afStates;
        public double AxisZPosition { get => ReadZPosition(); }

        public double Pattern { get; private set; }
        public int FSP { get => tempFSP; set => WriteFSP(value); }
        public int NSP { get => tempNSP; set => WriteNSP(value); }

        public double SensorA { get; private set; }
        public double SensorB { get; private set; }
        public double AFSignalA { get; private set; }
        public double AFSignalB { get; private set; }
     
        public int BPF { get; private set; }
        public int Balance { get; private set; }
        public void Open()
        {
            isRefresh = true;
            serialPort.Open();
            Stop();

            task = Task.Run(Refresh);
        }
        public void Close()
        {
            isRefresh = false;
            task.Wait();
            serialPort.Close();
        }

        public void Run()
        {
            try
            {
                IsRunning = true;
                string response = SendMessage("SC0");

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public void Stop()
        {
            try
            {
                IsRunning = false;
                string response = SendMessage("Q");

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void MoveTo(double position)
        {

            string response = SendMessage($"AB:{position}");


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
                if(message!="")
                {
                    serialPort.DiscardOutBuffer();
                    Task.Delay(20).Wait();
                   // var strtest = serialPort.ReadExisting();

                    serialPort.Write(message + "\r\n");
                }
              
                Task.Delay(200).Wait();
                var str = serialPort.ReadExisting();
                return str;

            }

        }
        public void Move(double distance)
        {
            try
            {
                if (distance == 0)
                    return;
                string response = "";
                if (distance < 0)
                    response = SendMessage($"F:{-distance}");

                else if (distance > 0)
                    response = SendMessage($"N:{distance}");

                
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public void PatternMove(double distance)
        {
            try
            {
                if (distance == 0)
                    return;

                if (distance < 0)
                {
                    SendMessage($"SF:{-distance}");
                    Pattern += distance;

                }                   
                else if (distance > 0)
                {
                    SendMessage($"SN:{distance}");
                    Pattern += distance;
                }
                  
               
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        public async Task Home()
        {
            string response = SendMessage($"FL");
            response = SendMessage("RST");

           
        }
        private void WriteFSP(int value)
        {
            try
            {
                string response = SendMessage($"P:001,01A{value}B{value}C{value}D{value}E{value}F{value}");
                tempFSP = value;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private void WriteNSP(int value)
        {
            try
            {
                string response = SendMessage($"P:004,01A{value}B{value}C{value}D{value}E{value}F{value}");

                tempNSP = value;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        /// <summary>
        /// 搜尋範圍下限
        /// </summary>
        /// <returns></returns>
        //public int ReadZFSP()
        //{
        //    string[] strSplit;
        //    try
        //    {

        //        string response = SendMessage("ASPD");
        //        strSplit = response.Split(new char[] { ',', 'K', 'S', 'P', 'A', 'B', 'J', '\r', '\n' }); 
        //        string[] data = strSplit.Where(s => s.Length > 0).ToArray();
        //        if (data.Length < 3)
        //            return tempFSP;
        //        if (int.TryParse(data[0], out int output))//判斷能不能轉換
        //        {
        //            tempFSP = output;
        //            return output;
        //        }                                                       
        //        else
        //            return tempFSP;
               
        //    }
        //    catch (Exception ex)
        //    {

        //        throw ex;
        //    }
        //}
        /// <summary>
        /// 搜尋範圍上限
        /// </summary>
        /// <returns></returns>
        public void ReadNSPandFSP()
        {
            string[] strSplit;
            string[] data = new string[] { };
            try
            {
                do
                {
                    string response = SendMessage("ASPD");
                    strSplit = response.Split(new char[] { ',', 'K', 'S', 'P', 'A', 'B', 'J', '\r', '\n' });
                    data = strSplit.Where(s => s.Length > 0).ToArray();

                }
                while (data.Length <3);
                              
               
                if (int.TryParse(data[2], out int nsp))//判斷能不能轉換
                {
                    tempNSP = nsp;
              
                }
                if (int.TryParse(data[0], out int fsp))//判斷能不能轉換
                {
                    tempFSP = fsp;
                   
                }    

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }



        /// <summary>
        ///讀取 INT  AGC
        /// </summary>
        /// <returns></returns>
        private double ReadAT()
        {
            if (IsRunning) return 0;
            string response = SendMessage("AT");

            return Convert.ToDouble(response);
        }
        private double ReadZPosition()
        {
            string response = "";
          //  tempPositionZ
            try
            {

                response = SendMessage("DP");

                //   if (IsRunning)
                //   {
                var strSplit = response.Split(new char[] { ',', 'S', 'P', 'A', 'B', 'J', '\r', '\n' });
                var data = strSplit.Where(s => s.Length > 0).FirstOrDefault();
                if (double.TryParse(data, out double output))//判斷能不能轉換
                    return output;
                else
                    return 0;
                //  }
                //  if (double.TryParse(response, out double output))
                //      return output;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private void ReadPattern()
        {
            string response = "";
            try
            {
                if (IsRunning) return;
                response = SendMessage("SDP");
            
                if (double.TryParse(response, out double output))//判斷能不能轉換
                    Pattern = output;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private async Task ReadSensor()
        {
            if (IsRunning)
                return;
            string response = SendMessage("SIGD");
            var strSplit = response.Split(new char[] { ',', '\r' });
           
            if (double.TryParse(strSplit[2], out double output))//判斷能不能轉換
            {
                SensorA = Convert.ToDouble(strSplit[2].Insert(1, "."));
                SensorB = Convert.ToDouble(strSplit[3].Insert(1, "."));
                AFSignalA = Convert.ToDouble(strSplit[0].Insert(1, "."));
                AFSignalB = Convert.ToDouble(strSplit[1].Insert(1, "."));
            }
        
        }


        private async Task ReadBPF()
        {
            string response = SendMessage("VR2D");


            BPF = Convert.ToInt32(response);
        }
        private async Task ReadBalance()
        {
            string response = SendMessage("VR3D");


            Balance = Convert.ToInt32(response);
        }



        private async Task Refresh()
        {
            try
            {
                serialPort.DiscardInBuffer();
                serialPort.DiscardOutBuffer();
                await Task.Delay(300);
                //先更新1次 獲取數值  未來用相加減的方式節省詢問次數
                ReadPattern();
                ReadNSPandFSP();

                while (isRefresh)
                {
                    if (IsRunning)
                    {
                        // string str = serialPort.ReadExisting();
                        string str = SendMessage("");
                        string[] strSplit = str.Split(new char[] { ',', '\r', '\n' });
                        var datas = strSplit.Where(s => s.Length > 0);
                        int ct = datas.Count();
                        if (ct < 5) continue;
                        List<string> lastDatas = datas.ToList().GetRange(ct - 5, 5);
                        foreach (var data in lastDatas)
                        {
                            switch (data)
                            {
                                case "J":
                                    afStates.OnNext(true);
                                    break;
                                case "K":
                                    afStates.OnNext(false);
                                    break;
                                case "B":
                                    afStates.OnNext(false);
                                    break;

                            }


                        }

                    }
                    else
                    {
                       // await ReadPattern();
                        await ReadSensor();
                    }

                    await Task.Delay(200);


                }
            }
            catch (Exception ex)
            {


            }


        }

    }


}
