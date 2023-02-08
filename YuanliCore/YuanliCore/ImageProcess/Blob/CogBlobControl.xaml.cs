using Cognex.VisionPro;
using Cognex.VisionPro.Blob;
using Cognex.VisionPro.PMAlign;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace YuanliCore.ImageProcess.Blob
{
    /// <summary>
    /// PatMaxControl.xaml 的互動邏輯
    /// </summary>
    public  partial class CogBlobControl : UserControl
    {
        private static readonly DependencyProperty ImageProperty = DependencyProperty.Register(nameof(Image), typeof(ICogImage), typeof(CogBlobControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnImageChanged)));
       private static readonly DependencyProperty BlobParamProperty = DependencyProperty.Register(nameof(BlobParam), typeof(BlobParams), typeof(CogBlobControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnPatmaxParamChanged)));

        private CogBlobTool tool;
        private CogBlobEditV2 editor;

        public CogBlobControl()
        {
            InitializeComponent();

            tool = new CogBlobTool();
            editor = new CogBlobEditV2();

        //   PatmaxParam = PatmaxParams.Default(tool, 0);

            tool.Changed += PatMaxTool_Changed;

            ((System.ComponentModel.ISupportInitialize)(editor)).BeginInit();

            wfcontrol.SuspendLayout();

            editor.AllowDrop = true;
            editor.Dock = System.Windows.Forms.DockStyle.Fill;
            editor.Name = "PMAlignEditV2";
            editor.SuspendElectricRuns = false;
            editor.TabIndex = 0;

            wfcontrol.Controls.Add(editor);
            wfcontrol.ResumeLayout();
            wfcontrol.PerformLayout();

            ((System.ComponentModel.ISupportInitialize)(editor)).EndInit();

            editor.Subject = tool;
        }

        private void SearchRegion_Changed(object sender, CogChangedEventArgs e)
        {
            var flagName = e.GetStateFlagNames(sender);

    //        ParameterChangedEvent?.Invoke(this, new PatmaxParamsEventArgs(PatmaxParam));
            Trace.WriteLine($"SearchRegion_Changed => {flagName}");
        }

     //   public event EventHandler<PatmaxParamsEventArgs> PatternTrainedEvent;

    //    public event EventHandler<PatmaxParamsEventArgs> ParameterChangedEvent;

        public ICogImage Image
        {
            get => (ICogImage)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public BlobParams BlobParam
        {
            get => (BlobParams)GetValue(BlobParamProperty);
            set => SetValue(BlobParamProperty, value);
        }
  
      private static void OnImageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as CogBlobControl;
            dp.SetImage();
        }

        private static void OnPatmaxParamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dp = d as CogBlobControl;
            dp.RefreshPatmaxParam();
        }

        private void SetImage()
        {
            tool.InputImage = (CogImage8Grey)Image;
        }

        private void RefreshPatmaxParam()
        {
            if (tool == null) { /* 先測試是否有可能會發生*/ throw new Exception("123"); }

            // 移除前次參數事件

            tool.RunParams.Changed -= RunParams_Changed;
            if (tool.Region != null) tool.Region.Changed -= SearchRegion_Changed;

            // 更新 tool 內的 Pax 參數
    
            tool.RunParams = BlobParam.RunParams;
            tool.Region = BlobParam.ROI;

            // 將新參數委派事件           
            if (tool.RunParams != null) tool.RunParams.Changed += RunParams_Changed;
           // if (tool.Pattern != null) tool.Pattern.Changed += Pattern_Changed;
            if (tool.Region != null) tool.Region.Changed += SearchRegion_Changed;
        }

        #region internal event
        private void Pattern_Changed(object sender, CogChangedEventArgs e)
        {
            var flagName = e.GetStateFlagNames(sender);

        //    if (flagName.Contains("SfTrained"))
        //        PatternTrainedEvent?.Invoke(this, new PatmaxParamsEventArgs(PatmaxParam));

            Trace.WriteLine($"Pattern_Changed => {flagName}");
        }

        private void RunParams_Changed(object sender, CogChangedEventArgs e)
        {
            var flagName = e.GetStateFlagNames(sender);

      //      ParameterChangedEvent?.Invoke(this, new PatmaxParamsEventArgs(PatmaxParam));
            Trace.WriteLine($"RunParams_Changed => {flagName}");
        }

        private void PatMaxTool_Changed(object sender, CogChangedEventArgs e)
        {
            var flagName = e.GetStateFlagNames(sender);
            Trace.WriteLine($"PatMaxTool_Changed => {flagName}");
        }

        public void SetToParam()
        {
    //        PatmaxParam.Pattern = tool.Pattern;
    //        PatmaxParam.SearchRegion = tool.SearchRegion;
     //       PatmaxParam.RunParams = tool.RunParams;
        }
        #endregion

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            bool isVisible = (bool)e.NewValue;

            if (!isVisible) SetToParam();
        }

       
    }

}
