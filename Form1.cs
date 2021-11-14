using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UIforSeletek
{
    public partial class Form1 : Form
    {
        static SerialPort _serialPort;
        bool conected = false;
        public Form1()
        {
            InitializeComponent();
            timer1.Enabled = false;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
		{
			getPorts();
		}

        private void init()
        {
            toolStripStatusLabel1.Visible = Properties.Settings.Default.toolStripVisible;
            progressBar1.Maximum = Math.Max(Properties.Settings.Default.cupolOpen, Properties.Settings.Default.cupolClose);
            progressBar1.Minimum = Math.Min(Properties.Settings.Default.cupolOpen, Properties.Settings.Default.cupolClose);
        }

		private void getPorts()
		{
			comboBoxDeviceList.Items.Clear();

			string[] ports = SerialPort.GetPortNames();

			foreach (var item in ports)
			{
				comboBoxDeviceList.Items.Add(item);
			}
		}

		private void buttonConDis_Click(object sender, EventArgs e)
        {
            if (!conected)
            {
                conection();
            }
            else
            {
                disconection();
            }
        }

        private void disconection()
        {
            timer1.Stop();

            _serialPort.Close();
            _serialPort.Dispose();
            _serialPort = null;

            buttonOpen.Enabled = false;
            buttonClose.Enabled = false;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;

            conected = false;
            buttonConDis.Text = "Подключиться";
        }

        private void conection()
        {
            var tt = comboBoxDeviceList.Items[comboBoxDeviceList.SelectedIndex].ToString();
            _serialPort = new SerialPort(comboBoxDeviceList.Items[comboBoxDeviceList.SelectedIndex].ToString());
            _serialPort.BaudRate = 115200;

            _serialPort.Open();

            _serialPort.Write($"!step model 1 3#!step halfstep 1 0#!step speed 1 45#!step stoppow 1 0#!step movepow 1 1024#!step setpos 1 {Properties.Settings.Default.lastCupolPos}#");

            _serialPort.DataReceived += _serialPort_DataReceived;


            buttonOpen.Enabled = true;
            buttonClose.Enabled = true;
            button1.Enabled = true;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;

            timer1.Start();

            conected = true;

            buttonConDis.Text = "Отключиться";
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //read data waiting in the buffer
            string msg = _serialPort.ReadExisting();
            toolStripStatusLabel1.Text = msg;
            infoUpdate(msg);
        }

        private bool[] onoff = new bool[4];
        private void button1_Click(object sender, EventArgs e)
        {

            int but = Convert.ToInt32(((Button)sender).Tag) - 1;
            onoff[but] = !onoff[but];
            _serialPort.Write($"!write dig 0 {but} { string.Format(onoff[but] ? "1" : "0") }#");

            rewrite();
        }
        private void rewrite()
        {
            for (int i = 0; i < 4; i++)
            {
                //checkedListBox1.SetItemChecked(i, onoff[i]);
            }
        }
        private void buttonOpen_Click(object sender, EventArgs e)
        {
            _serialPort.Write($"!step goto 1 {Properties.Settings.Default.cupolOpen}#");
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            _serialPort.Write($"!step goto 1 {Properties.Settings.Default.cupolClose}#");
        }

		private void buttonInfoUpdate_Click(object sender, EventArgs e)
		{
			getInfo();
		}

		private static void getInfo()
		{
			_serialPort.Write("!step getpos 1#");
            _serialPort.Write("!read dig 0 0#");
            _serialPort.Write("!read dig 0 1#");
            _serialPort.Write("!read dig 0 2#");
            _serialPort.Write("!read dig 0 3#");

        }

		private void infoUpdate(string msgs)
		{
            foreach (var msg in msgs.Split('!'))
			{
                if (msg.Contains("step getpos 1"))
                {
                    int pos = Convert.ToInt32(msg.Split(':')[1].Replace("#", ""));
                    if (pos == Properties.Settings.Default.cupolClose)
                    {
                        pictureBoxCupolClose.Image = Properties.Resources.circle_true;

                        labelTelescoupeState.Text = $"телескоп закрыт";
                    }
                    if (pos == Properties.Settings.Default.cupolOpen)
                    {
                        pictureBoxCupolOpen.Image = Properties.Resources.circle_true;
                        labelTelescoupeState.Text = $"телескоп открыт";
                    }
                    if (pos != Properties.Settings.Default.cupolClose && pos != Properties.Settings.Default.cupolOpen)
                    {
                        pictureBoxCupolClose.Image = Properties.Resources.circle_false;
                        pictureBoxCupolOpen.Image = Properties.Resources.circle_false;

                        labelTelescoupeState.Text = $"находится в движении - {pos}";
                    }
                    
                        progressBar1.BeginInvoke((MethodInvoker)(() => 
                        {
                            try
                            {
                                this.progressBar1.Value = pos;
                            }
                            catch (Exception)
                            {
                                timer1.Stop();
                                MessageBox.Show("Значения выходят из диапозона. Проверьте положение купола и его параметров.", msg);
                            }
                        }
                        ));
                    Properties.Settings.Default.lastCupolPos = pos;
                    Properties.Settings.Default.Save();
                }
                if (msg.Contains("read dig 0 0"))
                {
                    if (msg.Split(':')[1] == "0#")
                    {
                        pictureBox1.Image = Properties.Resources.circle_false;
                    }
                    if (msg.Split(':')[1] == "1#")
                    {
                        pictureBox1.Image = Properties.Resources.circle_true;
                    }
                }
                if (msg.Contains("read dig 0 1"))
                {
                    if (msg.Split(':')[1] == "0#")
                    {
                        pictureBox2.Image = Properties.Resources.circle_false;
                    }
                    if (msg.Split(':')[1] == "1#")
                    {
                        pictureBox2.Image = Properties.Resources.circle_true;
                    }
                }
                if (msg.Contains("read dig 0 2"))
                {
                    if (msg.Split(':')[1] == "0#")
                    {
                        pictureBox3.Image = Properties.Resources.circle_false;
                    }
                    if (msg.Split(':')[1] == "1#")
                    {
                        pictureBox3.Image = Properties.Resources.circle_true;
                    }
                }
                if (msg.Contains("read dig 0 3"))
                {
                    if (msg.Split(':')[1] == "0#")
                    {
                        pictureBox4.Image = Properties.Resources.circle_false;
                    }
                    if (msg.Split(':')[1] == "1#")
                    {
                        pictureBox4.Image = Properties.Resources.circle_true;
                    }
                }

            }

        }

		private void timer1_Tick(object sender, EventArgs e)
		{
            try
            {
                getInfo();
            }
            catch (Exception)
            {
                disconection();
                MessageBox.Show("Ваше устройство отключенно");
            }
                
            
        }

		private void Form1_Load(object sender, EventArgs e)
		{
            getPorts();
            init();
        }

        private void показатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Visible = true;
            Properties.Settings.Default.toolStripVisible = true;
            Properties.Settings.Default.Save();
        }

        private void скрытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStripStatusLabel1.Visible = false;
            Properties.Settings.Default.toolStripVisible = false;
            Properties.Settings.Default.Save();
        }

        private void изменитьЗначенияКуполаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FormCupolSettings fc = new FormCupolSettings();
            fc.FormClosed += Fc_FormClosed;
            this.Hide();
            fc.Show();
        }

        private void Fc_FormClosed(object sender, FormClosedEventArgs e)
        {
            init();
            this.Show();

        }
    }
}
