using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClosedXML.Excel;

namespace YuanliApplication.Tool
{
    public class OutputExcel
    {
        private IXLWorksheet sheet;
        private XLWorkbook wb;
        public OutputExcel()
        {
            wb = new XLWorkbook();
      
            sheet = wb.Worksheets.Add("工作表");
         
            //  sheet.Protect("LetMeEdit");

        }
        public void CreateTitle(IEnumerable<string> titles)
        {
            //for (int i = 1; i <= titles.Count(); i++) {
            //    sheet.Cells($"A1").Value
            //}
            var arr = titles.ToArray();
            for (int i = 1; i <= arr.Length; i++) {
                char c = (char)(i + 64);  // 將數字轉換為字元，A的ASCII碼是65
                string data = string.Format("{0}{1}", c, 1); // 格式化字串，例如A1、B1、C1等等
                sheet.Cells(data).Value = arr[i - 1];
            }
        }

        public void WriteData(int colums, string data1, string data2, string data3, string data4, string data5)
        {

            sheet.Cells($"A{colums+1}").Value = data1;
            sheet.Cells($"B{colums+1}").Value = data2;
            sheet.Cells($"C{colums+1}").Value = data3;
            if (data4 != "") {
                sheet.Cells($"D{colums + 1}").Value = data4;
                if(data4== "False")
                    sheet.Cells($"D{colums + 1}").Style.Fill.BackgroundColor = XLColor.Red;
           
            }
              
            if (data5 != "")
                sheet.Cells($"E{colums+1}").Value = data5;
        }


        public void Save(string path)
        {

            wb.SaveAs(path);
           

        }
        public void Dispose()
        {

            wb.Dispose();
        }
    }


}
