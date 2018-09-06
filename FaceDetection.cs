using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Collections;

namespace FaceDetection
{
    public partial class FaceDetection : Form
    {
        private int[] emotion = new int[7];
        private int[] FuPercent = new int[4];
        private double[] FuCurrent = new double[4];
        private int CounterCStatus = 0;
        private int CounterCStatus1 = 0;
        private Queue myQueue = new Queue();
        private double[] ScoreArray = new double[10];

        public FaceDetection()
        {
            InitializeComponent();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateFuDataToMySQL("ipcame_4");
            UpdateSkDataToMySQL("human_pose_ip_cam_4");

            int count = 0;
            for (int i = 0; i < 7; i++)
                count += emotion[i];

            if (count != 0)
            {
                FuCurrent[0] = (double)(emotion[3] + emotion[6]) / count;
                FuCurrent[1] = (double)(emotion[1] + emotion[4]) / count;
                FuCurrent[2] = (double)(emotion[2] + emotion[5]) / count;
                FuCurrent[3] = (double)(emotion[0]) / count;

                FuPercent[0] += emotion[3] + emotion[6];
                FuPercent[1] += emotion[1] + emotion[4];
                FuPercent[2] += emotion[2] + emotion[5];
                FuPercent[3] += emotion[0];

                label13.Text = (100 * (emotion[3] + emotion[6]) / count).ToString() + "%";
                label14.Text = (100 * (emotion[1] + emotion[4]) / count).ToString() + "%";
                label12.Text = (100 * (emotion[2] + emotion[5]) / count).ToString() + "%";
                label11.Text = (100 * emotion[0] / count).ToString() + "%";

                for (int i = 0; i < 7; i++)
                    emotion[i] = 0;
            }
            PaintPieChart();
            StatisticsCEmotion(FuCurrent);
        }

        private void PaintPieChart()
        {
            chartEmotion.Series["EmotionPercentage"].Points.DataBindXY(null, FuPercent);
            chartEmotion.Series["EmotionPercentage"].IsValueShownAsLabel = true;

            if (FuPercent.Sum() != 0)
            {
                label3.Text = (FuPercent[0] * 100 / FuPercent.Sum()).ToString() + "%";
                label4.Text = (FuPercent[1] * 100 / FuPercent.Sum()).ToString() + "%";
                label5.Text = (FuPercent[2] * 100 / FuPercent.Sum()).ToString() + "%";
                label2.Text = (FuPercent[3] * 100 / FuPercent.Sum()).ToString() + "%";
            }
        }

        private void StatisticsCEmotion(double[] temp)
        {
            if (CounterCStatus % 100 == 0)
                chartConcentration.Series[0].Points.Clear();
            Random crandom = new Random();
            double score = (crandom.NextDouble() * 0.1 + 1) * (temp[1] + temp[0] - temp[3] - temp[2]);

            if (score >= 1) score = 1.0;
            if (score <= -1) score = -1.0;

            chartConcentration.Series[0].Points.AddY(score);
            CounterCStatus++;
        }

        private void UpdateFuDataToMySQL(string TableName)
        {
            string dbHost = "192.168.0.203";
            string dbUser = "david";
            string dbPass = "hellohello";
            string dbName = "classfu_ipcam_database";
            string connStr = "server=" + dbHost + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8;";
            string ResultStr;

            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        ResultStr = "ErrCode = 0";
                        break; //"連線到資料庫."
                    case 1045:
                        ResultStr = "ErrCode = 1";
                        break; //"使用者帳號或密碼錯誤,請再試一次.";
                }
            }

            string SQL = "SELECT * FROM " + TableName + " ORDER BY id DESC LIMIT 1";
            try
            {
                MySqlCommand cmd = new MySqlCommand(SQL, conn);
                MySqlDataReader myData = cmd.ExecuteReader();

                if (!myData.HasRows)
                    ResultStr = "null"; // 如果沒有資料,顯示沒有資料的訊息
                else
                {
                    ResultStr = myData.Read().ToString(); // 讀取資料並且顯示出來

                    DateTime dt1 = Convert.ToDateTime(myData.GetString("time"));
                    DateTime dt2 = DateTime.Now;
                    TimeSpan ts = dt2 - dt1;

                    if (ts.TotalSeconds < 20)
                    {
                        emotion[0] += Int32.Parse(myData.GetString("angry_number"));
                        emotion[1] += Int32.Parse(myData.GetString("disgust_number"));
                        emotion[2] += Int32.Parse(myData.GetString("fear_number"));
                        emotion[3] += Int32.Parse(myData.GetString("happy_number"));
                        emotion[4] += Int32.Parse(myData.GetString("neutral_number"));
                        emotion[5] += Int32.Parse(myData.GetString("sad_number"));
                        emotion[6] += Int32.Parse(myData.GetString("surpris_number"));
                    }

                    if (TableName == "ipcame_4")
                    {
                        //string url = "http://" + dbHost + "/classfu_img_ipcam/ipcam_4/" + myData.GetString("img_name");
                        //pictureBox16.ImageLocation = url;
                    }
                }
                myData.Close();
            }
            catch (System.InvalidOperationException ex)
            {
                MessageBox.Show("網路異常,系統請重啟");
            }

            //if (id >= end_id) timer1.Stop();
        }

        private void UpdateSkDataToMySQL(string TableName)
        {
            string dbHost = "192.168.0.203";
            string dbUser = "david";
            string dbPass = "hellohello";
            string dbName = "human_pose";
            string connStr = "server=" + dbHost + ";uid=" + dbUser + ";pwd=" + dbPass + ";database=" + dbName + ";charset=utf8;";
            string ResultStr;

            MySqlConnection conn = new MySqlConnection(connStr);
            try
            {
                conn.Open();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        ResultStr = "ErrCode = 0";
                        break; //"連線到資料庫."
                    case 1045:
                        ResultStr = "ErrCode = 1";
                        break; //"使用者帳號或密碼錯誤,請再試一次.";
                }
            }

            string SQL = "SELECT * FROM " + TableName + " ORDER BY id DESC LIMIT 1";
            try
            {
                MySqlCommand cmd = new MySqlCommand(SQL, conn);
                MySqlDataReader myData = cmd.ExecuteReader();

                if (!myData.HasRows)
                    ResultStr = "null"; // 如果沒有資料,顯示沒有資料的訊息
                else
                {
                    ResultStr = myData.Read().ToString(); // 讀取資料並且顯示出來

                    DateTime dt1 = Convert.ToDateTime(myData.GetString("time"));
                    DateTime dt2 = DateTime.Now;
                    TimeSpan ts = dt2 - dt1;

                        if (CounterCStatus1 % 100 == 0)
                            chart1.Series[0].Points.Clear();
                        chart1.Series[0].Points.AddY(Int32.Parse(myData.GetString("people_number")));
                        CounterCStatus1++;

                    if (ts.TotalSeconds < 20)
                    {


                        /*data[0] = Int32.Parse(myData.GetString("people_number"));
                        data[1] = Int32.Parse(myData.GetString("body_positive"));
                        data[2] = Int32.Parse(myData.GetString("body_negative"));
                        data[3] = Int32.Parse(myData.GetString("raise_right_hand"));
                        data[4] = Int32.Parse(myData.GetString("raise_left_hand"));
                        data[5] = Int32.Parse(myData.GetString("raise_two_hand"));
                        data[6] = Int32.Parse(myData.GetString("look_left"));
                        data[7] = Int32.Parse(myData.GetString("look_mid"));
                        data[8] = Int32.Parse(myData.GetString("look_right"));*/
                        label17.Text = myData.GetString("people_number");
                        label7.Text = myData.GetString("body_positive");
                        label15.Text = myData.GetString("body_negative");
                        label16.Text = myData.GetString("right_hand");
                        label1.Text = myData.GetString("left_hand");
                        label9.Text = myData.GetString("two_hand");
                        label10.Text = myData.GetString("look_left");
                        label6.Text = myData.GetString("look_mid");
                        label8.Text = myData.GetString("look_right");
                    }

                    if (TableName == "human_pose_ip_cam_4")
                    {
                        string url = "http://"+ dbHost + "/HumanPose/human_pose_ipcam_4/" + myData.GetString("img_name");
                        pictureBox16.ImageLocation = url;
                    }
                }
                myData.Close();
            }
            catch (MySql.Data.MySqlClient.MySqlException ex)
            {
                ResultStr = "ErrCode = 2 :" + ex.ToString();
            }
        }

        private void FaceDetection_Load(object sender, EventArgs e)
        {

            if (groupBox1.Text == "  即時畫面 ")
            {
                groupBox1.Text = "  情境氣氛  ";
            }



            chart1.ChartAreas[0].AxisX.Maximum = 100;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisX.LineWidth = 1;

            chart1.ChartAreas[0].AxisY.Maximum = 50.0F;
            chart1.ChartAreas[0].AxisY.Minimum = 0.0F;
            chart1.ChartAreas[0].AxisY.LineWidth = 1;
            chart1.ChartAreas[0].AxisY.Interval = 10.0F;
            chart1.ChartAreas[0].AxisY.IsStartedFromZero = true;

            chart1.ChartAreas[0].BackColor = Color.Transparent;
            chart1.Series[0].Color = Color.White;

            double[] yValues2 = { 0.0F };
            chart1.Series[0].Points.DataBindXY(null, yValues2);


            chartConcentration.ChartAreas[0].AxisX.Maximum = 100;
            chartConcentration.ChartAreas[0].AxisX.Minimum = 0;
            chartConcentration.ChartAreas[0].AxisX.LineWidth = 1;

            chartConcentration.ChartAreas[0].AxisY.Maximum = 1.0F;
            chartConcentration.ChartAreas[0].AxisY.Minimum = -1.0F;
            chartConcentration.ChartAreas[0].AxisY.LineWidth = 1;
            chartConcentration.ChartAreas[0].AxisY.Interval = 1.0F;
            chartConcentration.ChartAreas[0].AxisY.IsStartedFromZero = true;

            chartConcentration.ChartAreas[0].BackColor = Color.Transparent;
            chartConcentration.Series[0].Color = Color.White;

            double[] yValues1 = { 0.0F };
            chartConcentration.Series[0].Points.DataBindXY(null, yValues1);



            Color ColorAnger = new Color();
            ColorAnger = Color.FromArgb(240, 96, 36);
            Color ColorHappiness = new Color();
            ColorHappiness = Color.FromArgb(255, 222, 23);
            Color ColorNeutral = new Color();
            ColorNeutral = Color.FromArgb(67, 191, 202);
            Color ColorSadness = new Color();
            ColorSadness = Color.FromArgb(1, 112, 144);

            chartEmotion.PaletteCustomColors = new Color[] {  ColorHappiness, ColorNeutral, ColorSadness, ColorAnger  };

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox16_Click(object sender, EventArgs e)
        {
            if (groupBox1.Text == "  情境氣氛  ")
            {
                groupBox1.Text = "  即時畫面 ";
            }
            else if (groupBox1.Text == "  即時畫面 ")
            {
                groupBox1.Text = "  情境氣氛  ";
            }
        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox13_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("chrome.exe", "http://192.168.0.203/report/print.php");
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }
    }
}