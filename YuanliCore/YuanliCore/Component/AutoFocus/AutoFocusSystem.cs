using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using YuanliCore.Interface;
//using YuanliCore.Motion.Marzhauser;

namespace YuanliCore
{ 
    public class AutoFocusSystem
    {
        private SerialPort serialPort;
        private readonly object lockObj = new object();
        private Task task = Task.CompletedTask;
        private bool isRefresh = false;
        private Subject<bool> afStates = new Subject<bool>();
        private int tempPattern, tempPositionZ, tempFSP, tempNSP;

        public AutoFocusSystem(string comPort, int baudrate =19200)
        {
            serialPort = new SerialPort(comPort, baudrate, Parity.None, 8, StopBits.Two);
            serialPort.WriteTimeout = 2000;
            serialPort.ReadTimeout = 2000;
            // serialPort.ReadBufferSize = 40960;

        }

        public bool IsOpen { get => isRefresh; }
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 對焦Z軸位置
        /// </summary>
        public double AxisZPosition { get => ReadZPosition(); }
        /// <summary>
        /// 對焦Z軸搜尋範圍 負極限
        /// </summary>
        public int FSP { get => ReadFSP(); set => WriteFSP(value); }
        /// <summary>
        ///  對焦Z軸搜尋範圍  正極限
        /// </summary>
        public int NSP { get => ReadNSP(); set => WriteNSP(value); }

        /// <summary>
        ///  調整 AF訊號 的能量數值
        /// </summary>
        public double Pattern { get; private set; }
        public AFSSignal Signals { get =>  ReadSensor(); }

        /// <summary>
        /// AF訊號 增益值
        /// </summary>
        public int BPF { get; private set; }
        /// <summary>
        /// AF訊號 單邊增益值(可能只調整AFSignalA)
        /// </summary>
        public int Balance { get; private set; }

        /// <summary>
        /// 判斷對焦結果 是否正確的依據(0-7) 
        /// INT 與AGC需以8進制合起來看    (1,0) 附近為適當  所以可行範圍應該為 (0,5) ~(1,2)
        /// </summary>
        public int INT { get; }
        /// <summary>
        ///  判斷對焦結果 是否正確的依據(0-7) 
        /// </summary>
        public int AGC { get; }

        public event Action<bool> JustFocus;

        public void Open()
        {
            isRefresh = true;
            serialPort.Open();
            Stop();
            Pattern = ReadPattern();
            //task = Task.Run(Refresh);
        }
        public void Close()
        {
            isRefresh = false;
            task.Wait();
            serialPort.Close();
        }
        /// <summary>
        ///  開始自動對焦
        /// </summary>
        public void Run()
        {
            try
            {
                if (!IsOpen) throw new Exception("System is not Open");
                IsRunning = true;
                string response = SendMessage("SC0");
                Task.Run(FocusState);
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
                if (message != "")
                {
                    serialPort.DiscardOutBuffer();
                    Task.Delay(30).Wait();
                    // var strtest = serialPort.ReadExisting();

                    serialPort.Write(message + "\r\n");
                }

                Task.Delay(100).Wait();
                var str = serialPort.ReadExisting();
                return str;

            }

        }
        public  Task Move(double distance)
        {
            try
            {
                return  Task.Run(()=> {
                    if (distance == 0)
                        return;
                    string response = "";
                    if (distance < 0)
                        response = SendMessage($"F:{-distance}");

                    else if (distance > 0)
                        response = SendMessage($"N:{distance}");
                });
         


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
        /// 搜尋範圍
        /// </summary>
        /// <returns></returns>
        public int ReadFSP()
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
                while (data.Length < 3);


                //    if (int.TryParse(data[2], out int nsp))//判斷能不能轉換
                //        tempNSP = nsp;

                if (int.TryParse(data[0], out int fsp))//判斷能不能轉換
                    return fsp;
                else
                    throw new Exception("FSP get fail");

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        public int ReadNSP()
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
                while (data.Length < 3);


                if (int.TryParse(data[2], out int nsp))//判斷能不能轉換
                {
                    return nsp;

                }
                else
                    throw new Exception("NSP get fail");
                //   if (int.TryParse(data[0], out int fsp))//判斷能不能轉換
                //        tempFSP = fsp;


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
        private int ReadPattern()
        {
            string response = "";
            try
            {
                if (IsRunning) return 0;
                response = SendMessage("SDP");

                if (int.TryParse(response, out int output))//判斷能不能轉換
                    return output;
                else
                    throw new Exception("Get Pattern is fail");
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private AFSSignal ReadSensor()
        {
            string response = SendMessage("SIGD");
            var strSplit = response.Split(new char[] { ',', '\r' });
            try
            {
                if (double.TryParse(strSplit[2], out double output))//判斷能不能轉換
                {
                    var sensorA = Convert.ToDouble(strSplit[2].Insert(1, "."));
                    var sensorB = Convert.ToDouble(strSplit[3].Insert(1, "."));
                    var aFSignalA = Convert.ToDouble(strSplit[0].Insert(1, "."));
                    var aFSignalB = Convert.ToDouble(strSplit[1].Insert(1, "."));

                    return new AFSSignal(sensorA, sensorB, aFSignalA, aFSignalB);
                }
                return new AFSSignal();
            }
            catch (Exception ex)
            {

                throw ex;
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
             //   ReadNSPandFSP();

                while (isRefresh)
                {
                    if (IsRunning)
                    {
                        // string str = serialPort.ReadExisting();
                        /*   string str = SendMessage("");
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
                        */
                    }
                    else
                    {
                        // await ReadPattern();
                    //    await ReadSensor();
                    }

                    await Task.Delay(200);


                }
            }
            catch (Exception ex)
            {


            }


        }

        private async Task FocusState()
        {
            try
            {

                while (IsRunning)
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
                    await Task.Delay(200);
                }

            }
            catch (Exception ex)
            {


            }
         
            
        }

    }


    public struct AFSSignal
    {

        public AFSSignal(double sensorA , double sensorB , double afSignalA, double afSignalB)
        {
            SensorA = sensorA;
            SensorB = sensorB;
            AFSignalA = afSignalA;
            AFSignalB = afSignalB;
        }

        /// <summary>
        /// IR訊號能量值
        /// </summary>
        public double SensorA { get;set; }
        /// <summary>
        ///  IR訊號能量值
        /// </summary>
        public double SensorB { get; set; }
        public double AFSignalA { get; set; }
        public double AFSignalB { get; set; }
    }

}
