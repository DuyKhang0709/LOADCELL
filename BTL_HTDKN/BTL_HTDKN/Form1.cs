using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.IO.Ports;
namespace BTL_HTDKN
{
    public partial class Form1 : Form
    {
        private byte[] receivedDataBuffer = new byte[256]; // Định danh buffer đệm cho dữ liệu nhận được
        private int receivedDataIndex = 0;
        private bool isReceivingFrame = false;
        bool flag = true;
        bool flag1 = false;
        float m1;
        int cnt=0;
        public Form1()
        {
            InitializeComponent();
        }
        const int BUFFER_SIZE = 12; 
        byte[] txbuff = new byte[BUFFER_SIZE]; // 1 byte == 8 bit unsigned integer
        /* Transfer data buffer txbuff[] */
        // 0       |   1       | 2 - 5       | 6 - 9         | 10           | 11   
        // 0x01    |    0x0C   |     ....    | ....          |              |0x02        
        // header  | length    | Zero value  | Span value    | checksum     |footer      
        const int HEADER = 0x01;
        const int LENGTH = 0x0C;
        const int FOOTER = 0x02;

        string[] Baudrate = { "1200", "2400", "4800", "9600", "115200" };
        double mass0;
        double  zero_calib;
        double  span_calib;
        double mass_zero_calib,  mass_span_calib;

       
        private void Form1_Load(object sender, EventArgs e)
        {
           

            Control.CheckForIllegalCrossThreadCalls = false;
            cboComPort.DataSource = SerialPort.GetPortNames(); //detect automatically
            cboBaudRate.Items.AddRange(Baudrate);
            cboBaudRate.Text = "9600"; //default

            zero.Text = 0.ToString();
            span.Text = 1.ToString();
            trackBar1.Value = 500;
        }

        private void serCOM_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) // 1122334455
        {
            try
            {

                while (serCOM.BytesToRead > 0 && flag)
                {
                    
                    byte receivedByte = (byte)serCOM.ReadByte();
                    if (receivedByte == 0x01) // Nếu nhận được byte header
                    {
                        receivedDataIndex = 0;
                        isReceivingFrame = true;
                    }
                    else if (receivedByte == 0x02 && isReceivingFrame) // Nếu nhận được byte footer
                    {
                        isReceivingFrame = false;
                        // Xử lý dữ liệu đã nhận được ở đây
                        ProcessReceivedData();                      
                    }
                    else if (isReceivingFrame) // Nếu đang nhận dữ liệu trong frame
                    {   
                        receivedDataBuffer[receivedDataIndex++] = receivedByte;
                    }
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi nhận dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void butSaveData_Click(object sender, EventArgs e)
        {
            StreamWriter text1 = new StreamWriter("D:\\HK232\\Data_BTL\\test.txt",true);
            text1.WriteLine(txtAllData );
            text1.Close(); 
        }

        private void butConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serCOM.IsOpen)
                {
                    butConnect.Text = "Disconnected";
                    serCOM.PortName = cboComPort.Text;
                    serCOM.BaudRate = Convert.ToInt32(cboBaudRate.Text);
                    serCOM.Open();
                    timer1.Enabled = true;
                }
                else
                {
                    butConnect.Text = "Connected";
                    serCOM.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Lỗi!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void butExit_Click(object sender, EventArgs e)
        {
            Application.Exit();

        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
           

            zero_calib = ((float)(trackBar0.Value))/1000;
            zero.Text =  zero_calib.ToString();
            mass_zero_calib = mass0 - zero_calib;
            mass.Text = (mass_zero_calib).ToString("N3");
            
        }

        private void trackBar1_Scroll_1(object sender, EventArgs e)
        {
            span_calib = ((float)(trackBar1.Value)) / 1000;
            span.Text = (span_calib ).ToString();
            mass_span_calib = m1 * (span_calib);

            mass.Text = (mass_span_calib).ToString("N3");
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        

        private void mass_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtAllData_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            if(serCOM.IsOpen == false)
            {
                MessageBox.Show("Device is not connected");
            }
            else
            {
                try
                {
                    flag1 = true;
                    txbuff[0] = HEADER;
                    txbuff[1] = LENGTH;
                    txbuff[11] = FOOTER;

                    byte[] zero_byte = new byte[4];
                    float zero_float = float.Parse(zero.Text);
                    zero_byte = BitConverter.GetBytes(zero_float);
                    Array.Copy(zero_byte, 0, txbuff, 2, 4);

                    byte[] span_byte = new byte[4];
                    float span_float = float.Parse(span.Text);
                    span_byte = BitConverter.GetBytes(span_float);
                    Array.Copy(span_byte, 0, txbuff, 6, 4);

                    byte checksum = CalculateChecksum(span_byte); // Tính toán checksum cho dữ liệu
                    checksum ^= CalculateChecksum(zero_byte);
                    checksum ^= LENGTH;
                    txbuff[10] = checksum;

                    serCOM.Write(txbuff, 0, txbuff.Length);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi gửi dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            mass.Text = mass0.ToString();
            trackBar0.Value = 0;
            trackBar1.Value = 1000;
            zero.Text = 0.ToString();
            span.Text = 1.ToString();
        }

        private void zero_TextChanged(object sender, EventArgs e)
        {
            //zero_raw = double.Parse(zero.Text);
        }

        private void ProcessReceivedData()
        {
            // Kiểm tra checksum
            byte calculatedChecksum = CalculateChecksum(receivedDataBuffer, receivedDataIndex);
            byte receivedChecksum = receivedDataBuffer[receivedDataIndex - 1];

            if (calculatedChecksum == receivedChecksum)
            {
                // Dữ liệu hợp lệ, xử lý dữ liệu ở đây
                // Ví dụ: hiển thị dữ liệu trong TextBox
                string frame = BitConverter.ToString(receivedDataBuffer);
                txtAllData.Text = frame ;
                byte[] receivedDataBuffer1 = new byte[4];

                //for (int i = 0; i < 4; i++)
                //  receivedDataBuffer1[i] = receivedDataBuffer[4 - i];

                //float myFloat = System.BitConverter.ToSingle(receivedDataBuffer1, 0);
                float myFloat = System.BitConverter.ToSingle(receivedDataBuffer, 1);
                mass0 =myFloat;
                mass.Text = myFloat.ToString();
            }
            else
            {
              //  MessageBox.Show("Checksum không khớp. Dữ liệu có thể đã bị hỏng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private byte CalculateChecksum(byte[] data, int length)
        {
            byte checksum = 0;
            for (int i = 0; i < length - 1; i++)
            {
                checksum ^= data[i];
            }
            return checksum;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            trackBar0.Value += 1;
            zero_calib=((float)(trackBar0.Value)) / 1000;
            zero.Text = zero_calib.ToString();
            mass_zero_calib = mass0 - zero_calib;
            mass.Text = (mass_zero_calib).ToString("N3");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            trackBar0.Value -= 1;
            zero_calib = ((float)(trackBar0.Value)) / 1000;
            zero.Text = zero_calib.ToString();
            mass_zero_calib = mass0 - zero_calib;
            mass.Text = (mass_zero_calib).ToString("N3");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            trackBar1.Value += 1;
            span_calib = ((float)(trackBar1.Value)) / 1000;
            span.Text = (span_calib).ToString();
            mass_span_calib = m1 * (span_calib);
            mass.Text = (mass_span_calib).ToString("N3");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            trackBar1.Value -= 1;
            span_calib = ((float)(trackBar1.Value)) / 1000;
            span.Text = (span_calib).ToString();
            mass_span_calib = m1 * (span_calib);
            mass.Text = (mass_span_calib).ToString("N3");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            flag = false;
            m1 = float.Parse(mass.Text);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            flag = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            
            
        }

        private byte CalculateChecksum(byte[] data)
        {
            byte checksum = 0;
            foreach (byte b in data)
            {
                checksum ^= b;
            }
            return checksum;
        }
        
       
        
    }
}
