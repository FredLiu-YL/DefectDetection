
namespace WindowsFormsApp1
{
    partial class Form1
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.button_open = new System.Windows.Forms.Button();
            this.textBox_comPort = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button_close = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_AF_A = new System.Windows.Forms.TextBox();
            this.textBox_AF_B = new System.Windows.Forms.TextBox();
            this.label_IR_B = new System.Windows.Forms.Label();
            this.label_IR_A = new System.Windows.Forms.Label();
            this.textBox_IR_B = new System.Windows.Forms.TextBox();
            this.textBox_IR_A = new System.Windows.Forms.TextBox();
            this.textBox_PosZ = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_Pattern = new System.Windows.Forms.TextBox();
            this.btn_MinusZ = new System.Windows.Forms.Button();
            this.btn_PulsZ = new System.Windows.Forms.Button();
            this.btn_PulsPattern = new System.Windows.Forms.Button();
            this.btn_MinusPattern = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lbl_PLibit_PoZ = new System.Windows.Forms.Label();
            this.lbl_NLibit_PoZ = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_open
            // 
            this.button_open.Location = new System.Drawing.Point(276, 103);
            this.button_open.Name = "button_open";
            this.button_open.Size = new System.Drawing.Size(75, 23);
            this.button_open.TabIndex = 0;
            this.button_open.Text = "OPEN";
            this.button_open.UseVisualStyleBackColor = true;
            this.button_open.Click += new System.EventHandler(this.button_open_Click_1);
            // 
            // textBox_comPort
            // 
            this.textBox_comPort.Location = new System.Drawing.Point(251, 59);
            this.textBox_comPort.Name = "textBox_comPort";
            this.textBox_comPort.Size = new System.Drawing.Size(100, 22);
            this.textBox_comPort.TabIndex = 1;
            this.textBox_comPort.Text = "COM1";
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(419, 59);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(100, 22);
            this.textBox2.TabIndex = 2;
            // 
            // button_close
            // 
            this.button_close.Location = new System.Drawing.Point(410, 103);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(75, 23);
            this.button_close.TabIndex = 3;
            this.button_close.Text = "CLOSE";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.button_close_Click_1);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(275, 168);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 4;
            this.button1.Text = "SET";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(373, 170);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(100, 22);
            this.textBox3.TabIndex = 5;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label1.Location = new System.Drawing.Point(37, 352);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "AF signalA";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label2.Location = new System.Drawing.Point(140, 352);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "AF signaB";
            // 
            // textBox_AF_A
            // 
            this.textBox_AF_A.Location = new System.Drawing.Point(40, 384);
            this.textBox_AF_A.Name = "textBox_AF_A";
            this.textBox_AF_A.Size = new System.Drawing.Size(76, 22);
            this.textBox_AF_A.TabIndex = 8;
            // 
            // textBox_AF_B
            // 
            this.textBox_AF_B.Location = new System.Drawing.Point(143, 384);
            this.textBox_AF_B.Name = "textBox_AF_B";
            this.textBox_AF_B.Size = new System.Drawing.Size(76, 22);
            this.textBox_AF_B.TabIndex = 9;
            // 
            // label_IR_B
            // 
            this.label_IR_B.AutoSize = true;
            this.label_IR_B.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_IR_B.Location = new System.Drawing.Point(140, 253);
            this.label_IR_B.Name = "label_IR_B";
            this.label_IR_B.Size = new System.Drawing.Size(79, 16);
            this.label_IR_B.TabIndex = 11;
            this.label_IR_B.Text = "IR SensorB";
            // 
            // label_IR_A
            // 
            this.label_IR_A.AutoSize = true;
            this.label_IR_A.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label_IR_A.Location = new System.Drawing.Point(37, 253);
            this.label_IR_A.Name = "label_IR_A";
            this.label_IR_A.Size = new System.Drawing.Size(80, 16);
            this.label_IR_A.TabIndex = 10;
            this.label_IR_A.Text = "IR SensorA";
            // 
            // textBox_IR_B
            // 
            this.textBox_IR_B.Location = new System.Drawing.Point(143, 283);
            this.textBox_IR_B.Name = "textBox_IR_B";
            this.textBox_IR_B.Size = new System.Drawing.Size(76, 22);
            this.textBox_IR_B.TabIndex = 13;
            // 
            // textBox_IR_A
            // 
            this.textBox_IR_A.Location = new System.Drawing.Point(40, 283);
            this.textBox_IR_A.Name = "textBox_IR_A";
            this.textBox_IR_A.Size = new System.Drawing.Size(76, 22);
            this.textBox_IR_A.TabIndex = 12;
            // 
            // textBox_PosZ
            // 
            this.textBox_PosZ.Location = new System.Drawing.Point(350, 253);
            this.textBox_PosZ.Name = "textBox_PosZ";
            this.textBox_PosZ.Size = new System.Drawing.Size(100, 22);
            this.textBox_PosZ.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label3.Location = new System.Drawing.Point(273, 253);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 16);
            this.label3.TabIndex = 15;
            this.label3.Text = "PositionZ:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label4.Location = new System.Drawing.Point(504, 253);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 16);
            this.label4.TabIndex = 17;
            this.label4.Text = "Pattern:";
            // 
            // textBox_Pattern
            // 
            this.textBox_Pattern.Location = new System.Drawing.Point(581, 253);
            this.textBox_Pattern.Name = "textBox_Pattern";
            this.textBox_Pattern.Size = new System.Drawing.Size(100, 22);
            this.textBox_Pattern.TabIndex = 16;
            // 
            // btn_MinusZ
            // 
            this.btn_MinusZ.Location = new System.Drawing.Point(276, 345);
            this.btn_MinusZ.Name = "btn_MinusZ";
            this.btn_MinusZ.Size = new System.Drawing.Size(75, 23);
            this.btn_MinusZ.TabIndex = 18;
            this.btn_MinusZ.Text = "-";
            this.btn_MinusZ.UseVisualStyleBackColor = true;
            this.btn_MinusZ.Click += new System.EventHandler(this.btn_MinusZ_Click);
            // 
            // btn_PulsZ
            // 
            this.btn_PulsZ.Location = new System.Drawing.Point(388, 345);
            this.btn_PulsZ.Name = "btn_PulsZ";
            this.btn_PulsZ.Size = new System.Drawing.Size(75, 23);
            this.btn_PulsZ.TabIndex = 19;
            this.btn_PulsZ.Text = "+";
            this.btn_PulsZ.UseVisualStyleBackColor = true;
            this.btn_PulsZ.Click += new System.EventHandler(this.btn_PulsZ_Click);
            // 
            // btn_PulsPattern
            // 
            this.btn_PulsPattern.Location = new System.Drawing.Point(606, 320);
            this.btn_PulsPattern.Name = "btn_PulsPattern";
            this.btn_PulsPattern.Size = new System.Drawing.Size(75, 23);
            this.btn_PulsPattern.TabIndex = 21;
            this.btn_PulsPattern.Text = "+";
            this.btn_PulsPattern.UseVisualStyleBackColor = true;
            this.btn_PulsPattern.Click += new System.EventHandler(this.btn_PulsPattern_Click);
            // 
            // btn_MinusPattern
            // 
            this.btn_MinusPattern.Location = new System.Drawing.Point(501, 320);
            this.btn_MinusPattern.Name = "btn_MinusPattern";
            this.btn_MinusPattern.Size = new System.Drawing.Size(75, 23);
            this.btn_MinusPattern.TabIndex = 20;
            this.btn_MinusPattern.Text = "-";
            this.btn_MinusPattern.UseVisualStyleBackColor = true;
            this.btn_MinusPattern.Click += new System.EventHandler(this.btn_MinusPattern_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label5.Location = new System.Drawing.Point(273, 289);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 16);
            this.label5.TabIndex = 22;
            this.label5.Text = "NLibit_PoZ:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.label6.Location = new System.Drawing.Point(385, 289);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(82, 16);
            this.label6.TabIndex = 23;
            this.label6.Text = "PLibit_PoZ:";
            // 
            // lbl_PLibit_PoZ
            // 
            this.lbl_PLibit_PoZ.AutoSize = true;
            this.lbl_PLibit_PoZ.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_PLibit_PoZ.Location = new System.Drawing.Point(385, 320);
            this.lbl_PLibit_PoZ.Name = "lbl_PLibit_PoZ";
            this.lbl_PLibit_PoZ.Size = new System.Drawing.Size(32, 16);
            this.lbl_PLibit_PoZ.TabIndex = 25;
            this.lbl_PLibit_PoZ.Text = "222";
            // 
            // lbl_NLibit_PoZ
            // 
            this.lbl_NLibit_PoZ.AutoSize = true;
            this.lbl_NLibit_PoZ.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lbl_NLibit_PoZ.Location = new System.Drawing.Point(273, 320);
            this.lbl_NLibit_PoZ.Name = "lbl_NLibit_PoZ";
            this.lbl_NLibit_PoZ.Size = new System.Drawing.Size(32, 16);
            this.lbl_NLibit_PoZ.TabIndex = 24;
            this.lbl_NLibit_PoZ.Text = "222";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lbl_PLibit_PoZ);
            this.Controls.Add(this.lbl_NLibit_PoZ);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btn_PulsPattern);
            this.Controls.Add(this.btn_MinusPattern);
            this.Controls.Add(this.btn_PulsZ);
            this.Controls.Add(this.btn_MinusZ);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_Pattern);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_PosZ);
            this.Controls.Add(this.textBox_IR_B);
            this.Controls.Add(this.textBox_IR_A);
            this.Controls.Add(this.label_IR_B);
            this.Controls.Add(this.label_IR_A);
            this.Controls.Add(this.textBox_AF_B);
            this.Controls.Add(this.textBox_AF_A);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.textBox_comPort);
            this.Controls.Add(this.button_open);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_open;
        private System.Windows.Forms.TextBox textBox_comPort;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Button button_close;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_AF_A;
        private System.Windows.Forms.TextBox textBox_AF_B;
        private System.Windows.Forms.Label label_IR_B;
        private System.Windows.Forms.Label label_IR_A;
        private System.Windows.Forms.TextBox textBox_IR_B;
        private System.Windows.Forms.TextBox textBox_IR_A;
        private System.Windows.Forms.TextBox textBox_PosZ;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_Pattern;
        private System.Windows.Forms.Button btn_MinusZ;
        private System.Windows.Forms.Button btn_PulsZ;
        private System.Windows.Forms.Button btn_PulsPattern;
        private System.Windows.Forms.Button btn_MinusPattern;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lbl_PLibit_PoZ;
        private System.Windows.Forms.Label lbl_NLibit_PoZ;
    }
}

