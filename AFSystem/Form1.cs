using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using YuanliCore;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private AutoFocusSystem focusSystem;
        private Task taskGetState = Task.CompletedTask;
        private bool isRefreshState;

        public Form1()
        {
            InitializeComponent();

        }



        private async void button1_Click(object sender, EventArgs e)
        {

            //var str = await focusSystem.SendMessage("VER");

            var str = focusSystem.AxisZPosition;


        }

        private void button_open_Click_1(object sender, EventArgs e)
        {
            string port = textBox_comPort.Text;
            if (focusSystem == null)
                focusSystem = new AutoFocusSystem(port, 19200);
            focusSystem.Open();
            // taskGetState = Task.Run(RefreshState);
            isRefreshState = true;
            RefreshState();
         


        }

        private void button_close_Click_1(object sender, EventArgs e)
        {
            focusSystem.Close();
            isRefreshState = false;
           // taskGetState.Wait();
        }



        private async Task RefreshState()
        {
          
        }

        private void btn_PulsZ_Click(object sender, EventArgs e)
        {
            focusSystem.Move(200);
        }

        private void btn_MinusZ_Click(object sender, EventArgs e)
        {
            focusSystem.Move(-200);
        }

        private void btn_PulsPattern_Click(object sender, EventArgs e)
        {
            focusSystem.PatternMove(100);
        }

        private void btn_MinusPattern_Click(object sender, EventArgs e)
        {
            focusSystem.PatternMove(-100);
        }
    }
}
