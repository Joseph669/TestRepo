using System;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Collections;
using System.Threading;
using VSCapture;
//using System.Data.SqlClient;
//using MySql.Data.MySqlClient;

namespace MainInterface
{
    public partial class MainFunc : Form
    {
        const int queueLen = 3000;
        private Queue<double> dataQueue = new Queue<double>(queueLen);
        private ArrayList aList = new ArrayList();
        private int curValue = 0;

        private int num = 75;//每次删除增加几个点

        DSerialPort _serialPort = DSerialPort.getInstance;

        //public MySqlConnection myConn = null;
        //public MySqlCommand myCommand = null;
        public string sql_test_data = "";
        private string sql_data_name;
        private string sql_tester;

        public string testerType;

        public MainFunc()
        {
            InitializeComponent();
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    UpdateQueueValue();
        //    this.chart1.Series[0].Points.Clear();
        //    for(int i=0;i<dataQueue.Count;i++){
        //        this.chart1.Series[0].Points.AddXY((i+1), dataQueue.ElementAt(i));
        //    }
        //} 

        //private void InitChart() 
        //{
        //    //定义图表区域
        //    this.chart1.ChartAreas.Clear();
        //    ChartArea chartArea1 = new ChartArea("C1");
        //    this.chart1.ChartAreas.Add(chartArea1);
        //    //定义存储和显示点的容器
        //    this.chart1.Series.Clear();
        //    Series series1 = new Series("S1");
        //    series1.ChartArea = "C1";
        //    this.chart1.Series.Add(series1);
        //    //设置图表显示样式
        //    this.chart1.ChartAreas[0].AxisY.Minimum = -30;
        //    this.chart1.ChartAreas[0].AxisY.Maximum = 30;
        //    this.chart1.ChartAreas[0].AxisX.Interval = 1;
        //    this.chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = System.Drawing.Color.Silver;
        //    this.chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = System.Drawing.Color.Silver;
        //    //设置标题
        //    //this.chart1.Titles.Clear();
        //    //this.chart1.Titles.Add("S01");
        //    //this.chart1.Titles[0].Text = "XXX显示";
        //    //this.chart1.Titles[0].ForeColor = Color.RoyalBlue;
        //    //this.chart1.Titles[0].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
        //    //设置图表显示样式
        //    this.chart1.Series[0].Color = Color.Red;
        //    this.chart1.Series[0].Points.Clear();
        //    //this.chart1.Titles[0].Text = string.Format("XXX  显示");
        //    this.chart1.Series[0].ChartType = SeriesChartType.Line;
        //}

        //初始化形状，将前面部分数据先画出来
        //private void InitShape()
        //{
        //    for (int i = 0; i < queueLen; i++)
        //    {
        //        dataQueue.Enqueue(Convert.ToDouble(aList[i]));
        //        this.chart1.Series[0].Points.AddXY((i + 1), dataQueue.ElementAt(i));
        //    }
        //}

        private void UpdateQueueValue()
        {
            if (dataQueue.Count > queueLen)
            {
                //先出列
                for (int i = 0; i < num; i++)
                {
                    dataQueue.Dequeue();
                }
            }

            for (int i = 0; i < num; i++)
            {
                dataQueue.Enqueue(Convert.ToDouble(aList[curValue]));
                curValue += 1;
                if (curValue == aList.Count)
                    timer1.Stop();
            }
        }

        //private void UpdateQueueValue()
        //{
        //    num = 300;
        //    if (dataQueue.Count > queueLen)
        //    {
        //        //先出列
        //        for (int i = 0; i < num; i++)
        //        {
        //            dataQueue.Dequeue();
        //        }
        //    }
        //    curValue = 0;
        //    for (int i = 0; i < num; i++)
        //    {
        //        dataQueue.Enqueue(Convert.ToDouble(DSerialPort.ECGList[curValue]));
        //        curValue += 1;
        //    }
        //}

        //private void openToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    OpenFileDialog fdlg = new OpenFileDialog();
        //    fdlg.Title = "C# Corner Open File Dialog";
        //    fdlg.InitialDirectory = @"c:\";   //@是取消转义字符的意思
        //    fdlg.Filter = "Excel Files|*.csv";
        //    /*
        //     * FilterIndex 属性用于选择了何种文件类型,缺省设置为0,系统取Filter属性设置第一项
        //     * ,相当于FilterIndex 属性设置为1.如果你编了3个文件类型，当FilterIndex ＝2时是指第2个.
        //     */
        //    fdlg.FilterIndex = 2;
        //    /*
        //     *如果值为false，那么下一次选择文件的初始目录是上一次你选择的那个目录，
        //     *不固定；如果值为true，每次打开这个对话框初始目录不随你的选择而改变，是固定的  
        //     */
        //    fdlg.RestoreDirectory = false;
        //    if (fdlg.ShowDialog() == DialogResult.OK)
        //    {
        //        StreamReader sr = new StreamReader(System.IO.Path.GetFullPath(fdlg.FileName));
                
        //        while (!sr.EndOfStream)
        //        {
        //            string[] csvList = sr.ReadLine().Split(',');
        //            aList.Add(double.Parse(csvList[1]));
        //        }

        //        InitChart();
        //        timer1.Start();
        //    }
        //}

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainFunc_Load(object sender, EventArgs e)
        {
            foreach (string str in SerialPort.GetPortNames())
            {
                this.comboBox1.Items.Add(str);
            }
            this.comboBox2.Items.Add("video1");
            this.comboBox2.Items.Add("video2");
            this.comboBox2.Items.Add("video3");

            this.comboBox3.Items.Add("志愿者");
            this.comboBox3.Items.Add("司机");
        }

        //private void test_Click(object sender, EventArgs e)
        //{
        //    StreamReader sr = new StreamReader(@"C:\Users\78144\Desktop\cai1.csv");
            
        //    while (!sr.EndOfStream)
        //    {
        //        string[] csvRawList = sr.ReadLine().Split(',');
        //        sql_test_data = sql_test_data + csvRawList[1] + ",";
        //        aList.Add(double.Parse(csvRawList[1]));
        //    }
            
        //    InitChart();
        //    InitShape();
        //    timer1.Start();
        //}
       
        private void test2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = DSerialPort.ECGList.Count.ToString();
            //MessageBox.Show("hello");
        }

        private bool CheckBoxFill()
        {
            if (this.textBox2.Text == string.Empty)
            {
                MessageBox.Show("测试者ID不能为空");
                return false;
            }
            else if (this.comboBox2.Text == string.Empty)
            {
                MessageBox.Show("视频序号不能为空");
                return false;
            }
            //else if (this.comboBox1.Text == string.Empty)
            //{
            //    MessageBox.Show("串口号不能为空");
            //    return false;
            //}
            else
                return true;
        }

        private void openPort_Click(object sender, EventArgs e)
        {
            if (CheckBoxFill())
            {
                //获取工作路径，若不存在该测试者，则创建文件夹
                CommonVal.DataWorkSpace = SetWorkSpace();

                _serialPort.Open();
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(p_DataReceived);
                MessageBox.Show("打开成功,正在接收数据");

                // 在开始之前，先清除buffer中缓存的数据
                _serialPort.DiscardInBuffer();

                _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2005);
                _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2003);
                _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2001);

                _serialPort.DiscardInBuffer();
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(p_DataReceived);
            }
        }

        private void closePort_Click(object sender, EventArgs e)
        {
            _serialPort.StopTransfer();
            _serialPort.StopWaveTransfer();
            _serialPort.Close();
            MessageBox.Show("关闭成功");
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            this.timer1.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            this.timer1.Stop();
        }

        static void p_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            ReadData(sender);
        }

        public static void ReadData(object sender)
        {
            try
            {
                (sender as DSerialPort).ReadBuffer();
            }
            catch (TimeoutException) { }
        }

        //若当前目录不存在该文件夹，则创建文件夹工作路径
        public string SetWorkSpace()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            string newDir = dir + "\\NO" + CommonVal.TesterID;
            if (!Directory.Exists(newDir))
            {
                Directory.CreateDirectory(newDir);
            }
            return newDir;
        }


        //选择串口
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            _serialPort.PortName = comboBox1.SelectedItem.ToString();
        }

        //选择视频序号
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            CommonVal.VideoNum = comboBox2.SelectedItem.ToString();
        }

        //选择测试者类型
        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            CommonVal.TesterType = comboBox3.SelectedItem.ToString();
        }

        //输入测试者ID
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            CommonVal.TesterID = textBox2.Text;
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (sql_tester == null)
            //{
            //    MessageBox.Show("请选择测试者");
            //}
            //else
            //{
            //    myConn = new MySqlConnection("Host =localhost;Port=3366;Database=" + sql_tester + ";Username=root;Password=123");
            //    myConn.Open();
            //    myCommand = myConn.CreateCommand();
            //}
        }

        private void insertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (textBox2.Text != null && sql_test_data != null)
            //{
            //    string str = "insert into volunteer(ID, {0}) values ({1}, {2})";
            //    string insert_cmd = string.Format(str, sql_data_name, textBox2.Text, sql_test_data);
            //    MySqlCommand sql_cmd = new MySqlCommand(insert_cmd, myConn);
            //    int result = sql_cmd.ExecuteNonQuery();

            //    if (result == 1)
            //    {
            //        MessageBox.Show("插入成功");
            //    }
            //    else
            //    {
            //        MessageBox.Show("插入失败");
            //    }
            //}
            //else if (textBox2.Text == null)
            //{
            //    MessageBox.Show("ID不能为空，请输入ID号");
            //}
            //else if (sql_test_data == null)
            //{
            //    MessageBox.Show("数据不能为空，请选择csv文件");
            //}
        }



        private void aboutElectrocardiographToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("  南方科技大学\n\r  虞亚军实验室");
        }

        private void usageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "1.连接好心电仪后，选择测试者类型、测试者ID、视频序号\r\n" +
                "2.之后选择串口并点击打开串口按钮，数据开始写入，并且软件开始显示实时心电图\r\n" +
                "3.测完一组数据后，点击关闭串口按钮，数据停止写入\r\n" +
                "4.重新选择测试者类型、测试者ID、视频序号，再点击打开串口按钮，即可继续写入数据\r\n" +
                "5.实验结束后，直接点击右上角的关闭，即可结束程序"
                );
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "1.连接好心电仪后，选择测试者类型、测试者ID、视频序号\r\n" +
                "2.之后选择串口并点击打开串口按钮，数据开始写入\r\n" +
                "3.测完一组数据后，点击关闭串口按钮，数据停止写入\r\n" +
                "4.重新选择测试者类型、测试者ID、视频序号，再点击打开串口按钮\r\n" +
                "5.实验结束后，直接点击右上角的关闭，即可结束程序\r\n\r\n" +
                "注意事项：\r\n" +
                "1.测完一组数据后，务必要先关闭串口，不然监护仪缓存区还会存在要发送的数据，"+
                "导致下次接收的数据前几秒的数据是上一次残留未发送的数据；\r\n" +
                "2.打开串口后，必要看一下在对应的路径下是否生成了该csv文件;\r\n" +
                "3.三个空白栏为必填选项，打开串口前请先填写完整；\r\n" +
                "4.上面菜单栏选项功能还未开发，请忽略上面的功能。"
            );
        }

        private void btnPlay_Click(object sender, EventArgs e)
        {
            string videoPath = Directory.GetCurrentDirectory() + "\\video\\"  + CommonVal.VideoNum + ".mp4";

            if (CheckBoxFill() == true)
            {
                axWindowsMediaPlayer1.Show();
                axWindowsMediaPlayer1.URL = videoPath;
                axWindowsMediaPlayer1.Ctlcontrols.play();
            }
        }

        private void axWindowsMediaPlayer1_StatusChange(object sender, EventArgs e)
        {
            if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsPlaying
                && CommonVal.isPlaying == false)
            {
                CommonVal.isPlaying = true;
                //获取工作路径，若不存在该测试者，则创建文件夹
                CommonVal.DataWorkSpace = SetWorkSpace();

                try
                {
                    _serialPort.Open();
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(p_DataReceived);
                    MessageBox.Show("播放中，后台正在接收数据，请耐心观看视频");

                    // 在开始之前，先清除buffer中缓存的数据
                    _serialPort.DiscardInBuffer();

                    _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2005);
                    _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2003);
                    _serialPort.RequestWaveTransfer(DataConstants.WF_REQ_CONT_START, DataConstants.DRI_LEVEL_2001);

                    //_serialPort.DiscardInBuffer();
                    _serialPort.DataReceived += new SerialDataReceivedEventHandler(p_DataReceived);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            else if (axWindowsMediaPlayer1.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                axWindowsMediaPlayer1.Hide();
                MessageBox.Show("视频停止，该数据已接收完全，谢谢您的配合");
                _serialPort.StopTransfer();
                _serialPort.StopWaveTransfer();
                _serialPort.Close();
                CommonVal.isPlaying = false;
            }
        }

    }
}


