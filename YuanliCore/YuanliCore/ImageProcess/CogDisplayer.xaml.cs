using Cognex.VisionPro;
using Cognex.VisionPro.Display;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoFocusMachine
{
  
    /// <summary>
    /// CogDisplayer.xaml 的互動邏輯
    /// </summary>
    public partial class CogDisplayer : UserControl
    {
        private CogRecordDisplay cogDisplay;
        private static readonly DependencyProperty RecordProperty = DependencyProperty.Register(nameof(Record), typeof(ICogRecord), typeof(CogDisplayer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnImageChanged)));
        private static readonly DependencyProperty WidthProperty = DependencyProperty.Register(nameof(Width), typeof(int), typeof(CogDisplayer), new FrameworkPropertyMetadata(800, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnImageSizeChanged) ));
        private static readonly DependencyProperty HeightProperty = DependencyProperty.Register(nameof(Height), typeof(int), typeof(CogDisplayer), new FrameworkPropertyMetadata(600, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnImageSizeChanged)));

        public CogDisplayer()
        {
            InitializeComponent();

            cogDisplay = new CogRecordDisplay();
            cogDisplay.Size = new System.Drawing.Size(Width, Height);


            ((System.ComponentModel.ISupportInitialize)(cogDisplay)).BeginInit();

            wfcontrol.SuspendLayout();

            //editor.AllowDrop = true;
            //editor.Dock = System.Windows.Forms.DockStyle.Fill;
            //editor.Name = "PMAlignEditV2";
            //editor.SuspendElectricRuns = false;
            //editor.TabIndex = 0;

            wfcontrol.Controls.Add(cogDisplay);
            wfcontrol.ResumeLayout();
            wfcontrol.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(cogDisplay)).EndInit();

      
        }

        public ICogRecord Record
        {
            get => (ICogRecord)GetValue(RecordProperty);
            set => SetValue(RecordProperty, value);
        }

        public int Width
        {
            get => (int)GetValue(WidthProperty);
            set => SetValue(WidthProperty, value);
        }
        public int Height
        {
            get => (int)GetValue(HeightProperty);
            set => SetValue(HeightProperty, value);
        }
        private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as CogDisplayer;
            dp.SetImage();


        }
        private static void OnImageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as CogDisplayer;
            dp.SetSize();


        }
        private void SetImage()
        {
            cogDisplay.Record = Record;
        }
        private void SetSize()
        {
            cogDisplay.Size = new System.Drawing.Size(Width, Height);
        }

    }
}
