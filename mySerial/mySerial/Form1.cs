using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.IO.Ports;
using System.IO;

namespace mySerial
{
    public partial class Form1 : Form
    {
        LSA myLSA;
        SerialPort mySerial = null; //声明串口类
        bool comExistence = false; //端口是否存在
        bool isOpen = false; //打开串口标志
        bool isHexReceive = false; //以十六进制显示接收
        bool isHexSend = false;//以十六进制显示发送
        bool isReceiveToFile = false; //接收到文件
        bool isSendFromFile = false; //从文件发送

        string DataReceive = string.Empty;
        int sentcount = 0;
        int receivecount = 0;

        Size groupBox1MinSize = new Size();
        int textBox2MinWidth = 0;

        public Form1()
        {
            InitializeComponent();
            this.MinimumSize = this.Size;
            myLSA = new LSA();
            groupBox1MinSize = groupBox1.Size;
            textBox2MinWidth = textBox2.Width;
            CheckCOM();
        }

        private void CheckCOM()
        {//检测端口
            comboBox1.Items.Clear();
            foreach (string comName in SerialPort.GetPortNames())
            {
                comboBox1.Items.Add(comName);
                comExistence = true;
            }
        }

        private bool SetPortProperty()
        {//设置串口属性
            try
            {
                mySerial.PortName = comboBox1.Text; //端口名
                mySerial.BaudRate = Convert.ToInt32(comboBox2.Text); //波特率

                string s = comboBox3.Text; //奇偶校验位
                if (s.CompareTo("NONE") == 0)
                {
                    mySerial.Parity = Parity.None;
                }
                else if (s.CompareTo("ODD") == 0)
                {
                    mySerial.Parity = Parity.Odd;
                }
                else if (s.CompareTo("EVEN") == 0)
                {
                    mySerial.Parity = Parity.Even;
                }
                else if (s.CompareTo("MARK") == 0)
                {
                    mySerial.Parity = Parity.Mark;
                }
                else if (s.CompareTo("SPACE") == 0)
                {
                    mySerial.Parity = Parity.Space;
                }
                else
                {
                    mySerial.Parity = Parity.None;
                }

                char[] ends = { 'b', 'i', 't'};
                mySerial.DataBits = Convert.ToInt16(comboBox4.Text.TrimEnd(ends)); //数据位

                float f = Convert.ToSingle(comboBox5.Text.TrimEnd(ends)); //停止位
                if (f == 0)
                {
                    mySerial.StopBits = StopBits.None;
                }
                else if (f == 1)
                {
                    mySerial.StopBits = StopBits.One;
                }
                else if (f == 1.5)
                {
                    mySerial.StopBits = StopBits.OnePointFive;
                }
                else if (f == 2)
                {
                    mySerial.StopBits = StopBits.Two;
                }
                else
                {
                    mySerial.StopBits = StopBits.One;
                }
                
                mySerial.ReadTimeout = -1;//读取超时时间
                mySerial.RtsEnable = true;
                mySerial.DataReceived += new SerialDataReceivedEventHandler(ReceiveEven);
                
                return true;
            }
            catch
            {
                label6.Text = "Setting port failed";
                return false;
            }
        }

        private void ReceiveEven(Object sender, SerialDataReceivedEventArgs e)
        {//接收数据事件
            Thread.Sleep(100);//等待接收完毕
            this.Invoke((EventHandler)(delegate { ReceiceData(); }));    
        }

        private void ReceiceData()
        {//接收数据
            DataReceive = string.Empty;
            Byte[] TempReceive = new Byte[mySerial.BytesToRead];
            mySerial.Read(TempReceive, 0, TempReceive.Length);
            if(TempReceive.Length == 14)
            if (TempReceive[0] == 0xff && TempReceive[13] == 0xee)
            {
                float mx = BitConverter.ToSingle(TempReceive, 1);
                float my = BitConverter.ToSingle(TempReceive, 5);
                float mz = BitConverter.ToSingle(TempReceive, 9);
                    myLSA.DataUpdate(mx, my);
                    textBox4.Text = myLSA.A.ToString();
                    textBox5.Text = myLSA.B.ToString();
                    magXY.Series[0].Points.AddXY(mx, my);
            }
            foreach (byte temp in TempReceive)
            {
                DataReceive += isHexReceive ? (temp.ToString("X2") + ' ') : ((char)temp).ToString();
            }
            textBox1.Text += DataReceive;
            receivecount += TempReceive.Length;
            label8.Text = "Received:" + receivecount.ToString();
            if (isReceiveToFile)
            {
                FileWrite(DataReceive);
            }
            mySerial.DiscardInBuffer();
        }

        private void OpenCloseCom() 
        {//打开或关闭串口
            if (isOpen == false) //尚未打开
            {
                mySerial = new SerialPort();
                if (!comExistence) //检测串口设置
                {
                    label6.Text = "Can't find port";
                    return;
                }
                
                if (!SetPortProperty())
                {
                    return;
                }
                try
                {
                    mySerial.Open();
                    isOpen = true;
                    button1.Text = "Close";
                    label6.Text = mySerial.PortName + " Open";
                    EnableOrNotCombo(false);
                }
                catch
                {
                    isOpen = false;
                    button1.Text = "Open";
                    label6.Text = mySerial.PortName + " open failed";
                    EnableOrNotCombo(true);
                }
            }
            else //已打开
            {
                try
                {
                    CheckCOM();
                    mySerial.Close();
                    mySerial.Dispose();
                    isOpen = false;
                    sentcount = 0;
                    receivecount = 0;
                    label7.Text = "Sent:0";
                    label8.Text = "Received:0";
                    button1.Text = "Open";
                    label6.Text = mySerial.PortName + " Closed";
                    EnableOrNotCombo(true);
                }
                catch
                {
                    label6.Text = mySerial.PortName + " close failed";
                    EnableOrNotCombo(true);
                }
            }
        }

        private void EnableOrNotCombo(bool value)
        {//使能参数设置框
            comboBox1.Enabled = value;
            comboBox2.Enabled = value;
            comboBox3.Enabled = value;
            comboBox4.Enabled = value;
            comboBox5.Enabled = value;
        }

        private void SendData(string DataToSend) 
        {//发送数据
            if (isSendFromFile)
            {
                DataToSend = FileRead();
                textBox2.Text = DataToSend;
            }
            if (isHexSend)
            {
                string[] ByteStrings = DataToSend.Split(" ".ToCharArray());
                DataToSend = string.Empty;
                foreach (string temp in ByteStrings)
                {
                    DataToSend += (char)Convert.ToByte(temp, 16);
                }
                Console.WriteLine(DataToSend);
            }
            if (DataToSend == string.Empty)
            {
                label6.Text = "Sent nothing";
                return;
            }
            if (isOpen)
            {
                try
                {
                    mySerial.Write(DataToSend);
                    sentcount += DataToSend.Length;
                    label7.Text = "Sent:" + sentcount.ToString();
                }
                catch
                {
                    label7.Text = "Sending failed";
                    return;
                }
            }
            else
            {
                label6.Text = "Port not open";
            }
        }

        private void FileWrite(string value)
        {//写入文件
            try
            {
                String FilePath = ".\\dataReceived.txt";
                FileStream myfile = new FileStream(FilePath, FileMode.Append);
                StreamWriter sw = new StreamWriter(myfile);
                sw.Write(value);
                sw.Close();
            }
            catch
            {
                label6.Text = "File write failed";
            }
        }

        private string FileRead()
        {//读取文件
            try
            {
                string FilePath = ".\\dataToSend.txt";
                FileStream myfile = new FileStream(FilePath, FileMode.OpenOrCreate);
                StreamReader sw = new StreamReader(myfile);
                int fileSize = (int)myfile.Length;
                char[] content = new char[fileSize];
                sw.Read(content, 0, fileSize);
                sw.Close();
                string contentStr = string.Empty;
                foreach (char alpha in content)
                {
                    contentStr += alpha;
                }
                return contentStr;
            }
            catch
            {
                label6.Text = "File read failed";
                return string.Empty;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {//打开串口
            OpenCloseCom();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {//清除接收缓存
            textBox1.Text = string.Empty;
        }

        private void button2_Click(object sender, EventArgs e)
        {//发送数据
            SendData(textBox2.Text);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {//以十六进制显示接收
            isHexReceive = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {//接收到文件
            isReceiveToFile = checkBox2.Checked;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {//调整窗口大小
            groupBox1.Size = groupBox1MinSize + this.Size - MinimumSize;
            //textBox2.Width = textBox2MinWidth + this.Size.Width - MinimumSize.Width;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {//清除发送缓存
            textBox2.Text = string.Empty;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {//以十六进制显示发送
            isHexSend = checkBox4.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {//发送文件内容
            isSendFromFile = checkBox3.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {//周期发送数据
            timer1.Interval = Convert.ToInt32(textBox3.Text);
            timer1.Enabled = checkBox5.Checked;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {//周期发送数据的定时器
            SendData(textBox2.Text);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {
           
        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}