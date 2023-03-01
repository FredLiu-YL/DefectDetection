using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YuanliCore.ImageProcess
{
    public class CogParameter
    {

        /// <summary>
        /// 取得或設定Patmax 的Id 預設為 = 0, 若一個料號有兩個以上的Patmax參數屬性, 請明確指定Id後再儲存
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 輸出資料的型態 ，FULL :完整輸出 ， 中心點 ， 起點 ， 終點
        /// </summary>
        public ResultSelect ResultOutput { get; set; } = ResultSelect.Full;

    }


    public enum ResultSelect
    {
        Full,
        Center,
        Begin,
        End,
    }
}
