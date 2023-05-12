using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using YuanliCore.CameraLib;

namespace YuanliCore.ImageProcess
{
    public class CogDisplayText
    {


    }

    public static class CogExten
    {
        private static CogRecordsDisplay cogDisplayers = new CogRecordsDisplay();

     //   private static CogRecordDisplay cogDisplay = new CogRecordDisplay();

        private static CogGraphicLabel cogGraphicLabel = new CogGraphicLabel();

        public static BitmapSource CogRecordAddText(this ICogRecord cogRecord, IEnumerable<DisplayLable> displayLable, int imageWidth, int imageHeight)
        {
            int id = Thread.CurrentThread.ManagedThreadId;

            //      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //      { }
            cogGraphicLabel.Color = CogColorConstants.Red;
            cogGraphicLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18);

            cogDisplayers.Size = new System.Drawing.Size(imageWidth, imageHeight);
            cogDisplayers.Subject = cogRecord;

            foreach (var lable in displayLable) {
                cogGraphicLabel.SetXYText(lable.Pos.X, lable.Pos.Y, lable.Text);

                cogDisplayers.Display.StaticGraphics.Add(cogGraphicLabel, "");
            }
            var bmp = cogDisplayers.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);

            var bmps = bmp.ToBitmapSource();
            return bmps;

        }
        public static BitmapSource CreateBmp(this ICogRecord cogRecord, int width, int height)
        {
            int id = Thread.CurrentThread.ManagedThreadId;
            BitmapSource bmps = null;
            //      Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            //      {


            cogDisplayers.Size = new System.Drawing.Size(width, height);
            cogDisplayers.Subject = cogRecord;

            var bmp = cogDisplayers.Display.CreateContentBitmap(CogDisplayContentBitmapConstants.Display);
            //    cogDisplayers.Display.InteractiveGraphics.Add();
            bmps = bmp.ToBitmapSource();
            // bmp.ToBitmapSource().Save("D:\\qaswed.bmp");

            // CogRecordDisplay cogDisplayer = new CogRecordDisplay();
            // cogDisplayer.Record = cogRecord;
            //        }));

            return bmps;

        }



        public static void AddGraphicLabel(this CogRecordDisplay cogRecordDisplay, DisplayLable displayLable)
        {
            cogGraphicLabel.Color = CogColorConstants.Red;
            cogGraphicLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 18);

            cogGraphicLabel.SetXYText(displayLable.Pos.X, displayLable.Pos.Y, displayLable.Text);

            cogRecordDisplay.StaticGraphics.Add(cogGraphicLabel, "");


        }
    }
}
